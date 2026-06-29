using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// An <see cref="ISinkFactory"/> that resolves sinks from registered <see cref="ISinkRegistration"/> instances.
/// When two registrations share a target type, the last one wins, so a host can override a built-in sink.
/// </summary>
public sealed class DiSinkFactory : ISinkFactory
{
    private readonly Dictionary<string, ISinkRegistration> _byType = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILoggerFactory _loggerFactory;

    public DiSinkFactory(IEnumerable<ISinkRegistration> registrations, ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        foreach (ISinkRegistration registration in registrations)
        {
            _byType[registration.TargetType] = registration; // last wins
        }

        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    /// <summary>The target types this factory can build.</summary>
    public IReadOnlyCollection<string> RegisteredTargetTypes => _byType.Keys;

    /// <inheritdoc />
    public bool CanCreate(string targetType) => _byType.ContainsKey(targetType);

    /// <inheritdoc />
    public ISink Create(TargetConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!_byType.TryGetValue(config.TargetType, out ISinkRegistration? registration))
        {
            throw new NotSupportedException($"No sink is registered for target type '{config.TargetType}'.");
        }

        ILogger logger = _loggerFactory.CreateLogger($"UniDataGen.Targets.{config.TargetType}");
        return registration.Create(config, logger);
    }
}
