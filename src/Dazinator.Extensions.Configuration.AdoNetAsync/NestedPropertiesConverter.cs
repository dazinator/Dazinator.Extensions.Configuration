namespace Dazinator.Extensions.Configuration.AdoNetAsync;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class NestedPropertiesConverter : JsonConverter<IDictionary<string, object>>
{
    private readonly Dictionary<string, string> _configData;

    public NestedPropertiesConverter(Dictionary<string, string> configData) => _configData = configData;

    public override void Write(Utf8JsonWriter writer, IDictionary<string, object> value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);

    public override bool CanConvert(Type typeToConvert) => typeof(IDictionary<string, object>).IsAssignableFrom(typeToConvert);

    public override IDictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var dictionary = new Dictionary<string, object>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    if (!_configData.ContainsKey(propertyName))
                    {
                        _configData[propertyName] = reader.GetString();
                    }

                    dictionary[propertyName] = ReadValue(ref reader, options);
                }
            }

            throw new JsonException("Unclosed object at the end of the JSON string.");
        }

        throw new JsonException("Expected StartObject token.");
    }

    private object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                return Read(ref reader, typeof(Dictionary<string, object>), options);
            case JsonTokenType.StartArray:
                return Read(ref reader, typeof(List<object>), options);
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                return reader.TryGetInt64(out var intValue) ? intValue : reader.GetDouble();
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }
}
