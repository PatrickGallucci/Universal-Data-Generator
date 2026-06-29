using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;

namespace UniDataGen.Targets;

/// <summary>Writes generated batches to Azure Blob Storage as date-partitioned newline-delimited JSON.</summary>
public sealed class AzureBlobSink : ISink
{
    private readonly BlobUploader _uploader;
    private readonly string? _baseDirectory;

    public AzureBlobSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> p = config.Properties;
        _uploader = new BlobUploader(
            AdlsUploader.ResolveAccountName(p),
            PropertyBag.GetString(p, "container") ?? throw new InvalidOperationException("AzureBlobStorage target requires a 'container' property."),
            config.Authentication,
            PropertyBag.GetString(p, "accountKey"),
            logger);
        _baseDirectory = PropertyBag.GetString(p, "path") ?? PropertyBag.GetString(p, "directory");
        TargetType = config.TargetType;
    }

    public SinkKind Kind => SinkKind.Batch;

    public string TargetType { get; }

    public Task OpenAsync(CancellationToken cancellationToken) => _uploader.OpenAsync(cancellationToken);

    public async Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (batch.Records.Count == 0)
        {
            return;
        }

        string directory = AdlsPath.BuildDirectory(_baseDirectory, batch.SchemaArea, batch.EntityName, batch.GeneratedAt);
        string name = $"{directory}/{AdlsPath.BuildFileName(batch.EntityName, batch.GeneratedAt)}";
        await _uploader.UploadAsync(name, NdjsonFormatter.SerializeToUtf8(batch), cancellationToken).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
