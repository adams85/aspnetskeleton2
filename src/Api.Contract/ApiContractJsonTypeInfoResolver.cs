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
using ProtoBuf.Meta;

namespace WebApp.Api;

/// <summary>
/// A custom resolver that creates serialization contracts based on protobuf-net's <see cref="RuntimeTypeModel"/>.
/// See also <seealso cref="ApiContractSerializer.Json"/>.
/// </summary>
internal sealed class ApiContractJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    // Customizability of serialization contract creation is quite limited in System.Text.Json currently.
    // A bunch of necessary types and methods are kept internal, leaving us with two options:
    // we either copy a lot of code or use reflection to get access to those APIs.
    // This implementation takes the former approach. (A third option would be to implement a source generator
    // but that would be a pretty big task.)

    // TODO: revise this solution when https://github.com/dotnet/runtime/issues/63791 gets resolved.

    private static readonly Action<JsonPropertyInfo, Delegate>? s_setPropertyGetter;
    private static readonly Action<JsonPropertyInfo, Delegate>? s_setPropertySetter;

    [DynamicDependency("SetGetter", typeof(JsonPropertyInfo))]
    [DynamicDependency("SetSetter", typeof(JsonPropertyInfo))]
    static ApiContractJsonTypeInfoResolver()
    {
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
        }
    }

    private readonly ApiContractSerializer.ModelMetadataProvider _modelMetadataProvider;

    public ApiContractJsonTypeInfoResolver(ApiContractSerializer.ModelMetadataProvider modelMetadataProvider)
    {
        _modelMetadataProvider = modelMetadataProvider;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var shouldUseBuiltInContract = IsBasicType(type)
            || ApiContractSerializer.ModelMetadataProvider.IsCollectionType(type) && _modelMetadataProvider.ShouldSerializeAsCollection(type);

        var typeInfo = shouldUseBuiltInContract
            ? base.GetTypeInfo(type, options)
            : CreateObjectTypeInfo(type, options);

        typeInfo.OriginatingResolver = this;
        return typeInfo;
    }

    private bool IsBasicType(Type type)
    {
        // This logic should be kept in sync with
        // https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs

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
            throw new InvalidOperationException($"{actualType} must be declared as serializable. Apply {nameof(DataContractAttribute)} to the type.");
    }

    private JsonTypeInfo CreateObjectTypeInfo(Type type, JsonSerializerOptions options)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Helpers.cs#L38

        CheckSerializable(type);

        var typeHelper = TypeHelper.For(type);

        var converter = GetConverterForType(type, options);
        if (converter != null)
            return typeHelper.CreateValueInfo(options, converter);

        var numberHandling = type.GetUniqueCustomAttribute<JsonNumberHandlingAttribute>(inherit: false)?.Handling;
        var creationHandling = type.GetUniqueCustomAttribute<JsonObjectCreationHandlingAttribute>(inherit: false)?.Handling;
        var unmappedMemberHandling = type.GetUniqueCustomAttribute<JsonUnmappedMemberHandlingAttribute>(inherit: false)?.UnmappedMemberHandling;

        var typeInfo = typeHelper.CreateObjectInfo(options);

        typeInfo.NumberHandling = numberHandling;
        typeInfo.PreferredPropertyObjectCreationHandling = creationHandling;
        typeInfo.UnmappedMemberHandling = unmappedMemberHandling;

        PopulatePolymorphismMetadata(typeInfo);

        PopulateProperties(typeInfo, options);

        return typeInfo;
    }

    private static JsonConverter? GetConverterForType(Type typeToConvert, JsonSerializerOptions options)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs#L143

        // Priority 1: Attempt to get custom converter from the Converters list.
        var converter = GetConverterFromList(typeToConvert, options);

        // Priority 2: Attempt to get converter from [JsonConverter] on the type being converted.
        if (converter == null)
        {
            JsonConverterAttribute? converterAttribute = typeToConvert.GetUniqueCustomAttribute<JsonConverterAttribute>(inherit: false);
            if (converterAttribute != null)
            {
                converter = GetConverterFromAttribute(converterAttribute, typeToConvert: typeToConvert, memberInfo: null, options);
            }
        }

        if (converter != null)
        {
            // Expand if factory converter & validate.
            converter = ExpandConverterFactory(converter, typeToConvert, options);
            if (!converter.Type!.IsInSubtypeRelationshipWith(typeToConvert))
            {
                throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{typeToConvert}'.");
            }

            CheckConverterNullabilityIsSameAsPropertyType(converter, typeToConvert);
        }

        return converter;
    }

    private static JsonConverter? GetCustomConverterForMember(Type typeToConvert, MemberInfo memberInfo, JsonSerializerOptions options)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs#L132

        Debug.Assert(memberInfo is FieldInfo or PropertyInfo);
        Debug.Assert(typeToConvert != null);

        JsonConverterAttribute? converterAttribute = memberInfo.GetUniqueCustomAttribute<JsonConverterAttribute>(inherit: false);
        return converterAttribute is null ? null : GetConverterFromAttribute(converterAttribute, typeToConvert!, memberInfo, options);
    }

    private static JsonConverter? GetConverterFromList(Type typeToConvert, JsonSerializerOptions options)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs#L143

        for (int i = 0, n = options.Converters.Count; i < n; i++)
        {
            var converter = options.Converters[i];
            if (converter != null && converter.CanConvert(typeToConvert))
            {
                return converter;
            }
        }
        return null;
    }

    private static JsonConverter GetConverterFromAttribute(JsonConverterAttribute converterAttribute, Type typeToConvert, MemberInfo? memberInfo, JsonSerializerOptions options)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs#L176

        JsonConverter? converter;

        Type declaringType = memberInfo?.DeclaringType ?? typeToConvert;
        Type? converterType = converterAttribute.ConverterType;
        if (converterType == null)
        {
            // Allow the attribute to create the converter.
            converter = converterAttribute.CreateConverter(typeToConvert);
            if (converter == null)
            {
                throw SerializationConverterOnAttributeNotCompatible(declaringType, memberInfo, typeToConvert);
            }
        }
        else
        {
            ConstructorInfo? ctor = converterType.GetConstructor(Type.EmptyTypes);
            if (!typeof(JsonConverter).IsAssignableFrom(converterType) || ctor == null || !ctor.IsPublic)
            {
                throw SerializationConverterOnAttributeInvalid(declaringType, memberInfo);
            }

            converter = (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        Debug.Assert(converter != null);
        if (!converter!.CanConvert(typeToConvert))
        {
            Type? underlyingType = Nullable.GetUnderlyingType(typeToConvert);
            if (underlyingType != null && converter.CanConvert(underlyingType))
            {
                if (converter is JsonConverterFactory converterFactory)
                {
                    converter = ExpandConverterFactory(converter, underlyingType, options);
                }

                // Allow nullable handling to forward to the underlying type's converter.
                var underlyingTypeHelper = PropertyHelper.For(underlyingType);
                return underlyingTypeHelper.GetNullableConverter(converter, options);
            }

            throw SerializationConverterOnAttributeNotCompatible(declaringType, memberInfo, typeToConvert);
        }

        return converter;

        static InvalidOperationException SerializationConverterOnAttributeNotCompatible(Type declaringType, MemberInfo? memberInfo, Type typeToConvert)
        {
            string location = declaringType.ToString();
            if (memberInfo != null)
                location = location + "." + memberInfo.Name;
            throw new InvalidOperationException($"The converter specified on '{location}' is not compatible with the type '{typeToConvert}'.");
        }

        static InvalidOperationException SerializationConverterOnAttributeInvalid(Type declaringType, MemberInfo? memberInfo)
        {
            string location = declaringType.ToString();
            if (memberInfo != null)
                location = location + "." + memberInfo.Name;
            throw new InvalidOperationException($"The converter specified on '{location}' does not derive from {nameof(JsonConverter)} or have a public parameterless constructor.");
        }
    }

    private static JsonConverter ExpandConverterFactory(JsonConverter converter, Type typeToConvert, JsonSerializerOptions options)
    {
        if (converter is JsonConverterFactory factory)
        {
            converter = factory.CreateConverter(typeToConvert, options)!;
            if (converter == null || converter is JsonConverterFactory)
            {
                throw new InvalidOperationException($"The converter '{factory.GetType()}' cannot return null or a {nameof(JsonConverterFactory)} instance.");
            }
        }
        return converter;
    }

    private static Func<JsonParameterInfoValues[]>? GetObjectCreationInfo(Type typeToConvert, out ConstructorInfo? constructor, out ParameterInfo[]? parameters)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Converters/Object/ObjectConverterFactory.cs#L41

        var useDefaultConstructorInUnannotatedStructs = !(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(KeyValuePair<,>));
        if (!TryGetDeserializationConstructor(typeToConvert, useDefaultConstructorInUnannotatedStructs, out constructor))
        {
            throw new InvalidOperationException($"The type '{typeToConvert}' cannot have more than one member that has the attribute '{typeof(JsonConstructorAttribute)}'.");
        }

        if (constructor == null || typeToConvert.IsAbstract || (parameters = constructor.GetParameters()).Length == 0)
        {
            parameters = null;
            return null;
        }

        var parameterInfoValues = GetParameterInfoValues(constructor, parameters);
        return () => parameterInfoValues;
    }

    private static bool TryGetDeserializationConstructor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type,
        bool useDefaultCtorInAnnotatedStructs,
        out ConstructorInfo? deserializationCtor)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Converters.cs#L143

        ConstructorInfo? ctorWithAttribute = null;
        ConstructorInfo? publicParameterlessCtor = null;
        ConstructorInfo? lonePublicCtor = null;

        ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        if (constructors.Length == 1)
        {
            lonePublicCtor = constructors[0];
        }

        foreach (ConstructorInfo constructor in constructors)
        {
            if (constructor.GetCustomAttribute<JsonConstructorAttribute>() != null)
            {
                if (ctorWithAttribute != null)
                {
                    deserializationCtor = null;
                    return false;
                }

                ctorWithAttribute = constructor;
            }
            else if (constructor.GetParameters().Length == 0)
            {
                publicParameterlessCtor = constructor;
            }
        }

        // Search for non-public ctors with [JsonConstructor].
        foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (constructor.GetCustomAttribute<JsonConstructorAttribute>() != null)
            {
                if (ctorWithAttribute != null)
                {
                    deserializationCtor = null;
                    return false;
                }

                ctorWithAttribute = constructor;
            }
        }

        // Structs will use default constructor if attribute isn't used.
        if (useDefaultCtorInAnnotatedStructs && type.IsValueType && ctorWithAttribute == null)
        {
            deserializationCtor = null;
            return true;
        }

        deserializationCtor = ctorWithAttribute ?? publicParameterlessCtor ?? lonePublicCtor;
        return true;
    }

    private static JsonParameterInfoValues[] GetParameterInfoValues(ConstructorInfo constructor, ParameterInfo[] parameters)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Helpers.cs#L263

        int parameterCount = parameters.Length;
        JsonParameterInfoValues[] jsonParameters = new JsonParameterInfoValues[parameterCount];

        for (int i = 0; i < parameterCount; i++)
        {
            ParameterInfo reflectionInfo = parameters[i];

            // Trimmed parameter names are reported as null in CoreCLR or "" in Mono.
            if (string.IsNullOrEmpty(reflectionInfo.Name))
            {
                Debug.Assert(constructor.DeclaringType != null);
                throw new NotSupportedException($"The deserialization constructor for type '{constructor.DeclaringType}' contains parameters with null names. This might happen because the parameter names have been trimmed by ILLink. Consider using the source generated serializer instead.");
            }

            JsonParameterInfoValues jsonInfo = new()
            {
                Name = reflectionInfo.Name,
                ParameterType = reflectionInfo.ParameterType,
                Position = reflectionInfo.Position,
                HasDefaultValue = reflectionInfo.HasDefaultValue,
                DefaultValue = reflectionInfo.GetDefaultValue()
            };

            jsonParameters[i] = jsonInfo;
        }

        return jsonParameters;
    }

    private static Func<T> CreateFactoryFromParameterlessCtorCodegen<T>(ConstructorInfo? constructor)
    {
        var createInstance = constructor != null ? Expression.New(constructor) : Expression.New(typeof(T));
        var lambda = Expression.Lambda<Func<T>>(createInstance);
        return lambda.Compile();
    }

    private static Func<T> CreateFactoryFromParameterlessCtorReflection<T>(ConstructorInfo? constructor)
    {
        return constructor != null
            ? () =>
            {
                object? instance = null;
                try { instance = (T)constructor.Invoke(null); }
                catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); }
                return (T)instance!;
            }
        : Activator.CreateInstance<T>;
    }

    private static Func<object[], T> CreateFactoryFromParameterizedCtorCodegen<T>(ConstructorInfo constructor, ParameterInfo[] parameters)
    {
        var ctorParamsParam = Expression.Parameter(typeof(object[]));
        var ctorParams = new Expression[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var expression = Expression.ArrayAccess(ctorParamsParam, Expression.Constant(i));
            var paramType = parameters[i].ParameterType;
            // NOTE: By default, STJ doesn't work with by-ref params, so we don't care about them either.
            ctorParams[i] = paramType.IsValueType
                ? Expression.Unbox(expression, paramType)
                : Expression.Convert(expression, paramType);
        }

        var createInstance = Expression.New(constructor, ctorParams);

        var lambda = Expression.Lambda<Func<object[], T>>(createInstance, ctorParamsParam);
        return lambda.Compile();
    }

    private static Func<object[], T> CreateFactoryFromParameterizedCtorReflection<T>(ConstructorInfo constructor, ParameterInfo[] parameters)
    {
        return (@params) =>
        {
            if (@params.Length > parameters.Length)
                Array.Resize(ref @params, parameters.Length);

            object? instance = null;
            try { instance = (T)constructor.Invoke(@params); }
            catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); }
            return (T)instance!;
        };
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

            var customConverter = GetCustomConverterForMember(member.MemberType, member.Member, options);
            var jsonPropertyInfo = typeInfo.CreateJsonPropertyInfo(member.MemberType, member.Name);
            PopulatePropertyInfo(jsonPropertyInfo, member, customConverter);

            properties.Add(jsonPropertyInfo);
        }
    }

    private void PopulatePropertyInfo(JsonPropertyInfo jsonPropertyInfo, ValueMember member, JsonConverter? customConverter)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Helpers.cs#L298

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
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Helpers.cs#L298

        var numberHandlingAttr = member.Member.GetCustomAttribute<JsonNumberHandlingAttribute>(inherit: false);
        propertyInfo.NumberHandling = numberHandlingAttr?.Handling;

        var objectCreationHandlingAttr = member.Member.GetCustomAttribute<JsonObjectCreationHandlingAttribute>(inherit: false);
        propertyInfo.ObjectCreationHandling = objectCreationHandlingAttr?.Handling;
    }

    private static void DeterminePropertyName(JsonPropertyInfo propertyInfo, ValueMember member)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Metadata/DefaultJsonTypeInfoResolver.Helpers.cs#L350

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

    private static void CheckConverterNullabilityIsSameAsPropertyType(JsonConverter converter, Type propertyType)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.Converters.cs#L97

        if (propertyType.IsValueType && (converter.Type?.IsValueType ?? false)
            && (propertyType.IsNullableOfT() ^ converter.Type!.IsNullableOfT()))
        {
            throw new InvalidOperationException($"The converter '{converter.GetType()}' handles type '{converter.Type}' but is being asked to convert type '{propertyType}'. Either create a separate converter for type '{propertyType}' or change the converter's 'CanConvert' method to only return 'true' for a single type.");
        }
    }

    internal abstract class TypeHelper
    {
        public static TypeHelper For(Type type)
        {
            return (TypeHelper)Activator.CreateInstance(typeof(TypeHelper<>).MakeGenericType(type));
        }

        protected TypeHelper() { }

        public abstract JsonTypeInfo CreateObjectInfo(JsonSerializerOptions options);

        public abstract JsonTypeInfo CreateValueInfo(JsonSerializerOptions options, JsonConverter converter);
    }

    internal sealed class TypeHelper<T> : TypeHelper where T : notnull
    {
        public override JsonTypeInfo CreateObjectInfo(JsonSerializerOptions options)
        {
            var constructorParameterMetadataInitializer = GetObjectCreationInfo(typeof(T), out var constructor, out var parameters);

            Func<T>? objectCreator;
            Func<object[], T>? objectWithParameterizedConstructorCreator = null;
            if (parameters == null)
            {
                objectCreator =
                    typeof(T).IsAbstract ? null
                    : ApiContractSerializer.FrozenOptions.AllowDynamicCodeGeneration ? CreateFactoryFromParameterlessCtorCodegen<T>(constructor)
                    : CreateFactoryFromParameterlessCtorReflection<T>(constructor);

                objectWithParameterizedConstructorCreator = null;
            }
            else
            {
                objectCreator = null;
                objectWithParameterizedConstructorCreator = ApiContractSerializer.FrozenOptions.AllowDynamicCodeGeneration
                    ? CreateFactoryFromParameterizedCtorCodegen<T>(constructor!, parameters)
                    : CreateFactoryFromParameterizedCtorReflection<T>(constructor!, parameters);
            }

            var objectInfoValues = new JsonObjectInfoValues<T>
            {
                ObjectCreator = objectCreator,
                ObjectWithParameterizedConstructorCreator = objectWithParameterizedConstructorCreator,
                PropertyMetadataInitializer = delegate { return Array.Empty<JsonPropertyInfo>(); },
                ConstructorParameterMetadataInitializer = constructorParameterMetadataInitializer
            };

            return JsonMetadataServices.CreateObjectInfo<T>(options, objectInfoValues);
        }

        public override JsonTypeInfo CreateValueInfo(JsonSerializerOptions options, JsonConverter converter)
        {
            return JsonMetadataServices.CreateValueInfo<T>(options, converter);
        }
    }

    internal abstract class PropertyHelper
    {
        public static PropertyHelper For(Type type)
        {
            return (PropertyHelper)Activator.CreateInstance(typeof(PropertyHelper<>).MakeGenericType(type));
        }

        protected PropertyHelper() { }

        public abstract JsonConverter GetNullableConverter(JsonConverter converter, JsonSerializerOptions options);
    }

    internal sealed class PropertyHelper<T> : PropertyHelper where T : struct
    {
        public override JsonConverter GetNullableConverter(JsonConverter converter, JsonSerializerOptions options)
        {
            var typeInfo = JsonMetadataServices.CreateValueInfo<T>(options, converter);
            return JsonMetadataServices.GetNullableConverter<T>(typeInfo);
        }
    }
}
