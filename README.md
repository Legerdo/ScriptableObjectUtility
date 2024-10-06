# DataInstanceFactory
- 직렬화 캐싱을 통한 ScriptableObject 생성 유틸리티

벤치마크 결과(Newtonsoft 사용)
1. DataInstanceFactory / 2. Instantiate / 3. ScriptableObject.CreateInstance

![Benchmark](https://github.com/user-attachments/assets/c4026fc2-d409-4f8a-a6af-bcea706184a6)

아직은 ScriptableObject.CreateInstance 에 비해선 약간 빠른편.
