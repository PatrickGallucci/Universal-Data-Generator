namespace UniDataGen.Abstractions;

/// <summary>An industry vertical from the workbook Industry sheet.</summary>
public sealed record IndustryInfo(string Name, string? Description);

/// <summary>A data source type from the workbook SourceType sheet.</summary>
public sealed record SourceTypeInfo(string Code, string Type, string? Description);

/// <summary>A storage target name from the workbook Storage sheet.</summary>
public sealed record StorageInfo(string Name, string? Description);

/// <summary>
/// A single attribute of an entity, sourced from the workbook Attributes sheet.
/// <see cref="DataType"/> and <see cref="Purpose"/> drive how a value is produced.
/// </summary>
public sealed record AttributeInfo(
    string Name,
    string? DisplayName,
    string? Description,
    string DataType,
    string? Purpose,
    bool IsNullable,
    string? SourceName,
    int? SourceOrdering);

/// <summary>An entity definition with its resolved attribute list.</summary>
public sealed record EntityInfo(
    string SchemaArea,
    string EntityName,
    string? DisplayName,
    string? Description,
    string? ExtendsEntity,
    IReadOnlyList<AttributeInfo> Attributes)
{
    /// <summary>Stable key used to address an entity across catalog and profile, for example "core/Account".</summary>
    public string Key => $"{SchemaArea}/{EntityName}";
}

/// <summary>
/// The read-only catalog parsed from the workbook. This is the menu a run is chosen from.
/// </summary>
public sealed class Catalog
{
    private readonly Dictionary<string, EntityInfo> _entitiesByKey;
    private readonly Dictionary<string, IndustryInfo> _industriesByName;
    private readonly Dictionary<string, SourceTypeInfo> _sourceTypesByCode;

    public Catalog(
        IReadOnlyList<IndustryInfo> industries,
        IReadOnlyList<SourceTypeInfo> sourceTypes,
        IReadOnlyList<StorageInfo> storages,
        IReadOnlyList<EntityInfo> entities)
    {
        Industries = industries;
        SourceTypes = sourceTypes;
        Storages = storages;
        Entities = entities;

        _entitiesByKey = entities.ToDictionary(e => e.Key, StringComparer.OrdinalIgnoreCase);
        _industriesByName = industries
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        _sourceTypesByCode = sourceTypes
            .GroupBy(s => s.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IndustryInfo> Industries { get; }
    public IReadOnlyList<SourceTypeInfo> SourceTypes { get; }
    public IReadOnlyList<StorageInfo> Storages { get; }
    public IReadOnlyList<EntityInfo> Entities { get; }

    /// <summary>Returns the entity for a "schemaArea/EntityName" key, or null when absent.</summary>
    public EntityInfo? FindEntity(string schemaArea, string entityName)
        => _entitiesByKey.TryGetValue($"{schemaArea}/{entityName}", out var e) ? e : null;

    public bool HasIndustry(string name) => _industriesByName.ContainsKey(name);

    public bool HasSourceType(string code) => _sourceTypesByCode.ContainsKey(code);
}
