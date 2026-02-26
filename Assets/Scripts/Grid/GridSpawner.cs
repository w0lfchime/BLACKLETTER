using System;
using System.Collections.Generic;
using BL_Grid;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public List<GridEntityData> Ores;
    GridEntityData CreateOre()
    {
        GridEntityData oreData = Ores[UnityEngine.Random.Range(0, Ores.Count)];
        GridEntityData oreDataCopy = new GridEntityData();
        oreDataCopy.visualDataIndex = oreData.visualDataIndex;

        oreDataCopy.Position = new Vector2Int(UnityEngine.Random.Range(0, BL_Grid.Grid.I.Width), UnityEngine.Random.Range(0, BL_Grid.Grid.I.Height));
        return oreDataCopy;
    }

    void Start()
    {
        for(int i = 0; i < 100; i++){
            BL_Grid.Grid.I.entities.Add(CreateOre());
        }
    }
}
