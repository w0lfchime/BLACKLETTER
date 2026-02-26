using System;
using System.Collections.Generic;
using BL_Grid;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Serializable]
    public struct ResourceEntry
    {
        public string Name;
        public Mesh Mesh;
    }
    public List<ResourceEntry> Ores;
    public List<GridEntityData> OreEntities;
    void Update()
    {
        
    }
}
