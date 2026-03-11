using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

using Umbraco.Extensions;

namespace uSync.Migrations.Core.Extensions;

public static class SyncMigrationDataTypeHelper
{

    public static XElement CreateDataType(string name, string propertyType, string folder, string configuration)
    {
        return new XElement("DataType",
            new XAttribute("Key", name.ToGuid()),
            new XAttribute("Alias", name),
            new XAttribute("Level", 2),
            new XElement("Info",
                new XElement("Name", name),
                new XElement("EditorAlias", propertyType),
                new XElement("Folder", "Grid+Editors")),
            new XElement("Config", new XCData(configuration))
        );
    }
}
