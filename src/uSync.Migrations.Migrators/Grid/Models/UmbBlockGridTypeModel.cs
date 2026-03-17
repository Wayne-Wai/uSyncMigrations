using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Grid.Models;

/// <summary>
///  in c# the default block configuration doesn't have all the extra bits the UI adds to the block. 
///  this means when we are making them in a migration we can't just use the default objects. 
/// </summary>
/// <remarks>
///  these are the same as the models in the UI, which contain all the bits we might want to
///  set and change. 
/// </remarks>
public class UmbBlockGridTypeModel : IBlockConfiguration
{
    // Block Settings
    public int? AreaGridColumns { get; set; }
    public UmbBlockGridAreaType[] Areas { get; set; } = [];
    public Guid ContentElementTypeKey { get; set; }
    public Guid? SettingsElementTypeKey { get; set; }
    public bool AllowAtRoot { get; set; }
    public bool AllowInAreas { get; set; }

    // UI Settings
    public string? Label { get; set; }
    public string? EditorSize { get; set; }
    public bool? InlineEditing { get; set; }
    public bool? HideContentEditor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? IconColor { get; set; }
    public string? Thumbnail { get; set; }
    public UmbBlockGridTypeColumnSpanOptions[]? ColumnSpanOptions { get; set; }
    public Guid? GroupKey { get; set; }
}

public class UmbBlockGridTypeColumnSpanOptions { 
    public required string ColumnSpan { get; set; }
}

public class UmbBlockGridAreaType : BlockGridConfiguration.BlockGridAreaConfiguration
{
    public List<UmbBlockGridTypeAreaTypePermissions>? SpecifiedAllowance { get; set; }
    public string? CreateLabel { get; set; }
}

public class UmbBlockGridTypeAreaTypePermissions
{
    public int MinAllowed { get; set; }
    public int? MaxAllowed { get; set; }
    public Guid ElementTypeKey { get; set; }
    public Guid? GroupKey { get; set; }
}

public class UmbBlockGridTypeGroupType
{
    public required string Name { get; set; }
    public Guid Key { get; set; }
}