using System;
using GameLogic;
using UnityEngine;

[Serializable]
public class GridVisualData
{
    public Mesh mesh;
    public Material[] materials;
    public float scale;
    public Vector3 rotation;
    public float Height;
    public bool randomRotation;
}
public class RendererDictionary : MonoBehaviour
{
    public GridVisualData[] visualDataArrayInspector;
    public static GridVisualData[] visualDataArray;

    void Start()
    {
        visualDataArray = visualDataArrayInspector;
    }

    void FixedUpdate()
    {
        visualDataArray = visualDataArrayInspector;
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        visualDataArray = visualDataArrayInspector;
        if (Application.isPlaying)
            GridView.I?.MarkEntitiesDirty();
    }
    #endif
}
