using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApp.Api;

/// <summary>
/// Extends JSON serialization of non-primitive, non-collection objects.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>Makes <see cref="JsonSerializer"/> respect <see cref="DataContractAttribute"/> and <see cref="DataMemberAttribute"/> (including fields).</item>
///   <item>Adds support for polymorphism based on <see cref="ProtoBuf.ProtoIncludeAttribute"/>.</item>
/// </list>
/// </remarks>
internal sealed partial class ApiObjectJsonConverterFactory : JsonConverterFactory
{
    private readonly ApiContractSerializer.ModelMetadataProvider _modelMetadataProvider;
    private readonly ConcurrentDictionary<Type, TypeHelper> _typeHelperCache;

    public ApiObjectJsonConverterFactory(ApiContractSerializer.ModelMetadataProvider modelMetadataProvider)
    {
        _typeHelperCache = new ConcurrentDictionary<Type, TypeHelper>();
        _modelMetadataProvider = modelMetadataProvider;
    }

    private bool CanConvertCore(Type typeToConvert)
    {
        if (typeToConvert.IsInterface)
            return true;

        if (typeToConvert.IsClass)
        {
            return
                typeToConvert != typeof(object) &&
                typeToConvert != typeof(ValueType) &&
                typeToConvert != typeof(Enum) &&
                typeToConvert != typeof(Delegate) &&
                typeToConvert != typeof(string) &&
                typeToConvert != typeof(Uri) &&
                typeToConvert != typeof(Version) &&
                typeToConvert != typeof(JsonDocument);
        }

        if (typeToConvert.IsValueType)
        {
            if (typeToConvert.IsGenericType)
            {
                typeToConvert = typeToConvert.GetGenericTypeDefinition();
                return
                    typeToConvert != typeof(Nullable<>) &&
                    typeToConvert != typeof(KeyValuePair<,>);
            }
            else
            {
                return
                    !typeToConvert.IsPrimitive &&
                    !typeToConvert.IsEnum &&
                    typeToConvert != typeof(decimal) &&
                    typeToConvert != typeof(DateTime) &&
                    typeToConvert != typeof(DateTimeOffset) &&
                    typeToConvert != typeof(TimeSpan) &&
                    typeToConvert != typeof(Guid) &&
                    typeToConvert != typeof(JsonElement);
            }
        }

        return false;
    }

    public override bool CanConvert(Type typeToConvert) =>
        CanConvertCore(typeToConvert) && !_modelMetadataProvider.ShouldSerializeAsList(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(typeToConvert), this, options);

    private TypeHelper GetTypeHelper(Type type) => _typeHelperCache.GetOrAdd(type, type => new TypeHelper(type));
}
