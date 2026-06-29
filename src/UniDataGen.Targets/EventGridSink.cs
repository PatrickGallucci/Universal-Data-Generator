using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Publishes generated records to an Azure Event Grid topic, one event per record. The subject encodes the entity
/// and action so subscribers can filter. Authentication is an access key or Microsoft Entra.
/// </summary>
public sealed class EventGridSink : ISink
{
    private readonly Uri _endpoint;
    private readonly string? _key;
    private readonly string _subjectPrefix;
    private readonly ILogger _logger;
    private EventGridPublisherClient? _client;

    public EventGridSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> p = config.Properties;
        _endpoint = new Uri(PropertyBag.GetString(p, "endpoint") ?? throw new InvalidOperationException("AzureEventGrid target requires an 'endpoint' property."));
        _key = PropertyBag.GetString(p, "accessKey") ?? PropertyBag.GetString(p, "key");
        _subjectPrefix = PropertyBag.GetString(p, "subjectPrefix") ?? "datagen";
        _logger = logger ?? NullLogger.Instance;
        TargetType = config.TargetType;
    }

    public SinkKind Kind => SinkKind.Streaming;

    public string TargetType { get; }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        _client = _key is { } key
            ? new EventGridPublisherClient(_endpoint, new AzureKeyCredential(key))
            : new EventGridPublisherClient(_endpoint, new DefaultAzureCredential());
        _logger.LogInformation("Event Grid publisher opened for {Endpoint}.", _endpoint);
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

        var events = new List<EventGridEvent>(batch.Records.Count);
        foreach (GeneratedRecord record in batch.Records)
        {
            string json = NdjsonFormatter.SerializeRecord(record, batch.GeneratedAt);
            events.Add(new EventGridEvent(
                subject: $"{_subjectPrefix}/{batch.Key}/{record.Action}",
                eventType: $"UniDataGen.{record.Action}",
                dataVersion: "1.0",
                data: BinaryData.FromString(json)));
        }

        await _client.SendEventsAsync(events, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Published {Count} events for {Entity}.", events.Count, batch.Key);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
