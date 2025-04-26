using System.Runtime.CompilerServices;

namespace System.Reflection;

internal static class ReflectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullableOfT(this Type type)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/ReflectionExtensions.cs#L20

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsAssignableFromInternal(this Type type, Type from)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/ReflectionExtensions.cs#L27

        if (from.IsNullableOfT() && type.IsInterface)
            return type.IsAssignableFrom(from.GetGenericArguments()[0]);

        return type.IsAssignableFrom(from);
    }

    public static bool IsInSubtypeRelationshipWith(this Type type, Type other)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/ReflectionExtensions.cs#L40

        return IsAssignableFromInternal(type, other) || IsAssignableFromInternal(other, type);
    }

    public static TAttribute? GetUniqueCustomAttribute<TAttribute>(this MemberInfo memberInfo, bool inherit)
        where TAttribute : Attribute
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/src/System/ReflectionExtensions.cs#L72

        object[] attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), inherit);

        if (attributes.Length == 0)
            return null;

        if (attributes.Length == 1)
            return (TAttribute)attributes[0];

        string location = memberInfo is Type type ? type.ToString() : $"{memberInfo.DeclaringType}.{memberInfo.Name}";
        throw new InvalidOperationException($"The attribute '{typeof(TAttribute)}' cannot exist more than once on '{location}'.");
    }

    public static object? GetDefaultValue(this ParameterInfo parameterInfo)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v8.0.15/src/libraries/System.Text.Json/Common/ReflectionExtensions.cs#L288

        var parameterType = parameterInfo.ParameterType;
        object? defaultValue = parameterInfo.DefaultValue;

        if (defaultValue is null)
            return null;

        // DBNull.Value is sometimes used as the default value (returned by reflection) of nullable params in place of null.
        if (defaultValue == DBNull.Value && parameterType != typeof(DBNull))
            return null;

        // Default values of enums or nullable enums are represented using the underlying type and need to be cast explicitly
        // cf. https://github.com/dotnet/runtime/issues/68647
        if (parameterType.IsEnum)
            return Enum.ToObject(parameterType, defaultValue);

        if (Nullable.GetUnderlyingType(parameterType) is Type underlyingType && underlyingType.IsEnum)
            return Enum.ToObject(underlyingType, defaultValue);

        return defaultValue;
    }
}
