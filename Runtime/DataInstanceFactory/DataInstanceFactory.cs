// DataInstanceFactory.cs
using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_COLLECTIONS_LOWLEVEL_UNSAFE
using Unity.Collections.LowLevel.Unsafe;
#endif

/// <summary>
/// Factory class for creating instances of serialized data with flexible serialization options.
/// </summary>
/// <typeparam name="T">The type of data to instantiate.</typeparam>
public class DataInstanceFactory<T> : ScriptableObject
{
    // Enum to select the desired serializer
    public enum SerializerType
    {
        JsonUtility,
#if USE_NEWTONSOFT_JSON
        NewtonsoftJson,
#endif
#if USE_MEMORYPACK
        MemoryPack,
#endif
    }

    [System.Serializable]
    private struct Wrapper<T>
    {
        public T Value;
    }

    private static bool dataTypeValidated;

    [SerializeField] private T prototypeData;
    [SerializeField] private SerializerType serializerType = SerializerType.JsonUtility;
    [SerializeField] private bool verbose = false;  // 추가된 필드: 로그 표시 여부
    [System.NonSerialized] private string cachedJson;

    private IJsonSerializer _serializer;

    // Serializer mapping for easy initialization
    private static readonly Dictionary<SerializerType, Func<IJsonSerializer>> SerializerFactory = new Dictionary<SerializerType, Func<IJsonSerializer>>
    {
        { SerializerType.JsonUtility, () => new UnityJsonSerializer() },
#if USE_NEWTONSOFT_JSON
        { SerializerType.NewtonsoftJson, () => new NewtonsoftJsonSerializer() },
#endif
#if USE_MEMORYPACK
        { SerializerType.MemoryPack, () => new MemoryPackSerializer() },
#endif
    };

    // Initialization of serializer
    private void InitializeSerializer()
    {
        if (_serializer != null) return;

        if (SerializerFactory.TryGetValue(serializerType, out var serializerConstructor))
        {
            _serializer = serializerConstructor();
        }
        else
        {
            LogMessage($"Selected serializer '{serializerType}' is not available. Falling back to JsonUtility.", LogType.Warning);
            _serializer = new UnityJsonSerializer();
        }
    }

    /// <summary>
    /// Create a new instance of the data.
    /// </summary>
    public T CreateDataInstance()
    {
        InitializeSerializer();

        if (!NeedsSerialization())
            return prototypeData;

        if (string.IsNullOrEmpty(cachedJson))
        {
            cachedJson = SerializeData(prototypeData);
        }

        return DeserializeData(cachedJson);
    }

    /// <summary>
    /// Utility method to set the PrototypeData from code.
    /// </summary>
    public void SetPrototypeDataValues(T data)
    {
        if (data == null)
        {
            LogMessage("Cannot set prototype to null.", LogType.Error);
            return;
        }

        InitializeSerializer();

        if (NeedsSerialization())
        {
            cachedJson = SerializeData(data);
            prototypeData = DeserializeData(cachedJson);
        }
        else
        {
            cachedJson = null;
            prototypeData = data;
        }
    }

    /// <summary>
    /// Serialize the data, handling simple types if necessary.
    /// </summary>
    private string SerializeData(T data)
    {
        if (IsSimpleType())
        {
            var wrapper = new Wrapper<T> { Value = data };
            return _serializer.ToJson(wrapper);
        }
        return _serializer.ToJson(data);
    }

    /// <summary>
    /// Deserialize the data, handling simple types if necessary.
    /// </summary>
    private T DeserializeData(string json)
    {
        if (IsSimpleType())
        {
            var wrapper = _serializer.FromJson<Wrapper<T>>(json);
            return wrapper.Value;
        }
        return _serializer.FromJson<T>(json);
    }

    /// <summary>
    /// Determines if the data type requires serialization.
    /// </summary>
    private bool NeedsSerialization()
    {
        return !(IsUnmanaged() || IsString() || IsUnityObject());
    }

    /// <summary>
    /// Checks if the type is an unmanaged struct.
    /// </summary>
    private bool IsUnmanaged()
    {
#if UNITY_COLLECTIONS_LOWLEVEL_UNSAFE
        return UnsafeUtility.IsUnmanaged<T>();
#else
        return false;
#endif
    }

    /// <summary>
    /// Checks if the type is a string.
    /// </summary>
    private bool IsString()
    {
        return typeof(T) == typeof(string);
    }

    /// <summary>
    /// Checks if the type is a UnityEngine.Object or its subclass.
    /// </summary>
    private bool IsUnityObject()
    {
        return typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
    }

    /// <summary>
    /// Checks if the type is a simple primitive type.
    /// </summary>
    private bool IsSimpleType()
    {
        Type type = typeof(T);
        return type.IsPrimitive || type.IsEnum || type == typeof(decimal);
    }

    protected virtual void OnEnable()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (!dataTypeValidated)
        {
            dataTypeValidated = true;
            var type = typeof(T);

            if (type.IsPrimitive
                || type.IsEnum
                || type.IsArray
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
#if USE_NEWTONSOFT_JSON || USE_MEMORYPACK
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
#endif
                )
            {
                LogMessage($"{type} is not fully compatible with DataInstanceFactory. Consider using a custom serializable container object or struct for complex types.", LogType.Error);
            }
            else if (type.IsAbstract || typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                LogMessage($"{type} is not supported by DataInstanceFactory. Make sure it's neither abstract nor a UnityEngine.Object.", LogType.Error);
            }
        }
#endif
    }

    protected virtual void OnValidate()
    {
        LogMessage("Reset cachedJson", LogType.Log);
        cachedJson = null;
        ValidateSerializerSelection();
    }

    /// <summary>
    /// Validates if the selected serializer is available based on defined symbols.
    /// </summary>
    private void ValidateSerializerSelection()
    {
        if (!SerializerFactory.ContainsKey(serializerType))
        {
            LogMessage($"Selected serializer '{serializerType}' is not available. Falling back to JsonUtility.", LogType.Warning);
            serializerType = SerializerType.JsonUtility;
        }
    }

    /// <summary>
    /// Logs a message based on the verbose flag.
    /// </summary>
    private void LogMessage(string message, LogType logType)
    {
        if (!verbose) return;

        switch (logType)
        {
            case LogType.Error:
                Debug.LogError(message, this);
                break;
            case LogType.Warning:
                Debug.LogWarning(message, this);
                break;
            case LogType.Log:
                Debug.Log(message, this);
                break;
        }
    }
}