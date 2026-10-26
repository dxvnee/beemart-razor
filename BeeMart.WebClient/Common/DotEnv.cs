namespace BeeMart.WebClient.Common;

public static class DotEnv
{
    public static void Load(string? filePath = null)
    {
        filePath ??= FindEnvFile();
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 && ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            Environment.SetEnvironmentVariable(key, value);

            if (key.Equals("API_BASE_URL", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("ApiBaseUrl", value);
            }
        }
    }

    private static string? FindEnvFile()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            var candidate = Path.Combine(dir, ".env");
            if (File.Exists(candidate))
                return candidate;

            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }
        return null;
    }
}
