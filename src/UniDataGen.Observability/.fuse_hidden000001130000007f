using Microsoft.Extensions.Logging;

namespace UniDataGen.Observability;

/// <summary>
/// An <see cref="ILoggerProvider"/> that forwards every formatted message to a delegate. Used by hosts to
/// surface logs in their own UI, for example a WinForms text box or a console, alongside Application Insights.
/// The delegate is invoked from arbitrary threads, so hosts that touch UI must marshal inside it.
/// </summary>
public sealed class DelegateLoggerProvider : ILoggerProvider
{
    private readonly Action<string> _sink;
    private readonly LogLevel _minLevel;

    public DelegateLoggerProvider(Action<string> sink, LogLevel minLevel = LogLevel.Information)
    {
        ArgumentNullException.ThrowIfNull(sink);
        _sink = sink;
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName) => new DelegateLogger(categoryName, _sink, _minLevel);

    public void Dispose()
    {
    }

    private sealed class DelegateLogger : ILogger
    {
        private readonly string _category;
        private readonly Action<string> _sink;
        private readonly LogLevel _minLevel;

        public DelegateLogger(string category, Action<string> sink, LogLevel minLevel)
        {
            _category = category;
            _sink = sink;
            _minLevel = minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel && logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string shortCategory = _category.Contains('.') ? _category[(_category.LastIndexOf('.') + 1)..] : _category;
            string message = $"{logLevel,-11} {shortCategory}: {formatter(state, exception)}";
            if (exception is not null)
            {
                message += $" | {exception.GetType().Name}: {exception.Message}";
            }

            _sink(message);
        }
    }
}
