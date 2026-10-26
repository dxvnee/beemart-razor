namespace BeeMart.Api.Common;

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

            switch (key.ToUpperInvariant())
            {
                case "DB_CONNECTION":
                case "DEFAULT_CONNECTION":
                    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", value);
                    break;
                case "JWT_KEY":
                    Environment.SetEnvironmentVariable("Jwt__Key", value);
                    break;
                case "JWT_ISSUER":
                    Environment.SetEnvironmentVariable("Jwt__Issuer", value);
                    break;
                case "JWT_AUDIENCE":
                    Environment.SetEnvironmentVariable("Jwt__Audience", value);
                    break;
                case "JWT_EXPIRE_MINUTES":
                    Environment.SetEnvironmentVariable("Jwt__ExpireMinutes", value);
                    break;
                case "TIMEZONE_ID":
                    Environment.SetEnvironmentVariable("TimeZoneId", value);
                    break;
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
