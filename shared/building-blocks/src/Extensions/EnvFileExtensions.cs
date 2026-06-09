using Microsoft.Extensions.Configuration;

namespace DeskMatch.BuildingBlocks.Extensions;

public static class EnvFileExtensions
{
    public static IConfigurationBuilder AddEnvFile(this IConfigurationBuilder builder, string? basePath = null)
    {
        var envFile = FindEnvFile(basePath ?? Directory.GetCurrentDirectory());
        if (envFile == null) return builder;

        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadAllLines(envFile))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;

            var eq = trimmed.IndexOf('=');
            if (eq < 0) continue;

            var key = trimmed[..eq].Trim();
            var value = trimmed[(eq + 1)..].Trim();

            if (value.StartsWith('"') && value.EndsWith('"')) value = value[1..^1];
            else if (value.StartsWith('\'') && value.EndsWith('\'')) value = value[1..^1];

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                data[key] = value;
            }
        }

        return builder.AddInMemoryCollection(data!);
    }

    private static string? FindEnvFile(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        for (var i = 0; i < 6 && dir != null; i++)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(dir.FullName, "infrastructure", "docker", ".env");
            if (File.Exists(candidate)) return candidate;

            dir = dir.Parent;
        }

        return null;
    }
}
