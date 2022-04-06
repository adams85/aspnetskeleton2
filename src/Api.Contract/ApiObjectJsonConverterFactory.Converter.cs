using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using ProtoBuf;

namespace WebApp.Api
{
    internal partial class ApiObjectJsonConverterFactory
    {
        private static readonly MethodInfo s_invokeCtorMethodDefinition = new Func<object>(InvokeCtor<object>).Method.GetGenericMethodDefinition();

        private static T InvokeCtor<T>() where T : new() => new T();

        private sealed class Converter<T> : JsonConverter<T>
            where T : notnull
        {
            private const string UnexpectedTokenMessage = "Unexpected token.";

            private static readonly bool s_typeAllowsNull = !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null;

            private readonly ApiObjectJsonConverterFactory _factory;
            private readonly JsonSerializerOptions _options;

            public Converter(ApiObjectJsonConverterFactory factory, JsonSerializerOptions options)
            {
                _factory = factory;
                _options = options;
            }

            public override bool HandleNull => false;

            private IReadOnlyDictionary<string, MemberHelper<T>>? _members;
            private IReadOnlyDictionary<string, MemberHelper<T>> Members =>
                LazyInitializer.EnsureInitialized(ref _members, () =>
                {
                    var convertMemberName = _options.PropertyNamingPolicy != null ? _options.PropertyNamingPolicy.ConvertName : new Func<string, string>(name => name);
                    var memberNameComparer = _options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    return _factory._modelMetadataProvider.GetMembers(typeof(T), out var _)
                        .ToDictionary(metaMember => convertMemberName(metaMember.Name), MemberHelper<T>.Create, memberNameComparer);
                })!;

            private ICollection<Type>? _subTypes;
            private ICollection<Type> SubTypes =>
                LazyInitializer.EnsureInitialized(ref _subTypes, () =>
                {
                    var subTypes = _factory._modelMetadataProvider.GetSubTypes(typeof(T), out var visited);
                    using (var enumerator = subTypes.GetEnumerator())
                        while (enumerator.MoveNext()) { }
                    return visited ?? (ICollection<Type>)Type.EmptyTypes;
                })!;

            private Func<T>? _objectFactory;
            private Func<T> ObjectFactory =>
                LazyInitializer.EnsureInitialized(ref _objectFactory, () =>
                {
                    MethodInfo? ctorMethod;

                    if (!typeof(T).IsValueType)
                        try { ctorMethod = s_invokeCtorMethodDefinition.MakeGenericMethod(typeof(T)); }
                        catch { ctorMethod = null; }
                    else
                        ctorMethod = null;

                    return ctorMethod != null ? (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), ctorMethod) : (() => default!);
                })!;

            private void CheckSerializable(Type actualType)
            {
                if (!_factory._modelMetadataProvider.CanSerialize(actualType))
                    throw new JsonException($"{actualType} must be declared as serializable. Apply {nameof(DataContractAttribute)} on the type.");
            }

            private void CheckAllowedSubType(Type actualType)
            {
                if (!SubTypes.Contains(actualType))
                    throw new JsonException($"{actualType} must be a declared subtype of {typeof(T)}. Apply {nameof(ProtoIncludeAttribute)} on the base type(s).");
            }

            public T ReadCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                var value = ObjectFactory();
                if (value is null)
                    throw new JsonException($"It is not possible to create an instance of {typeof(T)}.");

                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException(UnexpectedTokenMessage);

                    var propertyName = reader.GetString()!;
                    if (Members.TryGetValue(propertyName, out var member))
                        member.Read(ref value, ref reader, options);
                    else
                        reader.Skip();

                    reader.Read();
                }

                return value;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // null tokens should be handled by the framework (as we overridden the HandleNull property to return false)
                Debug.Assert(reader.TokenType != JsonTokenType.Null);

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException(UnexpectedTokenMessage);

                reader.Read();

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.GetString() == ApiContractSerializer.JsonTypePropertyName)
                    {
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException(UnexpectedTokenMessage);

                        var actualType = Type.GetType(reader.GetString(), throwOnError: true);
                        CheckSerializable(actualType);
                        CheckAllowedSubType(actualType);

                        var converter = options.GetConverter(actualType);
                        var typeHelper = _factory.GetTypeHelper(actualType);

                        reader.Read();
                        return (T)typeHelper.ReadSubType(converter, ref reader, options);
                    }
                }
                else if (reader.TokenType != JsonTokenType.EndObject)
                    throw new JsonException(UnexpectedTokenMessage);

                CheckSerializable(typeof(T));
                return ReadCore(ref reader, options);
            }

            public void WriteCore(Utf8JsonWriter writer, T value, JsonSerializerOptions options, bool addType)
            {
                writer.WriteStartObject();

                if (addType)
                    writer.WriteString(ApiContractSerializer.JsonTypePropertyName, ApiContractSerializer.TypeNameFormatter(typeof(T)));

                foreach (var member in Members)
                    member.Value.Write(ref value, member.Key, writer, options);

                writer.WriteEndObject();
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                // null values should be handled by the framework (as we overridden the HandleNull property to return false)
                Debug.Assert(value is not null);

                var actualType = value!.GetType();

                CheckSerializable(actualType);

                if (actualType != typeof(T))
                {
                    CheckAllowedSubType(actualType);

                    var converter = options.GetConverter(actualType);
                    var typeHelper = _factory.GetTypeHelper(actualType);
                    typeHelper.WriteSubType(converter, writer, value, options);
                    return;
                }

                WriteCore(writer, value, options, addType: false);
            }
        }
    }
}
