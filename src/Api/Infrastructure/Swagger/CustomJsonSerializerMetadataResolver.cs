using System;
using System.Collections.Generic;
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
    /// Together with <see cref="CustomModelMetadataProvider"/>, this class is necessary for correct Swagger JSON generation.
    /// </remarks>
    // based on: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v5.4.1/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/JsonSerializerDataContractResolver.cs
    public sealed class CustomJsonSerializerDataContractResolver : IDataContractResolver
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly Func<string, string> _convertMemberName;

        public CustomJsonSerializerDataContractResolver(IOptions<JsonOptions>? jsonOptions)
        {
            _serializerOptions = jsonOptions?.Value?.JsonSerializerOptions ?? new JsonSerializerOptions().ConfigureApiDefaults();
            _convertMemberName = _serializerOptions.PropertyNamingPolicy != null ? _serializerOptions.PropertyNamingPolicy.ConvertName : Identity<string>.Func;
        }

        public DataContract GetDataContractForType(Type type)
        {
            var underlyingType = type.IsNullable(out Type innerType) ? innerType : type;

            if (s_primitiveTypesAndFormats.ContainsKey(underlyingType))
            {
                var primitiveTypeAndFormat = s_primitiveTypesAndFormats[underlyingType];

                return new DataContract(
                    dataType: primitiveTypeAndFormat.Item1,
                    format: primitiveTypeAndFormat.Item2,
                    underlyingType: underlyingType);
            }

            if (underlyingType.IsEnum)
            {
                var enumValues = GetSerializedEnumValuesFor(underlyingType);

                var primitiveTypeAndFormat = (enumValues.Any(value => value is string))
                    ? s_primitiveTypesAndFormats[typeof(string)]
                    : s_primitiveTypesAndFormats[underlyingType.GetEnumUnderlyingType()];

                return new DataContract(
                    dataType: primitiveTypeAndFormat.Item1,
                    format: primitiveTypeAndFormat.Item2,
                    underlyingType: underlyingType,
                    enumValues: enumValues);
            }

            if (underlyingType.IsDictionary(out Type keyType, out Type valueType))
            {
                if (keyType.IsEnum)
                    throw new NotSupportedException(
                        $"Schema cannot be generated for type {underlyingType} as it's not supported by the System.Text.Json serializer");

                return new DataContract(
                    dataType: DataType.Object,
                    underlyingType: underlyingType,
                    additionalPropertiesType: valueType);
            }

            if (underlyingType.IsEnumerable(out Type itemType))
            {
                return new DataContract(
                    dataType: DataType.Array,
                    underlyingType: underlyingType,
                    arrayItemType: itemType);
            }

            return new DataContract(
                dataType: DataType.Object,
                underlyingType: underlyingType,
                properties: GetDataPropertiesFor(underlyingType, out Type extensionDataValueType),
                additionalPropertiesType: extensionDataValueType);
        }

        private IEnumerable<object> GetSerializedEnumValuesFor(Type enumType)
        {
            var underlyingValues = enumType.GetEnumValues().Cast<object>();

            //Test to determine if the serializer will treat as string or not
            var serializeAsString = underlyingValues.Any()
                && JsonSerializer.Serialize(underlyingValues.First(), _serializerOptions).StartsWith("\"");

            return serializeAsString
                ? underlyingValues.Select(value => JsonSerializer.Serialize(value, _serializerOptions).Replace("\"", string.Empty))
                : underlyingValues;
        }

        private IEnumerable<DataProperty>? GetDataPropertiesFor(Type objectType, out Type extensionDataValueType)
        {
            extensionDataValueType = null!;

            if (objectType == typeof(object) || !ApiContractSerializer.MetadataProvider.CanSerialize(objectType))
                return null;

            var metaMembers = ApiContractSerializer.MetadataProvider.GetMembers(objectType, out var metaType);
            if (metaType == null)
                return null;

            var dataProperties = new List<DataProperty>();
            var nonNullableContextCache = new Dictionary<Type, bool>();

            foreach (var metaMember in metaMembers)
                if (metaMember.Member is PropertyInfo property)
                {
                    //if (property.HasAttribute<JsonExtensionDataAttribute>() && property.PropertyType.IsDictionary(out Type _, out Type valueType))
                    //{
                    //    extensionDataValueType = valueType;
                    //    continue;
                    //}

                    var name = _convertMemberName(metaMember.Name);

                    var isReadable = property.IsPubliclyReadable();
                    var isWritable = property.IsPubliclyWritable();

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
                        isNullable = !property.GetAttributes<System.ComponentModel.DataAnnotations.RequiredAttribute>().Any();

                    dataProperties.Add(
                        new DataProperty(
                            name: name,
                            isRequired: false,
                            isNullable: isNullable,
                            isReadOnly: isReadable && !isWritable,
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
