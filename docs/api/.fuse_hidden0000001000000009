# API: UniDataGen.Targets

The sinks and the dependency-injection registration model.

## Registration

```csharp
public interface ISinkRegistration
{
    string TargetType { get; }
    SinkKind Kind { get; }
    ISink Create(TargetConfig config, ILogger logger);
}

public sealed class SinkRegistration : ISinkRegistration
{
    public SinkRegistration(string targetType, SinkKind kind, Func<TargetConfig, ILogger, ISink> factory);
}

public sealed class DiSinkFactory : ISinkFactory
{
    public DiSinkFactory(IEnumerable<ISinkRegistration> registrations, ILoggerFactory? loggerFactory = null);
    public IReadOnlyCollection<string> RegisteredTargetTypes { get; }
}

public static class DefaultSinks
{
    public static IReadOnlyList<ISinkRegistration> All();
}

public static class TargetServiceCollectionExtensions
{
    public static IServiceCollection AddUniDataGenTargets(this IServiceCollection services);
    public static IServiceCollection AddSink(this IServiceCollection services, string targetType, SinkKind kind, Func<TargetConfig, ILogger, ISink> factory);
}
```

`AddUniDataGenTargets` registers every built-in sink and a `DiSinkFactory` as `ISinkFactory`. `AddSink` adds or
overrides one target type; the last registration for a type wins. `SinkFactory` is a convenience that wraps
`DiSinkFactory` with `DefaultSinks.All()` for hosts that do not use a container.

## Sinks

| Class | Target type | Kind |
|-------|-------------|------|
| `JsonFileSink` | JsonFile | Batch |
| `ConsoleSink` | Console | Streaming |
| `NullSink` | Null | Batch |
| `AdlsGen2Sink` | ADLSGen2 | Batch |
| `AdlsGen2ParquetSink` | ADLSGen2Parquet | Batch |
| `AzureBlobSink` | AzureBlobStorage | Batch |
| `AzureFilesSink` | AzureFiles | Batch |
| `AzureDataShareSink` | AzureDataShare | Batch |
| `OneLakeLakehouseSink` | OneLakeLakehouse | Batch |
| `SqlServerSink` | AzureSqlDB, OneLakeWarehouse | Batch |
| `PostgreSqlSink` | PostgreSQL | Batch |
| `CosmosDbSink` | AzureCosmosDB | Batch |
| `AiSearchSink` | AzureAISearchIndex | Batch |
| `EventHubsSink` | EventHubs | Streaming |
| `ServiceBusSink` | AzureServiceBus | Streaming |
| `EventGridSink` | AzureEventGrid | Streaming |
| `EventhouseSink` | FabricEventhouse | Streaming |
| `DataverseSink` | Dataverse | Batch |
| `DatabricksUcSink` | DatabricksUC | Batch |

Each sink constructor takes a `TargetConfig` and an optional `ILogger`.

## Helpers

| Type | Purpose |
|------|---------|
| `NdjsonFormatter` | `Serialize`, `SerializeRecord`, `SerializeToUtf8`. |
| `ParquetBatchWriter` | `WriteAsync(EntityBatch)` returns Parquet bytes with an inferred schema. |
| `AdlsPath` | `BuildDirectory`, `BuildFileName` for partition paths. |
| `AdlsUploader`, `BlobUploader` | Shared Data Lake and Blob plumbing. |
| `RecordDocument` | Flattens a record to a document for row, document, and search targets. |

To add a target, implement `ISink` and register it with `AddSink`. No edit to the factory is required.
