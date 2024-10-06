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
/// <typeparam name="TData">The type of data to instantiate.</typeparam>
public class DataInstanceFactory<TData> : ScriptableObject
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

    private static bool s_DataTypeValidated;

    [SerializeField] private TData m_PrototypeData;
    [SerializeField] private SerializerType m_SerializerType = SerializerType.JsonUtility;
    [System.NonSerialized] private string m_CachedJson;

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

        if (SerializerFactory.TryGetValue(m_SerializerType, out var serializerConstructor))
        {
            _serializer = serializerConstructor();
        }
        else
        {
            Debug.LogWarning($"Selected serializer '{m_SerializerType}' is not available. Falling back to JsonUtility.", this);
            _serializer = new UnityJsonSerializer();
        }
    }

    /// <summary>
    /// Create a new instance of the data.
    /// </summary>
    public TData CreateDataInstance()
    {
        InitializeSerializer();

        if (!NeedsSerialization())
            return m_PrototypeData;

        if (string.IsNullOrEmpty(m_CachedJson))
        {
            m_CachedJson = SerializeData(m_PrototypeData);
        }

        return DeserializeData(m_CachedJson);
    }

    /// <summary>
    /// Utility method to set the PrototypeData from code.
    /// </summary>
    public void SetPrototypeDataValues(TData data)
    {
        if (data == null)
        {
            Debug.LogError("Cannot set prototype to null.", this);
            return;
        }

        InitializeSerializer();

        if (NeedsSerialization())
        {
            m_CachedJson = SerializeData(data);
            m_PrototypeData = DeserializeData(m_CachedJson);
        }
        else
        {
            m_CachedJson = null;
            m_PrototypeData = data;
        }
    }

    /// <summary>
    /// Serialize the data, handling simple types if necessary.
    /// </summary>
    private string SerializeData(TData data)
    {
        if (IsSimpleType())
        {
            var wrapper = new Wrapper<TData> { Value = data };
            return _serializer.ToJson(wrapper);
        }
        return _serializer.ToJson(data);
    }

    /// <summary>
    /// Deserialize the data, handling simple types if necessary.
    /// </summary>
    private TData DeserializeData(string json)
    {
        if (IsSimpleType())
        {
            var wrapper = _serializer.FromJson<Wrapper<TData>>(json);
            return wrapper.Value;
        }
        return _serializer.FromJson<TData>(json);
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
        return UnsafeUtility.IsUnmanaged<TData>();
#else
        return false;
#endif
    }

    /// <summary>
    /// Checks if the type is a string.
    /// </summary>
    private bool IsString()
    {
        return typeof(TData) == typeof(string);
    }

    /// <summary>
    /// Checks if the type is a UnityEngine.Object or its subclass.
    /// </summary>
    private bool IsUnityObject()
    {
        return typeof(UnityEngine.Object).IsAssignableFrom(typeof(TData));
    }

    /// <summary>
    /// Checks if the type is a simple primitive type.
    /// </summary>
    private bool IsSimpleType()
    {
        Type type = typeof(TData);
        return type.IsPrimitive || type.IsEnum || type == typeof(decimal);
    }

    protected virtual void OnEnable()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (!s_DataTypeValidated)
        {
            s_DataTypeValidated = true;
            var type = typeof(TData);

            if (type.IsPrimitive
                || type.IsEnum
                || type.IsArray
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
#if USE_NEWTONSOFT_JSON || USE_MEMORYPACK
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
#endif
                )
            {
                Debug.LogError($"{type} is not fully compatible with DataInstanceFactory. Consider using a custom serializable container object or struct for complex types.", this);
            }
            else if (type.IsAbstract || typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                Debug.LogError($"{type} is not supported by DataInstanceFactory. Make sure it's neither abstract nor a UnityEngine.Object.", this);
            }
        }
#endif
    }

    protected virtual void OnValidate()
    {
        Debug.Log("Reset m_CachedJson");
        m_CachedJson = null;
        ValidateSerializerSelection();
    }

    /// <summary>
    /// Validates if the selected serializer is available based on defined symbols.
    /// </summary>
    private void ValidateSerializerSelection()
    {
        if (!SerializerFactory.ContainsKey(m_SerializerType))
        {
            Debug.LogWarning($"Selected serializer '{m_SerializerType}' is not available. Falling back to JsonUtility.", this);
            m_SerializerType = SerializerType.JsonUtility;
        }
    }
}
