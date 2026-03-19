using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

[Serializable]
public class SharedEntityVisualData
{
    public Mesh mesh;
    public Material[] materials;
    public float scale;
    public Vector3 rotation;
    public float Height;
    public bool randomRotation;
}

[Serializable]
public class SharedEntityData
{
    public bool stackable;
    public int lootTable;
}

[Serializable]
public class EntityData
{
    public int ID;
    public SharedEntityVisualData sharedVisualData;
    public SharedEntityData sharedData;
}

public class sharedDataDictionary : MonoBehaviour
{
    public EntityData[] dataArrayInspector;
    public static Dictionary<int, EntityData> dataArray;

    void Start()
    {
        RebuildDictionary();
    }

    void RebuildDictionary()
    {
        dataArray = new Dictionary<int, EntityData>();
        foreach (var entry in dataArrayInspector)
            dataArray[entry.ID] = entry;
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildDictionary();
        if (Application.isPlaying)
            GridView.I?.MarkEntitiesDirty();
    }
    #endif
}
