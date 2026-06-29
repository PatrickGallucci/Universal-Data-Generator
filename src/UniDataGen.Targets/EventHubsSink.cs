using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Streams generated records to Azure Event Hubs, one event per record. Because a stream cannot mutate a past
/// event, Update and Delete are emitted as new events carrying the action in the change envelope, which is the
/// correct CDC behavior for downstream consumers. Authenticates with Microsoft Entra by default, or a connection
/// string when configured. An optional partition key field routes related records to the same partition.
/// </summary>
public sealed class EventHubsSink : ISink
{
    private readonly string _eventHubName;
    private readonly string? _fullyQualifiedNamespace;
    private readonly string? _connectionString;
    private readonly string? _partitionKeyField;
    private readonly ILogger _logger;
    private EventHubProducerClient? _producer;

    public EventHubsSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> properties = config.Properties;

        _eventHubName = PropertyBag.GetString(properties, "eventHub")
            ?? throw new InvalidOperationException("EventHubs target requires an 'eventHub' property.");
        _partitionKeyField = PropertyBag.GetString(properties, "partitionKeyField");
        _connectionString = PropertyBag.GetString(properties, "connectionString");
        _fullyQualifiedNamespace = ResolveNamespace(properties);
        _logger = logger ?? NullLogger.Instance;
        TargetType = config.TargetType;

        if (_connectionString is null && _fullyQualifiedNamespace is null)
        {
            throw new InvalidOperationException("EventHubs target requires a 'namespace' or a 'connectionString' property.");
        }
    }

    public SinkKind Kind => SinkKind.Streaming;

    public string TargetType { get; }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        _producer = _connectionString is { } cs
            ? new EventHubProducerClient(cs, _eventHubName)
            : new EventHubProducerClient(_fullyQualifiedNamespace, _eventHubName, new DefaultAzureCredential());

        _logger.LogInformation("Event Hubs producer opened for {EventHub}.", _eventHubName);
        return Task.CompletedTask;
    }

    public async Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (_producer is null)
        {
            throw new InvalidOperationException($"{nameof(OpenAsync)} must be called before {nameof(WriteAsync)}.");
        }

        if (batch.Records.Count == 0)
        {
            return;
        }

        int sent = 0;
        foreach (IGrouping<string?, GeneratedRecord> group in batch.Records.GroupBy(PartitionKeyOf))
        {
            sent += await SendGroupAsync(group.Key, group, batch.GeneratedAt, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Streamed {Count} events for {Entity} to {EventHub}.", sent, batch.Key, _eventHubName);
    }

    private async Task<int> SendGroupAsync(string? partitionKey, IEnumerable<GeneratedRecord> records, DateTimeOffset at, CancellationToken cancellationToken)
    {
        var options = partitionKey is null ? new CreateBatchOptions() : new CreateBatchOptions { PartitionKey = partitionKey };
        EventDataBatch eventBatch = await _producer!.CreateBatchAsync(options, cancellationToken).ConfigureAwait(false);
        int sent = 0;

        foreach (GeneratedRecord record in records)
        {
            var data = new EventData(BinaryData.FromString(NdjsonFormatter.SerializeRecord(record, at)))
            {
                Properties = { ["action"] = record.Action.ToString(), ["entity"] = record.Key.ToString() }
            };

            if (!eventBatch.TryAdd(data))
            {
                if (eventBatch.Count == 0)
                {
                    eventBatch.Dispose();
                    throw new InvalidOperationException("A single record exceeds the Event Hubs batch size limit.");
                }

                await _producer.SendAsync(eventBatch, cancellationToken).ConfigureAwait(false);
                sent += eventBatch.Count;
                eventBatch.Dispose();

                eventBatch = await _producer.CreateBatchAsync(options, cancellationToken).ConfigureAwait(false);
                eventBatch.TryAdd(data);
            }
        }

        if (eventBatch.Count > 0)
        {
            await _producer.SendAsync(eventBatch, cancellationToken).ConfigureAwait(false);
            sent += eventBatch.Count;
        }

        eventBatch.Dispose();
        return sent;
    }

    private string? PartitionKeyOf(GeneratedRecord record)
    {
        if (_partitionKeyField is null)
        {
            return null;
        }

        return record.Fields.TryGetValue(_partitionKeyField, out object? value) ? value?.ToString() : null;
    }

    private static string? ResolveNamespace(IReadOnlyDictionary<string, object?> properties)
    {
        string? ns = PropertyBag.GetString(properties, "namespace");
        if (string.IsNullOrWhiteSpace(ns))
        {
            return null;
        }

        return ns.Contains('.', StringComparison.Ordinal) ? ns : $"{ns}.servicebus.windows.net";
    }

    public async ValueTask DisposeAsync()
    {
        if (_producer is not null)
        {
            await _producer.DisposeAsync().ConfigureAwait(false);
        }
    }
}
