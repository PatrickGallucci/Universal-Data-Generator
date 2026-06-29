# Troubleshooting

This page lists common problems and their fixes.

## The run fails validation at startup

The validator checks the run against the catalog before any record is generated. Read the error:

- "Industry X is not in the catalog" means the industry name does not match the `Industry` sheet.
  Use the exact name, including spacing and capitalization.
- "Entity X is not in the catalog" means the schema area or entity name is wrong.
- "Target X has unknown type" means the `targetType` is misspelled.

## Updates and deletes produce nothing at the start

The engine keeps a per-entity pool of generated keys. Update and Delete draw from that pool. At the
start of a run the pool is empty, so the engine warms up: it skips Update and Delete until New
records have filled the pool. This is expected and logs at Debug level.

## A real-time entity writes to a batch-oriented target slowly

Real-time mode favors streaming targets. A real-time entity pointed only at a batch target such as
Azure Data Share generates correctly but delivers coarsely. Add a streaming target, or accept the
coarser delivery.

## Authentication fails against an Azure target

The default is Microsoft Entra through `DefaultAzureCredential`. Confirm:

- You are signed in locally with `az login`, or the host runs under a managed identity.
- The principal has the role the target requires. See [Deployment](./deployment.md).

To use a key instead, set the target `authentication.method` to `StorageKey` (ADLS) and supply the
key, or set a `connectionString` property (Event Hubs).

## Parquet output has a string column where a number is expected

The Parquet writer infers a column type from the values across a batch. A column with mixed types,
for example a number in one record and text in another, falls back to string. Make the value
provider return a consistent type for that attribute.

## The WinForms host does not build on Linux

The WinForms host targets Windows. Build it on Windows, or add `-p:EnableWindowsTargeting=true` to
compile it for inspection elsewhere. It runs on Windows only.
