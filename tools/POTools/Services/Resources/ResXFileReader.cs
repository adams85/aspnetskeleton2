using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace POTools.Services.Resources;

public class ResXFileReader : IEnumerable<KeyValuePair<string, string>>
{
    private readonly root _root;

    public ResXFileReader(TextReader reader)
    {
        var xmlSerializer = new XmlSerializer(typeof(root));
        using (var xmlReader = XmlReader.Create(reader))
            _root = (root?)xmlSerializer.Deserialize(xmlReader) ?? new root { Items = Array.Empty<object>() };
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var dataItem in _root.Items.OfType<rootData>())
            yield return new KeyValuePair<string, string>(dataItem.name, dataItem.value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
