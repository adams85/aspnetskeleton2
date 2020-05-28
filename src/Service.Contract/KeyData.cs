using System;
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
            key.ValueUntyped = value;
            return key;
        }

        public static KeyData<T> From<T>(T value) => new KeyData<T> { Value = value };

        public abstract object ValueUntyped { get; set; }
    }

    [DataContract]
    public sealed class KeyData<T> : KeyData
    {
        [DataMember(Order = 1)] public T Value { get; set; } = default!;

        public override object ValueUntyped
        {
            get => Value!;
            set => Value = (T)value;
        }
    }
}
