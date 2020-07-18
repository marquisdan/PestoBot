using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PestoBot.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.SpeedrunEvent;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Discord;
using ILogger = Serilog.ILogger;

namespace PestoBot.Services
{
    public class ReminderService
    {
        private ulong reminderServiceId;
        private string reminderServiceToken;
        private const int Minute = 60000;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        internal const int TaskReminderTime = 30; //Remind tasks this many minutes before due date
        private const string LogDateFormat = "MMMM dd, yyyy HH:mm:ss tt zz";

        private readonly List<ReminderTypes> _oneTimeReminderTypes;
        private readonly List<ReminderTypes> _recurringReminderTypes;

        //Service injection 
        private IConfiguration _config;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private Microsoft.Extensions.Logging.ILogger _logger;
        private IServiceProvider _serviceProvider;

        // ReSharper disable once NotAccessedField.Local
        private Timer _reminderTimer;
        private readonly ILogger _reminderServiceLog; //separate log to filter out Reminder Service events 

        public ReminderService(IServiceProvider services)
        {
            // ReSharper disable VirtualMemberCallInConstructor
            InitServices(services);
            _reminderServiceLog = CreateReminderServiceLoggerConfiguration();

            //Populate reminder lists
            _oneTimeReminderTypes = new List<ReminderTypes>
            {
                ReminderTypes.DebugTask,
                ReminderTypes.Run,
                ReminderTypes.Task
            };

            _recurringReminderTypes = new List<ReminderTypes>
            {
                ReminderTypes.Project,
                ReminderTypes.DebugProject
            };
        }

        protected internal virtual Logger CreateReminderServiceLoggerConfiguration()
        {
            var keys = ConfigService.BuildKeysConfig();
            reminderServiceId = ulong.Parse(keys.GetSection("Webhooks").GetSection("ReminderService").GetSection("Id").Value);
            reminderServiceToken = keys.GetSection("Webhooks").GetSection("ReminderService").GetSection("Token").Value;
            return new LoggerConfiguration()
                .WriteTo.Discord(reminderServiceId, reminderServiceToken)
                .WriteTo.File("PestoLogs/Services/Reminder_Service.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File("PestoLogs/Services/Reminder_Service_Debug.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File("PestoLogs/Services/Reminder_Service_Error.log", rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }

        protected internal virtual void InitServices(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfiguration>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();
            _serviceProvider = services;
        }

        public void Start()
        {
            _reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 5);
            //reminderTimer = new Timer(WakeReminderService, null, 0, 5000); //5 second sleep period for debugging/Testing
        }

        internal void WakeReminderService(object state)
        {
            _reminderServiceLog.Information($"{DateTime.Now.ToString(LogDateFormat)} : Waking Reminder Service");
            FireRemindersByType();
            _reminderServiceLog.Information($"{DateTime.Now.ToString(LogDateFormat)} : Reminder Service Sleeping");
        }

        protected internal void FireRemindersByType()
        {
            _reminderServiceLog.Information($"Processing tasks");
            ProcessReminders(ReminderTypes.Task);
            if (IsTimeToProcessReminders(ReminderTimes.Project))
            {
                _reminderServiceLog.Information("Processing Project Reminders");
                ProcessReminders(ReminderTypes.Project);
            }

            if (IsTimeToProcessReminders(ReminderTimes.GoobyTime))
            {
                _reminderServiceLog.Information("Processing Debug Reminders");
                ProcessReminders(ReminderTypes.DebugTask);
            }
        }

        protected internal virtual bool IsTimeToProcessReminders(ReminderTimes reminderTime)
        {
            var now = GetCurrentTime();
            return now.Hour == (int) reminderTime && now.Minute < 30;
        }

        protected internal virtual void ProcessReminders(ReminderTypes type)
        {
            if (type == ReminderTypes.Project)
            {
                _reminderServiceLog.Warning("Project reminders are disabled until design refactor is complete");
                return;
            }
            //Get list of reminders
            var reminders = GetListOfReminders(type);
            //Filter into reminders that are due 
            var dueReminders = reminders.Where(ShouldSendReminder);
            //Send reminders if required
            foreach (var reminder in dueReminders)
            {
                SendReminder(reminder);
            }
        }

        protected internal virtual List<ReminderModel> GetListOfReminders(ReminderTypes type)
        {
            var repo = new ReminderRepository();
            return repo.GetRemindersByType(type).Result;
        }

        internal virtual bool ShouldSendReminder(ReminderModel model)
        {
            var modelType = (ReminderTypes) model.Type;
            return _oneTimeReminderTypes.Contains(modelType) ? ShouldSendOneTimeReminder(model)
                : _recurringReminderTypes.Contains(modelType) && ShouldSendRecurringReminder(model);
        }

        protected internal virtual bool ShouldSendOneTimeReminder(ReminderModel model)
        {
            if (model.LastSent != DateTime.MinValue) return false; //Do not send reminder if it has already been sent

            var dueDate = GetDueDate(model);
            var currentTime = GetCurrentTime();
            var timeSpan = dueDate - currentTime;
            if (dueDate < currentTime)
            {
                //Do not send a reminder if reminder time already past
                return false;
            }
            //Finally, determine if within reminder window and return result
            return timeSpan <= TimeSpan.FromMinutes((int) ReminderTimes.Task); 
        }

        protected internal virtual bool ShouldSendRecurringReminder(ReminderModel model)
        {
            return false;
        }

        protected internal virtual DateTime GetDueDate(ReminderModel model)
        {
            var modelType = (ReminderTypes)model.Type;

            if (_oneTimeReminderTypes.Contains(modelType))
            {
                return GetOneTimeReminderDueDate(model);
            }

            if (_recurringReminderTypes.Contains(modelType))
            {
                return GetRecurringReminderDueDate(model);
            }
            
            throw new ArgumentException("Reminder does not have a valid type");
        }

        /// <summary>
        /// Returns a short term due date for Tasks or Runs
        /// These are reminders that occur a certain time before a scheduled run or task
        /// e.g. Tell a runner to get ready 30m before a run
        /// </summary>
        /// <returns></returns>
        protected internal DateTime GetOneTimeReminderDueDate(ReminderModel model)
        {
            var assignment = GetAssignmentForReminder(model) as MarathonTaskAssignmentModel;
            return assignment?.TaskStartTime ?? DateTime.MinValue;
        }

        /// <summary>
        /// Returns a long term due date for Projects that may span multiple marathons or may not be tied to a specific marathon.
        /// Examples include Creating a website, writing a schedule, recruiting runners etc
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected internal DateTime GetRecurringReminderDueDate(ReminderModel model)
        {
            var assignment = GetAssignmentForReminder(model) as MarathonProjectAssignmentModel;
            var project = GetProjectForAssignment(assignment);
            return project.DueDate ?? DateTime.MinValue;
        }

        protected internal virtual IPestoModel GetAssignmentForReminder(ReminderModel model)
        {
            return new ReminderRepository().GetAssignmentForReminder(model).Result;
        }

        protected internal virtual MarathonProjectModel GetProjectForAssignment(MarathonProjectAssignmentModel model)
        {
            throw new NotImplementedException();
        }

        protected internal virtual DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        protected internal void SendReminder(ReminderModel reminder)
        {
            throw new NotImplementedException();
        }
    }
}
