using UniDataGen.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// A scaffolded adapter for a cloud target whose SDK is not yet wired. It carries the correct kind and type
/// so the factory and config validate, and fails loud on open with the package needed to complete it.
/// Replace per family per the design doc: relational (Microsoft.Data.SqlClient, Npgsql), lakehouse and files
/// (Azure.Storage.Files.DataLake, Azure.Storage.Blobs), document and search (Microsoft.Azure.Cosmos,
/// Azure.Search.Documents), streaming (Azure.Messaging.EventHubs, Azure.Messaging.ServiceBus, Azure.Messaging.EventGrid).
/// </summary>
public sealed class CloudSinkStub : ISink
{
    private readonly string _package;

    public CloudSinkStub(TargetConfig config, SinkKind kind, string package)
    {
        ArgumentNullException.ThrowIfNull(config);
        TargetType = config.TargetType;
        Kind = kind;
        _package = package;
    }

    public SinkKind Kind { get; }

    public string TargetType { get; }

    public Task OpenAsync(CancellationToken cancellationToken)
        => throw new NotImplementedException(
            $"Adapter for '{TargetType}' is scaffolded but not implemented. Add the {_package} package and complete the adapter.");

    public Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
        => throw new NotImplementedException($"Adapter for '{TargetType}' is not implemented.");

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
