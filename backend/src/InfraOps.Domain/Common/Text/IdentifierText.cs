namespace InfraOps.Domain.Common.Text;

public static class IdentifierText
{
    public static string NormalizeSlugSeparators(string value, bool trimHyphens)
    {
        var normalizedValue = value.Trim().ToLowerInvariant();
        var characters = new List<char>(normalizedValue.Length);
        var previousWasHyphen = false;

        foreach (var character in normalizedValue)
        {
            var normalizedCharacter = char.IsWhiteSpace(character) || character == '_'
                ? '-'
                : character;

            if (normalizedCharacter == '-')
            {
                if (previousWasHyphen)
                {
                    continue;
                }

                previousWasHyphen = true;
            }
            else
            {
                previousWasHyphen = false;
            }

            characters.Add(normalizedCharacter);
        }

        var result = new string(characters.ToArray());

        return trimHyphens ? result.Trim('-') : result;
    }

    public static bool IsLowercaseSlug(string value)
    {
        if (string.IsNullOrEmpty(value) || value[0] == '-' || value[^1] == '-')
        {
            return false;
        }

        var previousWasHyphen = false;

        foreach (var character in value)
        {
            if (character == '-')
            {
                if (previousWasHyphen)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (!IsLowercaseAsciiLetter(character) && !char.IsAsciiDigit(character))
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return true;
    }

    public static bool IsLowerCamelAlphaNumericKey(string value)
    {
        if (string.IsNullOrEmpty(value) || !IsLowercaseAsciiLetter(value[0]))
        {
            return false;
        }

        return value.All(character => char.IsAsciiLetterOrDigit(character));
    }

    private static bool IsLowercaseAsciiLetter(char character)
    {
        return character is >= 'a' and <= 'z';
    }
}
