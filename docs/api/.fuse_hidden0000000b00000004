# API: UniDataGen.Configuration

Builds the catalog from embedded JSON and reads the run configuration.

```csharp
public static class CatalogStore
{
    public static Catalog Load();
}

public sealed class RunConfigurationLoader
{
    public RunConfiguration LoadFromJson(string json);
    public RunConfiguration LoadFromFile(string path);
    public string ToJson(RunConfiguration configuration);
}

public sealed class RunConfigurationValidator
{
    public RunConfigurationValidator(ILogger? logger = null);
    public ValidationResult Validate(RunConfiguration config, Catalog catalog);
}

public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);
```

`CatalogStore` builds the catalog from the embedded JSON resources, decompressing the gzip attribute schema on
first use. The loader uses
camelCase JSON with case-insensitive matching and string enums. The validator checks the industry,
source type, entities, and targets against the catalog and returns every problem at once.

## Locale catalog

```csharp
public sealed record LocaleInfo(string CountryCode, string Country, string Locale);

public static class LocaleCatalog
{
    public const string DefaultLocale = "en-US";
    public static IReadOnlyList<LocaleInfo> Locales { get; }
    public static IReadOnlyList<string> Languages { get; }
    public static bool IsSupported(string locale);
    public static LocaleInfo? Find(string locale);
}
```

`LocaleCatalog` loads the supported locales and languages from embedded resources. The validator uses
`IsSupported` to reject a run or entity locale outside the catalog. See [Localization](../guides/localization.md).

## Reference catalog

```csharp
public sealed record IndustryEntry(string Name, string? Description);
public sealed record EntityNameEntry(string SchemaArea, string EntityName, string? DisplayName);

public static class ReferenceCatalog
{
    public static IReadOnlyList<IndustryEntry> Industries { get; }
    public static IReadOnlyList<EntityNameEntry> Entities { get; }
    public static IReadOnlyList<string> SchemaAreas { get; }
    public static bool HasIndustry(string name);
    public static bool HasEntity(string schemaArea, string entityName);
}
```

`ReferenceCatalog` loads the industries and entity names from the embedded `industries.json` and `entities.json`.
Edit those files under `src/UniDataGen.Configuration/Resources` to change the catalog.
