using System;
using System.Collections.Generic;
using GameLogic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public List<GridEntityData> Ores;
    public int amount;
    GridEntityData CreateOre()
    {
        GridEntityData oreData = Ores[UnityEngine.Random.Range(0, Ores.Count)];
        GridEntityData oreDataCopy = new GridEntityData();
        oreDataCopy.ID = oreData.ID;

        oreDataCopy.Position = new Vector2Int(UnityEngine.Random.Range(0, GameLogic.GameGrid.I.Width), UnityEngine.Random.Range(0, GameLogic.GameGrid.I.Height));
        return oreDataCopy;
    }

    void Start()
    {
        for(int i = 0; i < amount; i++){
            GameLogic.GameGrid.I.entities.Add(CreateOre());
        }
    }
}
