using UniDataGen.Abstractions;
using UniDataGen.Core;
using Xunit;

namespace UniDataGen.Core.Tests;

public class ValueCoordinatorTests
{
    // A deterministic provider that echoes the requested semantic attributes with placeholder values.
    private sealed class FakeProvider : IValueProvider
    {
        public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GenerateAsync(ValueRequest request, CancellationToken cancellationToken)
        {
            var rows = new List<IReadOnlyDictionary<string, object?>>(request.Count);
            for (int i = 0; i < request.Count; i++)
            {
                var row = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (AttributeInfo a in request.SemanticAttributes)
                {
                    row[a.Name] = $"{a.Name}-{i}";
                }

                rows.Add(row);
            }

            return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(rows);
        }
    }

    private static EntityInfo SampleEntity()
    {
        var attributes = new List<AttributeInfo>
        {
            new("accountId", "Account", "id", "entityId", "identifiedBy", false, "accountid", 1),
            new("name", "Name", "name", "string", "hasA", true, "name", 2),
            new("createdOn", "Created", "audit", "dateTime", "createdOn", false, "createdon", 3),
            new("stateCode", "State", "state", "listLookup", "representsStateWith", false, "statecode", 4)
        };

        return new EntityInfo("core", "Account", "Account", "A customer account.", "CdsStandard", attributes);
    }

    [Fact]
    public async Task New_records_mint_keys_stamp_system_fields_and_fill_pool()
    {
        var coordinator = new ValueCoordinator(new FakeProvider());
        var pool = new IdentityPool();
        var at = new DateTimeOffset(2026, 8, 15, 10, 0, 0, TimeSpan.Zero);

        EntityBatch batch = await coordinator.BuildAsync(SampleEntity(), "Specialty Retail", "ECOM", "en-US", 3, 0, 0, pool, at, CancellationToken.None);

        Assert.Equal(3, batch.Records.Count);
        Assert.Equal(3, pool.Count);
        foreach (GeneratedRecord r in batch.Records)
        {
            Assert.Equal(RecordAction.New, r.Action);
            Assert.Equal(r.Key, r.Fields["accountId"]);       // identifiedBy stamped with the key
            Assert.Equal(at, r.Fields["createdOn"]);           // audit stamp
            Assert.Equal(0, r.Fields["stateCode"]);            // active
            Assert.StartsWith("name-", (string)r.Fields["name"]!); // semantic value from provider
        }
    }

    [Fact]
    public async Task Update_and_delete_warm_up_when_pool_is_empty()
    {
        var coordinator = new ValueCoordinator(new FakeProvider());
        var pool = new IdentityPool();
        var at = new DateTimeOffset(2026, 8, 15, 10, 0, 0, TimeSpan.Zero);

        EntityBatch batch = await coordinator.BuildAsync(SampleEntity(), "Specialty Retail", "ECOM", "en-US", 0, 5, 5, pool, at, CancellationToken.None);

        Assert.Empty(batch.Records); // nothing to update or delete yet
    }

    [Fact]
    public async Task Delete_retires_keys_and_update_keeps_them()
    {
        var coordinator = new ValueCoordinator(new FakeProvider());
        var pool = new IdentityPool();
        var at = new DateTimeOffset(2026, 8, 15, 10, 0, 0, TimeSpan.Zero);

        await coordinator.BuildAsync(SampleEntity(), "Specialty Retail", "ECOM", "en-US", 4, 0, 0, pool, at, CancellationToken.None);
        Assert.Equal(4, pool.Count);

        EntityBatch mutate = await coordinator.BuildAsync(SampleEntity(), "Specialty Retail", "ECOM", "en-US", 0, 2, 1, pool, at, CancellationToken.None);

        Assert.Equal(2, mutate.Records.Count(r => r.Action == RecordAction.Update));
        Assert.Equal(1, mutate.Records.Count(r => r.Action == RecordAction.Delete));
        Assert.Equal(3, pool.Count); // one retired by the delete
    }
}
