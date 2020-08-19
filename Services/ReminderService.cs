using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
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
        //Timing Stuff & File done flags
        private int _behind = 0;
        private bool RunnersDone = false;
        private bool VolunteersDone = false;

        //Runner Headers
        private const string ContentHeader = "Content";
        private const string DiscordUserHeader = "DiscordUserName";
        private const string ScheduleTimeHeader = "Scheduled";
        private const string VolunteerTypeHeader = "VolunteerType";
        private const string CategoryHeader = "Category";
        private const string GameHeader = "Game";
        private const string SentHeader = "Sent";

        //Volunteer Headers

        //Filename Stuff
        private const string Filename_Runners = "runners";
        private const string Filename_Volunteers = "volunteers";
        private const string Filename_Hosts = "hosts";
        private const string Filename_Restreamers = "restreamers";
        private const string Filename_Setup = "setup";
        private const string Filename_Social_Media = "social_media";
        //private readonly List<string> _listFileNames = new List<string>
        //{
        //    Filename_Runners, Filename_Hosts, Filename_Restreamers, Filename_Setup, Filename_Social_Media
        //};    
        //private readonly List<string> _listFileNames = new List<string>
        //{
        //    Filename_Runners, Filename_Volunteers
        //}; 
        private readonly List<string> _listFileNames = new List<string>
        {
            Filename_Runners, Filename_Volunteers
        };

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

            _reminderServiceLog.Verbose($"Checking reminders for {EventName}");

            foreach (var fileName in _listFileNames)
            {
                ReadReminderCSV(fileName);
            }
        }

        private void ReadReminderCSV(string fileName)
        {
            switch (fileName)
            {
                case Filename_Runners:
                    if (RunnersDone)
                    {
                        return;
                    }
                    break;
                case Filename_Volunteers:
                    if (VolunteersDone)
                    {
                        return;
                    }
                    break;

                default: throw new ArgumentException($"{fileName} is invalid!");
            }

            if (RunnersDone && VolunteersDone)
            {
                _reminderServiceLog.Information("All records sent, ending timer");
                _reminderTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            var filePath = $@"{EventName}\{fileName}.csv";
            var processingPath = $@"{EventName}\{fileName}_Processing.csv";

            try
            {
                
                var records = new List<ReminderRecord>();
                //Setup the CSV Reader
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Configuration.MissingFieldFound = null;
                csv.Read();
                csv.ReadHeader();

                //Read the CSV and populate list of records
                while (csv.Read())
                {
                    var record = new ReminderRecord
                    {
                        Scheduled = csv.GetField<DateTime>(ScheduleTimeHeader),
                        Game = csv.GetField<string>(GameHeader),
                        VolunteerType = csv.GetField<string>(VolunteerTypeHeader),
                        Category = csv.GetField<string>(CategoryHeader),
                        DiscordUserName = csv.GetField<string>(DiscordUserHeader), 
                        Sent = csv.GetField<int>(SentHeader)
                    };

                    records.Add(record);
                }

                //Process the reminders here, send any reminders that are due & update the sent flag
                var processedRecords = ProcessReminderRecords(records).ToList();

                //Mark the file done if all rows have been sent
                if (processedRecords.Count(x => x.Sent == 1) == processedRecords.Count)
                {
                    switch (fileName)
                    {
                        case Filename_Runners:
                            RunnersDone = true;
                            break;
                        case Filename_Volunteers:
                            VolunteersDone = true;
                            break;

                        default: throw new ArgumentException($"{fileName} is invalid!");
                    }
                }

                using (var writer = new StreamWriter(processingPath))
                {
                    using var csv2 = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv2.WriteRecords(processedRecords);
                }
                reader.Close();

                _reminderServiceLog.Verbose($"Replacing file {fileName} with processed content");
                File.Replace(processingPath, filePath, null); //Replace existing file with processing 
            }
            catch (Exception e)
            {
                _reminderServiceLog.Error(e.Message);
            }
        }

        private IEnumerable<ReminderRecord> ProcessReminderRecords(List<ReminderRecord> records)
        {
            foreach (var record in records)
            {
                _reminderServiceLog.Debug($"Processing record {record.Scheduled}:{record.DiscordUserName}:{record.Game}");
                if (record.Sent == 0 && IsScheduledTimeWithinReminderRange(record.Scheduled, out var minutes))
                {
                    //send any reminders that are due & update the sent flag
                    record.Sent = 1;
                    SendReminder(record, minutes);
                }
            }

            return records;
        }

        protected internal void SendReminder(ReminderRecord record, int minutes)
        {
            var user = GetUser(record.DiscordUserName);
            _reminderServiceLog.Information($"Sending record {record.DiscordUserName} : {record.Game} in {minutes} minutes");
            var reminderMessage = user != null ? user.Mention : record.DiscordUserName.Split('#')[0];
            ulong channelId = 0;
            if (!record.VolunteerType.IsNullOrEmpty())
            {
                reminderMessage += $" your shift for `Volunteer: {record.VolunteerType}` is coming up in **{minutes} minutes!**";
                channelId = 738224182587818004; //MWSF Volunteer Reminders
            }
            else
            {
                reminderMessage += $" your run for `{record.Game}: {record.Category}` is coming up in **{minutes} minutes!**";
                channelId = 738222137449381888; //MWSF Runner Reminders
            }
            ((IMessageChannel) _client.GetChannel(channelId)).SendMessageAsync(reminderMessage);
        }

        private IUser GetUser(string discordNameOrId)
        {
            var isId = ulong.TryParse(discordNameOrId, out var id);
            IUser user;
            if (isId)
            {
                user = _client.GetUser(id);
            }
            else
            {
                var userName = discordNameOrId.Split('#');
                user = _client.GetUser(userName[0], userName[1]);
            }

            return user;
        }

        protected internal virtual bool IsScheduledTimeWithinReminderRange(DateTime scheduledTime, out int minutes)
        {
            var runTime = scheduledTime.AddMinutes(_behind);  //Adjust if marathon is ahead or behind schedule
            var currentTime = GetCurrentTime();
            var timeSpan = runTime - currentTime;
            minutes = timeSpan.Minutes;

            if (runTime < currentTime)
            {
                //Do not send a reminder if reminder time already past
                return false;
            }
            //Finally, determine if within reminder window and return result
            return timeSpan <= TimeSpan.FromMinutes((int)ReminderTimes.Task+1); //Makes most messages say 30m not 29
        } 

    }
}

public class ReminderRecord
{
    public DateTime Scheduled { get; set; }
    public string Game { get; set; }
    public string Category { get; set; }
    public string VolunteerType { get; set; }
    public string DiscordUserName { get; set; }
    public int Sent { get; set; }

}

/*
                        Scheduled = csv.GetField<DateTime>(ScheduleTimeHeader),
                        Game = csv.GetField<string>(GameHeader),
                        DiscordUserName = csv.GetField<string>(DiscordUserHeader), 
                        Sent = csv.GetField<string>(SentHeader)

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