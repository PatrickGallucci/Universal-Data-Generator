# Configuration editor

`UniDataGen.ConfigEditor` is a Windows desktop application for editing the core configuration. It edits the full
run configuration, the run header, every entity profile, and every target, and reads and writes run-configuration
JSON. The editor keeps its own reference lists and preferences in local JSON files, so it carries its own data.

The editor runs on Windows only, like the other WinForms host.

## What it edits

The editor binds the run configuration to a property grid, so every setting in the model is editable:

- The run header: industry, source type, locale, default time zone, and the Foundry endpoint, deployment, and
  authentication method.
- Each entity profile: schema area and entity name, mode, locale and time zone overrides, the schedule split and
  day-part weights, real-time and batch rates, batch cadence, and boost dates.
- Each target: type, name, enabled flag, authentication method and secret reference, and the property name and
  value pairs.

Dropdowns are populated from the local reference lists: industry, entity name, schema area, locale, source type,
target type, authentication method, and time zone. Tools, Validate checks the industry, entities, and locales
against the catalog. The industry and entity lists come from the configuration catalog (industries.json and
entities.json); edit them under `src/UniDataGen.Configuration/Resources` as described in
[Configuration](../getting-started/configuration.md).

## Local JSON data

On first run the editor creates a folder under the user's local application data and seeds these files. It reads
them on every start, so you can edit the lists directly.

| File | Purpose |
|------|---------|
| `settings.json` | Window size, the last opened file, and the recent files list. |
| `locales.json` | The supported locales, seeded from the core locale catalog. |
| `source-types.json` | The source-type codes offered in the dropdown. |
| `target-types.json` | The target types and their property templates. |
| `industries.json` | The industries offered in the Industry dropdown, seeded from the configuration catalog. |
| `entities.json` | The schema areas and entity names offered in the Entity dropdowns, seeded from the configuration catalog. |

Open the folder from **File, Open data folder**.

## Using the editor

1. **File, New** starts an empty configuration. **File, Open** loads a run-config JSON. **File, Save** and
   **Save As** write it back.
2. The property grid edits every field. Nested settings such as the Foundry block and the schedule expand inline.
3. **Edit, Add entity** and **Edit, Add target** add an item. The Entities and Targets collections also open a
   collection editor from the grid for adding, removing, and reordering items.
4. **Tools, Validate** checks the required fields and the locales against the catalog.

The editor saves the same run-configuration JSON the hosts consume, so a file you edit here runs unchanged in the
PowerShell module, the Azure Functions host, the WinForms client, or the Foundry tool. See
[Run configuration](./run-configuration.md) for the field reference.
