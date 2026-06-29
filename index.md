# Universal Data Generator

Universal Data Generator is a JSON-configured synthetic data engine. One C# Core library does the
work: it reads a catalog, computes how many records each entity owes at a given moment, fills values
through Azure AI Foundry, and writes to a target. Five thin hosts call that Core: a PowerShell module,
an Azure Functions worker, a WinForms client, a configuration editor, and an Azure AI Foundry agent tool.

The target framework is `net10.0` / C# 14. The WinForms and configuration-editor hosts target
`net10.0-windows` and run on Windows only.

## Start here

- [Documentation home](docs/README.md) — the full guide index.
- [Installation](docs/getting-started/installation.md) — prerequisites and how to build.
- [Quick start](docs/getting-started/quick-start.md) — generate data offline in a few minutes.
- [Targets](docs/guides/targets.md) — the twenty supported sink types.
- [Architecture](docs/architecture/overview.md) — components, data flow, and decisions.

## Source

The code lives at
[github.com/PatrickGallucci/Universal-Data-Generator](https://github.com/PatrickGallucci/Universal-Data-Generator).
