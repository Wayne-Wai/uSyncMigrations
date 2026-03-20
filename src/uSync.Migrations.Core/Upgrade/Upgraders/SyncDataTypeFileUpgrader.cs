using System.Xml.Linq;

using uSync.Core;
using uSync.Core.DataTypes;

using UmbConstants = Umbraco.Cms.Core.Constants;

namespace uSync.Migrations.Core.Upgrade.Upgraders;

internal class SyncDataTypeFileUpgrader : SyncItemTypeFileUpgraderBase, ISyncFileUpgrader
{

    /// <summary>
    ///  these are the 'safe' editors that we don't have to upgrade (we *think*)
    ///  we will need to check that some of these don't need upgrading is some circumstances,
    /// </summary>
    private static readonly string[] _safeEditorAliases = [
        UmbConstants.PropertyEditors.Aliases.TextArea,
        UmbConstants.PropertyEditors.Aliases.TextBox,
        UmbConstants.PropertyEditors.Aliases.Label,
        UmbConstants.PropertyEditors.Aliases.Boolean,
        UmbConstants.PropertyEditors.Aliases.Integer,
        UmbConstants.PropertyEditors.Aliases.Decimal,
        UmbConstants.PropertyEditors.Aliases.DateTime,
        UmbConstants.PropertyEditors.Aliases.MultiUrlPicker,
        UmbConstants.PropertyEditors.Aliases.ListView,
        UmbConstants.PropertyEditors.Aliases.MemberPicker,
        UmbConstants.PropertyEditors.Aliases.MemberGroupPicker,
        UmbConstants.PropertyEditors.Aliases.ImageCropper,
    ];

    private readonly ConfigurationSerializerCollection _configurationSerializers;

    public SyncDataTypeFileUpgrader(SyncFileUpgraderCollection fileUpgraders, ConfigurationSerializerCollection configurationSerializers)
        : base(fileUpgraders)
    {
        _configurationSerializers = configurationSerializers;
    }

    public override string ItemType => "DataType";

    protected override string? GetItemKey(XElement node)
        => node.Element("Info")?.Element("EditorAlias").ValueOrDefault<string?>(null);

    public override async Task<IEnumerable<SyncUpgradeMessage>> AnalyseFilesAsync(SyncUpgradeFile file)
    {
        List<SyncUpgradeMessage> results = [.. await base.AnalyseFilesAsync(file)];

        var editorAlias = GetItemKey(file.Node);
        if (editorAlias is not null)
        {
            var serializer = _configurationSerializers.GetSerializer(editorAlias);
            if (serializer is null)
            {
                if (_safeEditorAliases.Contains(editorAlias, StringComparer.InvariantCultureIgnoreCase) is false)
                {
                    results.Add(new SyncUpgradeMessage
                    {
                        Status = SyncUpgradeStatus.Warning,
                        Upgrader = nameof(SyncDataTypeFileUpgrader),
                        FileName = file.Filename,
                        Message = $"No serializer found for editor alias {editorAlias}"
                    });
                }
            }
            else
            {
                var newEdtiorAlias = serializer.GetEditorAlias() ?? editorAlias;
                if (editorAlias.Equals(newEdtiorAlias, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    results.Add(new SyncUpgradeMessage
                    {
                        Status = SyncUpgradeStatus.Info,
                        Upgrader = nameof(SyncDataTypeFileUpgrader),
                        FileName = file.Filename,
                        Message = $"Serializer {serializer.GetType().Name} will migrate from {editorAlias} to {newEdtiorAlias}"
                    });
                }
            }
        }



        return results;
    }

}