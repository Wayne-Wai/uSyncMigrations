using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Migrators.Grid.Models;

namespace uSync.Migrations.Migrators.Grid.Content.BlockMigrators;

public interface ISyncBlockMigrator
{
    string[] Aliases { get; }

    Dictionary<string, object> GetPropertyValues(GridValue.GridControl control);
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