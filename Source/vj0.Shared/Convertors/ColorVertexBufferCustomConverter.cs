using System;
using System.Collections.Generic;

using CUE4Parse.UE4.Objects.Meshes;
using CUE4Parse.Utils;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vj0.Shared.Convertors;

public class FColorVertexBufferCustomConverter : JsonConverter<FColorVertexBuffer>
{
    public override void WriteJson(JsonWriter writer, FColorVertexBuffer? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Data");
        writer.WriteStartArray();

        foreach (var c in value!.Data)
        {
            writer.WriteValue(UnsafePrint.BytesToHex(c.A, c.R, c.G, c.B));
        }

        writer.WriteEndArray();

        writer.WritePropertyName("Stride");
        writer.WriteValue(value.Stride);

        writer.WritePropertyName("NumVertices");
        writer.WriteValue(value.NumVertices);

        writer.WriteEndObject();
    }

    public override FColorVertexBuffer ReadJson(JsonReader reader, Type objectType, FColorVertexBuffer? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class FColorVertexBufferCustomResolver(Dictionary<Type, JsonConverter?> converters) : DefaultContractResolver
{
    private Dictionary<Type, JsonConverter?> _Converters { get; } = converters;

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var contract = base.CreateObjectContract(objectType);
        if (_Converters.TryGetValue(objectType, out var converter))
        {
            contract.Converter = converter;
        }
        
        return contract;
    }
}