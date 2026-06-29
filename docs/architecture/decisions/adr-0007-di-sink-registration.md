# ADR-0007: Dependency-injected sink registration

## Status

Accepted

## Context

The generator writes to many target types, and more will be added. The first factory used a switch over the
target type, which meant editing the factory for every new sink and made it hard for a host to add or override
a target without changing the library.

## Decision

Resolve sinks from dependency injection. Each target type is an `ISinkRegistration` that carries its type, its
kind, and a factory delegate. `AddUniDataGenTargets` registers the built-ins and a `DiSinkFactory`; `AddSink`
registers a custom sink or overrides a built-in, with the last registration winning. A non-DI `SinkFactory`
wraps the same built-in registrations for hosts that do not use a container.

## Consequences

- Adding a target is one `AddSink` call, with no edit to the factory.
- A host can override a built-in sink, for example to swap in a test double.
- The Azure Functions host resolves `ISinkFactory` from the container; the PowerShell and WinForms hosts
  construct `SinkFactory` directly.
- The registry is the single source of truth for which target types ship, exercised by a test that constructs
  every registered sink.

## Alternatives considered

- A switch over the target type. Simple but requires a library edit per sink and offers no host override.
- Assembly scanning for `ISink` implementations. Automatic but hides which targets ship and complicates passing
  per-target configuration and loggers.
