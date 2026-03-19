namespace uSync.Migrations.Migrators.NuPickers.Configuration;

internal class NuPickerMigrationOptions
{
    public const string Section = "Usync:Migrations:NuPickers";
    public Dictionary<string, string> AssembliesMapping { get; set; } = [];
    public Dictionary<string, string> NamespacesMapping { get; set; } = [];
}