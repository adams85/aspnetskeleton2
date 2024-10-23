using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ProtoBuf;
using ProtoBuf.Meta;

namespace WebApp.Api;

/// <summary>
/// A custom resolver that creates serialization contracts based on protobuf-net's <see cref="RuntimeTypeModel"/>.
/// See also <seealso cref="ApiContractSerializer.Json"/>.
/// </summary>
// Based on: https://github.com/dotnet/runtime/blob/v8.0.10/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Helpers.cs
internal sealed class ApiContractJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private static readonly JsonConverterFactory? s_objectConverterFactory;
    private static readonly Func<Type, JsonConverter, JsonSerializerOptions, JsonTypeInfo>? s_createTypeInfo;
    private static readonly Action<JsonPropertyInfo, Delegate>? s_setPropertyGetter;
    private static readonly Action<JsonPropertyInfo, Delegate>? s_setPropertySetter;
    private static readonly Func<Type, MemberInfo, JsonSerializerOptions, JsonConverter?>? s_getCustomConverterForMember;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, "System.Text.Json.Serialization.Converters.ObjectConverterFactory", "System.Text.Json")]
    [DynamicDependency("CreateJsonTypeInfo", typeof(JsonTypeInfo))]
    [DynamicDependency("GetCustomConverterForMember", typeof(DefaultJsonTypeInfoResolver))]
    [DynamicDependency("SetGetter", typeof(JsonPropertyInfo))]
    [DynamicDependency("SetSetter", typeof(JsonPropertyInfo))]
    static ApiContractJsonTypeInfoResolver()
    {
        // Customizability of serialization contract creation is quite limited in System.Text.Json currently.
        // A bunch of necessary types and methods are kept internal, leaving us with two options:
        // we either copy a LOT of code or use reflection to get access to those APIs. In the hope that
        // the situation around API accessibility will improve, we use reflection for now. (Alternatively,
        // we can consider implementing a source generator but that seems a pretty big task.)

        // TODO: remove this workaround when https://github.com/dotnet/runtime/issues/63791 gets resolved.
        var objectConverterFactoryType = typeof(JsonConverterFactory).Assembly
            .GetType("System.Text.Json.Serialization.Converters.ObjectConverterFactory", throwOnError: false);
        var objectConverterFactoryCtor = objectConverterFactoryType?.GetConstructor(new[] { typeof(bool) });

        var createJsonTypeInfoMethod = typeof(JsonTypeInfo)
    .       GetMethod("CreateJsonTypeInfo", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(JsonConverter), typeof(JsonSerializerOptions) }, null);
        if (createJsonTypeInfoMethod.ReturnType != typeof(JsonTypeInfo))
            createJsonTypeInfoMethod = null;

        var getCustomConverterForMemberMethod = typeof(DefaultJsonTypeInfoResolver)
            .GetMethod("GetCustomConverterForMember", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(MemberInfo), typeof(JsonSerializerOptions) }, null);
        if (getCustomConverterForMemberMethod.ReturnType != typeof(JsonConverter))
            getCustomConverterForMemberMethod = null;

        Debug.Assert(objectConverterFactoryCtor != null && createJsonTypeInfoMethod != null && getCustomConverterForMemberMethod != null, "System.Text.Json internals have apparently changed.");

        if (ApiContractSerializer.FrozenOptions.AllowDynamicCodeGeneration)
        {
            var paramTypes = new[] { typeof(Delegate) };
            var setGetterMethod = typeof(JsonPropertyInfo).GetMethod("SetGetter", BindingFlags.Instance | BindingFlags.NonPublic, null, paramTypes, null);
            var setSetterMethod = typeof(JsonPropertyInfo).GetMethod("SetSetter", BindingFlags.Instance | BindingFlags.NonPublic, null, paramTypes, null);

            Debug.Assert(setGetterMethod != null && setSetterMethod != null, "System.Text.Json internals have apparently changed.");

            if (setGetterMethod != null && setSetterMethod != null)
            {
                s_setPropertyGetter = (Action<JsonPropertyInfo, Delegate>)Delegate.CreateDelegate(typeof(Action<JsonPropertyInfo, Delegate>), setGetterMethod);
                s_setPropertySetter = (Action<JsonPropertyInfo, Delegate>)Delegate.CreateDelegate(typeof(Action<JsonPropertyInfo, Delegate>), setSetterMethod);
            }

            if (createJsonTypeInfoMethod != null)
            {
                s_createTypeInfo = (Func<Type, JsonConverter, JsonSerializerOptions, JsonTypeInfo>)Delegate.CreateDelegate(
                    typeof(Func<Type, JsonConverter, JsonSerializerOptions, JsonTypeInfo>), createJsonTypeInfoMethod);
            }

            if (getCustomConverterForMemberMethod != null)
            {
                s_getCustomConverterForMember = (Func<Type, MemberInfo, JsonSerializerOptions, JsonConverter?>)Delegate.CreateDelegate(
                    typeof(Func<Type, MemberInfo, JsonSerializerOptions, JsonConverter?>), getCustomConverterForMemberMethod);
            }
        }
        else
        {
            if (createJsonTypeInfoMethod != null)
            {
                s_createTypeInfo = (type, converter, options) =>
                {
                    JsonTypeInfo? typeInfo = null;
                    try { typeInfo = (JsonTypeInfo)createJsonTypeInfoMethod.Invoke(null, new object[] { type, converter, options }); }
                    catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); }
                    return typeInfo!;
                };
            }

            if (getCustomConverterForMemberMethod != null)
            {
                s_getCustomConverterForMember = (type, member, options) =>
                {
                    JsonConverter? converter = null;
                    try { converter = (JsonConverter?)getCustomConverterForMemberMethod.Invoke(null, new object[] { type, member, options }); }
                    catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); }
                    return converter;
                };
            }
        }

        if (objectConverterFactoryCtor != null)
            s_objectConverterFactory = (JsonConverterFactory)objectConverterFactoryCtor.Invoke(new object[] { true });
    }

    private readonly ApiContractSerializer.ModelMetadataProvider _modelMetadataProvider;
    private readonly DefaultJsonTypeInfoResolver _defaultResolver;

    public ApiContractJsonTypeInfoResolver(ApiContractSerializer.ModelMetadataProvider modelMetadataProvider)
    {
        _modelMetadataProvider = modelMetadataProvider;
        _defaultResolver = new DefaultJsonTypeInfoResolver();
    }

    public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonSerializerOptions optionsForDefaultResolver = options;
        var shouldUseBuiltInContract = IsBasicType(type);
        if (!shouldUseBuiltInContract && ApiContractSerializer.ModelMetadataProvider.IsCollectionType(type))
        {
            if (_modelMetadataProvider.ShouldSerializeAsCollection(type))
            {
                shouldUseBuiltInContract = true;
            }
            else
            {
                if (s_objectConverterFactory == null)
                    throw new TypeLoadException(string.Format("Could not resolve type '{0}' in assembly '{1}'.", "System.Text.Json.Serialization.Converters.ObjectConverterFactory", typeof(JsonConverterFactory).Assembly.FullName));

                optionsForDefaultResolver = new JsonSerializerOptions(options);

                // Force serialization as object.
                // See also: https://github.com/dotnet/runtime/blob/v8.0.10/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs#L148
                optionsForDefaultResolver.Converters.Insert(0, s_objectConverterFactory);
            }
        }

        var typeInfo = _defaultResolver.GetTypeInfo(type, optionsForDefaultResolver);
        typeInfo.OriginatingResolver = this;

        if (shouldUseBuiltInContract)
            return typeInfo;

        if (!ReferenceEquals(options, optionsForDefaultResolver))
        {
            if (s_createTypeInfo == null)
                throw new MissingMethodException(string.Format("Member '{0}.{1}' not found.", typeof(JsonTypeInfo), "CreateJsonTypeInfo"));

            var originalTypeInfo = typeInfo;
            typeInfo = s_createTypeInfo(originalTypeInfo.Type, originalTypeInfo.Converter, options);
            typeInfo.CreateObject = originalTypeInfo.CreateObject;
            typeInfo.NumberHandling = originalTypeInfo.NumberHandling;
            typeInfo.OnDeserialized = originalTypeInfo.OnDeserialized;
            typeInfo.OnDeserializing = originalTypeInfo.OnDeserializing;
            typeInfo.OnSerialized = originalTypeInfo.OnSerialized;
            typeInfo.OnSerializing = originalTypeInfo.OnSerializing;
            typeInfo.PreferredPropertyObjectCreationHandling = originalTypeInfo.PreferredPropertyObjectCreationHandling;
            typeInfo.UnmappedMemberHandling = originalTypeInfo.UnmappedMemberHandling;
        }

        CheckSerializable(type);

        PopulatePolymorphismMetadata(typeInfo);

        PopulateProperties(typeInfo, options);

        return typeInfo;
    }

    private bool IsBasicType(Type type)
    {
        // This logic should be kept in sync with
        // https://github.com/dotnet/runtime/blob/v8.0.10/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs

        if (type.IsInterface)
            return false;

        if (!type.IsValueType)
        {
            return type == typeof(object)
                || type == typeof(ValueType)
                || type == typeof(Enum)
                || type == typeof(Delegate)
                || type == typeof(string)
                || type == typeof(Uri)
                || type == typeof(Version)
                || type == typeof(JsonDocument)
                || type == typeof(JsonNode)
                || type.IsSubclassOf(typeof(JsonNode));
        }

        if (type.IsGenericType)
        {
            type = type.GetGenericTypeDefinition();
            return type == typeof(Nullable<>)
                || type == typeof(KeyValuePair<,>)
                || type == typeof(Memory<>)
                || type == typeof(ReadOnlyMemory<>);
        }
        else
        {
            return type.IsPrimitive
                || type.IsEnum
#if NET7_0_OR_GREATER
                || type == typeof(Int128)
                || type == typeof(UInt128)
#endif
                || type == typeof(decimal)
#if NET5_0_OR_GREATER
                || type == typeof(Half)
#endif
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
#if NET6_0_OR_GREATER
                || type == typeof(DateOnly)
                || type == typeof(TimeOnly)
#endif
                || type == typeof(TimeSpan)
                || type == typeof(Guid)
                || type == typeof(JsonElement);
        }
    }

    private void CheckSerializable(Type actualType)
    {
        var canSerialize = ApiContractSerializer.FrozenOptions.AllowDynamicCodeGeneration
            ? _modelMetadataProvider.CanSerialize(actualType)
            // protobuf-net's TypeModel.CanSerialize doesn't work with trimming,
            // so we use this approximation of ModelMetadataProvider.CanSerialize as a workaround.
            : actualType.GetCustomAttribute<DataContractAttribute>() != null || ApiContractSerializer.AdditionalDataContracts.ContainsKey(actualType);

        if (!canSerialize)
            throw new JsonException($"{actualType} must be declared as serializable. Apply {nameof(DataContractAttribute)} on the type.");
    }

    private void PopulatePolymorphismMetadata(JsonTypeInfo typeInfo)
    {
        typeInfo.PolymorphismOptions = null;

        var subTypes = _modelMetadataProvider.GetSubTypes(typeInfo.Type);
        if (subTypes is IReadOnlyCollection<Type> { Count: 0 })
            return;

        JsonPolymorphismOptions? polymorphismOptions = null;

        foreach (var subType in subTypes)
        {
            if (subType.IsAbstract)
                continue;

            polymorphismOptions ??= new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = ApiContractSerializer.JsonTypeDiscriminatorPropertyName
            };

            polymorphismOptions.DerivedTypes.Add(new JsonDerivedType(subType, ApiContractSerializer.FrozenOptions.TypeNameFormatter(subType)));
        }

        typeInfo.PolymorphismOptions = polymorphismOptions;
    }

    private void PopulateProperties(JsonTypeInfo typeInfo, JsonSerializerOptions options)
    {
        typeInfo.Properties.Clear();

        var members = _modelMetadataProvider.GetMembers(typeInfo.Type, out var metaType);
        if (metaType == null)
            return;

        var properties = typeInfo.Properties;
        var propertySet = new HashSet<string>();
        foreach (var member in members)
        {
            // Prevent adding duplicate properties when the member is shadowed by a subclass.
            // NOTE: This works because subclass members are enumerated before superclass members.
            if (!propertySet.Add(member.Name))
                continue;

            var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo(member.MemberType, member.Name);
            JsonConverter? customConverter;
            if (member.Member.GetCustomAttributes(typeof(JsonConverterAttribute), inherit: false).Length == 0)
                customConverter = null;
            else if (s_getCustomConverterForMember != null)
                customConverter = s_getCustomConverterForMember(member.MemberType, member.Member, options);
            else
                throw new MissingMethodException(string.Format("Member '{0}.{1}' not found.", typeof(DefaultJsonTypeInfoResolver), "GetCustomConverterForMember"));
            PopulatePropertyInfo(jsonPropertyInfo, member, customConverter);

            properties.Add(jsonPropertyInfo);
        }
    }

    private void PopulatePropertyInfo(JsonPropertyInfo jsonPropertyInfo, ValueMember member, JsonConverter? customConverter)
    {
        jsonPropertyInfo.CustomConverter = customConverter;

        DeterminePropertyPolicies(jsonPropertyInfo, member);
        DeterminePropertyName(jsonPropertyInfo, member);
        DeterminePropertyIsRequired(jsonPropertyInfo, member);

        if (ApiContractSerializer.FrozenOptions.AllowDynamicCodeGeneration && s_setPropertyGetter != null && s_setPropertySetter != null)
            DeterminePropertyAccessorsCodeGen(jsonPropertyInfo, member);
        else
            DeterminePropertyAccessorsReflection(jsonPropertyInfo, member);
    }

    private static void DeterminePropertyPolicies(JsonPropertyInfo propertyInfo, ValueMember member)
    {
        var numberHandlingAttr = member.Member.GetCustomAttribute<JsonNumberHandlingAttribute>(inherit: false);
        propertyInfo.NumberHandling = numberHandlingAttr?.Handling;

        var objectCreationHandlingAttr = member.Member.GetCustomAttribute<JsonObjectCreationHandlingAttribute>(inherit: false);
        propertyInfo.ObjectCreationHandling = objectCreationHandlingAttr?.Handling;
    }

    private static void DeterminePropertyName(JsonPropertyInfo propertyInfo, ValueMember member)
    {
        var name = propertyInfo.Options.PropertyNamingPolicy != null
            ? propertyInfo.Options.PropertyNamingPolicy.ConvertName(member.Member.Name)
            : member.Member.Name;

        if (name == null)
            throw new InvalidOperationException($"The JSON property name for '{member.ParentType}.{member.Member.Name}' cannot be null.");

        propertyInfo.Name = name;
    }

    private static void DeterminePropertyIsRequired(JsonPropertyInfo propertyInfo, ValueMember member)
    {
        propertyInfo.IsRequired = member.IsRequired;
    }

    private static void DeterminePropertyAccessorsCodeGen(JsonPropertyInfo jsonPropertyInfo, ValueMember member)
    {
        PropertyInfo? property;
        FieldInfo? field = null;

        if ((property = member.Member as PropertyInfo) != null
            || (field = member.Member as FieldInfo) != null)
        {
            var objParam = Expression.Parameter(typeof(object));
            var obj = member.Member.DeclaringType.IsValueType
                ? Expression.Unbox(objParam, member.Member.DeclaringType)
                : Expression.Convert(objParam, member.Member.DeclaringType);

            if (property == null || property.GetGetMethod() != null)
            {
                var body = Expression.MakeMemberAccess(obj, member.Member);
                var lambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(object), member.MemberType), body, objParam);
                var getter = lambda.Compile();
                s_setPropertyGetter!(jsonPropertyInfo, getter);
            }

            if (property == null ? !field!.IsInitOnly : property.GetSetMethod() != null)
            {
                var valueParam = Expression.Parameter(member.MemberType);
                var body = Expression.Assign(Expression.MakeMemberAccess(obj, member.Member), valueParam);
                var lambda = Expression.Lambda(typeof(Action<,>).MakeGenericType(typeof(object), member.MemberType), body, objParam, valueParam);
                var setter = lambda.Compile();
                s_setPropertySetter!(jsonPropertyInfo, setter);
            }
        }
        else
        {
            Debug.Fail($"Invalid MemberInfo type: {member.Member.MemberType}");
        }
    }

    private static void DeterminePropertyAccessorsReflection(JsonPropertyInfo jsonPropertyInfo, ValueMember member)
    {
        if (member.Member is PropertyInfo property)
        {
            if (property.GetGetMethod() is { } getMethod)
                jsonPropertyInfo.Get = obj => getMethod.Invoke(obj, null);

            if (property.GetSetMethod() is { } setMethod)
                jsonPropertyInfo.Set = (obj, value) => setMethod.Invoke(obj, new object?[] { value });
        }
        else if (member.Member is FieldInfo field)
        {
            jsonPropertyInfo.Get = field.GetValue;

            if (!field.IsInitOnly)
                jsonPropertyInfo.Set = field.SetValue;
        }
        else
        {
            Debug.Fail($"Invalid MemberInfo type: {member.Member.MemberType}");
        }
    }
}
