using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace WebApp.Service
{
    public abstract class SerializerBase
    {
        protected SerializerBase() { }

        public abstract void Serialize(Stream stream, object? obj, Type type);
        public abstract byte[] Serialize(object? obj, Type type);
        public abstract void Serialize<T>(Stream stream, T obj);
        public abstract byte[] Serialize<T>(T obj);

        public abstract object? Deserialize(Stream stream, Type type);
        public abstract object? Deserialize(ArraySegment<byte> bytes, Type type);
        [return: MaybeNull] public abstract T Deserialize<T>(Stream stream);
        [return: MaybeNull] public abstract T Deserialize<T>(ArraySegment<byte> bytes);

        public abstract class StreamBased : SerializerBase
        {
            protected StreamBased() { }

            public sealed override byte[] Serialize(object? obj, Type type)
            {
                using var ms = new MemoryStream();
                Serialize(ms, obj, type);
                return ms.ToArray();
            }

            public sealed override byte[] Serialize<T>(T obj)
            {
                using var ms = new MemoryStream();
                Serialize(ms, obj);
                return ms.ToArray();
            }

            public sealed override object? Deserialize(ArraySegment<byte> bytes, Type type)
            {
                using var ms = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count);
                return Deserialize(ms, type);
            }

            [return: MaybeNull]
            public sealed override T Deserialize<T>(ArraySegment<byte> bytes)
            {
                using var ms = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count);
                return Deserialize<T>(ms);
            }
        }

        public abstract class ByteArrayBased : SerializerBase
        {
            protected ByteArrayBased() { }

            public sealed override void Serialize(Stream stream, object? obj, Type type)
            {
                var bytes = Serialize(obj, type);
                stream.Write(bytes, 0, bytes.Length);
            }

            public sealed override void Serialize<T>(Stream stream, T obj)
            {
                var bytes = Serialize(obj);
                stream.Write(bytes, 0, bytes.Length);
            }

            public sealed override object? Deserialize(Stream stream, Type type)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return Deserialize(new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length), type);
            }

            [return: MaybeNull]
            public sealed override T Deserialize<T>(Stream stream)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return Deserialize<T>(new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length));
            }
        }
    }
}
