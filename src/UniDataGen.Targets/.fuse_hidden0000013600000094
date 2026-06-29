using UniDataGen.Abstractions;
using Microsoft.Extensions.Logging;

namespace UniDataGen.Targets;

/// <summary>
/// Describes how to build one sink type. Registrations are resolved from dependency injection, so adding a new
/// sink in the future means registering one of these rather than editing a switch.
/// </summary>
public interface ISinkRegistration
{
    /// <summary>The target type name this registration handles, for example "AzureBlobStorage".</summary>
    string TargetType { get; }

    /// <summary>Whether the sink is naturally streaming or batch oriented.</summary>
    SinkKind Kind { get; }

    /// <summary>Builds a sink for a target configuration, with a logger already scoped to the target type.</summary>
    ISink Create(TargetConfig config, ILogger logger);
}

/// <summary>A registration backed by a factory delegate, the common case.</summary>
public sealed class SinkRegistration : ISinkRegistration
{
    private readonly Func<TargetConfig, ILogger, ISink> _factory;

    public SinkRegistration(string targetType, SinkKind kind, Func<TargetConfig, ILogger, ISink> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetType);
        ArgumentNullException.ThrowIfNull(factory);
        TargetType = targetType;
        Kind = kind;
        _factory = factory;
    }

    public string TargetType { get; }

    public SinkKind Kind { get; }

    public ISink Create(TargetConfig config, ILogger logger) => _factory(config, logger);
}
