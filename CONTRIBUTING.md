# Contributing

Thank you for working on Universal Data Generator. This guide covers how to set up the project,
the standards the code follows, and how to keep documentation current.

## Prerequisites

- The .NET 10 SDK.
- On Windows, the Windows desktop workload for the WinForms host.

## Build and test

Build the whole solution and run the tests:

```bash
dotnet build UniDataGen.slnx
dotnet test tests/UniDataGen.Core.Tests
dotnet test tests/UniDataGen.Targets.Tests
```

On a non-Windows machine, add `-p:EnableWindowsTargeting=true` to compile the WinForms host.

## Code standards

The project follows the conventions in [docs/contributing/style-guide.md](docs/contributing/style-guide.md):
file-scoped namespaces, `is null` checks, pattern matching, `nameof`, XML doc comments on
public APIs, and `ConfigureAwait(false)` in library code. Format with the rules in `.editorconfig`.

## Documentation

Documentation is part of every change. When you add, alter, or remove a feature, update the
matching documents in the same change:

- The [CHANGELOG](CHANGELOG.md) under the `Unreleased` section.
- The affected guide under [docs/guides/](docs/guides/).
- The affected API reference under [docs/api/](docs/api/).
- A new ADR under [docs/architecture/decisions/](docs/architecture/decisions/) for a
  substantive design decision.

See [docs/contributing/development.md](docs/contributing/development.md) for the full workflow.

## Pull requests

Keep changes focused. Confirm the build succeeds and the tests pass before you open a pull
request, and confirm the documentation matches the code.
