using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApp.Api
{
    internal sealed class DelegatedStringJsonConverter<T> : JsonConverter<T>
        where T : notnull
    {
        private readonly Func<string, T> _parse;
        private readonly Func<T, string> _toString;

        public DelegatedStringJsonConverter(Func<string, T> parse, Func<T, string> toString)
        {
            _parse = parse;
            _toString = toString;
        }

        public override bool HandleNull => false;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // null tokens should be handled by the framework (as we overridden the HandleNull property to return false)
            Debug.Assert(reader.TokenType != JsonTokenType.Null);

            return _parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // null values should be handled by the framework (as we overridden the HandleNull property to return false)
            Debug.Assert(!(value is null));

            writer.WriteStringValue(_toString(value!));
        }
    }
}
