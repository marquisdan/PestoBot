using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace PestoBot.Modules
{
    public class OwnerModule : ModuleBase
    {

        [RequireOwner]
        [Command("SetGame")]
        [Summary("Sets current game name for PestoBot")]
        public async Task SetGame(string gameName)
        {
            if (Context.Client is DiscordSocketClient client)
            {
                await client.SetGameAsync(gameName);
            }
        }

        [RequireOwner]
        [Command("userinfo")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
            SocketUser user = null)
        {
            var userInfo = user ?? (SocketUser)Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }

        [RequireOwner]
        [Command("ListGuilds")]
        [Alias("ListServers")]
        [Summary("Returns a list of connected Discord Servers/Guilds")]
        public async Task ListGuilds()
        {
            var guilds = Context.Client.GetGuildsAsync().Result;
            var sb = new StringBuilder();
            sb.AppendLine($"Total connected guilds: {guilds.Count}\r\n");
            foreach (var guild in guilds)
            {
                sb.Append($"{guild.Name} | ");
            }

            await ReplyAsync(sb.ToString());
        }

    }
}