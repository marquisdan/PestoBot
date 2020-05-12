using System;
using Microsoft.Extensions.Configuration;

namespace PestoBot
{
    class ConfigService
    {
        internal static IConfiguration BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json");
            return builder.Build();
        }
    }
}