using System;
using System.Text.RegularExpressions;

namespace Zapadi.Vies
{
    public static class Extensions
    {
        private const string SPACE_STRING = " ";
        private const string DASH_STRING = "-";

        public static string SanitizeVatNumber(this string vatNumber)
        {
            return vatNumber.Replace(SPACE_STRING, string.Empty).Replace(DASH_STRING, string.Empty).Trim().ToUpperInvariant();
        }

		public static  string GetValue(this string content, string pattern, Func<string, string> func = null)
		{
			var match = Regex.Match(content, pattern);
			if (match.Success)
			{
				var result = match.Groups[1].Value;

				if (func != null)
				{
					return func.Invoke(result);
				}

				return result;
			}
			return default(string);
		}
    }
}