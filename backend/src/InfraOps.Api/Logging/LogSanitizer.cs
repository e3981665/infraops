namespace InfraOps.Api.Logging;

public static class LogSanitizer
{
    private const int DefaultMaxLength = 256;

    public static string Sanitize(string? value, int maxLength = DefaultMaxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sanitized = new char[Math.Min(value.Length, maxLength)];
        var writeIndex = 0;

        foreach (var character in value)
        {
            if (writeIndex >= sanitized.Length)
            {
                break;
            }

            sanitized[writeIndex] = char.IsControl(character) ? '_' : character;
            writeIndex++;
        }

        return new string(sanitized, 0, writeIndex);
    }
}
