using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApp.Api
{
    internal partial class ApiObjectJsonConverterFactory
    {
        private delegate object ReadSubTypeDelegate(JsonConverter converter, ref Utf8JsonReader reader, JsonSerializerOptions options);

        private delegate void WriteSubTypeDelegate(JsonConverter converter, Utf8JsonWriter writer, object value, JsonSerializerOptions options);

        private sealed class TypeHelper
        {
            private static readonly MethodInfo s_invokeReadSubTypeMethodDefinition = new ReadSubTypeDelegate(InvokeReadSubType<object>).Method.GetGenericMethodDefinition();

            private static readonly MethodInfo s_invokeWriteSubTypeMethodDefinition = new WriteSubTypeDelegate(InvokeWriteSubType<object>).Method.GetGenericMethodDefinition();

            private static object InvokeReadSubType<T>(JsonConverter converter, ref Utf8JsonReader reader, JsonSerializerOptions options) where T : notnull =>
                ((Converter<T>)converter).ReadCore(ref reader, options);

            private static void InvokeWriteSubType<T>(JsonConverter converter, Utf8JsonWriter writer, object value, JsonSerializerOptions options) where T : notnull =>
                ((Converter<T>)converter).WriteCore(writer, (T)value, options, addType: true);

            public TypeHelper(Type type)
            {
                ReadSubType = (ReadSubTypeDelegate)Delegate.CreateDelegate(typeof(ReadSubTypeDelegate),
                    s_invokeReadSubTypeMethodDefinition.MakeGenericMethod(type));

                WriteSubType = (WriteSubTypeDelegate)Delegate.CreateDelegate(typeof(WriteSubTypeDelegate),
                    s_invokeWriteSubTypeMethodDefinition.MakeGenericMethod(type));
            }

            public ReadSubTypeDelegate ReadSubType { get; }
            public WriteSubTypeDelegate WriteSubType { get; }
        }
    }
}
