using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PestoBot.Database.Models;
using PestoBot.Database.Repositories.Guild;
using PestoBot.Services;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


namespace PestoBot
{
    class PestoBot
    {
        private static DiscordSocketClient _client;
        private static CommandService _commandService;
        private static ReminderService _reminderService;
        private static IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;


        #region Constructor & Initialize
        public PestoBot()
        {
            _client = InitClient();
            _commandService = InitCommandService();
            _config = ConfigService.BuildConfig();
            _serviceProvider = ConfigureServices(_client);
            _logger = _serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();
        }

        /// <summary>
        /// Initialize Discord Client
        /// </summary>
        /// <returns></returns>
        private DiscordSocketClient InitClient() =>
            new DiscordSocketClient(new DiscordSocketConfig
            {
                HandlerTimeout = 5000
            });

        /// <summary>
        /// Initialize Commandservice with desired config (Call from constructor)
        /// </summary>
        /// <returns></returns>
        private CommandService InitCommandService() =>
            new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });

        /// <summary>
        /// Adds all services
        /// </summary>
        /// <returns></returns>
        private static IServiceProvider ConfigureServices(DiscordSocketClient client)
        {
            var map = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_commandService)
                .AddSingleton<CommandHandler>()
                .AddSingleton<LogService>()
                .AddSingleton<ReminderService>()
                .AddLogging(configure => configure.AddSerilog());

            map.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            return map.BuildServiceProvider();
        }

        #endregion

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
            _serviceProvider.GetRequiredService<LogService>();
            await _serviceProvider.GetRequiredService<CommandHandler>().InstallCommandsAsync();

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
