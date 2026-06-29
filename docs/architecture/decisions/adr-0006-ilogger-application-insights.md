# ADR-0006: ILogger with Application Insights

## Status

Accepted

## Context

The system spans libraries and four hosts. Logging has to be complete and detailed across every tier
and flow to Azure Application Insights, without coupling the libraries to a specific telemetry product.

## Decision

Every tier logs through `Microsoft.Extensions.Logging.ILogger`. The libraries depend only on the
logging abstractions. `UniDataGen.Observability` wires Application Insights at the host boundary through
`AppInsightsLogging.CreateFactory`, and a `DelegateLoggerProvider` lets a host mirror logs into its own
UI. Logs carry structured properties and a run-scoped correlation id.

## Consequences

- The libraries stay free of a telemetry dependency and are testable with a null logger.
- Each host wires the destination once; Core, Configuration, Foundry, and the target adapters all flow
  through it.
- A single run is traceable end to end across hosts by its `RunId`.

## Alternatives considered

- Reference the Application Insights SDK directly in each library. Couples the libraries to one product
  and complicates testing.
- A custom logging interface. Reinvents `ILogger` and loses the provider ecosystem.
