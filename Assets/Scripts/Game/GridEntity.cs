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

    public abstract class GridEntity : MonoBehaviour
    {


        public void SingleStepInDirection(GridDirection direction, bool wrapAroundEnabled = false) //ignore wrap for now
        {

        }

        void Start()
        {

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
