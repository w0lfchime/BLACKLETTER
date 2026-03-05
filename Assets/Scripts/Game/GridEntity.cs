using System;
using GameLogic;
using UnityEngine;


namespace GameLogic
{

    public enum GridDirection
    {
        Null,
        North,
        East,
        South,
        West
    }


    public struct EntityAction
    {
        public int TickDuration;


    }
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
        public void SingleStepInDirection(GridDirection direction, bool wrapAroundEnabled = false) //ignore wrap for now
        {

        }

        void Start()
        {
            View.GoToPosition(new Vector3Int(Data.Position.x, (int)Height, Data.Position.y), 0f);
        }

        public Vector2Int GetDirectionVector(GridDirection direction) => direction switch
        {
            GridDirection.North => new Vector2Int(0, 1),
            GridDirection.East => new Vector2Int(1, 0),
            GridDirection.South => new Vector2Int(0, -1),
            GridDirection.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };

    }
}
