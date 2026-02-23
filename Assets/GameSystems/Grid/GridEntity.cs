using UnityEngine;
using UnityEngineInternal;


namespace BL_Grid
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
        public Vector2Int Position;
        public float height;


        public void SingleStepInDirection(GridDirection direction, bool wrapAroundEnabled = false) //ignore wrap for now
        {

        }



        //function 
        public Vector2Int QueryPosition(Vector2Int Query, bool wrapAroundEnabled = false)
        {
            Vector2Int position = this.Position;

            if (!wrapAroundEnabled)
            {

            } 
            else
            {

            }

            return position;
        }

        public static Vector2Int GetDirectionVector(GridDirection direction) => direction switch
        {
            GridDirection.North => new Vector2Int(0, 1),
            GridDirection.East => new Vector2Int(1, 0),
            GridDirection.South => new Vector2Int(0, -1),
            GridDirection.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };

    }
}
