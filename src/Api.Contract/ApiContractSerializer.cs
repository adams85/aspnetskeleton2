using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using ProtoBuf.Meta;
using WebApp.Service;

namespace WebApp.Api
{
    public static partial class ApiContractSerializer
    {
        public const string JsonTypePropertyName = "$type";

        private static Func<Type, string>? s_typeNameFormatter;
        public static Func<Type, string> TypeNameFormatter => LazyInitializer.EnsureInitialized(ref s_typeNameFormatter, TypeNameFormatterFactory)!;
        public static Func<Func<Type, string>> TypeNameFormatterFactory { get; set; } = () => type => type.AssemblyQualifiedName;

        public static readonly ModelMetadataProvider MetadataProvider = new ModelMetadataProvider(ProtoBufSerializer.TypeModel);

        public static readonly SerializerBase ProtoBuf = new ProtoBufSerializer();
        public static readonly SerializerBase Json = new JsonSerializer();

        private sealed class JsonSerializer : SerializerBase.ByteArrayBased
        {
            private static readonly JsonSerializerOptions s_options = new JsonSerializerOptions().ConfigureApiDefaults();

            public override byte[] Serialize(object? obj, Type type) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, type, s_options);
            public override byte[] Serialize<T>(T obj) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, s_options);

            public override object? Deserialize(ArraySegment<byte> bytes, Type type) => System.Text.Json.JsonSerializer.Deserialize(bytes, type, s_options);
            [return: MaybeNull]
            public override T Deserialize<T>(ArraySegment<byte> bytes) => System.Text.Json.JsonSerializer.Deserialize<T>(bytes, s_options);
        }

        private sealed class ProtoBufSerializer : SerializerBase.StreamBased
        {
            internal static readonly RuntimeTypeModel TypeModel = RuntimeTypeModel.Create().ConfigureApiDefaults();

            public override void Serialize(Stream stream, object? obj, Type type) => TypeModel.Serialize(stream, obj);
            public override void Serialize<T>(Stream stream, T obj) => TypeModel.Serialize(stream, obj);

            public override object? Deserialize(Stream stream, Type type) => TypeModel.Deserialize(stream, null, type);
            [return: MaybeNull]
            public override T Deserialize<T>(Stream stream) => (T)TypeModel.Deserialize(stream, null, typeof(T));
        }
    }
}

