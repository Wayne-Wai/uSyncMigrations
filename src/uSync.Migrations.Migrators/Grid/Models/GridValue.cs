using System.Text.Json.Nodes;

namespace uSync.Migrations.Migrators.Grid.Models;

/// <summary>
///  this is a copy of the GridValue classes from v13 and below.
///  these have been removed from core, so we have to have our 
///  own copies to be able to migrate from them.
/// </summary>
public class GridValue
{
    public string? Name { get; set; }

    public IEnumerable<GridSection> Sections { get; set; } = null!;

    public class GridSection
    {
        public string? Grid { get; set; } // TODO: what is this?
        public IEnumerable<GridRow> Rows { get; set; } = null!;
    }

    public class GridRow
    {
        public string? Name { get; set; }
        public Guid Id { get; set; }
        public IEnumerable<GridArea> Areas { get; set; } = null!;
        public JsonObject? Styles { get; set; }
        public JsonObject? Config { get; set; }
    }
    public class GridArea
    {
        public string? Grid { get; set; } // TODO: what is this?

        public IEnumerable<GridControl> Controls { get; set; } = null!;

        public JsonObject? Styles { get; set; }
        public JsonObject? Config { get; set; }
    }
    public class GridControl
    {
        public JsonObject? Value { get; set; }

        public GridEditor Editor { get; set; } = null!;

        public JsonObject? Styles { get; set; }
        public JsonObject? Config { get; set; }
    }

    public class GridEditor
    {
        public string Alias { get; set; } = null!;

        public string? View { get; set; }
    }
}
