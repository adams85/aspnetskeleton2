using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace POTools.Services.Resources;

public class ResXFileReader : IEnumerable<(string Name, string Value, string? Comment)>
{
    private readonly root _root;

    public ResXFileReader(TextReader reader)
    {
        var xmlSerializer = new XmlSerializer(typeof(root));
        using (var xmlReader = XmlReader.Create(reader))
            _root = (root?)xmlSerializer.Deserialize(xmlReader) ?? new root { Items = Array.Empty<object>() };
    }

    public IEnumerator<(string Name, string Value, string? Comment)> GetEnumerator()
    {
        foreach (var dataItem in _root.Items.OfType<rootData>())
            yield return (dataItem.name, dataItem.value, dataItem.comment);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
