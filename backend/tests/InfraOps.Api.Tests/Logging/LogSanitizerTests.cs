using InfraOps.Api.Logging;

namespace InfraOps.Api.Tests.Logging;

public sealed class LogSanitizerTests
{
    [Fact]
    public void Should_RemoveControlCharacters_When_ValueContainsLogForgingCharacters()
    {
        var result = LogSanitizer.Sanitize("admin@example.com\r\nrole=admin\t");

        Assert.Equal("admin@example.com__role=admin_", result);
    }

    [Fact]
    public void Should_TruncateValue_When_ValueExceedsMaximumLength()
    {
        var result = LogSanitizer.Sanitize("abcdef", maxLength: 3);

        Assert.Equal("abc", result);
    }
}
