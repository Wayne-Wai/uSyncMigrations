namespace uSync.Migrations.Migrators.Grid.Models;

internal class GridEditor
{
    public required string Name { get; set; }
    public required string Alias { get; set; }
    public string? NameTemplate { get; set; }
    public string? View { get; set; }
    public string? Icon { get; set; }
    public GridEditorConfig? Config { get; set; }
}

internal class GridEditorConfig
{
    public string? Style { get; set; }
    public string? Markup { get; set; }
}
