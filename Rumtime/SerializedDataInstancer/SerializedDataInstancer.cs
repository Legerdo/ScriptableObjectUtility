using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

// T는 [System.Serializable] 특성을 가진 평범한 클래스나 구조체여야 합니다.
// T는 필요한 경우 ISerializationCallbackReceiver를 구현할 수 있습니다.
public class SerializedDataInstancer<T> : ScriptableObject
{
    private static bool dataTypeValidated; // 데이터 타입이 유효한지 여부를 추적하는 변수

    [SerializeField] private T prototypeData; // 원본 데이터를 저장하는 변수
    [System.NonSerialized] private byte[] cachedData; // 직렬화된 데이터를 캐시하는 변수

    // 데이터 인스턴스를 생성하는 메서드
    public T CreateDataInstance()
    {
        // 직렬화된 데이터를 캐싱하여 성능을 향상시킵니다.
        if (cachedData == null || cachedData.Length == 0)
            cachedData = MemoryPackSerializer.Serialize(prototypeData);

        // 직렬화된 데이터를 역직렬화하여 새 데이터 인스턴스를 생성합니다.
        return MemoryPackSerializer.Deserialize<T>(cachedData);
    }

    // 코드에서 PrototypeData 값을 설정하는 유틸리티 메서드입니다. 대부분의 사용 사례에서는 필요하지 않습니다.
    public void SetPrototypeDataValues(T data)
    {
        if (data == null)
        {
            Debug.LogError("프로토타입을 null로 설정할 수 없습니다.", this);
            return;
        }

        // 데이터를 복사하여 일관성을 유지합니다. 데이터를 할당한 후 변경된 경우를 대비합니다.
        cachedData = MemoryPackSerializer.Serialize(data);
        prototypeData = MemoryPackSerializer.Deserialize<T>(cachedData);
    }

    // 스크립트가 활성화될 때 호출되는 메서드
    protected virtual void OnEnable()
    {
        #if UNITY_EDITOR
        if (!dataTypeValidated)
        {
            dataTypeValidated = true;
            var type = typeof(T);

            // 원시 타입, 열거형, 배열, 리스트 등 비호환성 타입에 대한 유효성 검사를 수행합니다.
            if (type.IsPrimitive
                || type.IsEnum
                || type.IsArray
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                Debug.LogError($"{type}은(는) PlainDataInstancer와 호환되지 않습니다. 원시 타입, 리스트 또는 배열 대신 사용자 정의 직렬화 가능 컨테이너 객체나 구조체를 사용하세요.", this);
            }
            else if (type.IsAbstract || type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                Debug.LogError($"{type}은(는) PlainDataInstancer에서 지원되지 않습니다. 추상 클래스거나 UnityEngine.Object의 서브클래스가 아닌지 확인하세요.", this);
            }
        }
        #endif
    }

    #if UNITY_EDITOR
    // 유니티 에디터에서 속성이 변경될 때 호출되는 메서드
    protected virtual void OnValidate()
    {
        cachedData = null; // 캐시된 직렬화 데이터를 무효화합니다.
    }
    #endif    
}