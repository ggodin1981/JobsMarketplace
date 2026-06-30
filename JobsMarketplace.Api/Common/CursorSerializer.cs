using System.Text;
using System.Text.Json;

namespace JobsMarketplace.Api.Common;

public static class CursorSerializer
{
    public static string Serialize<T>(T value)
    {
        var json = JsonSerializer.Serialize(value);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static T? Deserialize<T>(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (FormatException)
        {
            return default;
        }
        catch (JsonException)
        {
            return default;
        }
    }
}

