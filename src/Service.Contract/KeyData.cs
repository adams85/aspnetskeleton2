using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service
{
    [DataContract]
    [ProtoInclude(1, typeof(KeyData<string>))]
    [ProtoInclude(2, typeof(KeyData<int>))]
    [ProtoInclude(3, typeof(KeyData<long>))]
    [ProtoInclude(4, typeof(KeyData<Guid>))]
    public abstract class KeyData
    {
        public static KeyData From(object value)
        {
            var key = (KeyData)Activator.CreateInstance(typeof(KeyData<>).MakeGenericType(value.GetType()))!;
            key.ValueInternal = value;
            return key;
        }

        public static KeyData<T> From<T>([DisallowNull] T value) => new KeyData<T> { Value = value };

        protected abstract object ValueInternal { get; set; }

        public object Value
        {
            get => ValueInternal;
            set => ValueInternal = value;
        }
    }

    [DataContract]
    public sealed class KeyData<T> : KeyData
    {
        [NotNull]
        [DataMember(Order = 1)] public new T Value { get; set; } = default!;

        protected override object ValueInternal
        {
            get => Value;
            set => Value = (T)value;
        }
    }
}
