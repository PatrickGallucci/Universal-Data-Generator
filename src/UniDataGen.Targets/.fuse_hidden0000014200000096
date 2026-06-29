using Azure.Identity;
using Azure.Storage.Files.DataLake;
using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Writes generated batches as Apache Parquet to a Microsoft Fabric OneLake lakehouse, under the lakehouse Files
/// area. OneLake exposes an ADLS Gen2 compatible DFS endpoint, so the Data Lake client targets
/// onelake.dfs.fabric.microsoft.com with the workspace as the filesystem. Authentication is Microsoft Entra.
/// </summary>
public sealed class OneLakeLakehouseSink : ISink
{
    private const string OneLakeEndpoint = "https://onelake.dfs.fabric.microsoft.com";
    private readonly string _workspace;
    private readonly string _baseDirectory;
    private readonly ILogger _logger;
    private DataLakeFileSystemClient? _fileSystem;

    public OneLakeLakehouseSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> p = config.Properties;
        _workspace = PropertyBag.GetString(p, "workspace") ?? PropertyBag.GetString(p, "workspaceName")
            ?? throw new InvalidOperationException("OneLakeLakehouse target requires a 'workspace' property.");
        string lakehouse = PropertyBag.GetString(p, "lakehouse") ?? PropertyBag.GetString(p, "lakehouseName")
            ?? throw new InvalidOperationException("OneLakeLakehouse target requires a 'lakehouse' property.");
        string subPath = PropertyBag.GetString(p, "path") ?? "datagen";
        _baseDirectory = $"{lakehouse}.Lakehouse/Files/{subPath.Trim('/')}";
        _logger = logger ?? NullLogger.Instance;
        TargetType = config.TargetType;
    }

    public SinkKind Kind => SinkKind.Batch;

    public string TargetType { get; }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        var service = new DataLakeServiceClient(new Uri(OneLakeEndpoint), new DefaultAzureCredential());
        _fileSystem = service.GetFileSystemClient(_workspace);
        _logger.LogInformation("OneLake lakehouse ready: workspace {Workspace}, base {Base}.", _workspace, _baseDirectory);
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (_fileSystem is null)
        {
            throw new InvalidOperationException($"{nameof(OpenAsync)} must be called before {nameof(WriteAsync)}.");
        }

        if (batch.Records.Count == 0)
        {
            return;
        }

        string directory = AdlsPath.BuildDirectory(_baseDirectory, batch.SchemaArea, batch.EntityName, batch.GeneratedAt);
        string fileName = AdlsPath.BuildFileName(batch.EntityName, batch.GeneratedAt, "parquet");
        DataLakeDirectoryClient dir = _fileSystem.GetDirectoryClient(directory);
        await dir.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        DataLakeFileClient file = dir.GetFileClient(fileName);
        byte[] content = await ParquetBatchWriter.WriteAsync(batch, cancellationToken).ConfigureAwait(false);
        using var stream = new MemoryStream(content, writable: false);
        await file.UploadAsync(stream, overwrite: true, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Wrote {Bytes} Parquet bytes to {Path}.", content.Length, $"{directory}/{fileName}");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
