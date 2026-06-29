using UniDataGen.Abstractions;
using UniDataGen.Targets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace UniDataGen.Targets.Tests;

public class DiSinkFactoryTests
{
    private sealed class FakeSink : ISink
    {
        public SinkKind Kind => SinkKind.Batch;
        public string TargetType => "JsonFile";
        public Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private static TargetConfig Config(string type) => new()
    {
        TargetType = type,
        Name = type.ToLowerInvariant(),
        Properties = new Dictionary<string, object?> { ["directory"] = "./out" }
    };

    [Fact]
    public void Default_registrations_cover_every_storage_target()
    {
        var types = DefaultSinks.All().Select(r => r.TargetType).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (string expected in new[]
        {
            "ADLSGen2", "ADLSGen2Parquet", "AzureBlobStorage", "AzureFiles", "AzureDataShare", "OneLakeLakehouse",
            "AzureSqlDB", "OneLakeWarehouse", "PostgreSQL", "AzureCosmosDB", "AzureAISearchIndex",
            "EventHubs", "AzureServiceBus", "AzureEventGrid", "FabricEventhouse", "Dataverse", "DatabricksUC"
        })
        {
            Assert.Contains(expected, types);
        }
    }

    [Fact]
    public void Factory_builds_a_concrete_sink()
    {
        var factory = new DiSinkFactory(DefaultSinks.All());
        Assert.True(factory.CanCreate("JsonFile"));
        Assert.IsType<JsonFileSink>(factory.Create(Config("JsonFile")));
    }

    [Fact]
    public void Unknown_target_type_throws()
    {
        var factory = new DiSinkFactory(DefaultSinks.All());
        Assert.False(factory.CanCreate("Nope"));
        Assert.Throws<NotSupportedException>(() => factory.Create(Config("Nope")));
    }

    [Fact]
    public void Container_resolves_factory_and_custom_sink_overrides_builtin()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUniDataGenTargets();
        // Override the built-in JsonFile with a custom sink; last registration wins.
        services.AddSink("JsonFile", SinkKind.Batch, (_, _) => new FakeSink());

        using ServiceProvider provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ISinkFactory>();

        Assert.True(factory.CanCreate("AzureBlobStorage"));
        Assert.IsType<FakeSink>(factory.Create(Config("JsonFile")));
    }
}
