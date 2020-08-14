using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal.Execution;
using PestoBot.Common.DBUtils;
using PestoBot.Entity;
using PestoBot.Entity.Common;
using PestoBot.Entity.Event;
using PestoBot.Services;

namespace PestoBot.Common.CSVReaders
{
    internal class ReminderReader
    {
        //Headers
        private const string ContentHeader = "Content";
        private const string DiscordUserHeader = "DiscordUserName";
        private const string ScheduleTimeHeader = "Scheduled";
        private const string GameHeader = "Game";

        private static DiscordSocketClient _client;
        private static ILogger _logger;

        public string FilePath { get; set; }
        public ReminderTypes ReminderType { get; set; }


        internal ReminderReader(DiscordSocketClient client)
        {
            _client = client;
        }

        internal ReminderReader(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        internal ReminderReader(IServiceProvider services, string filePath = "", ReminderTypes type = ReminderTypes.Run)
        {
            FilePath = filePath;
            ReminderType = type;

            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();
        }


        internal ICollection<EventTaskAssignment> ReadRunnerReminderCsvForEvent(string filepath, ulong eventId)
        {
            using var reader = new StreamReader(filepath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";

            var assignmentsList = new List<EventTaskAssignment>();
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var userName = csv.GetField<string>(DiscordUserHeader).Split('#');
                var user = UserUtils.GetUserByDiscordName(_client, userName[0],userName[1]);
                var game = csv.GetField<string>(GameHeader);

                var record = new EventTaskAssignment
                {
                    Content = $"@<{user.Id}> your run for {game} is coming up!",
                    Type = ReminderTypes.Run,
                    TaskStartTime = csv.GetField<DateTime>(ScheduleTimeHeader),
                    AssignedUser = user,
                    EventId = eventId
                };

            }

            return assignmentsList;
        }

        internal ICollection<EventTaskAssignment> ReadRunnerReminderCsvForEvent(string filepath, EventEntity evnt)
        {
            return ReadRunnerReminderCsvForEvent(filepath, evnt.Id);
        }
    }
}
