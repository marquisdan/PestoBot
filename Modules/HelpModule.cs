using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PestoBot.Modules.EmbedBuilders;

namespace PestoBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private ModuleInfoUtils moduleInfoUtils;

        public HelpModule(CommandService service)
        {
            _service = service;
            moduleInfoUtils = new ModuleInfoUtils();
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
        [Summary("Lists all available commands")]
        public async Task ListCommands()
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

        [Command("Help")]
        [Summary("Top Level Help")]
        public async Task Help(string topic = "")
        {
            if (topic == "")
                await ShowTopLevelHelp();
            else
                await ReplyAsync("Specific command info coming soon");
        }

        private async Task ShowTopLevelHelp()
        {
            var modules = moduleInfoUtils.GetAllModules();
            var eb = new EmbedBuilder()
            {
                Title = "A brief summary of commands for PestoBot",
                Description = "To find commands for a specific module use **Help \"ModuleName\"**"
            };

            //Add a list of modules & descriptions as inline fields to the embed
            foreach (var module in modules)
            {
                //Ignore all owner & debug modules
                if (!module.FullName.Contains("OwnerModule") && !module.FullName.Contains("Debug"))
                {
                    eb.AddField(module.Name.Replace("Module", ""), GetMethodsStringForEmbed(module), true);
                }
            }

            await ReplyAsync("", false, eb.Build());
        }

        /// <summary>
        /// Get the first 3 public methods for a given module
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        private string GetMethodsStringForEmbed(TypeInfo typeInfo)
        {
            var methods = moduleInfoUtils.GetFirstPublicMethods(typeInfo, 3);
            var result = methods.Select(x => moduleInfoUtils.GetCommands(x));
            return $"`{result.Aggregate((a, x) => a + "`  `" + x)}`";
        }

        private List<MethodInfo> GetFirstPublicMethodsForModule(TypeInfo typeInfo, int numMethods)
        {
             return moduleInfoUtils.GetPublicMethods(typeInfo).Take(numMethods).ToList();
        } 

        private static bool IsOwnerCommand(CommandInfo command)
        {
            //We never want to display owner level commands
            return command.Preconditions.Any(x => x.GetType() == typeof(RequireOwnerAttribute));
        }
    }
}
