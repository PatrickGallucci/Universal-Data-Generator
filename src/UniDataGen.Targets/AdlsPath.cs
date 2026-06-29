namespace UniDataGen.Targets;

/// <summary>
/// Builds Hive-style partition paths for Data Lake writes: {base}/{schemaArea}/{entity}/{yyyy}/{MM}/{dd}.
/// Path building is separated from the sink so it can be unit-tested without a storage account.
/// </summary>
public static class AdlsPath
{
    /// <summary>Builds the directory path (no leading or trailing slash) for an entity batch.</summary>
    public static string BuildDirectory(string? baseDirectory, string schemaArea, string entityName, DateTimeOffset at)
    {
        var parts = new List<string>(6);
        if (!string.IsNullOrWhiteSpace(baseDirectory))
        {
            parts.Add(baseDirectory.Trim('/'));
        }

        parts.Add(schemaArea);
        parts.Add(entityName);
        parts.Add(at.ToString("yyyy"));
        parts.Add(at.ToString("MM"));
        parts.Add(at.ToString("dd"));
        return string.Join('/', parts);
    }

    /// <summary>Builds a collision-resistant file name for a batch with the given extension (default ndjson).</summary>
    public static string BuildFileName(string entityName, DateTimeOffset at, string extension = "ndjson")
        => $"{entityName}-{at:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.{extension}";
}
