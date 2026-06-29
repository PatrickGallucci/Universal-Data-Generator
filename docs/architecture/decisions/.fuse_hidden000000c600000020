# ADR-0001: Single net10 target

## Status

Accepted

## Context

The engine ships in libraries consumed by a PowerShell module, an Azure Functions worker, a WinForms
client, and an Azure AI Foundry tool. Windows PowerShell 5.1 runs on .NET Framework, which loads only
`netstandard2.0` assemblies. Targeting the newest runtime and supporting Windows PowerShell 5.1 pull
in opposite directions.

## Decision

Target `net10.0` across the solution, with the WinForms host on `net10.0-windows`. Use the latest C#
and .NET features per the project conventions.

## Consequences

- The PowerShell module requires PowerShell 7 or later, which runs on the same runtime.
- Windows PowerShell 5.1 and .NET Framework WinForms are out of scope.
- The code uses current language features (records, primary constructors, collection expressions, and
  `System.Threading.Lock`).

## Alternatives considered

- Multi-target `netstandard2.0` plus the latest `net`. Widest reach, including Windows PowerShell 5.1,
  at the cost of build complexity and conditional code. Rejected because the hosts run on PowerShell 7
  and the .NET isolated worker.
- A single `net8.0` LTS target. Broad support today but still no Windows PowerShell 5.1, and it gives
  up the newest features.
