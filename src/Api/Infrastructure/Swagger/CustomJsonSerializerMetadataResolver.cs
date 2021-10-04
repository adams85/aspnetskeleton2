using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.Api.Infrastructure.ModelBinding;
using WebApp.Core.Helpers;

namespace WebApp.Api.Infrastructure.Swagger
{
    /// <summary>
    /// Customizes Swagger JSON schema generation to match the behavior of <see cref="ApiContractSerializer"/> (including <seealso cref="ApiObjectJsonConverterFactory"/>).
    /// </summary>
    /// <remarks>
    /// Together with <see cref="DataContractMetadataDetailsProvider"/>, this class is necessary for correct Swagger JSON generation.
    /// </remarks>
    // based on: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v5.5.1/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/JsonSerializerDataContractResolver.cs
    public sealed class CustomJsonSerializerDataContractResolver : ISerializerDataContractResolver
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly Func<string, string> _convertMemberName;

        public CustomJsonSerializerDataContractResolver(IOptions<JsonOptions>? jsonOptions)
        {
            _serializerOptions = jsonOptions?.Value?.JsonSerializerOptions ?? new JsonSerializerOptions().ConfigureApiDefaults();
            _convertMemberName = _serializerOptions.PropertyNamingPolicy != null ? _serializerOptions.PropertyNamingPolicy.ConvertName : CachedDelegates.Identity<string>.Func;
        }

        public DataContract GetDataContractForType(Type type)
        {
            if (type.IsOneOf(typeof(object), typeof(JsonDocument), typeof(JsonElement)))
            {
                return DataContract.ForDynamic(
                    underlyingType: type,
                    jsonConverter: JsonConverterFunc);
            }

            if (s_primitiveTypesAndFormats.ContainsKey(type))
            {
                var primitiveTypeAndFormat = s_primitiveTypesAndFormats[type];

                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2,
                    jsonConverter: JsonConverterFunc);
            }

            if (type.IsEnum)
            {
                var enumValues = type.GetEnumValues();

                //Test to determine if the serializer will treat as string
                var serializeAsString = (enumValues.Length > 0)
                    && JsonConverterFunc(enumValues.GetValue(0)).StartsWith("\"", StringComparison.Ordinal);

                var primitiveTypeAndFormat = serializeAsString
                    ? s_primitiveTypesAndFormats[typeof(string)]
                    : s_primitiveTypesAndFormats[type.GetEnumUnderlyingType()];

                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2,
                    jsonConverter: JsonConverterFunc);
            }

            if (IsSupportedDictionary(type, out Type? keyType, out Type? valueType))
            {
                return DataContract.ForDictionary(
                    underlyingType: type,
                    valueType: valueType,
                    keys: null, // STJ doesn't currently support dictionaries with enum key types
                    jsonConverter: JsonConverterFunc);
            }

            if (IsSupportedCollection(type, out Type? itemType))
            {
                return DataContract.ForArray(
                    underlyingType: type,
                    itemType: itemType,
                    jsonConverter: JsonConverterFunc);
            }

            return DataContract.ForObject(
                underlyingType: type,
                properties: GetDataPropertiesFor(type, out Type? extensionDataType),
                extensionDataType: extensionDataType,
                jsonConverter: JsonConverterFunc);
        }

        private string JsonConverterFunc(object? value)
        {
            return JsonSerializer.Serialize(value, _serializerOptions);
        }

        public bool IsSupportedDictionary(Type type, [MaybeNullWhen(false)] out Type keyType, [MaybeNullWhen(false)] out Type valueType)
        {
            if (type.IsConstructedFrom(typeof(IDictionary<,>), out Type constructedType)
                || type.IsConstructedFrom(typeof(IReadOnlyDictionary<,>), out constructedType))
            {
                keyType = constructedType.GenericTypeArguments[0];
                valueType = constructedType.GenericTypeArguments[1];
                return true;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                keyType = valueType = typeof(object);
                return true;
            }

            keyType = valueType = null;
            return false;
        }

        public bool IsSupportedCollection(Type type, [MaybeNullWhen(false)] out Type itemType)
        {
            if (type.IsConstructedFrom(typeof(IEnumerable<>), out Type constructedType))
            {
                itemType = constructedType.GenericTypeArguments[0];
                return true;
            }

            if (type.IsConstructedFrom(typeof(IAsyncEnumerable<>), out constructedType))
            {
                itemType = constructedType.GenericTypeArguments[0];
                return true;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                itemType = typeof(object);
                return true;
            }

            itemType = null;
            return false;
        }

        private IEnumerable<DataProperty> GetDataPropertiesFor(Type objectType, out Type? extensionDataType)
        {
            extensionDataType = null;

            if (!ApiContractSerializer.MetadataProvider.CanSerialize(objectType))
                return Enumerable.Empty<DataProperty>();

            var metaMembers = ApiContractSerializer.MetadataProvider.GetMembers(objectType, out var metaType);
            if (metaType == null)
                return Enumerable.Empty<DataProperty>();

            var dataProperties = new List<DataProperty>();
            var nonNullableContextCache = new Dictionary<Type, bool>();

            foreach (var metaMember in metaMembers)
                if (metaMember.Member is PropertyInfo property)
                {
                    //if (propertyInfo.HasAttribute<JsonExtensionDataAttribute>()
                    //    && propertyInfo.PropertyType.IsConstructedFrom(typeof(IDictionary<,>), out Type constructedDictionary))
                    //{
                    //    extensionDataType = constructedDictionary.GenericTypeArguments[1];
                    //    continue;
                    //}

                    var name = _convertMemberName(metaMember.Name);

                    bool isNullable;
                    if (property.PropertyType.IsValueType)
                        isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null;
                    else if (property.IsNonNullableRefType() is bool isNonNullableRefType)
                        isNullable = !isNonNullableRefType;
                    else if (!property.DeclaringType!.IsGenericType)
                        isNullable = !property.DeclaringType.IsNonNullableContext(nonNullableContextCache);
                    else
                        // for properties declared on generic types determining reference type nullability is too complicated,
                        // so we resort to checking for RequiredAttribute in this case
                        isNullable = !property.GetCustomAttributes<System.ComponentModel.DataAnnotations.RequiredAttribute>().Any();

                    var isReadable = property.IsPubliclyReadable();
                    var isWritable = property.IsPubliclyWritable();

                    //var isSetViaConstructor = property.DeclaringType != null && property.DeclaringType.GetConstructors()
                    //    .SelectMany(c => c.GetParameters())
                    //    .Any(p =>
                    //    {
                    //        // STJ supports setting via constructor if either underlying OR JSON names match
                    //        return
                    //            string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase) ||
                    //            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase);
                    //    });

                    dataProperties.Add(
                        new DataProperty(
                            name: name,
                            isRequired: false,
                            isNullable: isNullable,
                            isReadOnly: isReadable && !isWritable /* && !isSetViaConstructor */,
                            isWriteOnly: isWritable && !isReadable,
                            memberType: property.PropertyType,
                            memberInfo: property));
                }

            return dataProperties;
        }

        private static readonly Dictionary<Type, (DataType, string?)> s_primitiveTypesAndFormats = new Dictionary<Type, (DataType, string?)>
        {
            [typeof(bool)] = (DataType.Boolean, null),
            [typeof(byte)] = (DataType.Integer, "int32"),
            [typeof(sbyte)] = (DataType.Integer, "int32"),
            [typeof(short)] = (DataType.Integer, "int32"),
            [typeof(ushort)] = (DataType.Integer, "int32"),
            [typeof(int)] = (DataType.Integer, "int32"),
            [typeof(uint)] = (DataType.Integer, "int32"),
            [typeof(long)] = (DataType.Integer, "int64"),
            [typeof(ulong)] = (DataType.Integer, "int64"),
            [typeof(float)] = (DataType.Number, "float"),
            [typeof(double)] = (DataType.Number, "double"),
            [typeof(decimal)] = (DataType.Number, "double"),
            [typeof(byte[])] = (DataType.String, "byte"),
            [typeof(string)] = (DataType.String, null),
            [typeof(char)] = (DataType.String, null),
            [typeof(DateTime)] = (DataType.String, "date-time"),
            [typeof(DateTimeOffset)] = (DataType.String, "date-time"),
            [typeof(Guid)] = (DataType.String, "uuid"),
            [typeof(Uri)] = (DataType.String, "uri"),
            // TimeSpan is not supported out-of-the-box by System.Json.Text currently (https://github.com/dotnet/runtime/issues/29932),
            // but Api.Contract defines a converter for this type (see ApiContractSerializer.Configuration)
            [typeof(TimeSpan)] = (DataType.String, "date-span")
        };
    }
}
