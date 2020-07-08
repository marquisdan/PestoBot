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
    public enum ReminderTimes
    {
        Task = 30, //Fire task reminders 30m in advance
        Project = 17, //Fire daily reminders at 5pm
        GoobyTime = 20 //Fire Gooby reminder at 8pm
    }

    public class ReminderService
    {
        private ulong reminderServiceId;
        private string reminderServiceToken;
        private const int Minute = 60000;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        internal const int TaskReminderTime = 30; //Remind tasks this many minutes before due date
        private const string LogDateFormat = "MMMM dd, yyyy HH:mm:ss tt zz";

        //Service injection 
        private IConfiguration _config;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private Microsoft.Extensions.Logging.ILogger _logger;
        private IServiceProvider _serviceProvider;

        //Times for specific reminder types

        private Timer reminderTimer;
        private readonly ILogger ReminderServiceLog; //separate log to filter out Reminder Service events 

        public ReminderService(IServiceProvider services)
        {
            // ReSharper disable VirtualMemberCallInConstructor
            InitServices(services);
            ReminderServiceLog = CreateReminderServiceLoggerConfiguration();
        }

        protected internal virtual Logger CreateReminderServiceLoggerConfiguration()
        {
            reminderServiceId = ulong.Parse(_config.GetSection("Webhooks").GetSection("ReminderService").GetSection("Id").Value);
            reminderServiceToken = _config.GetSection("Webhooks").GetSection("ReminderService").GetSection("Token").Value;
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
            reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 5);
            //reminderTimer = new Timer(WakeReminderService, null, 0, 5000); //5 second sleep period for debugging/Testing
        }

        internal void WakeReminderService(object state)
        {
            ReminderServiceLog.Information($"{DateTime.Now.ToString(LogDateFormat)} : Waking Reminder Service");
            FireRemindersByType();
            ReminderServiceLog.Information($"{DateTime.Now.ToString(LogDateFormat)} : Reminder Service Sleeping");
        }

        protected internal void FireRemindersByType()
        {
            ReminderServiceLog.Information($"Processing tasks");
            ProcessReminders(ReminderTypes.Task);
            if (IsTimeToProcessReminders(ReminderTimes.Project))
            {
                ReminderServiceLog.Information("Processing Project Reminders");
                ProcessReminders(ReminderTypes.Project);
            }

            if (IsTimeToProcessReminders(ReminderTimes.GoobyTime))
            {
                ReminderServiceLog.Information("Processing Debug Reminders");
                ProcessReminders(ReminderTypes.Debug);
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
                ReminderServiceLog.Warning("Project reminders are disabled until design refactor is complete");
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
            switch ((ReminderTypes) model.Type)
            {
                case ReminderTypes.Task:
                    return ShouldSendTaskReminder(model);
                //Implement other reminder types here
                default: return false;
            }
            
        }

        protected internal virtual bool ShouldSendTaskReminder(ReminderModel model)
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

        protected internal virtual DateTime GetDueDate(ReminderModel model)
        {
            switch ((ReminderTypes) model.Type) {
                case ReminderTypes.Task:
                    return GetShortTermDueDate(model);
                case ReminderTypes.Project:
                    return GetLongTermDueDate(model);
                case ReminderTypes.Run:
                    return GetShortTermDueDate(model);
                case ReminderTypes.Debug:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a short term due date for Tasks or Runs
        /// These are reminders that occur a certain time before a scheduled run or task
        /// e.g. Tell a runner to get ready 30m before a run
        /// </summary>
        /// <returns></returns>
        protected internal DateTime GetShortTermDueDate(ReminderModel model)
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
        protected internal DateTime GetLongTermDueDate(ReminderModel model)
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
