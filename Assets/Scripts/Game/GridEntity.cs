using System;
using GameLogic;
using UnityEngine;


namespace GameLogic
{



    [Serializable]
    public class GridEntityData
    {
        public Vector2Int Position;
        public bool Stackable;
        public int visualDataIndex;
    }

    public abstract class GridEntity : MonoBehaviour
    {
        public GridEntityData Data;
        public ViewEntity View;
        public float Height = 0f;
        public void SingleStepInDirection(AdjacentDirection direction, bool wrapAroundEnabled = false) //ignore wrap for now
        {

        }

        void Start()
        {
            View.GoToPosition(new Vector3Int(Data.Position.x, (int)Height, Data.Position.y), 0f);
        }

        public Vector2Int GetDirectionVector(AdjacentDirection direction) => direction switch
        {
            AdjacentDirection.North => new Vector2Int(0, 1),
            AdjacentDirection.East => new Vector2Int(1, 0),
            AdjacentDirection.South => new Vector2Int(0, -1),
            AdjacentDirection.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };

    }
}
