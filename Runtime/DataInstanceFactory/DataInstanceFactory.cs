// DataInstanceFactory.cs

using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_COLLECTIONS_LOWLEVEL_UNSAFE
using Unity.Collections.LowLevel.Unsafe;
#endif

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
    [SerializeField] private bool verbose = false;  
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

    private static bool? _isSimpleType;
    private static bool? _isUnmanagedType;
    private static bool? _isStringType;
    private static bool? _isUnityObjectType;

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

    private string SerializeData(T data)
    {
        if (IsSimpleType())
        {
            var wrapper = new Wrapper<T> { Value = data };
            return _serializer.ToJson(wrapper);
        }
        return _serializer.ToJson(data);
    }

    private T DeserializeData(string json)
    {
        if (IsSimpleType())
        {
            var wrapper = _serializer.FromJson<Wrapper<T>>(json);
            return wrapper.Value;
        }
        return _serializer.FromJson<T>(json);
    }

    private bool NeedsSerialization()
    {
        return !(IsUnmanaged() || IsString() || IsUnityObject());
    }

    // Optimized type checks with caching
    private bool IsSimpleType()
    {
        if (_isSimpleType.HasValue) return _isSimpleType.Value;
        var type = typeof(T);
        _isSimpleType = type.IsPrimitive || type.IsEnum || type == typeof(decimal);
        return _isSimpleType.Value;
    }

    private bool IsUnmanaged()
    {
#if UNITY_COLLECTIONS_LOWLEVEL_UNSAFE
        if (_isUnmanagedType.HasValue) return _isUnmanagedType.Value;
        _isUnmanagedType = UnsafeUtility.IsUnmanaged<T>();
        return _isUnmanagedType.Value;
#else
        return false;
#endif
    }

    private bool IsString()
    {
        if (_isStringType.HasValue) return _isStringType.Value;
        _isStringType = typeof(T) == typeof(string);
        return _isStringType.Value;
    }

    private bool IsUnityObject()
    {
        if (_isUnityObjectType.HasValue) return _isUnityObjectType.Value;
        _isUnityObjectType = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
        return _isUnityObjectType.Value;
    }

    protected virtual void OnEnable()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (!dataTypeValidated)
        {
            dataTypeValidated = true;
            ValidateDataType();
        }
#endif
    }

    private void ValidateDataType()
    {
        var type = typeof(T);

        if (type.IsPrimitive || type.IsEnum || type.IsArray || 
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
        {
            LogMessage($"{type} is not fully compatible with DataInstanceFactory.", LogType.Error);
        }
    }

    protected virtual void OnValidate()
    {
        LogMessage("Reset cachedJson", LogType.Log);
        cachedJson = null;
        ValidateSerializerSelection();
    }

    private void ValidateSerializerSelection()
    {
        if (!SerializerFactory.ContainsKey(serializerType))
        {
            LogMessage($"Selected serializer '{serializerType}' is not available. Falling back to JsonUtility.", LogType.Warning);
            serializerType = SerializerType.JsonUtility;
        }
    }

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
