using HVO.Weather;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace HVO.JsonConverters
{
    public class DoubleConverter : JsonConverter<double>
    {
        private readonly string _format = null;

        public DoubleConverter() : this(null)
        {
        }

        public DoubleConverter(string format)
        {
            _format = format;
        }

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
            double.Parse(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(this._format))
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                writer.WriteStringValue(value.ToString(this._format));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DoubleFormatAttribute : JsonConverterAttribute
    {
        private readonly string _format;

        public DoubleFormatAttribute(string format) => _format = format;

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            if (typeToConvert != typeof(double))
            {
                throw new ArgumentException(
                    $"This converter only works with double, and it was provided {typeToConvert.Name}.");
            }

            return new DoubleConverter(_format);
        }
    }
}
