namespace uSync.Migrations.Migrators.Grid;

internal static class SyncGridMigrations
{
    public const string ContentTypeFolder = "ContentTypes/grid/";
    public const string DataTypeFolder = "DataTypes/grid/";

    private const string ContainerName = "Grid+Editors";
    public const string LayoutContainerName = ContainerName + "/Layouts";
    public const string SettingsContainerName = ContainerName + "/Settings";
    public const string ElementContainerName = ContainerName + "/Elements";

    public const int DefaultGridColumns = 12;

    public const string DefaultMigratorType = "__default__";

    public static class ApplyTo
    {
        public const string ApplyToAll = "All";
        public const string ApplyToRow = "Rows";
        public const string ApplyToArea = "Areas";
    }


}
