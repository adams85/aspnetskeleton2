using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApp.Api
{
    internal sealed class DelegatedStringJsonConverter<T> : JsonConverter<T>
        where T : notnull
    {
        private static readonly bool s_typeAllowsNull = !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null;

        private readonly Func<string, T> _parse;
        private readonly Func<T, string> _toString;

        public DelegatedStringJsonConverter(Func<string, T> parse, Func<T, string> toString)
        {
            _parse = parse;
            _toString = toString;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // null tokens are handled by the framework except when the expected type is a non-nullable value type
            // https://github.com/dotnet/corefx/blob/v3.1.6/src/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializer.Read.HandleNull.cs#L58
            if (!s_typeAllowsNull && reader.TokenType == JsonTokenType.Null)
                throw new JsonException($"{typeof(T)} does not accept null values.");

            return _parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // value is presumably not null here as null values are handled by the framework

            writer.WriteStringValue(_toString(value));
        }
    }
}
