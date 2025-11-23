namespace LongRunningJobApp.Application.Interfaces;

/// <summary>
/// Service contract for processing input strings 
/// </summary>
public interface IStringProcessor
{
    /// <summary>
    /// Processes input string to generate result string
    /// Format: {char}{count}.../{base64}
    /// Example: " 1!1,1H1W1d1e1l3o2r1/SGVsbG8sIFdvcmxkIQ=="
    /// </summary>
    /// <param name="input">The input string to process</param>
    /// <returns>Processed result string</returns>
    string Process(string input);
    
}
