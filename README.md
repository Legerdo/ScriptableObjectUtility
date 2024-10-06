# DataInstanceFactory
- 직렬화 캐싱을 통한 ScriptableObject 생성 유틸리티

벤치마크 결과(Newtonsoft 사용)
1. DataInstanceFactory / 2. Instantiate / 3. ScriptableObject.CreateInstance

![Test](https://github.com/user-attachments/assets/193f11cf-b29b-45d9-adce-90e8477ff8d0)

아직은 ScriptableObject.CreateInstance 에 비해선 약간 빠른편.
