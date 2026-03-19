using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Blocks;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content.BlockMigrators;

public interface ISyncBlockMigrator
{
    /// <summary>
    ///  supported grid item aliases that this migrator will support.
    /// </summary>
    string[] Aliases { get; }


    /// <summary>
    ///  return the expected content type alias for this control.
    /// </summary>
    string? GetContentTypeAlias(GridValue.GridControl control);


    /// <summary>
    ///  convert the values in teh grid element into values that will
    ///  be used in the block grid element. 
    /// </summary>
    Dictionary<string, object> GetPropertyValues(GridValue.GridControl control);

    /// <summary>
    ///  get any additional GridContentBlocks that might be required for this 
    ///  property to be converted to the grid. 
    /// </summary>
    /// <remarks>
    ///  primarliy when properties are converted into content types in the grid
    ///  so we need to point to the content type and say this block is of type.
    ///  
    ///  macros is a good example of when this happens. 
    /// </remarks>
    IEnumerable<BlockItemData> GetPropertyContentBlocks(GridValue.GridControl control);

    /// <summary>
    ///  Retrieves the collection of property settings blocks associated with the specified grid control.
    /// </summary>
    /// <param name="control">The grid control for which to obtain property settings blocks. Cannot be null.</param>
    /// <returns>An enumerable collection of block item data representing the property settings for the specified control. The
    /// collection will be empty if no property settings are available.</returns>
    IEnumerable<BlockItemData> GetPropertySettingsBlocks(GridValue.GridControl control);
}

public class SyncBlockMigratorCollection
    : BuilderCollectionBase<ISyncBlockMigrator>
{
    public SyncBlockMigratorCollection(Func<IEnumerable<ISyncBlockMigrator>> items) : base(items)
    {
    }

    public IEnumerable<ISyncBlockMigrator> GetMigrators(string? controlAlias)
    {
        if (controlAlias is null) return [];
        return this.Where(x => x.Aliases.Contains(controlAlias, StringComparer.InvariantCultureIgnoreCase));
    }

    public IEnumerable<ISyncBlockMigrator> GetDefaultMigrators()
        => GetMigrators(SyncGridMigrations.DefaultMigratorType);

    public IEnumerable<ISyncBlockMigrator> GetMigrators(GridValue.GridEditor? editor)
    {
        if (editor is null) return [];

        var migrators = new List<ISyncBlockMigrator>();
        var viewName = Path.GetFileNameWithoutExtension(editor.View);
        migrators.AddRange(GetMigrators(viewName));

        migrators.AddRange(GetMigrators(editor.Alias));

        if (migrators.Count == 0)
            return GetDefaultMigrators();
        return
            migrators;
    }
}

public class SyncBlockMigratorCollectionBuilder
    : LazyCollectionBuilderBase<SyncBlockMigratorCollectionBuilder, SyncBlockMigratorCollection, ISyncBlockMigrator>
{
    protected override SyncBlockMigratorCollectionBuilder This => this;
}