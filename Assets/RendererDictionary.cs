using System;
using UnityEngine;

[Serializable]
public class GridVisualData
{
    public Mesh mesh;
    public Material material;
    public float scale;
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
}
