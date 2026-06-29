using System.Globalization;
using System.Text;
using System.Text.Json;
using UniDataGen.Abstractions;

namespace UniDataGen.Generation.Foundry;

/// <summary>Builds the system and user prompts and the JSON schema for a structured-output generation call.</summary>
public static class FoundryPromptBuilder
{
    /// <summary>The system prompt establishes the role, the output locale, and the schema-conformance rule.</summary>
    public static string SystemPrompt(string industry, string sourceType, string locale) =>
        $"You generate realistic synthetic business records for the {industry} industry, " +
        $"as they would appear in a {sourceType} source system. " +
        $"Produce all text values in {DescribeLocale(locale)} (locale {locale}), using that language and its " +
        "regional conventions for names, addresses, and formatting. " +
        "Values must be plausible for that industry and internally consistent within a record. " +
        "Return only data that conforms to the provided JSON schema. Do not include identifiers, audit " +
        "timestamps, or status fields; those are added by the host.";

    /// <summary>Returns the English name of a locale for the prompt, or the raw code when unknown.</summary>
    private static string DescribeLocale(string locale)
    {
        try
        {
            return CultureInfo.GetCultureInfo(locale).EnglishName;
        }
        catch (CultureNotFoundException)
        {
            return locale;
        }
    }

    /// <summary>The user prompt names the entity and asks for the requested number of records.</summary>
    public static string UserPrompt(EntityInfo entity, int count)
    {
        var sb = new StringBuilder();
        sb.Append("Generate ").Append(count).Append(" records for the entity '")
          .Append(entity.DisplayName ?? entity.EntityName).Append("'.");
        if (!string.IsNullOrWhiteSpace(entity.Description))
        {
            sb.Append(' ').Append(entity.Description);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a JSON schema describing an object with a "records" array of objects, one property per
    /// semantic attribute. Used as the response_format json_schema so the model returns conforming data.
    /// </summary>
    public static JsonElement RecordsSchema(IReadOnlyList<AttributeInfo> semanticAttributes)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "object");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            writer.WritePropertyName("records");
            writer.WriteStartObject();
            writer.WriteString("type", "array");
            writer.WritePropertyName("items");
            WriteItemSchema(writer, semanticAttributes);
            writer.WriteEndObject(); // records
            writer.WriteEndObject(); // properties

            writer.WritePropertyName("required");
            writer.WriteStartArray();
            writer.WriteStringValue("records");
            writer.WriteEndArray();

            writer.WriteBoolean("additionalProperties", false);
            writer.WriteEndObject();
        }

        return JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
    }

    private static void WriteItemSchema(Utf8JsonWriter writer, IReadOnlyList<AttributeInfo> attributes)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "object");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();

        foreach (AttributeInfo attribute in attributes)
        {
            writer.WritePropertyName(attribute.Name);
            writer.WriteStartObject();
            writer.WriteString("type", JsonTypeFor(attribute.DataType));
            if (!string.IsNullOrWhiteSpace(attribute.Description))
            {
                writer.WriteString("description", attribute.Description);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject(); // properties

        writer.WritePropertyName("required");
        writer.WriteStartArray();
        foreach (AttributeInfo attribute in attributes)
        {
            writer.WriteStringValue(attribute.Name);
        }

        writer.WriteEndArray();
        writer.WriteBoolean("additionalProperties", false);
        writer.WriteEndObject();
    }

    private static string JsonTypeFor(string dataType) => dataType.ToLowerInvariant() switch
    {
        "integer" or "biginteger" => "integer",
        "decimal" or "currency" or "amountcur" or "amountmst" or "real" or "double" => "number",
        "boolean" or "noyesid" => "boolean",
        _ => "string"
    };
}
