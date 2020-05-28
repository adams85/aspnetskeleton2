using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service
{
    [DataContract]
    [ProtoInclude(1, typeof(ServiceErrorArgData<string>))]
    [ProtoInclude(2, typeof(ServiceErrorArgData<char>))]
    [ProtoInclude(3, typeof(ServiceErrorArgData<bool>))]
    [ProtoInclude(4, typeof(ServiceErrorArgData<sbyte>))]
    [ProtoInclude(5, typeof(ServiceErrorArgData<byte>))]
    [ProtoInclude(6, typeof(ServiceErrorArgData<short>))]
    [ProtoInclude(7, typeof(ServiceErrorArgData<ushort>))]
    [ProtoInclude(8, typeof(ServiceErrorArgData<int>))]
    [ProtoInclude(9, typeof(ServiceErrorArgData<uint>))]
    [ProtoInclude(10, typeof(ServiceErrorArgData<long>))]
    [ProtoInclude(11, typeof(ServiceErrorArgData<ulong>))]
    [ProtoInclude(12, typeof(ServiceErrorArgData<float>))]
    [ProtoInclude(13, typeof(ServiceErrorArgData<double>))]
    [ProtoInclude(14, typeof(ServiceErrorArgData<decimal>))]
    [ProtoInclude(15, typeof(ServiceErrorArgData<Type>))]
    [ProtoInclude(16, typeof(ServiceErrorArgData<Uri>))]
    [ProtoInclude(17, typeof(ServiceErrorArgData<Guid>))]
    [ProtoInclude(18, typeof(ServiceErrorArgData<TimeSpan>))]
    [ProtoInclude(19, typeof(ServiceErrorArgData<DateTime>))]
    public abstract class ServiceErrorArgData
    {
        public static ServiceErrorArgData From(object value)
        {
            var key = (ServiceErrorArgData)Activator.CreateInstance(typeof(ServiceErrorArgData<>).MakeGenericType(value.GetType()))!;
            key.ValueUntyped = value;
            return key;
        }

        public static ServiceErrorArgData<T> From<T>(T value) => new ServiceErrorArgData<T> { Value = value };

        public abstract object ValueUntyped { get; set; }
    }

    [DataContract]
    public class ServiceErrorArgData<T> : ServiceErrorArgData
    {
        [DataMember(Order = 1)] public T Value { get; set; } = default!;

        public override object ValueUntyped
        {
            get => Value!;
            set => Value = (T)value;
        }
    }
}
