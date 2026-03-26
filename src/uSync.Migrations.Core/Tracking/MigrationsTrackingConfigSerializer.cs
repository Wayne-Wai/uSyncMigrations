using uSync.Core.DataTypes;

namespace uSync.Migrations.Core.Tracking;

/// <summary>
///  this is the generic tracking serializer that we use to keep track
///  of datatypes that change editor alias during an import. 
/// </summary>
public class MigrationsTrackingConfigSerializer : IConfigurationTrackingSerializer
{
    private readonly ISyncMigrationTrackingService _trackingService;

    public MigrationsTrackingConfigSerializer(ISyncMigrationTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    public string Name => nameof(MigrationsTrackingConfigSerializer);

    public string[] Editors => [];

    /// <summary>
    ///  this serializer will check all editor types. 
    /// </summary>
    public bool IsSerializer(string propertyName) => true;


    /// <summary>
    ///  when uSync detects a name change of dataType's editorAlias, we track that, 
    ///  in the migrations table. so later when content is being imported we can 
    ///  fetch any legacy migrators to handle the content changes. 
    /// </summary>
    public async Task TrackRenamedEditorAsync(string oldEditorAlias, string newEditorAlias)
        => await _trackingService.AddRenameAsync(newEditorAlias, oldEditorAlias, null);
    
}
