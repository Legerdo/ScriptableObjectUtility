// MemoryPackSerializer.cs
#if USE_MEMORYPACK
using MemoryPack;
using System;

public class MemoryPackSerializer : IJsonSerializer
{
    public string ToJson<T>(T data)
    {
        // MemoryPack serializes to bytes; convert to Base64 string for storage
        byte[] bytes = MemoryPack.MemoryPackSerializer.Serialize(data);
        return Convert.ToBase64String(bytes);
    }

    public T FromJson<T>(string json)
    {
        // Convert Base64 string back to bytes and deserialize
        byte[] bytes = Convert.FromBase64String(json);
        return MemoryPack.MemoryPackSerializer.Deserialize<T>(bytes);
    }
}
#endif
