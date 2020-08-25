using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using CsvHelper;
using Discord;
using PestoBot.Common;

namespace PestoBot.Services
{
    public  class MarathonReminderService : ReminderService
    {
        #region Properties & Fields

        internal ulong StatusChannelId; //For sending statuses back to where the service started.

        //Timing Stuff & File done flags
        private int _minutesBehind = 0;
        private bool _runnersDone = false;
        private bool _volunteersDone = false;

        //Filepath stuff
        private static readonly char Separator = Path.DirectorySeparatorChar;
        private const string LockFileName = "LockFile.txt";
        private const string EndRunFileName = "EndRun";
        internal const string MinutesBehindFileName = "minutesBehind";
        private string _filePath;

        private readonly List<string> _listFileNames = new List<string>
        {
            LockFileName, EndRunFileName, MinutesBehindFileName
        };

        //CSV Headers
        private const string ContentHeader = "Content";
        private const string DiscordUserHeader = "DiscordUserName";
        private const string ScheduleTimeHeader = "Scheduled";
        private const string VolunteerTypeHeader = "VolunteerType";
        private const string CategoryHeader = "Category";
        private const string GameHeader = "Game";
        private const string SentHeader = "Sent";

        //Filename Stuff
        private const string Filename_Runners = "runners";
        private const string Filename_Volunteers = "volunteers";

        private readonly List<string> _listCsvNames = new List<string>
        {
            Filename_Runners, Filename_Volunteers
        };

        public string EventName { get; set; }

        #endregion

        #region Constructors & Destructors

        public MarathonReminderService(IServiceProvider services) : base(services)
        {
            if(_serviceProvider == null){ _serviceProvider = ServicesConfiguration.GetServiceProvider();}

            _filePath = $"{EventName}{Separator}";
        }

        public MarathonReminderService(IServiceProvider services, ulong statusChannelId, string eventName)
        {
            if (_serviceProvider == null) { _serviceProvider = ServicesConfiguration.GetServiceProvider(); }
            StatusChannelId = statusChannelId;
            EventName = eventName;
            _filePath = $"{eventName}{Separator}";
        }

        public MarathonReminderService(ulong statusChannelId, string eventName)
        {
            _serviceProvider = ServicesConfiguration.GetServiceProvider();
            StatusChannelId = statusChannelId;
            EventName = eventName;
            _filePath = $"{eventName}{Separator}";
        }

        public MarathonReminderService() : base() { }

        #endregion

        internal override void Start()
        {
            _filePath = $"{EventName}{Separator}";
            if (DoesLockFileExist() == false)
            {
                ClearFiles();
                CreateLockFile();
                ReminderTimer = new Timer(WakeReminderService, null, 0, Minute * 1);
            }
            else
            {
                throw new Exception($"Cannot start - Reminder Service for {EventName} already running!");
            }

        }

        #region Static methods for communicating with service
        internal static void EndRun(string eventName)
        {
            var writer = new StreamWriter($"{eventName}{Separator}{EndRunFileName}");
            writer.Write("End of the line");
            writer.Close();
        }

        internal static void SetMinutesBehind(string eventName, int minutes)
        {
            var path = $"{eventName}{Separator}{MinutesBehindFileName}";
            //First, delete existing file
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            //Write a new file containing the minutes behind
            var writer = new StreamWriter(path);
            writer.Write(minutes.ToString());
            writer.Close();
        }

        internal static bool DoesLockFileExist(string eventName)
        {
            var path = $"{eventName}{Separator}{LockFileName}";
            return File.Exists(path);
        }

        #endregion

        #region Dynamic file interaction

        private void ClearFiles()
        {
            //Clear files used to communicate with client
            foreach (var file in _listFileNames)
            {
                var path = $"{_filePath}{file}";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private void CreateLockFile()
        {
            CreateFile(LockFileName);
        }

        private void CreateFile(string fileName)
        {
            var writer = new StreamWriter($"{_filePath}{fileName}");
            writer.Write("file created, yo");
            writer.Close();
        }

        private bool DoesLockFileExist()
        {
            return File.Exists($"{_filePath}{LockFileName}");
        }

        private bool DoesEndRunFileExist()
        {
            return File.Exists($"{_filePath}{EndRunFileName}");
        }

        #endregion

        protected void UpdateTimeBehind()
        {
            var path = $"{EventName}{Separator}{MinutesBehindFileName}";
            string txt = "";
            if (File.Exists(path))
            {
                txt = File.ReadAllText(path);
            }

            var canParseFile = int.TryParse(txt, out int minutes);

            if (canParseFile == false && File.Exists(path))
            {
                ReminderServiceLog.Error($"Unable to parse {path}. [{txt}] is not a valid integer");
                return;
            }

            if (txt.IsNullOrEmpty())
            {
                _minutesBehind = 0;
            }
            else
            {
                _minutesBehind = minutes;
            }
            
        }

        protected internal override void FireRemindersByAssignmentType()
        {
            //First, check if end run file exists, exit immediately if it does
            if (DoesEndRunFileExist())
            {
                ReminderServiceLog.Debug($"End run file found. Preventing further reads for {EventName} and shutting down");
                KillTimer();
                ClearFiles();
                return;
            }

            ReminderServiceLog.Verbose($"Checking reminders for {EventName}");

            foreach (var fileName in _listCsvNames)
            {
                ReadReminderCsv(fileName);
            }
        }

        private void ReadReminderCsv(string fileName)
        {
            //if all records have been read in every file, kill the timer
            if (_runnersDone && _volunteersDone)
            {
                ReminderServiceLog.Information($"All records sent, ending {EventName} timer");
                KillTimer();
                return;
            }

            //Update how far we are behind
            UpdateTimeBehind();

            //Don't process files that are already finished 
            switch (fileName)
            {
                case Filename_Runners:
                    if (_runnersDone)
                    {
                        return;
                    }
                    break;
                case Filename_Volunteers:
                    if (_volunteersDone)
                    {
                        return;
                    }
                    break;

                default: throw new ArgumentException($"{fileName} is invalid!");
            }

            var filePath = $@"{_filePath}{fileName}.csv";
            var processingPath = $@"{_filePath}{fileName}_Processing.csv";

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
                            _runnersDone = true;
                            break;
                        case Filename_Volunteers:
                            _volunteersDone = true;
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

                ReminderServiceLog.Verbose($"Replacing file {fileName} with processed content");
                File.Replace(processingPath, filePath, null); //Replace existing file with processing 
            }
            catch (Exception e)
            {
                ReminderServiceLog.Error(e.Message);
            }
        }

        private void KillTimer()
        {
            ReminderTimer.Change(Timeout.Infinite, Timeout.Infinite); //stop the timer
            //Log & send message to user that timer has stopped
            ReminderServiceLog.Information($"Timer {EventName} ended. Bye bye!");
            var msg = TextUtils.GetInfoText($"Reminder service for {EventName} stopped successfully");
            ((IMessageChannel) _client.GetChannel(StatusChannelId)).SendMessageAsync(msg);
        }

        private IEnumerable<ReminderRecord> ProcessReminderRecords(List<ReminderRecord> records)
        {
            foreach (var record in records)
            {
                ReminderServiceLog.Debug($"Processing record {record.Scheduled}:{record.DiscordUserName}:{record.Game}");
                if (record.Sent == 0 && IsScheduledTimeWithinReminderRange(record, out var minutes))
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
            ReminderServiceLog.Information($"Sending record {record.DiscordUserName} : {record.Game} in {minutes} minutes");
            var reminderMessage = user != null ? user.Mention : record.DiscordUserName.Split('#')[0];
            ulong channelId = 0;
            
            if (!record.VolunteerType.IsNullOrEmpty())
            {
                reminderMessage += $" your shift for `Volunteer: {record.VolunteerType}` is coming up in **{minutes} minutes!**";
                channelId = GetVolunteerReminderChannel(); //MWSF Volunteer Reminders
            }
            else
            {
                reminderMessage += $" your run for `{record.Game}: {record.Category}` is coming up in **{minutes} minutes!**";
                channelId = GetRunnerReminderChannel(); //MWSF Runner Reminders
            }

            ((IMessageChannel) _client.GetChannel(channelId)).SendMessageAsync(reminderMessage);
        }

        private void SendMessageToBartaan(ReminderRecord record)
        {
            const ulong id = 264274966025994241; //Bartaan's ID
            var bartaan = _client.GetUser(id);
            string msg = $"Hey {bartaan.Mention}";
            var userName = record.DiscordUserName.Split('#')[0];

            if (userName.ToLower() == "cajink")
            {
                msg += $" Cajink's run will start in 30 minutes!";
            }
            if(userName.ToLower() == "marquisdan" && record.VolunteerType.ToLower() == "host")
            {
                msg += $" marquisdan's hosting will start in 30 minutes!";
            }

            bartaan.SendMessageAsync(msg);
        }

        private ulong GetVolunteerReminderChannel()
        {
            //TODO: Fetch this dynamically from DB 
            return 738224182587818004;
            // return GetDebugChannel();
        }

        private ulong GetRunnerReminderChannel()
        {
            //TODO: Fetch this dynamically from DB 
            return 738222137449381888;
            //  return GetDebugChannel();
        }

        private ulong GetDebugChannel()
        {
            return 745655771181744258;
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

        protected internal virtual bool IsScheduledTimeWithinReminderRange(ReminderRecord record, out int minutes)
        {
            //Find the (possibly adjusted) run time of the record. Store the value in minutes.
            var runTime = record.Scheduled;
            if(ShouldRecordScheduleBeAdjusted(record)){
                runTime = runTime.AddMinutes(_minutesBehind); //Adjust if marathon is ahead or behind schedule
            };  
            var currentTime = GetCurrentTime();
            var timeSpan = runTime - currentTime;
            minutes = timeSpan.Minutes;

            //Determine if a reminder should be sent. 
            if (runTime < currentTime)
            {
                //Do not send a reminder if reminder time already past
                return false;
            }
            //Finally, determine if within reminder window and return result
            return timeSpan <= TimeSpan.FromMinutes((int)ReminderTimes.Task+1); //Makes most messages say 30m not 29
        }

        private bool ShouldRecordScheduleBeAdjusted(ReminderRecord record)
        {
            //Only records with flexible schedules should be adjusted (e.g. those tied specifically to runs.)
            //Static volunteer records should not be adjusted (restreaming, setup, Social Media etc) 
            return !record.Game.IsNullOrEmpty() || record.VolunteerType.ToLower() == "host";
        }

    }
}