using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PestoBot.Common
{
    public static class TextUtils
    {
        public static string EmbedDateFormat { get; } = "MMMM dd";

        public static string GetSuccessText(string msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```diff");
            sb.AppendLine($"+ {msg}");
            sb.AppendLine("```");
            return sb.ToString();
        }

        public static string GetWarnText(string msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```fix");
            sb.AppendLine($"{msg}");
            sb.AppendLine("```");
            return sb.ToString();
        }

        public static string GetErrorText(string msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```diff");
            sb.AppendLine($"- {msg}");
            sb.AppendLine("```");
            return sb.ToString();
        }

        public static string GetInfoText(string msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine("```md");
            sb.AppendLine($"# {msg}");
            sb.AppendLine("```");
            return sb.ToString();
        }

        public static string GetHighlightedFields(IEnumerable<string> fields)
        {
            return $"`{fields.Aggregate((a, x) => a + "`  `" + x)}`";
        }

    }
}