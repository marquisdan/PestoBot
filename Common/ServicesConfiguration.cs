using System;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PestoBot.Services;
using Serilog;

namespace PestoBot.Common
{
    internal static class ServicesConfiguration
    {
        private static readonly IServiceProvider ServiceProvider;
        private static DiscordSocketClient _client;
        private static CommandService _commandService;
        private static IConfiguration _config;
        private static Microsoft.Extensions.Logging.ILogger _logger;

        static ServicesConfiguration()
        {
            _client = InitClient();
            _commandService = InitCommandService();
            _config = ConfigService.BuildConfig();
            ServiceProvider = BuildServiceProvider();
            _logger = ServiceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();
        }

        /// <summary>
        /// Initialize Commandservice with desired config (Call from constructor)
        /// </summary>
        /// <returns></returns>
        private static CommandService InitCommandService() =>
            new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            });

        /// <summary>
        /// Initialize Discord Client
        /// </summary>
        /// <returns></returns>
        private static DiscordSocketClient InitClient() =>
            new DiscordSocketClient(new DiscordSocketConfig
            {
                HandlerTimeout = 5000
            });

        /// <summary>
        /// Adds all services
        /// </summary>
        /// <returns></returns>
        internal static IServiceProvider GetServiceProvider()
        {
            return ServiceProvider;
        }

        private static IServiceProvider BuildServiceProvider()
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
    }
}