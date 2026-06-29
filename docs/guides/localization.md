# Localization

The generator can produce values in a specific locale. The locale applies to the generated content only,
the text values Azure AI Foundry produces such as names, addresses, and descriptions. Identity, audit, and
numeric system fields are unaffected.

## Global default and per-entity override

Set a global locale in the run header. It defaults to `en-US` when absent.

```json
{ "run": { "industry": "Apparel Retail", "sourceType": "ECOM", "locale": "en-US" } }
```

Override the locale on any entity. A null or absent entity locale inherits the run locale.

```json
{ "schemaArea": "core", "entityName": "RetailTransactionTable", "mode": "Batch", "locale": "es-MX" }
```

The engine resolves the effective locale as the entity locale when set, otherwise the run locale, otherwise
`en-US`, and passes it to value generation. The Foundry prompt asks the model to produce text in that locale's
language and regional conventions.

## Supported locales

The catalog of supported locales ships with the configuration as an embedded resource and is validated at load:
a run or entity locale outside the catalog fails validation. The catalog pairs a BCP-47 locale with its country,
for example `es-MX` (Mexico) and `fr-CA` (Canada). The available languages are stored alongside the locales.

Query the catalog in code through `LocaleCatalog`:

```csharp
bool ok = LocaleCatalog.IsSupported("es-MX");          // true
LocaleInfo? info = LocaleCatalog.Find("fr-CA");        // Canada
IReadOnlyList<LocaleInfo> all = LocaleCatalog.Locales; // every supported locale
```

The WinForms client exposes the global locale on the toolbar and a per-entity locale in the profile editor,
both populated from the catalog. See [Run configuration](./run-configuration.md) for the field reference.
