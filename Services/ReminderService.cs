using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PestoBot.Api.Common;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Discord;
using ILogger = Serilog.ILogger;

namespace PestoBot.Services
{
    public class ReminderService
    {
        #region Constructor & Setup

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

        #endregion

        public void Start()
        {
            _reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 5);
            //reminderTimer = new Timer(WakeReminderService, null, 0, 5000); //5 second sleep period for debugging/Testing
        }

        internal void WakeReminderService(object state)
        {
            _reminderServiceLog.Information($"{DateTime.Now.ToString(LogDateFormat)} : Waking Reminder Service");
            FireRemindersByAssignmentType();
            _reminderServiceLog.Information($"{DateTime.Now.ToString(LogDateFormat)} : Reminder Service Sleeping");
        }

        protected internal void FireRemindersByAssignmentType()
        {
            _reminderServiceLog.Information($"Processing one time reminders");
            ProcessReminders(ReminderTypes.Task);
            if (IsTimeToProcessRecurringReminders(ReminderTimes.Project))
            {
                _reminderServiceLog.Information("Processing recurring reminders");
                ProcessReminders(ReminderTypes.Project);
            }

            if (IsTimeToProcessRecurringReminders(ReminderTimes.GoobyTime))
            {
                _reminderServiceLog.Information("Processing Debug Reminders");
                ProcessReminders(ReminderTypes.DebugTask);
            }
        }

        protected internal virtual bool IsTimeToProcessRecurringReminders(ReminderTimes reminderTime)
        {
            var now = GetCurrentTime();
            return now.Hour == (int) reminderTime && now.Minute < 30;
        }

        protected internal virtual void ProcessReminders(ReminderTypes type)
        {
            if (type == ReminderTypes.Project)
            {
                _reminderServiceLog.Warning("Recurring reminders are disabled until design refactor is complete");
                return;
            }
            //Get list of Event Assignments by type 
            var eventTaskAssignments = GetListOfAssignments(type);
            //Filter into reminders that are due 
            var dueReminders = eventTaskAssignments.Where(ShouldSendReminder);
            //Send reminders if required
            foreach (var reminder in dueReminders)
            {
                SendReminder(reminder);
            }
        }

        protected internal virtual List<EventTaskAssignmentModel> GetListOfAssignments(ReminderTypes type)
        {
            return EventTaskAssignmentApi.GetAllAssignmentsByType(type);
        }

        internal virtual bool ShouldSendReminder(EventTaskAssignmentModel eventTaskAssignment)
        {
            var reminderType = (ReminderTypes) eventTaskAssignment.AssignmentType;
            return _oneTimeReminderTypes.Contains(reminderType) ? ShouldSendOneTimeReminder(eventTaskAssignment)
                : _recurringReminderTypes.Contains(reminderType) && ShouldSendRecurringReminder(eventTaskAssignment);
        }

        protected internal virtual bool ShouldSendOneTimeReminder(EventTaskAssignmentModel eventTaskAssignment)
        {
            if (eventTaskAssignment.LastReminderSent != DateTime.MinValue) return false; //Do not send one time reminder if it has already been sent

            var dueDate = GetDueDate(eventTaskAssignment);
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

        protected internal virtual bool ShouldSendRecurringReminder(EventTaskAssignmentModel eventTaskAssignment)
        {
            //Not implemented yet
            return false;
        }

        protected internal virtual DateTime GetDueDate(EventTaskAssignmentModel eventTaskAssignment)
        {
            var modelType = (ReminderTypes)eventTaskAssignment.AssignmentType;

            if (_oneTimeReminderTypes.Contains(modelType))
            {
                // These are reminders that occur a certain time before a scheduled run or task
                // e.g. Tell a runner to get ready 30m before a run
                return eventTaskAssignment.TaskStartTime;
            }

            if (_recurringReminderTypes.Contains(modelType))
            {
                // Returns a long term due date for Projects that may span multiple marathons or may not be tied to a specific marathon.
                // Examples include Creating a website, writing a schedule, recruiting runners etc
                return eventTaskAssignment.ProjectDueDate;
            }
            
            throw new ArgumentException("Assignment does not have a valid type");
        }

        protected internal virtual DateTime GetCurrentTime()
        {
            //This is just virtualized for testing 
            return DateTime.Now;
        }

        protected internal void SendReminder(EventTaskAssignmentModel eventTaskAssignment)
        {
            throw new NotImplementedException();
        }
    }
}
