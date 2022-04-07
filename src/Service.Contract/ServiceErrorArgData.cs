using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ProtoBuf;
using WebApp.Service.Infrastructure.Validation;

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
    [ProtoInclude(15, typeof(ServiceErrorArgData<Uri>))]
    [ProtoInclude(16, typeof(ServiceErrorArgData<Guid>))]
    [ProtoInclude(17, typeof(ServiceErrorArgData<TimeSpan>))]
    [ProtoInclude(18, typeof(ServiceErrorArgData<DateTime>))]
    [ProtoInclude(101, typeof(ServiceErrorArgData<PasswordRequirementsData>))]
    public abstract record class ServiceErrorArgData
    {
        public static ServiceErrorArgData From(object value)
        {
            var key = (ServiceErrorArgData)Activator.CreateInstance(typeof(ServiceErrorArgData<>).MakeGenericType(value.GetType()))!;
            key.ValueInternal = value;
            return key;
        }

        public static ServiceErrorArgData<T> From<T>([DisallowNull] T value) => new ServiceErrorArgData<T> { Value = value };

        protected abstract object ValueInternal { get; set; }

        public object Value
        {
            get => ValueInternal;
            init => ValueInternal = value;
        }
    }

    [DataContract]
    public record class ServiceErrorArgData<T> : ServiceErrorArgData
    {
        [NotNull]
        private T _value = default!;

        [DataMember(Order = 1)]
        public new T Value
        {
            get => _value;
            init => _value = value;
        }

        protected override object ValueInternal
        {
            get => _value;
            set => _value = (T)value;
        }
    }
}
