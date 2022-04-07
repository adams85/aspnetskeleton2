using System;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf.Meta;

namespace WebApp.Api;

public partial class ApiContractSerializer
{
    public static RuntimeTypeModel ConfigureApiDefaults(this RuntimeTypeModel typeModel)
    {
        typeModel.Add(typeof(NetworkCredential), applyDefaultBehaviour: false)
            .Add(1, nameof(NetworkCredential.Domain))
            .Add(2, nameof(NetworkCredential.UserName))
            .Add(3, nameof(NetworkCredential.Password));

        return typeModel;
    }

    public static JsonSerializerOptions ConfigureApiDefaults(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());

        // TODO: revise this approach when upgrading to .NET 7+ (see https://github.com/dotnet/runtime/issues/1562)
        options.Converters.Add(new ApiObjectJsonConverterFactory(MetadataProvider));

        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;

        return options;
    }
}

