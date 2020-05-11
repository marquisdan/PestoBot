using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace SpeedathonBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("About")]
        [Alias("AboutBot")]
        [Summary("Gets general information about the bot")]
        public async Task AboutBotEmbed()
        {
            ShowUnderConstructionWarning();

            var eb = new EmbedBuilder()
            {
                Color = Color.DarkGreen,
                Title = $"About Pestobot",
                Description =$"Hello, I am a speedrun marathon organizing bot written in C# using Discord.net!",
                Footer = new EmbedFooterBuilder().WithText("Profile Icon by Mili Vigerova https://unsplash.com/@mili_vigerova").WithIconUrl("https://images.unsplash.com/profile-1478097671033-99cc86c22f38?dpr=1&auto=format&fit=crop&w=150&h=150&q=60&crop=faces&bg=fff")
            };

            eb.AddField("Author", "@marquisdan#4463", true);
            eb.AddField("Contact", "pestobot@marquisdan.com", true);
            eb.AddField("View Source", "[github](https://www.github.com/marquisdan/pestobot)", true);

            await ReplyAsync("Pestobot is Dabestobot!", false, eb.Build());
        }

        private void ShowUnderConstructionWarning()
        {
            var warnEb = new EmbedBuilder()
            {
                Color = Color.Orange,
                Title = "Heads up",
                Description =
                    $"Pestobot is a little baby bot that doesn't know how to do much. Please send feature suggestions, bug reports etc to marquisdan or pestobot@marquisdan.com"
            };
            ReplyAsync("", false, warnEb.Build());
        }

        [Command("ListCommands")]
        [Alias("Help")]
        [Summary("Lists all available commands")]
        public async Task Help()
        {
            List<CommandInfo> commands = _service.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                var permCheck = await command.CheckPreconditionsAsync(Context);
                //Filter commands based on user's permissions. 
                if (permCheck.IsSuccess && !IsOwnerCommand(command))
                {
                    string embedFieldText = command.Summary ?? "No description available\n";
                    embedBuilder.AddField(command.Name, embedFieldText);
                }
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

        private static bool IsOwnerCommand(CommandInfo command)
        {
            //We never want to display owner level commands
            return command.Preconditions.Any(x => x.GetType() == typeof(RequireOwnerAttribute));
        }
    }
}
