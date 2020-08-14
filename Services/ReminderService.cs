using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CsvHelper;
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

        // ReSharper disable once NotAccessedField.Local
        protected Timer _reminderTimer;
        protected readonly ILogger _reminderServiceLog; //separate log to filter out Reminder Service events 

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

        public ReminderService()
        {
            _serviceProvider = ServicesConfiguration.GetServiceProvider();
            InitServices(_serviceProvider);

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

        internal virtual void Start()
        {
            //_reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 5);
            //reminderTimer = new Timer(WakeReminderService, null, 0, 5000); //5 second sleep period for debugging/Testing
        }

        internal virtual void WakeReminderService(object state)
        {
            _reminderServiceLog.Verbose($"{DateTime.Now.ToString(LogDateFormat)} : Waking Reminder Service");
            FireRemindersByAssignmentType();
            _reminderServiceLog.Verbose($"{DateTime.Now.ToString(LogDateFormat)} : Reminder Service Sleeping");
        }

        protected internal virtual void FireRemindersByAssignmentType()
        {
            //Always process One-time reminders
            _reminderServiceLog.Information($"Processing one time reminders");
            ProcessReminders(ReminderTypes.Task);

            //Process Recurring Reminders
            if (IsTimeToProcessRecurringReminders(ReminderTimes.Project))
            {
                _reminderServiceLog.Information("Processing recurring reminders");
                ProcessReminders(ReminderTypes.Project);
            }

            //Process Debug Reminders
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
            if(channel == null) { _reminderServiceLog.Warning($"{guild.Name} does not have a type {reminderType} channel set");}
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
                _reminderServiceLog.Error($"Could not send reminder. {guild.Name} is likely missing a {reminderType} reminder channel");
            }
            else
            {
                //Send the reminder
                var reminderChannel = (IMessageChannel)_client.GetChannel((ulong)logChannelId);
                await reminderChannel.SendMessageAsync(eventTaskAssignment.ReminderText);
            }
        }
    }

    public  class MarathonReminderService : ReminderService
    {
        //Timing Stuff
        private int _behind = 0;

        //Runner Headers
        private const string ContentHeader = "Content";
        private const string DiscordUserHeader = "DiscordUserName";
        private const string ScheduleTimeHeader = "Scheduled";
        private const string GameHeader = "Game";

        //Volunteer Headers

        //Filename Stuff
        // ReSharper disable InconsistentNaming
        private const string KEY_RUNNERS = "Runners";
        private const string KEY_HOSTS = "Hosts";
        private const string KEY_RESTREAM = "Restreamers";
        private const string KEY_SETUP = "Setup";
        private const string KEY_SOCIAL_MEDIA = "Social_Media";
        // ReSharper restore InconsistentNaming

        public string EventName { get; set; }
        
        public MarathonReminderService(IServiceProvider services) : base(services)
        {
            if(_serviceProvider == null){ _serviceProvider = ServicesConfiguration.GetServiceProvider();}
        }

        public MarathonReminderService() : base() { }

        internal override void Start()
        {
            _reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 1);
        }

        protected void SetBehind(int behind)
        {
            _behind = behind;
            _reminderTimer.Change(0, Minute * behind);
        }

        protected internal override void FireRemindersByAssignmentType()
        {

            _reminderServiceLog.Information($"Checking reminders for {EventName}");
            //Handle Runners
            ReadReminderCSV("runners");
            //Handle Hosts

            //Handle Restreamers

            //Handle Setup

            //Handle Social Media

        }

        private void ReadReminderCSV(string fileName)
        {

            try
            {
                var filePath = $@"{EventName}\{fileName}.csv";
                var records = new List<object>();

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Configuration.Delimiter = ";"; //Horaro uses ;, yay
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var record = new
                    {
                        RunStart = csv.GetField<DateTime>(ScheduleTimeHeader),
                        Game = csv.GetField<string>(GameHeader),
                        Username = csv.GetField<string>(DiscordUserHeader), 
                        //Sent = csv.GetField<string>(DiscordUserHeader)
                    };

                    records.Add(record);
                }

                using (var writer = new StreamWriter($@"{EventName}\{fileName}_Processing.csv"))
                using (var csv2 = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv2.WriteRecords(records);
                }
                reader.Close();
                
            }
            catch (Exception e)
            {
                _reminderServiceLog.Error(e.Message);
            }
        }

    }
}

/*
            var isId = ulong.TryParse(pingTarget, out var id);
            IUser user;
            if (isId)
            {
                user = Context.Client.GetUserAsync(id).Result;
            }
            else
            {
                var userName = pingTarget.Split('#');
                user = Context.Client.GetUserAsync(userName[0], userName[1]).Result;
            }

            await ReplyAsync($"{user.Mention} test");
*/