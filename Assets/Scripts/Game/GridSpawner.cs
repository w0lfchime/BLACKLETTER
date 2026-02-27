using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public List<GPUEntityData> Ores;
    public int amount;
    GPUEntityData CreateOre()
    {
        GPUEntityData oreData = Ores[UnityEngine.Random.Range(0, Ores.Count)];
        GPUEntityData oreDataCopy = new GPUEntityData();
        oreDataCopy.visualDataIndex = oreData.visualDataIndex;

        oreDataCopy.Position = new Vector2Int(UnityEngine.Random.Range(0, GameLogic.Grid.I.Width), UnityEngine.Random.Range(0, GameLogic.Grid.I.Height));
        return oreDataCopy;
    }

    void Start()
    {
        for(int i = 0; i < amount; i++){
            GameLogic.Grid.I.entities.Add(CreateOre());
        }
    }
}
