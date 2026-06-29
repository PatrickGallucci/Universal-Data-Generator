using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Shared Azure Blob Storage plumbing: builds the authenticated service client, ensures the container exists,
/// and uploads bytes to a blob. The blob and data-share sinks use this.
/// </summary>
internal sealed class BlobUploader
{
    private readonly string _accountName;
    private readonly string _container;
    private readonly TargetAuth _auth;
    private readonly string? _accountKey;
    private readonly ILogger _logger;
    private BlobContainerClient? _client;

    public BlobUploader(string accountName, string container, TargetAuth auth, string? accountKey, ILogger? logger = null)
    {
        _accountName = accountName;
        _container = container;
        _auth = auth;
        _accountKey = accountKey;
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        BlobServiceClient service = CreateServiceClient();
        _client = service.GetBlobContainerClient(_container);
        await _client.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Blob container ready: account {Account}, container {Container}.", _accountName, _container);
    }

    public async Task UploadAsync(string blobName, byte[] content, CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            throw new InvalidOperationException($"{nameof(OpenAsync)} must be called before uploading.");
        }

        BlobClient blob = _client.GetBlobClient(blobName);
        using var stream = new MemoryStream(content, writable: false);
        await blob.UploadAsync(stream, overwrite: true, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Uploaded {Bytes} bytes to {Blob}.", content.Length, blobName);
    }

    private BlobServiceClient CreateServiceClient()
    {
        var uri = new Uri($"https://{_accountName}.blob.core.windows.net");
        if (_auth.Method.Equals("StorageKey", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_accountKey))
        {
            return new BlobServiceClient(uri, new StorageSharedKeyCredential(_accountName, _accountKey));
        }

        return new BlobServiceClient(uri, new DefaultAzureCredential());
    }
}
