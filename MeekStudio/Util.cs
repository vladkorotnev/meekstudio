using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeekStudio
{
    static class Util
    {
		private static char[] _invalidCharacters;
		/// <summary>
		/// List of invalid characters
		/// </summary>
		public static char[] InvalidCharacters
		{
			get
			{
				if (_invalidCharacters == null)
					_invalidCharacters = getInvalidCharacters();
				return _invalidCharacters;
			}
		}
		private static char[] getInvalidCharacters()
		{
			var invalidChars = new List<char>();
			invalidChars.AddRange(Path.GetInvalidFileNameChars());
			invalidChars.AddRange(Path.GetInvalidPathChars());
			return invalidChars.ToArray();
		}

		public static string RemoveInvalidCharacters(string content, char replace = '_', bool doNotReplaceBackslashes = false)
		{
			if (string.IsNullOrEmpty(content))
				return content;

			var idx = content.IndexOfAny(InvalidCharacters);
			if (idx >= 0)
			{
				var sb = new StringBuilder(content);
				while (idx >= 0)
				{
					if (sb[idx] != '\\' || !doNotReplaceBackslashes)
						sb[idx] = replace;
					idx = content.IndexOfAny(InvalidCharacters, idx + 1);
				}
				return sb.ToString();
			}
			return content;
		}
	}
}
