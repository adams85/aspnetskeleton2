using System;
using System.IO;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Meta;

namespace WebApp.Service.Host
{
    public static partial class ServiceHostContractSerializer
    {
        public static readonly SerializerBase Default = new ProtoBufSerializer();

        public static BinderConfiguration CreateBinderConfiguration(ServiceBinder? serviceBinder = null) =>
            BinderConfiguration.Create(new[] { ProtoBufMarshallerFactory.Create(ProtoBufSerializer.TypeModel) }, serviceBinder);

        private sealed class ProtoBufSerializer : SerializerBase.StreamBased
        {
            internal static readonly RuntimeTypeModel TypeModel = RuntimeTypeModel.Create().ConfigureServiceHostDefaults();

            public override void Serialize(Stream stream, object? obj, Type type) => TypeModel.Serialize(stream, obj);
            public override void Serialize<T>(Stream stream, T obj) => TypeModel.Serialize(stream, obj);

            public override object Deserialize(Stream stream, Type type) => TypeModel.Deserialize(stream, null, type);
            public override T Deserialize<T>(Stream stream) => (T)TypeModel.Deserialize(stream, null, typeof(T));
        }
    }
}
