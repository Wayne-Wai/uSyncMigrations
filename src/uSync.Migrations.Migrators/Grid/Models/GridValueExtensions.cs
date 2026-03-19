namespace uSync.Migrations.Migrators.Grid.Models;

internal static class GridValueExtensions
{
    public static bool HasConfigOrStyles(this GridValue.GridBlock gridBlock)
        => gridBlock.HasConfig() || gridBlock.HasStyles();
}
