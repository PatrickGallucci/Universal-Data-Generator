using UniDataGen.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UniDataGen.Targets;

/// <summary>
/// Registers the target sinks with a dependency injection container. This is the supported way to add sinks:
/// call <see cref="AddUniDataGenTargets"/> for the built-ins, then <see cref="AddSink"/> for any custom sink.
/// </summary>
public static class TargetServiceCollectionExtensions
{
    /// <summary>Registers every built-in sink and the DI-backed <see cref="ISinkFactory"/>.</summary>
    public static IServiceCollection AddUniDataGenTargets(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        foreach (ISinkRegistration registration in DefaultSinks.All())
        {
            services.AddSingleton(registration);
        }

        services.AddSingleton<ISinkFactory, DiSinkFactory>();
        return services;
    }

    /// <summary>
    /// Registers one custom sink. Call this after <see cref="AddUniDataGenTargets"/> to add a new target type, or to
    /// override a built-in by reusing its target type name.
    /// </summary>
    /// <example>
    ///   <code>
    ///   services.AddUniDataGenTargets()
    ///           .AddSink("MyTarget", SinkKind.Batch, (config, logger) => new MySink(config, logger));
    ///   </code>
    /// </example>
    public static IServiceCollection AddSink(
        this IServiceCollection services,
        string targetType,
        SinkKind kind,
        Func<TargetConfig, ILogger, ISink> factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ISinkRegistration>(new SinkRegistration(targetType, kind, factory));
        return services;
    }
}
