using UniDataGen.Abstractions;
using UniDataGen.Configuration;
using Xunit;

namespace UniDataGen.Configuration.Tests;

public class CatalogStoreTests
{
    [Fact]
    public void Load_builds_catalog_from_embedded_json()
    {
        Catalog catalog = CatalogStore.Load();

        Assert.True(catalog.Industries.Count >= 159);
        Assert.True(catalog.SourceTypes.Count >= 39);
        Assert.Equal(16, catalog.Storages.Count);
        Assert.True(catalog.Entities.Count >= 9000);
    }

    [Fact]
    public void Account_entity_has_attributes()
    {
        EntityInfo? account = CatalogStore.Load().FindEntity("core", "Account");
        Assert.NotNull(account);
        Assert.True(account!.Attributes.Count > 0);
        Assert.Contains(account.Attributes, a => !string.IsNullOrEmpty(a.DataType));
    }

    [Fact]
    public void Catalog_lookups_work()
    {
        Catalog catalog = CatalogStore.Load();
        Assert.True(catalog.HasIndustry("Apparel Retail"));
        Assert.True(catalog.HasSourceType("ECOM"));
    }
}
