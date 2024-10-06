using UnityEngine;
using MemoryPack;
using System.Collections.Generic;

[System.Serializable]
[MemoryPackable]
public partial class MyCustomData
{
    public int intValue;
    public float floatValue;
    public string stringValue;

    // 추가된 필드들
    public bool boolValue;
    public double doubleValue;
    public List<int> intList;
    public Vector3 vectorValue;
    public Dictionary<string, string> stringDictionary;
}
