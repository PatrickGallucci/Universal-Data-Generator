using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;

namespace UniDataGen.Targets;

/// <summary>
/// Writes generated batches to Azure Data Lake Storage Gen2 as Apache Parquet, partitioned the same way as the
/// NDJSON sink. Parquet lands query-ready for OneLake shortcuts, Fabric, Synapse, and Databricks without a parse
/// step. Shares authentication and container handling with the NDJSON sink through <see cref="AdlsUploader"/>.
/// </summary>
public sealed class AdlsGen2ParquetSink : ISink
{
    private readonly AdlsUploader _uploader;
    private readonly string? _baseDirectory;

    public AdlsGen2ParquetSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> properties = config.Properties;

        _uploader = new AdlsUploader(
            AdlsUploader.ResolveAccountName(properties),
            AdlsUploader.ResolveFileSystem(properties),
            config.Authentication,
            PropertyBag.GetString(properties, "accountKey"),
            logger);
        _baseDirectory = PropertyBag.GetString(properties, "directory");
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
        string fileName = AdlsPath.BuildFileName(batch.EntityName, batch.GeneratedAt, "parquet");
        byte[] content = await ParquetBatchWriter.WriteAsync(batch, cancellationToken).ConfigureAwait(false);
        await _uploader.UploadAsync(directory, fileName, content, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
