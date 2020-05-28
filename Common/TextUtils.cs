using System.Runtime.CompilerServices;
using System.Text;

namespace PestoBot.Common
{
    public static class TextUtils
    {
        public static string EmbedDateFormat { get; } = "MMMM dd, yyyy";

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

    }
}