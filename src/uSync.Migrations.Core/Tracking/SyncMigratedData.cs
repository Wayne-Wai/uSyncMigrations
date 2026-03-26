using NPoco;

using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

using uSync.Migrations.Core.Persistance;

namespace uSync.Migrations.Core.Tracking;

/// <summary>
///  basic information about the migrated item. 
/// </summary>
[TableName("uSyncMigratedData")]
[PrimaryKey("Id")]
[ExplicitColumns]
public class SyncMigratedData : ISyncDataEntity<string>
{
    [Column("Id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("Key")]
    public required string Key { get; set; }

    [Column("Original")]
    public required string Orginal { get; set; }

    [Column("Additional")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? AdditionalData { get; set; }
   
}
