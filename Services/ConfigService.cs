using System;
using Microsoft.Extensions.Configuration;

namespace PestoBot.Services
{
    class ConfigService
    {
        private const string configJson = "config.json";
        private const string keysJson = "keys.json";

        internal static IConfiguration BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(configJson);
            return builder.Build();
        }

        internal static IConfiguration BuildKeysConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(keysJson);
            return builder.Build();
        }
    }
}