using UniDataGen.Abstractions;
using UniDataGen.Configuration;
using UniDataGen.Core;
using UniDataGen.Generation.Foundry;
using UniDataGen.Targets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Foundry.Tool;

/// <summary>
/// Wraps the Core engine as a single callable operation so an Azure AI Foundry agent can start a generation
/// run through function calling. The same Core is reused; this is a thin dispatcher, not a reimplementation.
/// All tiers log through the supplied logger factory.
/// </summary>
public sealed class GeneratorTool
{
    private readonly ILoggerFactory _loggerFactory;

    public GeneratorTool(ILoggerFactory? loggerFactory = null)
        => _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

    /// <summary>
    /// Starts a run from a run-config file and returns a short status. The catalog is embedded. Honors cancellation so an
    /// agent can bound the run. When <paramref name="offline"/> is true no Foundry call is made.
    /// </summary>
    public async Task<string> StartRunAsync(string configPath, bool offline, TimeSpan duration, CancellationToken cancellationToken)
    {
        Catalog catalog = CatalogStore.Load();
        RunConfiguration config = new RunConfigurationLoader().LoadFromFile(configPath);

        ValidationResult validation = new RunConfigurationValidator(_loggerFactory.CreateLogger<RunConfigurationValidator>()).Validate(config, catalog);
        if (!validation.IsValid)
        {
            return "Invalid run configuration: " + string.Join("; ", validation.Errors);
        }

        IValueProvider provider = offline
            ? new SimpleOfflineProvider(config.Run.Industry)
            : new FoundryValueProvider(config.Run.Foundry, logger: _loggerFactory.CreateLogger<FoundryValueProvider>());

        var orchestrator = new GenerationOrchestrator(catalog, provider, new SinkFactory(_loggerFactory), _loggerFactory);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(duration);

        try
        {
            await orchestrator.RunAsync(config, TimeSpan.FromSeconds(2), linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when the duration elapses.
        }
        finally
        {
            (provider as IDisposable)?.Dispose();
        }

        return $"Run complete for industry '{config.Run.Industry}', {config.Entities.Count} entities, {config.Targets.Count} targets.";
    }

    private sealed class SimpleOfflineProvider(string industry) : IValueProvider
    {
        public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GenerateAsync(ValueRequest request, CancellationToken cancellationToken)
        {
            var rows = new List<IReadOnlyDictionary<string, object?>>(request.Count);
            for (int i = 0; i < request.Count; i++)
            {
                var row = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (AttributeInfo a in request.SemanticAttributes)
                {
                    row[a.Name] = $"{industry}:{a.Name}:{i}";
                }

                rows.Add(row);
            }

            return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(rows);
        }
    }
}
