using System.Text.RegularExpressions;

namespace SpiceWeaver;

internal static class StringExtensions
{
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) { return input; }

        // Remove non-alphanumeric characters and split the string into words
        string[] words = AlphaNumeric.Split(input);

        // Capitalize the first letter of each word and concatenate them
        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                words[i] = words[i][0].ToString().ToUpper() + words[i].Substring(1).ToLower();
            }
        }

        // Join the words to form the PascalCase string
        string pascalCaseString = string.Join("", words);

        return pascalCaseString;
    }

    private static readonly Regex AlphaNumeric = new("[^a-zA-Z0-9]+", RegexOptions.Compiled);
}