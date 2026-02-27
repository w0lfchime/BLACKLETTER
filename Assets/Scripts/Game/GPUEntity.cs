using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngineInternal;


namespace GameLogic
{

    [Serializable]
    public class GPUEntityData
    {
        public Vector2Int Position;
        public bool Stackable;
        public int visualDataIndex;
    }

    public abstract class GPUEntity : GridEntity
    {
        public ViewEntity View;
        public GPUEntityData Data;
        public float Height;

        void Start()
        {
            View.GoToPosition(new Vector3Int(Data.Position.x, (int)Height, Data.Position.y), 0f);
        }

    }
}
