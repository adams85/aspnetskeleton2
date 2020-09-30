using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebApp.Core.Helpers
{
    public static class EnumMetadata
    {
        public static EnumMetadata<TEnum>? For<TEnum>(string name) where TEnum : struct, Enum =>
            EnumMetadata<TEnum>.Cache.TryGetValue(name, out var enumInfo) ? enumInfo : null;

        public static EnumMetadata<TEnum>? For<TEnum>(TEnum value) where TEnum : struct, Enum =>
            For<TEnum>(value.ToString());
    }

    public sealed class EnumMetadata<TEnum> where TEnum : struct, Enum
    {
        public static readonly IReadOnlyDictionary<string, EnumMetadata<TEnum>> Cache = Enum.GetNames(typeof(TEnum))
            .Select(name => (name, field: typeof(TEnum).GetField(name)))
            .Where(item => item.field != null)
            .ToDictionary(item => item.name, item => new EnumMetadata<TEnum>(item.name, item.field!));

        private EnumMetadata(string name, FieldInfo field)
        {
            Name = name;
            Value = (TEnum)field.GetValue(null)!;
            Attributes = Attribute.GetCustomAttributes(field, typeof(Attribute));
        }

        public string Name { get; }
        public TEnum Value { get; }
        public IReadOnlyList<Attribute> Attributes { get; }
    }
}
