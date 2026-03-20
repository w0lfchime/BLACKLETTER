using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

[Serializable]
public class SharedEntityVisualData
{
    public Mesh mesh;
    public Mesh[] splitMeshes;
    public int splitCount = 8;
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
    public bool breakable;
    public List<Vector2Int> lootTable;
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
        {
            dataArray[entry.ID] = entry;

            // Pre-split meshes for breakable entities
            if (entry.sharedData != null && entry.sharedData.breakable
                && entry.sharedVisualData?.mesh != null)
            {
                entry.sharedVisualData.splitMeshes = MeshSplitter.Split(entry.sharedVisualData.mesh, entry.sharedVisualData.splitCount);
            }
        }
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
