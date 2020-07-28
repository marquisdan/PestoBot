using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PestoBot.Database.Models;
using PestoBot.Database.Repositories.Guild;
using PestoBot.Services;
using Serilog;


namespace PestoBot
{
    class PestoBot
    {
        private static DiscordSocketClient _client;
        private static CommandHandler _commandHandler;
        private readonly ReminderService _reminderService;

        public PestoBot(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = services.GetRequiredService<CommandHandler>();
            _reminderService = services.GetService<ReminderService>();
        }

        /// <summary>
        /// Connects bot to server, runs services. Keeps bot connected
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            Log.Information("Starting Up!");

            //Connect to Discord with key stored in Windows config
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("KEY_PESTOBOT"));
            await _client.StartAsync();

            //Setup connection/disconnection logging events
            _client.GuildAvailable += AnnounceConnectionAsync;
            _client.GuildUnavailable += AnnounceDisconnectAsync;

            //Start up / init our various services
            _reminderService.Start();
           // _serviceProvider.GetRequiredService<LogService>();
            await _commandHandler.InstallCommandsAsync();

            //Keep bot connected
            await Task.Delay(Timeout.Infinite);
        }

        #region Guild Connection Events
        /// <summary>
        /// Log when connecting to servers
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public async Task AnnounceConnectionAsync(SocketGuild g)
        {
            await UpdateGuildConnectionInfo(g);
            ulong.TryParse(Environment.GetEnvironmentVariable("KEY_SPEEDATHON_TEST_SERVER"), out var testServerId);
            var logMsg = $"Successfully connected to {g.Name} : {g.Id}";

            Log.Information(logMsg);

            if (g.Id == testServerId)
            {
                //Post in special channel when connecting to bot sandbox
                ulong.TryParse(Environment.GetEnvironmentVariable("KEY_SPEEDATHON_LOG_CHAN_ID"), out var logChannelId);
                var connectionLogChannel = (IMessageChannel)_client.GetChannel(logChannelId);
                await connectionLogChannel.SendMessageAsync($"{DateTime.Now}: I am online!");
            }
        }

        public async Task UpdateGuildConnectionInfo(SocketGuild guild)
        {
            var server = DiscordNetToModelConverter.SocketGuildToModel(guild);
            var repo = new GuildRepository();
            if (repo.ServerExists(server))
            {
                //Update server connection time
                await repo.UpdateLastConnectionTime(server, DateTime.Now);
                return;
            }

            //If no server/guild exists, make a new database entry for it and its table.
            Log.Information($"Creating new Guild Entry: {server.Name}, {server.Id} | Owner: {server.OwnerUsername}, {server.OwnerId}");
            await repo.SaveNewServer(server);
        }

        /// <summary>
        /// Log when disconnecting from a server
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public Task AnnounceDisconnectAsync(SocketGuild g)
        {
            var logMsg = $"Disconnected from {g.Name} : {g.Id}";
            Log.Warning(logMsg);
            return Task.CompletedTask;
        }
        #endregion
    }
}
