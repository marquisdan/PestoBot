using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PestoBot.Services
{
    class LogService
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        public LogService(IServiceProvider services)
        {
            // get the services we need via DI, and assign the fields declared above to them
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();
            _logger = services.GetRequiredService<ILogger<LogService>>();

            //Hook up events
            _client.Ready += OnReadyAsync;
            _client.Log += OnLogAsync;
            _commandService.Log += OnLogAsync;
        }

        public Task OnReadyAsync()
        {
            //Log connection info
            _logger.LogInformation($"Connected as [{_client.CurrentUser}] :D");
            _logger.LogInformation($"We are on [{_client.Guilds.Count}] servers");
            return Task.CompletedTask;
        }

        public Task OnLogAsync(LogMessage msg)
        {
            //Generate log string
            string logText = $": {msg.Exception?.ToString() ?? msg.Message}";

            //Log to file with correct severity level
            switch (msg.Severity.ToString())
            {
                case "Critical":
                {
                    _logger.LogCritical(logText);
                    break;
                }
                case "Warning":
                {
                    _logger.LogWarning(logText);
                    break;
                }
                case "Info":
                {
                    _logger.LogInformation(logText);
                    break;
                }
                case "Verbose":
                {
                    _logger.LogInformation(logText);
                    break;
                }
                case "Debug":
                {
                    _logger.LogDebug(logText);
                    break;
                }
                case "Error":
                {
                    _logger.LogError(logText);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }


}
