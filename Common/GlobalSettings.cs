using System;
using System.Threading.Tasks;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Common
{
    public static class GlobalSettings
    {
        private static readonly GlobalSettingsRepository Repo;

         static GlobalSettings()
        {
            Repo = new GlobalSettingsRepository();
        }

         public static GlobalSettingsModel GetGlobalSettings()
         {
             return Repo.GetAllGlobalSettings().Result;
         }

         /// <summary>
         /// Initializes global settings with default values if no values are already set
         /// </summary>
         /// <returns>True if initialized success. False if values already exist.</returns>
         public static bool InitGlobalSettings()
         {
             return Repo.InitGlobalSettings().Result;
         }

         public static bool AreDebugRemindersEnabled()
         {
             return Repo.AreDebugRemindersEnabled().Result;
         }

         /// <summary>
         /// Update global settings with any non default values 
         /// </summary>
         /// <param name="model"></param>
         /// <returns></returns>
         public static async Task SetGlobalSettings(GlobalSettingsModel model)
         {
             var updateModel = GetGlobalSettings();
             updateModel.Modified = DateTime.Now;
             updateModel.DebugRemindersEnabled = model.DebugRemindersEnabled;
             updateModel.DebugReminderHour =
                 model.DebugReminderHour != 0 ? model.DebugReminderHour : updateModel.DebugReminderHour;
             updateModel.DebugReminderMinutes =
                 model.DebugReminderMinutes != 0 ? model.DebugReminderMinutes : updateModel.DebugReminderMinutes;
            await Repo.UpdateAsync(updateModel);
        }
    }
}
