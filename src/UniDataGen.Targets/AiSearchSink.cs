using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Writes generated batches to an Azure AI Search index. New and Update merge or upload; Delete removes by key.
/// Each record becomes a search document keyed by the configured key field. Authentication is an admin API key or
/// Microsoft Entra. The index must already exist with a matching schema.
/// </summary>
public sealed class AiSearchSink : ISink
{
    private readonly Uri _endpoint;
    private readonly string _indexName;
    private readonly string _keyField;
    private readonly string? _apiKey;
    private readonly ILogger _logger;
    private SearchClient? _client;

    public AiSearchSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> p = config.Properties;
        _endpoint = new Uri(PropertyBag.GetString(p, "endpoint") ?? throw new InvalidOperationException("AzureAISearchIndex target requires an 'endpoint' property."));
        _indexName = PropertyBag.GetString(p, "indexName") ?? throw new InvalidOperationException("AzureAISearchIndex target requires an 'indexName' property.");
        _keyField = PropertyBag.GetString(p, "keyField") ?? "id";
        _apiKey = PropertyBag.GetString(p, "apiKey");
        _logger = logger ?? NullLogger.Instance;
        TargetType = config.TargetType;
    }

    public SinkKind Kind => SinkKind.Batch;

    public string TargetType { get; }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        _client = _apiKey is { } key
            ? new SearchClient(_endpoint, _indexName, new AzureKeyCredential(key))
            : new SearchClient(_endpoint, _indexName, new DefaultAzureCredential());
        _logger.LogInformation("AI Search ready: index {Index}.", _indexName);
        return Task.CompletedTask;
    }

    public async Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (_client is null)
        {
            throw new InvalidOperationException($"{nameof(OpenAsync)} must be called before {nameof(WriteAsync)}.");
        }

        if (batch.Records.Count == 0)
        {
            return;
        }

        var actions = new List<IndexDocumentsAction<SearchDocument>>(batch.Records.Count);
        foreach (GeneratedRecord record in batch.Records)
        {
            Dictionary<string, object?> document = RecordDocument.ToDocument(record, batch.GeneratedAt, _keyField);
            var searchDocument = new SearchDocument(document!);
            actions.Add(record.Action == RecordAction.Delete
                ? IndexDocumentsAction.Delete(_keyField, document[_keyField]!.ToString())
                : IndexDocumentsAction.MergeOrUpload(searchDocument));
        }

        IndexDocumentsBatch<SearchDocument> indexBatch = IndexDocumentsBatch.Create(actions.ToArray());
        await _client.IndexDocumentsAsync(indexBatch, cancellationToken: cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Indexed {Count} documents to {Index}.", actions.Count, _indexName);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
