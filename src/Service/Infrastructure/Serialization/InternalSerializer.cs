using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ProtoBuf.Meta;

namespace WebApp.Service.Infrastructure.Serialization;

internal static class InternalSerializer
{
    public static readonly SerializerBase ProtoBuf = new ProtoBufSerializer();

    public static readonly SerializerBase CacheKey = ProtoBuf;
    public static readonly SerializerBase MailModel = ProtoBuf;

    private sealed class ProtoBufSerializer : SerializerBase.StreamBased
    {
        private static readonly RuntimeTypeModel s_typeModel = RuntimeTypeModel.Create();

        public override void Serialize(Stream stream, object? obj, Type type) => s_typeModel.Serialize(stream, obj);
        public override void Serialize<T>(Stream stream, T obj) => s_typeModel.Serialize(stream, obj);

        public override object? Deserialize(Stream stream, Type type) => s_typeModel.Deserialize(stream, null, type);
        [return: MaybeNull]
        public override T Deserialize<T>(Stream stream) => (T)s_typeModel.Deserialize(stream, null, typeof(T));
    }
}
