namespace uSync.Migrations.Core;

public static class SyncMigrations
{

    /// <summary>
    ///  legacy property editor names, 
    /// </summary>
    /// <remarks>
    ///  saved for prosperity should they be removed from the core. 
    /// </remarks>
    public static class SyncLegacyTypes
    {
        public const string NestedContent = "Umbraco.NestedContent";
        public const string OurNestedContent = "Our.Umbraco.NestedContent";
        public const string Grid = "Umbraco.Grid";
        public const string MediaPicker = "Umbraco.MediaPicker";
        public const string MediaPicker2 = "Umbraco.MediaPicker2";
        public const string MultipleMediaPicker = "Umbraco.MultipleMediaPicker";
    }

}
