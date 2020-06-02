using Discord;
using Discord.Commands;
using PestoBot.Common;

namespace PestoBot.Modules.EmbedBuilders
{
    public class HelpEmbedBuilder
    {
        private readonly EmbedBuilder _embedBuilder;
        private readonly CommandInfo _command;

        public HelpEmbedBuilder(CommandInfo command)
        {
            _embedBuilder = new EmbedBuilder();
            _command = command;
        }

        public Embed Build()
        {
            PopulateEmbedBuilder();
            return _embedBuilder.Build();
        }

        private void PopulateEmbedBuilder()
        {
            _embedBuilder.Color = GetCommandEmbedColor();
            _embedBuilder.Title = $"Help for {_command.Name}";
            _embedBuilder.Description = _command.Summary;
            _embedBuilder.AddField(GetAliasesFieldBuilder());
        }

        private EmbedFieldBuilder GetAliasesFieldBuilder()
        {
            return  new EmbedFieldBuilder
            {
                Name = "Available Aliases", 
                Value = TextUtils.GetHighlightedFields(_command.Aliases), 
                IsInline = true
            };
        }

        private Color GetCommandEmbedColor()
        {
            var name = _command.Module.Name;
            if (name.Contains("Admin"))
            {
                return Color.Orange;
            }

            if (name.Contains("Owner") || name.Contains("Debug"))
            {
                return Color.Red;
            }

            return Color.Blue;
        }
    }
}
