using System.Threading.Tasks;
using Discord.Commands;

namespace SpeedathonBot.Modules
{
    public class FunModule : ModuleBase
    {
        [Command("Pesto")]
        public async Task Pesto()
        {
            await ReplyAsync("Is dabesto!");
        }

    }
}
