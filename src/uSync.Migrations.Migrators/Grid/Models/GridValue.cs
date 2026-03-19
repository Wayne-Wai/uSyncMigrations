using System.Diagnostics.CodeAnalysis;
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

    public IEnumerable<GridSection> Sections { get; set; } = [];

    public class GridSection
    {
        public int? Grid { get; set; } // count of columns in a section,.
        public bool? AllowAll { get; set; }
        public IEnumerable<GridRow> Rows { get; set; } = [];
    }

    public class GridBlock
    {
        public JsonObject? Styles { get; set; }
        public JsonObject? Config { get; set; }

        [MemberNotNullWhen(true, nameof(Config))]
        public bool HasConfig()
            => Config != null && Config.Count > 0;

        [MemberNotNullWhen(true, nameof(Styles))]
        public bool HasStyles()
            => Styles != null && Styles.Count > 0;

    }

    public class GridRow : GridBlock
    {
        public string? Name { get; set; }
        public Guid Id { get; set; }
        public IEnumerable<GridArea> Areas { get; set; } = null!;
    }
    public class GridArea : GridBlock
    {
        public int? Grid { get; set; } // TODO: what is this?
        public IEnumerable<GridControl> Controls { get; set; } = null!;
    }

    public class GridControl : GridBlock
    {
        public JsonNode? Value { get; set; }
        public GridEditor Editor { get; set; } = null!;
    }

    public class GridEditor
    {
        public string Alias { get; set; } = null!;
        public string? View { get; set; }
    }
}
