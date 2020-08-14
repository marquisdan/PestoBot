using PestoBot.Common;
using PestoBot.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Discord;

namespace PestoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ServicesConfiguration.GetServiceProvider();
            SetUpLogger();
            new PestoBot(serviceProvider).MainAsync().GetAwaiter().GetResult();
        }

        private static void SetUpLogger()
        {
            //Get webhooks set for Discord logging
            var keys = ConfigService.BuildKeysConfig();
            ulong fullLogId = ulong.Parse(keys.GetSection("Webhooks").GetSection("FullLog").GetSection("Id").Value);
            var fullLogToken = keys.GetSection("Webhooks").GetSection("FullLog").GetSection("Token").Value;
            ulong warnId = ulong.Parse(keys.GetSection("Webhooks").GetSection("Warn").GetSection("Id").Value);
            var warnToken = keys.GetSection("Webhooks").GetSection("Warn").GetSection("Token").Value;
            ulong errorId = ulong.Parse(keys.GetSection("Webhooks").GetSection("Error").GetSection("Id").Value);
            var errorToken = keys.GetSection("Webhooks").GetSection("Error").GetSection("Token").Value;
            ulong debugId = ulong.Parse(keys.GetSection("Webhooks").GetSection("Debug").GetSection("Id").Value);
            var debugToken = keys.GetSection("Webhooks").GetSection("Debug").GetSection("Token").Value;

            //Instantiate and configure static logger 
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("PestoLogs/Pesto_All.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .WriteTo.Discord(fullLogId, fullLogToken)
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File("PestoLogs/Pesto_Info.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File("PestoLogs/Pesto_Debug.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.Discord(debugId, debugToken))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File("PestoLogs/Pesto_Warn.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.Discord(warnId, warnToken))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File("PestoLogs/Pesto_Error.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.Discord(errorId, errorToken))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal).WriteTo.File("PestoLogs/Pesto_Error.log", rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }
    }
}
