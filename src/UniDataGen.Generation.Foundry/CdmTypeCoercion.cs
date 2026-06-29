using System.Text.Json;

namespace UniDataGen.Generation.Foundry;

/// <summary>Coerces a model-produced JSON value to the CLR type implied by a CDM datatype.</summary>
public static class CdmTypeCoercion
{
    /// <summary>Returns a CLR value for a JSON element given a CDM datatype string. Falls back to the raw string.</summary>
    public static object? Coerce(JsonElement element, string dataType)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return dataType.ToLowerInvariant() switch
        {
            "integer" or "biginteger" => element.TryGetInt64(out long l) ? l : ToLong(element),
            "decimal" or "currency" or "amountcur" or "amountmst" or "real" or "double"
                => element.TryGetDecimal(out decimal d) ? d : ToDecimal(element),
            "boolean" or "noyesid" => ToBool(element),
            "datetime" or "transdate" or "date"
                => DateTimeOffset.TryParse(element.ToString(), out DateTimeOffset dto) ? dto : null,
            "guid" => Guid.TryParse(element.ToString(), out Guid g) ? g : element.ToString(),
            _ => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString()
        };
    }

    private static long ToLong(JsonElement e) => long.TryParse(e.ToString(), out long l) ? l : 0L;

    private static decimal ToDecimal(JsonElement e) => decimal.TryParse(e.ToString(), out decimal d) ? d : 0m;

    private static bool ToBool(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number => e.GetDouble() != 0,
        _ => bool.TryParse(e.ToString(), out bool b) && b
    };
}
