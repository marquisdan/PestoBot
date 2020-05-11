using System;
using Serilog;

namespace PestoBot
{
    class Program
    {
        static void Main(string[] args)
        {

            //Get webhook set for Discord logging
            ulong.TryParse(Environment.GetEnvironmentVariable("KEY_SPEEDATHON_LOG_WEBHOOK_ID"), out ulong logWebhookId);
            var logWebhookToken = Environment.GetEnvironmentVariable("KEY_SPEEDATHON_LOG_WEBHOOK_TOKEN");

            //Instantiate and configure logger 
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("SpeedathonBotLogs/SpeedathonBot.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                //.WriteTo.Discord(logWebhookId, logWebhookToken)
                .CreateLogger();

            Console.WriteLine("Hello World!");
            new PestoBot().MainAsync().GetAwaiter().GetResult();
        }
    }
}
