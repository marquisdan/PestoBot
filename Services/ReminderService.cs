﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PestoBot.Api.Common;
using PestoBot.Api.Event;
using PestoBot.Common;
using PestoBot.Database.Models.Event;
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

        protected ulong reminderServiceId;
        protected string reminderServiceToken;
        protected const int Minute = 60000;
        protected const int Hour = Minute * 60;
        protected const int Day = Hour * 24;
        internal const int TaskReminderTime = 30; //Remind tasks this many minutes before due date
        protected const string LogDateFormat = "MMMM dd, yyyy HH:mm:ss tt zz";

        protected readonly List<ReminderTypes> _oneTimeReminderTypes;
        protected readonly List<ReminderTypes> _recurringReminderTypes;

        //Service injection 
        protected IConfiguration _config;
        protected DiscordSocketClient _client;
        protected CommandService _commands;
        protected Microsoft.Extensions.Logging.ILogger _logger;
        protected IServiceProvider _serviceProvider;

        protected Timer ReminderTimer;
        protected readonly ILogger ReminderServiceLog; //separate log to filter out Reminder Service events 

        public ReminderService(IServiceProvider services)
        {
            // ReSharper disable VirtualMemberCallInConstructor
            InitServices(services);
            ReminderServiceLog = CreateReminderServiceLoggerConfiguration();

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

        public ReminderService()
        {
            _serviceProvider = ServicesConfiguration.GetServiceProvider();
            InitServices(_serviceProvider);

            ReminderServiceLog = CreateReminderServiceLoggerConfiguration();
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

        internal virtual void Start()
        {
            //_reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 5);
            //reminderTimer = new Timer(WakeReminderService, null, 0, 5000); //5 second sleep period for debugging/Testing
        }

        internal virtual void WakeReminderService(object state)
        {
            ReminderServiceLog.Verbose($"{DateTime.Now.ToString(LogDateFormat)} : Waking Reminder Service");
            FireRemindersByAssignmentType();
            ReminderServiceLog.Verbose($"{DateTime.Now.ToString(LogDateFormat)} : Reminder Service Sleeping");
        }

        protected internal virtual void FireRemindersByAssignmentType()
        {
            //Always process One-time reminders
            ReminderServiceLog.Information($"Processing one time reminders");
            ProcessReminders(ReminderTypes.Task);

            //Process Recurring Reminders
            if (IsTimeToProcessRecurringReminders(ReminderTimes.Project))
            {
                ReminderServiceLog.Information("Processing recurring reminders");
                ProcessReminders(ReminderTypes.Project);
            }

            //Process Debug Reminders
            if (IsTimeToProcessRecurringReminders(ReminderTimes.GoobyTime))
            {
                ReminderServiceLog.Information("Processing Debug Reminders");
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
                ReminderServiceLog.Warning("Recurring reminders are disabled until design refactor is complete");
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
            return new EventTaskAssignmentApi().GetAllAssignmentsByType(type);
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

        protected virtual ulong? GetReminderChannelForType(SocketGuild guild, ReminderTypes reminderType)
        {
            var channel = new GuildSettingsApi().GetReminderChannelForType(guild.Id, reminderType);
            if(channel == null) { ReminderServiceLog.Warning($"{guild.Name} does not have a type {reminderType} channel set");}
            return channel;
        }

        protected internal virtual async void SendReminder(EventTaskAssignmentModel eventTaskAssignment)
        {
            var guild = _client.GetGuild(eventTaskAssignment.Id);
            var reminderType = (ReminderTypes) eventTaskAssignment.AssignmentType;
            var logChannelId = GetReminderChannelForType(guild, reminderType);

            if (logChannelId == null)
            {
                //Check if the guild has that type of channel set
                ReminderServiceLog.Error($"Could not send reminder. {guild.Name} is likely missing a {reminderType} reminder channel");
            }
            else
            {
                //Send the reminder
                var reminderChannel = (IMessageChannel)_client.GetChannel((ulong)logChannelId);
                await reminderChannel.SendMessageAsync(eventTaskAssignment.ReminderText);
            }
        }

    }
}