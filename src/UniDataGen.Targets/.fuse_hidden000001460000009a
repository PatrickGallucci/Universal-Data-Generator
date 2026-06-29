using UniDataGen.Abstractions;

namespace UniDataGen.Targets;

/// <summary>Builds a flat document from a record for row-, document-, and search-oriented targets.</summary>
internal static class RecordDocument
{
    /// <summary>
    /// Returns the record fields plus an id and change metadata. The id field name defaults to "id"; the change
    /// metadata uses underscore-prefixed keys so they do not collide with entity attributes.
    /// </summary>
    public static Dictionary<string, object?> ToDocument(GeneratedRecord record, DateTimeOffset at, string idField = "id")
    {
        var doc = new Dictionary<string, object?>(record.Fields, StringComparer.Ordinal)
        {
            [idField] = record.Key.ToString(),
            ["_action"] = record.Action.ToString(),
            ["_generatedAt"] = at
        };

        return doc;
    }
}
