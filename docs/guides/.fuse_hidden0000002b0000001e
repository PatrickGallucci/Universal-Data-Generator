# Logging and Application Insights

Every tier logs through `Microsoft.Extensions.Logging.ILogger`: the Core orchestrator and value
coordinator, the Configuration catalog store and validator, the Foundry value provider, and every
target adapter. The libraries depend only on the logging abstractions, so the host chooses the
destination.

## How logging flows

```mermaid
flowchart LR
    Core[UniDataGen.Core] --> ILogger
    Config[UniDataGen.Configuration] --> ILogger
    Foundry[UniDataGen.Generation.Foundry] --> ILogger
    Targets[UniDataGen.Targets] --> ILogger
    ILogger[ILogger abstraction] --> Factory[ILoggerFactory at the host]
    Factory --> AI[Application Insights]
    Factory --> UI[Host UI or console]
```

`UniDataGen.Observability` wires Application Insights through `AppInsightsLogging.CreateFactory`. It
reads the connection string from the argument or the `APPLICATIONINSIGHTS_CONNECTION_STRING`
environment variable. A `DelegateLoggerProvider` forwards formatted messages to a host-supplied sink,
which is how the WinForms client mirrors logs into its in-app log box.

## What each host does

| Host | Application Insights | Local output |
|------|----------------------|--------------|
| WinForms | Connection-string field on the toolbar | In-app log box |
| PowerShell | `-AppInsightsConnectionString` or the environment variable | Verbose stream |
| Functions | Worker Application Insights integration, set the app setting | Function host logs |
| Foundry tool | The environment variable | Console, always on |

## Structured properties

Logs carry named properties so a single run is traceable end to end:

| Property | Where | Meaning |
|----------|-------|---------|
| `RunId` | Run scope | A correlation id minted per run. |
| `Industry`, `SourceType` | Run scope | The run selection. |
| `Entity`, `Mode` | Entity scope | The entity and its generation mode. |
| `TargetType` | Sink logs | The target being written. |

## Levels

| Level | Used for |
|-------|----------|
| Information | Run lifecycle, sink open, and each write. |
| Debug | Per-tick accrual and per-batch detail. |
| Warning | Identity-pool warm-up and runs with no enabled targets. |
| Error | Failures, with the exception attached. |

## Example query

In Application Insights, find every write for one run:

```kusto
traces
| where customDimensions.RunId == "the-run-id"
| order by timestamp asc
```
