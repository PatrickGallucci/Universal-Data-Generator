using System.Text.Json;
using System.Text.Json.Serialization;
using UniDataGen.Abstractions;

namespace UniDataGen.Configuration;

/// <summary>Loads and saves the run configuration JSON shared by every host.</summary>
public sealed class RunConfigurationLoader
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>Deserializes a run configuration from a JSON string.</summary>
    public RunConfiguration LoadFromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        RunConfiguration? config = JsonSerializer.Deserialize<RunConfiguration>(json, Options);
        return config ?? throw new InvalidDataException("Run configuration JSON deserialized to null.");
    }

    /// <summary>Reads a run configuration from a file.</summary>
    public RunConfiguration LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Run configuration file not found.", path);
        }

        return LoadFromJson(File.ReadAllText(path));
    }

    /// <summary>Serializes a run configuration to indented JSON.</summary>
    public string ToJson(RunConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return JsonSerializer.Serialize(configuration, Options);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
