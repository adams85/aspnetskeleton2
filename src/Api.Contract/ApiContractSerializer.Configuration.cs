using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf.Meta;

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

    public static RuntimeTypeModel ConfigureApiDefaults(this RuntimeTypeModel typeModel)
    {
        foreach (var kvp in AdditionalDataContracts)
            kvp.Value(typeModel.Add(kvp.Key, applyDefaultBehaviour: false));

        return typeModel;
    }

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

