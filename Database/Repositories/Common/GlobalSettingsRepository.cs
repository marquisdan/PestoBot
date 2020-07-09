using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Repositories.Common
{
    class GlobalSettingsRepository : AbstractPestoRepository<GlobalSettingsModel>
    {
        public GlobalSettingsRepository()
        {
            TableName = "GlobalSettings";
        }


    }
}