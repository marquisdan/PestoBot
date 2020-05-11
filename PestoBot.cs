using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PestoBot.Services;
using Serilog;

namespace PestoBot
{
    class PestoBot
    {
        #region startup and config
        private static DiscordSocketClient _client;
        private static CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public PestoBot()
        {
            _client = InitClient();
            _commandService = InitCommandService();
            _serviceProvider = ConfigureServices(_client);
          //  _logger = _serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();
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
                CaseSensitiveCommands = false
            });

        /// <summary>
        /// Adds all services
        /// </summary>
        /// <returns></returns>
        private static IServiceProvider ConfigureServices(DiscordSocketClient client)
        {
            var map = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commandService)
                .AddSingleton<CommandHandler>()
                .AddSingleton<LogService>()
                .AddLogging(configure => configure.AddSerilog());

            //map.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            return map.BuildServiceProvider();
        }

        /// <summary>
        /// Connects bot to server, runs services. Keeps bot connected
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
          //  Log.Information("Starting Up!");
          Console.WriteLine("Starting up!");

            //Connect to Discord with key stored in Windows config
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("KEY_PESTOBOT"));
            await _client.StartAsync();

            //Setup connection/disconnection logging events
     //       _client.GuildAvailable += AnnounceConnectionAsync;
            _client.GuildUnavailable += AnnounceDisconnectAsync;

            //Start up / init our various services
            _serviceProvider.GetRequiredService<LogService>();
            await _serviceProvider.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            //Keep bot connected
            await Task.Delay(Timeout.Infinite);
        }
        #endregion

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
    }
}
