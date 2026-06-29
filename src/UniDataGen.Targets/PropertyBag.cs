using System.Text.Json;

namespace UniDataGen.Targets;

/// <summary>Reads target properties whose values may be plain CLR objects or JsonElement from a parsed config.</summary>
internal static class PropertyBag
{
    public static string? GetString(IReadOnlyDictionary<string, object?> properties, string key)
    {
        if (!properties.TryGetValue(key, out object? value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement { ValueKind: JsonValueKind.String } e => e.GetString(),
            JsonElement e => e.ToString(),
            _ => value.ToString()
        };
    }

    public static string GetRequiredString(IReadOnlyDictionary<string, object?> properties, string key)
        => GetString(properties, key) ?? throw new InvalidOperationException($"Target property '{key}' is required.");
}
