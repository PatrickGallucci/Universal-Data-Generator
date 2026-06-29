using UniDataGen.Abstractions;
using UniDataGen.Targets;
using Xunit;

namespace UniDataGen.Targets.Tests;

public class AllSinksConstructTests
{
    // A superset property bag so every sink finds the keys it requires at construction.
    private static readonly Dictionary<string, object?> Superset = new(StringComparer.Ordinal)
    {
        ["accountName"] = "acct",
        ["endpoint"] = "https://acct.dfs.core.windows.net",
        ["container"] = "c",
        ["filesystem"] = "fs",
        ["shareName"] = "share",
        ["directory"] = "d",
        ["path"] = "p",
        ["server"] = "srv.database.windows.net",
        ["database"] = "db",
        ["table"] = "t",
        ["schema"] = "dbo",
        ["keyColumn"] = "id",
        ["namespace"] = "ns",
        ["eventHub"] = "eh",
        ["entityName"] = "q",
        ["indexName"] = "idx",
        ["clusterUri"] = "https://cluster.kusto.windows.net",
        ["workspace"] = "ws",
        ["lakehouse"] = "lh",
        ["workspaceUrl"] = "https://adb-1.azuredatabricks.net",
        ["warehouseId"] = "wh",
        ["catalog"] = "cat",
        ["token"] = "tok",
        ["environmentUrl"] = "https://org.crm.dynamics.com",
        ["partitionKey"] = "id",
        ["datasetName"] = "ds"
    };

    public static IEnumerable<object[]> AllTargetTypes()
        => DefaultSinks.All().Select(r => new object[] { r.TargetType });

    [Theory]
    [MemberData(nameof(AllTargetTypes))]
    public void Every_registered_target_constructs(string targetType)
    {
        var factory = new DiSinkFactory(DefaultSinks.All());
        Assert.True(factory.CanCreate(targetType));

        var config = new TargetConfig
        {
            TargetType = targetType,
            Name = targetType.ToLowerInvariant(),
            Properties = Superset
        };

        ISink sink = factory.Create(config);
        Assert.Equal(targetType, sink.TargetType);
    }

    [Fact]
    public void Twenty_target_types_are_registered()
        => Assert.Equal(20, DefaultSinks.All().Count);
}
