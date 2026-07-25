using System.Text.RegularExpressions;
using System.Web;

namespace SafeVault.Services
{
    // basic sanitizing/validation helpers
    // the real SQL injection protection comes from using EF Core + parameterized
    // queries everywhere (see VaultItemsController), this is just an extra layer
    // for catching obviously bad input early
    public static class InputValidator
    {
        // pretty loose but blocks the classic sqli patterns like ' OR 1=1 --
        private static readonly Regex SqlInjectionPattern = new Regex(
            @"(--|;|'|""|\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|EXEC)\b)",
            RegexOptions.IgnoreCase);

        public static bool ContainsSqlInjectionAttempt(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return SqlInjectionPattern.IsMatch(input);
        }

        // strip/encode anything that looks like it's trying to inject html/script
        public static string SanitizeForOutput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return HttpUtility.HtmlEncode(input);
        }

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]{3,50}$");
        }
    }
}
