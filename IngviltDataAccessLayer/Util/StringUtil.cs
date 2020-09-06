using System.Text.RegularExpressions;

namespace Ingvilt.Util {
    public class StringUtil {
		private static bool IsLowerCase(string str) {
			return str.ToLower() == str;
		}

		private static string CaseMatchedReplacement(string textToReplaceWith, string textBeingReplaced) {
			if (IsLowerCase(textBeingReplaced)) {
				return textToReplaceWith.ToLower();
			}

			return textToReplaceWith;
		}

		public static string GetReplacedText(string text, string searchValue, string textToReplaceWith) {
			var searchRegex = new Regex(searchValue, RegexOptions.IgnoreCase);
			var replacedText = searchRegex.Replace(text, (match) => {
				return CaseMatchedReplacement(textToReplaceWith, match.Value);
			});

			if (textToReplaceWith.EndsWith("s")) {
				return replacedText.Replace(textToReplaceWith + "s", textToReplaceWith).Replace(textToReplaceWith.ToLower() + "s", textToReplaceWith.ToLower());
			}

			return replacedText;
		}

		public static string LimitStringLength(string name, int maxLength) {
			if (name.Length > maxLength) {
				return name.Substring(0, maxLength) + "...";
			}

			return name;
		}
	}
}
