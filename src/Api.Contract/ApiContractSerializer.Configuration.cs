using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf.Meta;
using WebApp.Service;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Api;

public partial class ApiContractSerializer
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(NetworkCredential))]
    internal static readonly IReadOnlyDictionary<Type, Action<MetaType>> AdditionalDataContracts = new Dictionary<Type, Action<MetaType>>
    {
        [typeof(NetworkCredential)] = metaType => metaType
            .Add(1, nameof(NetworkCredential.Domain))
            .Add(2, nameof(NetworkCredential.UserName))
            .Add(3, nameof(NetworkCredential.Password))
    };

    private static void EnsureAotGenericInstantiations()
    {
        // This method is never called, it's just a workaround for DynamicDependency not being able to handle generic type names.
        // See also: 
        // * https://github.com/dotnet/runtime/issues/95058
        // * https://github.com/dotnet/runtime/issues/71625#issuecomment-1174588306
        // * https://github.com/dotnet/runtime/issues/95058#issuecomment-1822398349

        new ServiceErrorArgData<string>();
        new ServiceErrorArgData<char>();
        new ServiceErrorArgData<bool>();
        new ServiceErrorArgData<sbyte>();
        new ServiceErrorArgData<byte>();
        new ServiceErrorArgData<short>();
        new ServiceErrorArgData<ushort>();
        new ServiceErrorArgData<int>();
        new ServiceErrorArgData<uint>();
        new ServiceErrorArgData<long>();
        new ServiceErrorArgData<ulong>();
        new ServiceErrorArgData<float>();
        new ServiceErrorArgData<double>();
        new ServiceErrorArgData<decimal>();
        new ServiceErrorArgData<Uri>();
        new ServiceErrorArgData<Guid>();
        new ServiceErrorArgData<TimeSpan>();
        new ServiceErrorArgData<DateTime>();
        new ServiceErrorArgData<PasswordRequirementsData>();

        new KeyData<string>();
        new KeyData<int>();
        new KeyData<long>();
        new KeyData<Guid>();

        // Value type DTOs should be avoided as they won't work with Native AOT out of the box.
        // If value type DTOs are still necessary, generic instantiation of JsonConverter for those types should be listed here. E.g.:
        // Type.GetType("System.Text.Json.Serialization.Converters.ObjectDefaultConverter`1[[WebApp.Service.StructDTO, WebApp.Service.Contract]], System.Text.Json", throwOnError: true);
    }

    public static RuntimeTypeModel ConfigureApiDefaults(this RuntimeTypeModel typeModel)
    {
        foreach (var kvp in AdditionalDataContracts)
            kvp.Value(typeModel.Add(kvp.Key, applyDefaultBehaviour: false));

        return typeModel;
    }

    [DynamicDependency(nameof(EnsureAotGenericInstantiations), typeof(ApiContractSerializer))]
    public static JsonSerializerOptions ConfigureApiDefaults(this JsonSerializerOptions options)
    {
        options.TypeInfoResolver = new ApiContractJsonTypeInfoResolver(MetadataProvider);

        options.Converters.Add(new JsonStringEnumConverter());

        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;

        return options;
    }
}

