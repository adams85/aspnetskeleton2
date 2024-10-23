// source: https://github.com/protobuf-net/protobuf-net/blob/3.2.30/src/protobuf-net.Core/ProtoIncludeAttribute.cs

using System;

namespace ProtoBuf;

// Protobuf-net needs some information which cannot be specified using the BCL attributes,
// but we want no 3rd party dependencies in the Service.Contract project.
// Luckily, protobuf-net allows us to replicate the necessary attribute.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
internal sealed class ProtoIncludeAttribute : Attribute
{
    public ProtoIncludeAttribute(int tag, Type knownType)
        : this(tag, knownType.AssemblyQualifiedName) { }

    public ProtoIncludeAttribute(int tag, string knownTypeName)
    {
        if (tag <= 0)
            throw new ArgumentOutOfRangeException(nameof(tag));
        Tag = tag;
        KnownTypeName = knownTypeName ?? throw new ArgumentNullException(nameof(knownTypeName));
    }

    public int Tag { get; }
    public string KnownTypeName { get; }
}
