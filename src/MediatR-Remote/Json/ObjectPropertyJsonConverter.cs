using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MediatR.Remote.Json;

internal class ObjectPropertyJsonConverter : JsonConverter<object>
{
    private const string TypePropertyName = "$type";

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonElement = JsonElement.ParseValue(ref reader);
        if (jsonElement.TryGetProperty(TypePropertyName, out var typeNode))
        {
            var typeName = typeNode.GetString()!;
            var type = Type.GetType(typeName) ?? throw new TypeAccessException(typeName);
            var obj = jsonElement.Deserialize(type, options);

            return obj;
        }

        return JsonSerializer.Deserialize(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var jsonElement = JsonSerializer.SerializeToElement(value, options);
        var jsonObject = JsonObject.Create(jsonElement);
        jsonObject?.Add(TypePropertyName, value.GetType().AssemblyQualifiedName);

        jsonObject?.WriteTo(writer, options);
    }
}