using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlyTextFilter.Model;

public class ConcurrentDictionaryConverter<TKey, TValue> : JsonConverter where TKey : Enum
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            return;
        }

        var dictionary = (ConcurrentDictionary<TKey, TValue>)value;

        writer.WriteStartObject();

        foreach (var pair in dictionary)
        {
            writer.WritePropertyName(Convert.ToInt32(pair.Key).ToString());
            serializer.Serialize(writer, pair.Value);
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var result = new ConcurrentDictionary<TKey, TValue>();
        var jObject = JObject.Load(reader);

        foreach (var (s, value) in jObject)
        {
            var key = (TKey)(object)int.Parse(s);
            var addValue = value?.ToObject(typeof(TValue));
            if (addValue != null)
            {
                result.TryAdd(key, (TValue)addValue);
            }
        }

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(ConcurrentDictionary<TKey, TValue>) == objectType;
    }
}
