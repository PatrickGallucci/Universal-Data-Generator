# API reference

Universal Data Generator is a set of libraries. This reference documents the public surface project by project.
The dependency direction is one way: hosts depend on the libraries, the libraries depend on
`UniDataGen.Abstractions`, and nothing depends on a host.

| Project | Reference |
|---------|-----------|
| `UniDataGen.Abstractions` | [abstractions.md](./abstractions.md) |
| `UniDataGen.Core` | [core.md](./core.md) |
| `UniDataGen.Configuration` | [configuration.md](./configuration.md) |
| `UniDataGen.Targets` | [targets.md](./targets.md) |
| `UniDataGen.Generation.Foundry` | [foundry.md](./foundry.md) |
| `UniDataGen.Observability` | [observability.md](./observability.md) |

## Typical wiring

```csharp
ILoggerFactory loggerFactory = AppInsightsLogging.CreateFactory();

Catalog catalog = CatalogStore.Load();
RunConfiguration config = new RunConfigurationLoader().LoadFromFile("run-config.json");

IValueProvider provider = new FoundryValueProvider(
    config.Run.Foundry,
    logger: loggerFactory.CreateLogger<FoundryValueProvider>());

var orchestrator = new GenerationOrchestrator(catalog, provider, new SinkFactory(loggerFactory), loggerFactory);
await orchestrator.RunAsync(config, TimeSpan.FromSeconds(2), cancellationToken);
```
