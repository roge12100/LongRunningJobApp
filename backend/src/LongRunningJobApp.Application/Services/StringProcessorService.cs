using System.Text;
using LongRunningJobApp.Application.Interfaces;

namespace LongRunningJobApp.Application.Services;

/// <summary>
/// Service implementation for processing input strings according to business rules
/// Generates: {char}{count}.../{base64}
/// Example: "Hello, World!" -> " 1!1,1H1W1d1e1l3o2r1/SGVsbG8sIFdvcmxkIQ=="
/// </summary>
public sealed class StringProcessorService : IStringProcessor
{
    /// <summary>
    /// Processes input string to generate result string
    /// </summary>
    /// <param name="input">The input string to process</param>
    /// <returns>Processed result string in format: {char}{count}.../{base64}</returns>
    public string Process(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        var characterFrequencyPart = GenerateCharacterFrequencyString(input);
        var base64Part = GenerateBase64String(input);

        return $"{characterFrequencyPart}/{base64Part}";
    }

    /// <summary>
    /// Generates character frequency string
    /// All unique characters sorted with their occurrence count
    /// Example: "Hello" -> "H1e1l2o1"
    /// </summary>
    private static string GenerateCharacterFrequencyString(string input)
    {
        var charCounts = new Dictionary<char, int>();
        foreach (var c in input)
        {
            if (charCounts.ContainsKey(c))
                charCounts[c]++;
            else
                charCounts[c] = 1;
        }

        var sortedChars = charCounts.OrderBy(kvp => kvp.Key);
        var result = new StringBuilder();

        foreach (var (character, count) in sortedChars)
        {
            result.Append(character);
            result.Append(count);
        }

        return result.ToString();
    }

    /// <summary>
    /// Generates Base64 encoded string
    /// </summary>
    private static string GenerateBase64String(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }
}
