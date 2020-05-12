using System.Threading.Tasks;
using Discord.Commands;

namespace PestoBot.Modules
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
