// NewtonsoftJsonSerializer.cs
#if USE_NEWTONSOFT_JSON
using Newtonsoft.Json;

public class NewtonsoftJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerSettings _settings;

    public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null)
    {
        _settings = settings ?? new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }

    public string ToJson<T>(T data)
    {
        return JsonConvert.SerializeObject(data, _settings);
    }

    public T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }
}
#endif
