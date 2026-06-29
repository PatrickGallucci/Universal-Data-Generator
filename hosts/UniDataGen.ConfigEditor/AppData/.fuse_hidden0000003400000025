namespace UniDataGen.ConfigEditor.AppData;

/// <summary>User preferences for the editor, persisted to settings.json in the local app data folder.</summary>
public sealed class AppSettings
{
    /// <summary>The path of the last opened or saved run configuration.</summary>
    public string? LastFilePath { get; set; }

    /// <summary>The last window width.</summary>
    public int WindowWidth { get; set; } = 900;

    /// <summary>The last window height.</summary>
    public int WindowHeight { get; set; } = 680;

    /// <summary>Recently opened run-configuration files, most recent first.</summary>
    public List<string> RecentFiles { get; set; } = [];
}

/// <summary>A locale entry stored in the editor's local locales.json.</summary>
public sealed class LocaleEntry
{
    public string CountryCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
}

/// <summary>A target type and its property template, stored in the editor's local target-types.json.</summary>
public sealed class TargetTypeEntry
{
    public string TargetType { get; set; } = string.Empty;
    public string Kind { get; set; } = "Batch";
    public List<string> Properties { get; set; } = [];
}

/// <summary>An industry entry stored in the editor's local industries.json.</summary>
public sealed class IndustryItem
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>An entity entry stored in the editor's local entities.json.</summary>
public sealed class EntityItem
{
    public string SchemaArea { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
