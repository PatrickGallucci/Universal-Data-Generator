using System.Text;
using System.Text.Json;
using UniDataGen.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Serializes a batch as newline-delimited JSON. Each line is one record with a change envelope
/// (_action, _key, _generatedAt) followed by the record fields, so streaming and file targets agree on shape.
/// </summary>
public static class NdjsonFormatter
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    /// <summary>Serializes every record in the batch, one JSON object per line.</summary>
    public static string Serialize(EntityBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        var sb = new StringBuilder();
        foreach (GeneratedRecord record in batch.Records)
        {
            sb.AppendLine(SerializeRecord(record, batch.GeneratedAt));
        }

        return sb.ToString();
    }

    /// <summary>Serializes a single record with its change envelope.</summary>
    public static string SerializeRecord(GeneratedRecord record, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(record);
        var envelope = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["_action"] = record.Action.ToString(),
            ["_key"] = record.Key.ToString(),
            ["_generatedAt"] = at
        };

        foreach (KeyValuePair<string, object?> field in record.Fields)
        {
            envelope[field.Key] = field.Value;
        }

        return JsonSerializer.Serialize(envelope, Json);
    }

    /// <summary>The UTF-8 bytes of the serialized batch, ready to upload.</summary>
    public static byte[] SerializeToUtf8(EntityBatch batch) => Encoding.UTF8.GetBytes(Serialize(batch));
}
