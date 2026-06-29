using UniDataGen.Abstractions;
using UniDataGen.Targets;
using Xunit;

namespace UniDataGen.Targets.Tests;

public class EventHubsSinkTests
{
    private static TargetConfig Config(params (string, object?)[] props) => new()
    {
        TargetType = "EventHubs",
        Name = "eh",
        Properties = props.ToDictionary(p => p.Item1, p => p.Item2)
    };

    [Fact]
    public void Factory_creates_event_hubs_sink()
    {
        var factory = new SinkFactory();
        Assert.True(factory.CanCreate("EventHubs"));
        ISink sink = factory.Create(Config(("namespace", "myns"), ("eventHub", "events")));
        Assert.IsType<EventHubsSink>(sink);
        Assert.Equal(SinkKind.Streaming, sink.Kind);
    }

    [Fact]
    public void Requires_event_hub_name()
        => Assert.Throws<InvalidOperationException>(() => new EventHubsSink(Config(("namespace", "myns"))));

    [Fact]
    public void Requires_namespace_or_connection_string()
        => Assert.Throws<InvalidOperationException>(() => new EventHubsSink(Config(("eventHub", "events"))));

    [Fact]
    public async Task Write_before_open_throws()
    {
        var sink = new EventHubsSink(Config(("namespace", "myns"), ("eventHub", "events")));
        var batch = new EntityBatch
        {
            SchemaArea = "core",
            EntityName = "Account",
            GeneratedAt = DateTimeOffset.UtcNow,
            Records = [new GeneratedRecord { Action = RecordAction.New, Key = 1, Fields = new Dictionary<string, object?>() }]
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.WriteAsync(batch, CancellationToken.None));
    }
}
