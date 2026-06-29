using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Writes generated batches to an Azure Files share as date-partitioned newline-delimited JSON. The Files SDK
/// requires each directory segment to exist and a file to be created before content is uploaded, which this sink
/// handles. Entra authentication uses the backup token intent the Files data plane requires.
/// </summary>
public sealed class AzureFilesSink : ISink
{
    private readonly string _accountName;
    private readonly string _shareName;
    private readonly string? _baseDirectory;
    private readonly string? _connectionString;
    private readonly string? _accountKey;
    private readonly TargetAuth _auth;
    private readonly ILogger _logger;
    private ShareClient? _share;

    public AzureFilesSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> p = config.Properties;
        _accountName = AdlsUploader.ResolveAccountName(p);
        _shareName = PropertyBag.GetString(p, "shareName")
            ?? throw new InvalidOperationException("AzureFiles target requires a 'shareName' property.");
        _baseDirectory = PropertyBag.GetString(p, "directory");
        _connectionString = PropertyBag.GetString(p, "connectionString");
        _accountKey = PropertyBag.GetString(p, "accountKey");
        _auth = config.Authentication;
        _logger = logger ?? NullLogger.Instance;
        TargetType = config.TargetType;
    }

    public SinkKind Kind => SinkKind.Batch;

    public string TargetType { get; }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        _share = CreateShareClient();
        await _share.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Azure Files share ready: account {Account}, share {Share}.", _accountName, _shareName);
    }

    public async Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (_share is null)
        {
            throw new InvalidOperationException($"{nameof(OpenAsync)} must be called before {nameof(WriteAsync)}.");
        }

        if (batch.Records.Count == 0)
        {
            return;
        }

        string directory = AdlsPath.BuildDirectory(_baseDirectory, batch.SchemaArea, batch.EntityName, batch.GeneratedAt);
        ShareDirectoryClient dir = await EnsureDirectoryAsync(directory, cancellationToken).ConfigureAwait(false);

        string fileName = AdlsPath.BuildFileName(batch.EntityName, batch.GeneratedAt);
        byte[] content = NdjsonFormatter.SerializeToUtf8(batch);
        ShareFileClient file = dir.GetFileClient(fileName);
        await file.CreateAsync(content.Length, cancellationToken: cancellationToken).ConfigureAwait(false);
        using var stream = new MemoryStream(content, writable: false);
        await file.UploadRangeAsync(new HttpRange(0, content.Length), stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Wrote {Bytes} bytes to {Path}.", content.Length, $"{directory}/{fileName}");
    }

    private async Task<ShareDirectoryClient> EnsureDirectoryAsync(string directory, CancellationToken cancellationToken)
    {
        ShareDirectoryClient current = _share!.GetRootDirectoryClient();
        foreach (string segment in directory.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            current = current.GetSubdirectoryClient(segment);
            await current.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return current;
    }

    private ShareClient CreateShareClient()
    {
        if (_connectionString is { } cs)
        {
            return new ShareClient(cs, _shareName);
        }

        var uri = new Uri($"https://{_accountName}.file.core.windows.net/{_shareName}");
        if (_auth.Method.Equals("StorageKey", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_accountKey))
        {
            return new ShareClient(uri, new StorageSharedKeyCredential(_accountName, _accountKey));
        }

        // The Files data plane requires the backup token intent for Entra authentication.
        var options = new ShareClientOptions { ShareTokenIntent = ShareTokenIntent.Backup };
        return new ShareClient(uri, new DefaultAzureCredential(), options);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
