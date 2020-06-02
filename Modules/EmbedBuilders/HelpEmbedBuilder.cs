using System.Collections.Generic;
using System.Reflection;
using Discord.Commands;

namespace PestoBot.Modules.EmbedBuilders
{
    public class HelpEmbedBuilder<T> where T : ModuleBase
    {
        private ModuleInfoUtils moduleInfoUtils;

        public HelpEmbedBuilder()
        {
            moduleInfoUtils = new ModuleInfoUtils();
        }

    }
}
