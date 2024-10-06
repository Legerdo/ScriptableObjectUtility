// UnityJsonSerializer.cs
using UnityEngine;

public class UnityJsonSerializer : IJsonSerializer
{
    public string ToJson<T>(T data)
    {
        return JsonUtility.ToJson(data);
    }

    public T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}
