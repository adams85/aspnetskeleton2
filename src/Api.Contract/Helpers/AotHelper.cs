using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace WebApp.Api.Helpers;

public static class AotHelper
{
    public static void RootTypeForJsonSerialization<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : notnull
    {
        new ApiContractJsonTypeInfoResolver.TypeHelper<T>();
    }

    public static void RootRefTypeForPropertyJsonSerialization<T>() where T : class
    {
        JsonMetadataServices.CreatePropertyInfo<T>(default(JsonSerializerOptions)!, null!);
    }

    public static void RootValueTypeForPropertyJsonSerialization<T>() where T : struct
    {
        new ApiContractJsonTypeInfoResolver.PropertyHelper<T>();
        JsonMetadataServices.CreatePropertyInfo<T>(default(JsonSerializerOptions)!, null!);
        JsonMetadataServices.CreatePropertyInfo<T?>(default(JsonSerializerOptions)!, null!);
        JsonMetadataServices.GetNullableConverter<T>(default(JsonSerializerOptions)!);
    }

    public static void RootEnumTypeForPropertyJsonSerialization<T>() where T : struct, Enum
    {
        RootValueTypeForPropertyJsonSerialization<T>();
        JsonMetadataServices.GetEnumConverter<T>(default(JsonSerializerOptions)!);
    }
}
