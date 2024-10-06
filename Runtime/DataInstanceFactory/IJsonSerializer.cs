using System;

public interface IJsonSerializer
{
    string ToJson<T>(T data);
    T FromJson<T>(string json);
}
