namespace UniDataGen.Abstractions;

/// <summary>A request to produce values for a number of records of one entity.</summary>
public sealed class ValueRequest
{
    public required EntityInfo Entity { get; init; }
    public required string Industry { get; init; }
    public required string SourceType { get; init; }
    public required int Count { get; init; }

    /// <summary>The subset of attributes the provider should fill. System fields are excluded by the coordinator.</summary>
    public required IReadOnlyList<AttributeInfo> SemanticAttributes { get; init; }

    /// <summary>BCP-47 locale the produced values should use, for example "es-MX".</summary>
    public string Locale { get; init; } = "en-US";
}

/// <summary>
/// Produces semantic field values for records. The current implementation is Foundry-backed,
/// but the engine depends only on this interface so tests can substitute a deterministic fake.
/// </summary>
public interface IValueProvider
{
    /// <summary>Returns <see cref="ValueRequest.Count"/> dictionaries of semantic field values.</summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GenerateAsync(ValueRequest request, CancellationToken cancellationToken);
}

/// <summary>A write target. One implementation per storage type in the workbook Storage sheet.</summary>
public interface ISink : IAsyncDisposable
{
    /// <summary>Whether this target is naturally streaming or batch oriented.</summary>
    SinkKind Kind { get; }

    /// <summary>The target type name, matching data-targets.schema.json, for example "AzureSqlDB".</summary>
    string TargetType { get; }

    /// <summary>Opens the connection and prepares the target. Called once before writes.</summary>
    Task OpenAsync(CancellationToken cancellationToken);

    /// <summary>Writes a batch. New inserts, Update mutates by key, Delete removes by key (or emits a tombstone event for streaming targets).</summary>
    Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken);
}

/// <summary>Creates an <see cref="ISink"/> from a target configuration.</summary>
public interface ISinkFactory
{
    /// <summary>The target types this factory can build.</summary>
    bool CanCreate(string targetType);

    /// <summary>Builds a sink for the given target configuration.</summary>
    ISink Create(TargetConfig config);
}
