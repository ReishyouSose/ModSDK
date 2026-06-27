using Newtonsoft.Json;
using System;
using Unity.Mathematics;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    public class Int2JsonConverter : JsonConverter<int2>
    {
        public override void WriteJson(JsonWriter writer, int2 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.x);
            writer.WriteValue(value.y);
            writer.WriteEndArray();
        }

        public override int2 ReadJson(JsonReader reader, Type objectType, int2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var array = serializer.Deserialize<int[]>(reader);
            if (array == null || array.Length < 2)
                return default;

            return new int2(array[0], array[1]);
        }
    }
}
