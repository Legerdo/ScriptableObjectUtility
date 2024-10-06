using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class BenchmarkTest : MonoBehaviour
{
    [Header("Data Instancer Setup")]
    [SerializeField] private MyCustomDataInstancer dataInstancer;

    [Header("Benchmark Settings")]
    [SerializeField] private int numberOfInstances = 10000;
    [SerializeField] private int iterations = 5;
    [SerializeField] private int sampleSize = 10; // Inspector에 저장할 샘플 크기

    [Header("Benchmark Control")]
    [Tooltip("Enable benchmark for DataInstanceFactory")]
    public bool enableDataInstancerBenchmark = true;

    [Tooltip("Enable benchmark for SimpleDataObject")]
    public bool enableSimpleDataObjectBenchmark = true;

    [Tooltip("Enable benchmark for CreateInstance of SimpleDataObject")]
    public bool enableCreateInstanceBenchmark = true; // 새로 추가된 옵션

    [Header("Benchmark Results")]
    [Tooltip("Sample of instances created using DataInstanceFactory")]
    public List<MyCustomData> dataInstancerSamples = new List<MyCustomData>();

    [Tooltip("Sample of instances created without using DataInstanceFactory")]
    public List<SimpleDataObject> simpleDataObjectSamples = new List<SimpleDataObject>();

    [Tooltip("Sample of instances created using CreateInstance")]
    public List<SimpleDataObject> createInstanceSamples = new List<SimpleDataObject>(); // 새로 추가된 리스트

    // 미리 생성된 SimpleDataObject 인스턴스
    private SimpleDataObject preConfiguredSimpleDataObject;

    void Start()
    {
        // DataInstancer가 할당되지 않은 경우 에러 출력
        if (dataInstancer == null)
        {
            UnityEngine.Debug.LogError("DataInstancer is not assigned.");
            return;
        }

        // 벤치마크 시작 전에 공유 데이터를 한 번만 생성
        preConfiguredSimpleDataObject = ScriptableObject.CreateInstance<SimpleDataObject>();
        preConfiguredSimpleDataObject.data = new MyCustomData
        {
            intValue = 42,
            floatValue = 3.14f,
            stringValue = "InitialData",
            boolValue = true,
            doubleValue = 2.71828,
            intList = new List<int> { 1, 2, 3, 4, 5 },
            vectorValue = new Vector3(1.0f, 2.0f, 3.0f),
            stringDictionary = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            }
        };

        // 지정된 횟수만큼 벤치마크 반복 실행
        for (int i = 0; i < iterations; i++)
        {
            UnityEngine.Debug.Log($"--- Iteration {i + 1} ---");

            if (enableDataInstancerBenchmark)
            {
                RunBenchmarkWithDataInstancer();
            }
            else
            {
                UnityEngine.Debug.Log("Data Instancer Benchmark is disabled.");
            }

            if (enableSimpleDataObjectBenchmark)
            {
                RunBenchmarkWithoutDataInstancer();
            }
            else
            {
                UnityEngine.Debug.Log("Simple Data Object Benchmark is disabled.");
            }

            if (enableCreateInstanceBenchmark) // 새로 추가된 벤치마크 조건
            {
                RunBenchmarkWithoutDataCreateInstance();
            }
            else
            {
                UnityEngine.Debug.Log("CreateInstance Benchmark is disabled.");
            }
        }
    }

    /// <summary>
    /// DataInstanceFactory 사용하여 MyCustomData 인스턴스를 생성하는 벤치마크
    /// </summary>
    private void RunBenchmarkWithDataInstancer()
    {
        System.GC.Collect(); // 이전 메모리 할당 정리
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // 이전 샘플 데이터 초기화
        dataInstancerSamples.Clear();

        for (int i = 0; i < numberOfInstances; i++)
        {
            // DataInstanceFactory 사용하여 데이터 인스턴스 생성
            MyCustomData instance = dataInstancer.CreateDataInstance();

            // 데이터의 독립성을 위해 랜덤 값 할당
            instance.intValue = UnityEngine.Random.Range(0, 1000);
            instance.floatValue = UnityEngine.Random.Range(0f, 1000f);
            instance.stringValue = $"DataInstancer_{i}";
            instance.boolValue = UnityEngine.Random.value > 0.5f;
            instance.doubleValue = UnityEngine.Random.Range(0.0f, 1000.0f);
            instance.intList = new List<int>
            {
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 100)
            };
            instance.vectorValue = new Vector3(
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-100f, 100f)
            );
            instance.stringDictionary = new Dictionary<string, string>
            {
                { $"Key_{i}_1", $"Value_{i}_1" },
                { $"Key_{i}_2", $"Value_{i}_2" }
            };

            // 샘플 크기 내에서만 리스트에 추가
            if (i < sampleSize)
            {
                dataInstancerSamples.Add(instance);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"[DataInstancer] Created {numberOfInstances} instances in {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// SimpleDataObject를 ScriptableObject.CreateInstance로 생성하는 벤치마크
    /// </summary>
    private void RunBenchmarkWithoutDataCreateInstance()
    {
        System.GC.Collect(); // 이전 메모리 할당 정리
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // 이전 샘플 데이터 초기화
        createInstanceSamples.Clear();

        for (int i = 0; i < numberOfInstances; i++)
        {
            // 미리 생성된 SimpleDataObject를 ScriptableObject.CreateInstance 하여 새로운 인스턴스 생성
            SimpleDataObject instance = ScriptableObject.CreateInstance<SimpleDataObject>();
            instance.data = new MyCustomData();

            // 데이터의 독립성을 위해 랜덤 값 할당
            instance.data.intValue = UnityEngine.Random.Range(0, 1000);
            instance.data.floatValue = UnityEngine.Random.Range(0f, 1000f);
            instance.data.stringValue = $"SimpleDataObject_{i}";
            instance.data.boolValue = UnityEngine.Random.value > 0.5f;
            instance.data.doubleValue = UnityEngine.Random.Range(0.0f, 1000.0f);
            instance.data.intList = new List<int>
            {
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 100)
            };
            instance.data.vectorValue = new Vector3(
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-100f, 100f)
            );
            instance.data.stringDictionary = new Dictionary<string, string>
            {
                { $"Key_{i}_1", $"Value_{i}_1" },
                { $"Key_{i}_2", $"Value_{i}_2" }
            };

            // 샘플 크기 내에서만 리스트에 추가
            if (i < sampleSize)
            {
                createInstanceSamples.Add(instance);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"[CreateInstance] Created {numberOfInstances} instances in {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// SimpleDataObject를 Instantiate하여 생성하는 벤치마크
    /// </summary>
    private void RunBenchmarkWithoutDataInstancer()
    {
        System.GC.Collect(); // 이전 메모리 할당 정리
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // 이전 샘플 데이터 초기화
        simpleDataObjectSamples.Clear();

        for (int i = 0; i < numberOfInstances; i++)
        {
            // 미리 생성된 SimpleDataObject를 Instantiate하여 새로운 인스턴스 생성
            SimpleDataObject instance = Instantiate(preConfiguredSimpleDataObject);

            // 데이터의 독립성을 위해 랜덤 값 할당
            instance.data.intValue = UnityEngine.Random.Range(0, 1000);
            instance.data.floatValue = UnityEngine.Random.Range(0f, 1000f);
            instance.data.stringValue = $"SimpleDataObject_{i}";
            instance.data.boolValue = UnityEngine.Random.value > 0.5f;
            instance.data.doubleValue = UnityEngine.Random.Range(0.0f, 1000.0f);
            instance.data.intList = new List<int>
            {
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 100),
                UnityEngine.Random.Range(0, 100)
            };
            instance.data.vectorValue = new Vector3(
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-100f, 100f)
            );
            instance.data.stringDictionary = new Dictionary<string, string>
            {
                { $"Key_{i}_1", $"Value_{i}_1" },
                { $"Key_{i}_2", $"Value_{i}_2" }
            };

            // 샘플 크기 내에서만 리스트에 추가
            if (i < sampleSize)
            {
                simpleDataObjectSamples.Add(instance);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"[SimpleDataObject] Created {numberOfInstances} instances in {stopwatch.ElapsedMilliseconds} ms");
    }
}