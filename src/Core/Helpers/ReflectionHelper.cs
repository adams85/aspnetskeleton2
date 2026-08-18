using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Karambolo.Common;

namespace WebApp.Core.Helpers;

public static class ReflectionHelper
{
    #region Nullable reference types

    // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md

    private const string MaybeNullAttributeFullName = "System.Diagnostics.CodeAnalysis.MaybeNullAttribute";
    private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
    private const string NullableContextAttributeFullName = "System.Runtime.CompilerServices.NullableContextAttribute";

    private static readonly ConcurrentDictionary<Type, Delegate?> s_nullableFlagsAccessorCache = new();

    private static Func<Attribute, byte[]>? GetNullableAttributeFlagsAccessor(Type attributeType) =>
        (Func<Attribute, byte[]>?)s_nullableFlagsAccessorCache.GetOrAdd(attributeType, type =>
        {
            var field = attributeType.GetField("NullableFlags");
            return field is not null && field.FieldType == typeof(byte[]) ? field.MakeFastGetter<Attribute, byte[]>() : null;
        });

    public static bool? IsNonNullableRefType(this MemberInfo member)
    {
        switch (member)
        {
            case FieldInfo field:
                if (field.FieldType.IsValueType
                    || field.CustomAttributes.Any(attr => attr.AttributeType.FullName == MaybeNullAttributeFullName))
                {
                    return false;
                }

                break;
            case PropertyInfo property:
                if (property.PropertyType.IsValueType
                    || property.GetMethod?.ReturnParameter?.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == MaybeNullAttributeFullName) is not null)
                {
                    return false;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(member));
        }

        var attribute = Attribute.GetCustomAttributes(member).FirstOrDefault(attr => attr.GetType().FullName is NullableAttributeFullName);
        Func<Attribute, byte[]>? flagsAccessor;
        byte[]? flags;
        if (attribute is not null && (flagsAccessor = GetNullableAttributeFlagsAccessor(attribute.GetType())) is not null && (flags = flagsAccessor(attribute)) is not null)
            return flags.FirstOrDefault() == 1;

        return null;
    }

    private static Func<Attribute, byte>? GetNullableContextAttributeFlagAccessor(Type attributeType) =>
        (Func<Attribute, byte>?)s_nullableFlagsAccessorCache.GetOrAdd(attributeType, type =>
        {
            var field = attributeType.GetField("Flag");
            return field is not null && field.FieldType == typeof(byte) ? field.MakeFastGetter<Attribute, byte>() : null;
        });

    private static bool IsNonNullableContext(this Type type)
    {
        Attribute? attribute;
        Func<Attribute, byte>? flagAccessor;

        var currentType = type;
        do
        {
            attribute = Attribute.GetCustomAttributes(currentType).FirstOrDefault(attr => attr.GetType().FullName is NullableContextAttributeFullName);
            if (attribute is not null && (flagAccessor = GetNullableContextAttributeFlagAccessor(attribute.GetType())) is not null)
                return flagAccessor(attribute) == 1;
        }
        while ((currentType = currentType!.DeclaringType) is not null);

        attribute = Attribute.GetCustomAttributes(type.Module).FirstOrDefault(attr => attr.GetType().FullName is NullableContextAttributeFullName);
        if (attribute is not null && (flagAccessor = GetNullableContextAttributeFlagAccessor(attribute.GetType())) is not null)
            return flagAccessor(attribute) == 1;

        return false;
    }

    public static bool IsNonNullableContext(this Type type, IDictionary<Type, bool> cache)
    {
        if (type is null)
            return false;

        if (!cache.TryGetValue(type, out var result))
            cache[type] = result = type.IsNonNullableContext();

        return result;
    }

    #endregion
}
