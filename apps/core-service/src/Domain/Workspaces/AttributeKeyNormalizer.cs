using System.Text;
using System.Text.RegularExpressions;

namespace DeskMatch.CoreService.Domain.Workspaces;

public static class AttributeKeyNormalizer
{
    private static readonly Dictionary<char, char> AccentMap = new()
    {
        ['á'] = 'a', ['é'] = 'e', ['í'] = 'i', ['ó'] = 'o', ['ú'] = 'u',
        ['à'] = 'a', ['è'] = 'e', ['ì'] = 'i', ['ò'] = 'o', ['ù'] = 'u',
        ['ä'] = 'a', ['ë'] = 'e', ['ï'] = 'i', ['ö'] = 'o', ['ü'] = 'u',
        ['â'] = 'a', ['ê'] = 'e', ['î'] = 'i', ['ô'] = 'o', ['û'] = 'u',
        ['ñ'] = 'n', ['ç'] = 'c',
        ['Á'] = 'a', ['É'] = 'e', ['Í'] = 'i', ['Ó'] = 'o', ['Ú'] = 'u',
        ['À'] = 'a', ['È'] = 'e', ['Ì'] = 'i', ['Ò'] = 'o', ['Ù'] = 'u',
        ['Ä'] = 'a', ['Ë'] = 'e', ['Ï'] = 'i', ['Ö'] = 'o', ['Ü'] = 'u',
        ['Â'] = 'a', ['Ê'] = 'e', ['Î'] = 'i', ['Ô'] = 'o', ['Û'] = 'u',
        ['Ñ'] = 'n', ['Ç'] = 'c'
    };

    private static readonly Regex NonAlphaRegex = new(@"[^a-z0-9-]", RegexOptions.Compiled);

    public static string Normalize(string raw)
    {
        var sb = new StringBuilder(raw.Length);

        foreach (var c in raw)
        {
            sb.Append(AccentMap.TryGetValue(c, out var mapped) ? mapped : c);
        }

        var result = sb.ToString().ToLowerInvariant().Trim();
        result = NonAlphaRegex.Replace(result, "-");
        result = Regex.Replace(result, @"-{2,}", "-");
        result = result.Trim('-');

        return result;
    }
}
