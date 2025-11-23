using FluentAssertions;
using LongRunningJobApp.Application.Services;

namespace LongRunningJobApp.Application.Tests.Services;

public class StringProcessorServiceTests
{
    private readonly StringProcessorService _service;

    public StringProcessorServiceTests()
    {
        _service = new StringProcessorService();
    }

    [Theory]
    [InlineData("Hello, World!", " 1!1,1H1W1d1e1l3o2r1/SGVsbG8sIFdvcmxkIQ==")]
    [InlineData("aabbcc", "a2b2c2/YWFiYmNj")]
    [InlineData("test", "e1s1t2/dGVzdA==")]
    [InlineData("abc", "a1b1c1/YWJj")]
    public void Process_WithVariousInputs_ShouldReturnExpectedResult(string input, string expected)
    {
        // Act
        var result = _service.Process(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("aabbcc", "a2b2c2")]
    [InlineData("hello", "e1h1l2o1")]
    [InlineData("mississippi", "i4m1p2s4")]
    public void Process_ShouldGenerateCorrectCharacterFrequency(string input, string expectedFrequency)
    {
        // Act
        var result = _service.Process(input);

        // Assert
        var parts = result.Split('/');
        parts[0].Should().Be(expectedFrequency);
    }

    [Theory]
    [InlineData("dcba", "a1b1c1d1")]
    [InlineData("zyxabc", "a1b1c1x1y1z1")]
    [InlineData("321!abc", "!1112131a1b1c1")]
    public void Process_ShouldSortCharactersAlphabetically(string input, string expectedFrequency)
    {
        // Act
        var result = _service.Process(input);

        // Assert
        var parts = result.Split('/');
        parts[0].Should().Be(expectedFrequency);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Process_WithInvalidInput_ShouldThrowArgumentException(string? invalidInput)
    {
        // Act
        var act = () => _service.Process(invalidInput!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Input cannot be null or empty*");
    }

    [Theory]
    [InlineData("test")]
    [InlineData("Hello, World!")]
    [InlineData("Special chars: !@#$%")]
    [InlineData("Unicode: café")]
    public void Process_ShouldGenerateValidBase64ThatDecodesBackToOriginalInput(string input)
    {
        // Act
        var result = _service.Process(input);

        // Assert
        var parts = result.Split('/');
        var base64 = parts[1];
    
        var decodedBytes = Convert.FromBase64String(base64);
        var decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);
        decodedString.Should().Be(input);
    }
}
