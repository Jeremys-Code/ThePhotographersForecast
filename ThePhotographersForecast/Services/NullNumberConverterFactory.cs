using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThePhotographersForecast.Services.JsonConverters
{
    public class NullNumberConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var t = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            return t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var underlying = Nullable.GetUnderlyingType(typeToConvert);
            if (underlying != null)
            {
                var convType = typeof(NullableNumberConverter<>).MakeGenericType(underlying);
                return (JsonConverter)Activator.CreateInstance(convType);
            }
            else
            {
                var convType = typeof(NumberConverter<>).MakeGenericType(typeToConvert);
                return (JsonConverter)Activator.CreateInstance(convType);
            }
        }

        private class NumberConverter<T> : JsonConverter<T> where T : struct, IConvertible
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (string.IsNullOrWhiteSpace(s) || string.Equals(s, "null", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "Null", StringComparison.OrdinalIgnoreCase))
                        return default;
                    return (T)Convert.ChangeType(s, typeof(T), CultureInfo.InvariantCulture);
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (typeof(T) == typeof(int)) return (T)(object)reader.GetInt32();
                    if (typeof(T) == typeof(long)) return (T)(object)reader.GetInt64();
                    if (typeof(T) == typeof(float)) return (T)(object)reader.GetSingle();
                    if (typeof(T) == typeof(double)) return (T)(object)reader.GetDouble();
                    if (typeof(T) == typeof(decimal)) return (T)(object)reader.GetDecimal();

                    var d = reader.GetDouble();
                    return (T)Convert.ChangeType(d, typeof(T), CultureInfo.InvariantCulture);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                // write as number
                if (value is int i) writer.WriteNumberValue(i);
                else if (value is long l) writer.WriteNumberValue(l);
                else if (value is float f) writer.WriteNumberValue(f);
                else if (value is double d) writer.WriteNumberValue(d);
                else if (value is decimal m) writer.WriteNumberValue(m);
                else writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private class NullableNumberConverter<T> : JsonConverter<T?> where T : struct, IConvertible
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.None) return null;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (string.IsNullOrWhiteSpace(s) || string.Equals(s, "null", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "Null", StringComparison.OrdinalIgnoreCase))
                        return null;
                    return (T)Convert.ChangeType(s, typeof(T), CultureInfo.InvariantCulture);
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (typeof(T) == typeof(int)) return (T?)(object)reader.GetInt32();
                    if (typeof(T) == typeof(long)) return (T?)(object)reader.GetInt64();
                    if (typeof(T) == typeof(float)) return (T?)(object)reader.GetSingle();
                    if (typeof(T) == typeof(double)) return (T?)(object)reader.GetDouble();
                    if (typeof(T) == typeof(decimal)) return (T?)(object)reader.GetDecimal();

                    var d = reader.GetDouble();
                    return (T)Convert.ChangeType(d, typeof(T), CultureInfo.InvariantCulture);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
            {
                if (!value.HasValue) writer.WriteNullValue();
                else
                {
                    var v = value.Value;
                    if (v is int i) writer.WriteNumberValue(i);
                    else if (v is long l) writer.WriteNumberValue(l);
                    else if (v is float f) writer.WriteNumberValue(f);
                    else if (v is double d) writer.WriteNumberValue(d);
                    else if (v is decimal m) writer.WriteNumberValue(m);
                    else writer.WriteStringValue(v.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}