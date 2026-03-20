using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System.Runtime.Serialization;
using System.Text.Json.Nodes;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core.Extensions;
using uSync.Core.Mapping;
using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Seven.MegaNav;

internal class MegaNavContentMigrator : SyncValueMapperBase, ISyncMapper, ISyncPropertyMapper
{
    public MegaNavContentMigrator(IEntityService entityService) : base(entityService)
    {
    }

    public override string Name => nameof(MegaNavContentMigrator);

    public override string[] Editors => ["Cogworks.Meganav", "Meganav"];

    public Task<string?> GetImportValueAsync(string value, IPropertyType propertyType)
    {
        var data = value.DeserializeJson<JsonArray>();
        var entities = ConvertToEntity(data);

        return Task.FromResult<string?>(entities.SerializeJsonString());
    }

    private IEnumerable<MeganavEntity> ConvertToEntity(JsonArray? data)
    {
        foreach (var item in data?.Cast<JsonObject>() ?? [])
        {
            var entity = new MeganavEntity
            {
                Title = item.GetPropertyValue("title", string.Empty),
                Target = item.GetPropertyValue("target", string.Empty),
                Visible = item.GetPropertyValue("naviHide", false)
            };


            var id = item.GetPropertyValue("id", 0);

            UdiParser.TryParse(item.GetPropertyValue("udi", string.Empty), out Udi? udi);

            if (id > 0 || udi != null)
            {
                if (udi is GuidUdi guidUdi)
                {
                    // UDI is healthy

                    // UNDONE: the original source still obtains the object here but I think it's not needed.
                    // All I can see it'd do is check the node still exists, which is handled by the editor anyway.
                }
                else
                {
                    // convert ID to key
                    guidUdi = new GuidUdi(Constants.UdiEntityType.Document, id.ToGuid());
                }

                entity.Udi = guidUdi;
            }
            else
            {
                entity.Url = item.GetPropertyValue("url", string.Empty);
            }

            var children = item.GetPropertyValue("children", new JsonArray());

            if (children != null)
            {
                entity.Children = ConvertToEntity(children);
            }

            var ignoreProperties = new[]
            {
                "id", "key", "udi", "name", "title", "description", "target", "url", "children", "icon", "published",
                "naviHide", "culture"
            };

            var settings = item.ConvertToDictionary() ?? new Dictionary<string, object>();
            settings.RemoveAll(x => ignoreProperties.InvariantContains(x.Key));
            entity.Settings = settings;
            yield return entity;
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class MeganavEntity
    {
        [DataMember(Name = "title")]
        public string? Title { get; set; }

        [DataMember(Name = "url")]
        public string? Url { get; set; }

        [DataMember(Name = "target")]
        public string? Target { get; set; }

        [DataMember(Name = "visible")]
        public bool Visible { get; set; } = true;

        [DataMember(Name = "udi")]
        public GuidUdi? Udi { get; set; }

        [DataMember(Name = "itemTypeId")]
        public Guid? ItemTypeId { get; set; }

        [DataMember(Name = "settings")]
        public IDictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        [DataMember(Name = "children")]
        public IEnumerable<MeganavEntity> Children { get; set; } = new List<MeganavEntity>();
    }
}
