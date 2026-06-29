using UniDataGen.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UniDataGen.Targets;

/// <summary>
/// Writes generated batches to a SQL Server compatible target: Azure SQL Database or the Microsoft Fabric
/// Warehouse T-SQL endpoint. New records insert, Update updates by the configured key column, and Delete deletes
/// by key. The target table must already exist with columns that match the generated fields. Authentication is
/// Microsoft Entra by default (Active Directory Default), with SQL login or a full connection string as options.
/// </summary>
public sealed class SqlServerSink : ISink
{
    private readonly string _connectionString;
    private readonly string _schema;
    private readonly string _table;
    private readonly string? _keyColumn;
    private readonly ILogger _logger;

    public SqlServerSink(TargetConfig config, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        IReadOnlyDictionary<string, object?> p = config.Properties;
        _table = PropertyBag.GetString(p, "table") ?? throw new InvalidOperationException("SQL target requires a 'table' property.");
        _schema = PropertyBag.GetString(p, "schema") ?? "dbo";
        _keyColumn = PropertyBag.GetString(p, "keyColumn");
        _connectionString = BuildConnectionString(config);
        _logger = logger ?? NullLogger.Instance;
        TargetType = config.TargetType;
    }

    public SinkKind Kind => SinkKind.Batch;

    public string TargetType { get; }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("SQL target ready: {Schema}.{Table}.", _schema, _table);
    }

    public async Task WriteAsync(EntityBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (batch.Records.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        int written = 0;
        foreach (GeneratedRecord record in batch.Records)
        {
            written += await ApplyAsync(connection, transaction, record, cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Applied {Count} rows to {Schema}.{Table}.", written, _schema, _table);
    }

    private async Task<int> ApplyAsync(SqlConnection connection, SqlTransaction transaction, GeneratedRecord record, CancellationToken cancellationToken)
    {
        if (record.Action != RecordAction.New && _keyColumn is null)
        {
            _logger.LogDebug("Skipping {Action} for {Table}: no 'keyColumn' configured.", record.Action, _table);
            return 0;
        }

        await using SqlCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        var columns = record.Fields.Keys.ToList();

        switch (record.Action)
        {
            case RecordAction.New:
                string insertCols = string.Join(", ", columns.Select(c => $"[{c}]"));
                string insertParams = string.Join(", ", columns.Select((_, i) => $"@p{i}"));
                command.CommandText = $"INSERT INTO [{_schema}].[{_table}] ({insertCols}) VALUES ({insertParams});";
                AddParameters(command, columns, record);
                break;

            case RecordAction.Update:
                var setColumns = columns.Where(c => !c.Equals(_keyColumn, StringComparison.OrdinalIgnoreCase)).ToList();
                string setClause = string.Join(", ", setColumns.Select((c, i) => $"[{c}] = @p{i}"));
                command.CommandText = $"UPDATE [{_schema}].[{_table}] SET {setClause} WHERE [{_keyColumn}] = @key;";
                for (int i = 0; i < setColumns.Count; i++)
                {
                    command.Parameters.AddWithValue($"@p{i}", record.Fields[setColumns[i]] ?? DBNull.Value);
                }

                command.Parameters.AddWithValue("@key", record.Key);
                break;

            case RecordAction.Delete:
                command.CommandText = $"DELETE FROM [{_schema}].[{_table}] WHERE [{_keyColumn}] = @key;";
                command.Parameters.AddWithValue("@key", record.Key);
                break;
        }

        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0 ? 1 : 0;
    }

    private static void AddParameters(SqlCommand command, List<string> columns, GeneratedRecord record)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            command.Parameters.AddWithValue($"@p{i}", record.Fields[columns[i]] ?? DBNull.Value);
        }
    }

    private static string BuildConnectionString(TargetConfig config)
    {
        IReadOnlyDictionary<string, object?> p = config.Properties;
        if (PropertyBag.GetString(p, "connectionString") is { } cs)
        {
            return cs;
        }

        string server = PropertyBag.GetString(p, "server") ?? throw new InvalidOperationException("SQL target requires a 'server' or 'connectionString' property.");
        string database = PropertyBag.GetString(p, "database") ?? throw new InvalidOperationException("SQL target requires a 'database' property.");
        string port = PropertyBag.GetString(p, "port") ?? "1433";
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"tcp:{server},{port}",
            InitialCatalog = database,
            Encrypt = true
        };

        string? user = PropertyBag.GetString(p, "username");
        string? password = PropertyBag.GetString(p, "password");
        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
        {
            builder.UserID = user;
            builder.Password = password;
        }
        else
        {
            builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryDefault;
        }

        return builder.ConnectionString;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
