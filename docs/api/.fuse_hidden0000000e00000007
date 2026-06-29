# API: UniDataGen.Observability

Application Insights logging helpers.

```csharp
public static class AppInsightsLogging
{
    public static ILoggerFactory CreateFactory(
        string? connectionString = null,
        LogLevel minimumLevel = LogLevel.Information,
        Action<ILoggingBuilder>? configure = null);

    public static ILoggingBuilder AddDelegate(this ILoggingBuilder builder, Action<string> sink, LogLevel minLevel = LogLevel.Information);
}

public sealed class DelegateLoggerProvider : ILoggerProvider
{
    public DelegateLoggerProvider(Action<string> sink, LogLevel minLevel = LogLevel.Information);
}
```

`CreateFactory` wires Application Insights when a connection string is available, from the argument or
the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable, plus any extra providers from
`configure`. `AddDelegate` forwards formatted messages to a host sink, such as a text box or console.
