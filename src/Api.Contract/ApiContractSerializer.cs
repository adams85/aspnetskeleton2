using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using WebApp.Service;

namespace WebApp.Api;

public static partial class ApiContractSerializer
{
    public const string JsonTypeDiscriminatorPropertyName = "$type";

    private static volatile bool s_allowDynamicCodeGeneration;
    /// <remarks>
    /// Only applies to JSON serialization as Protobuf serialization currently requires dynamic code generation anyway.
    /// </remarks>
    public static bool AllowDynamicCodeGeneration { get => s_allowDynamicCodeGeneration; set => s_allowDynamicCodeGeneration = value; }

    private static volatile Func<Func<Type, string>> s_typeNameFormatterFactory = () => type => type.AssemblyQualifiedName;
    public static Func<Func<Type, string>> TypeNameFormatterFactory { get => s_typeNameFormatterFactory; set => s_typeNameFormatterFactory = value; }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataContractAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataMemberAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(EnumMemberAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ProtoIncludeAttribute))]
    public static readonly ModelMetadataProvider MetadataProvider = new ModelMetadataProvider(ProtoBufSerializer.TypeModel);

    /// <summary>
    /// A serializer that can be used to serialize DTOs to Protobuf format.
    /// (Does not work with Native AOT currently.)
    /// </summary>
    public static readonly SerializerBase ProtoBuf = new ProtoBufSerializer();

    /// <summary>
    /// A serializer that can be used to serialize DTOs to JSON format.
    /// (Can be made to work with Native AOT, provided that <see cref="AllowDynamicCodeGeneration"/> is set to <see langword="false" />
    /// and assembly trimming is configured to preserve all DTO types, including generic instantiations, plus the necessary
    /// JSON serializer infrastructure types for DTO types and property types. See also <see cref="Helpers.AotHelper"/>.)
    /// </summary>
    /// <remarks>
    /// Uses System.Text.Json under the hood, with a custom contract resolver (<see cref="ApiContractJsonTypeInfoResolver"/>)
    /// that makes the serializer respect the attributes used to annotate DTO types for Protobuf
    /// (<see cref="DataContractAttribute"/>, <see cref="DataContractAttribute"/> and <see cref="ProtoIncludeAttribute"/> ).
    /// Because of this, the following attributes are ignored:
    /// <list type="bullet">
    ///  <item><see cref="JsonDerivedTypeAttribute"/></item>
    ///  <item><see cref="JsonExtensionDataAttribute"/></item>
    ///  <item><see cref="JsonIgnoreAttribute"/></item>
    ///  <item><see cref="JsonIncludeAttribute"/></item>
    ///  <item><see cref="JsonPolymorphicAttribute"/></item>
    ///  <item><see cref="JsonPropertyNameAttribute"/></item>
    ///  <item><see cref="JsonPropertyOrderAttribute"/></item>
    ///  <item><see cref="JsonRequiredAttribute"/> and the <see langword="required" /> keyword</item>
    /// </list>
    /// </remarks>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JsonConstructorAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JsonConverterAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JsonNumberHandlingAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JsonObjectCreationHandlingAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(JsonUnmappedMemberHandlingAttribute))]
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

    internal static class FrozenOptions
    {
        public static readonly Func<Type, string> TypeNameFormatter = TypeNameFormatterFactory();
        public static readonly bool AllowDynamicCodeGeneration = ApiContractSerializer.AllowDynamicCodeGeneration;
    }
}
