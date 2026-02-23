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
        public Vector3Int Position;

        public Grid Grid;





        public void SingleStepInDirection(GridDirection direction, bool wrapAroundEnabled = false) //ignore wrap for now
        {

        }



        //function 
        public Vector3Int QueryPosition(Vector2Int Query, bool wrapAroundEnabled = false)
        {
            Vector3Int position = this.Position;

            if (!wrapAroundEnabled)
            {

            } 
            else
            {

            }

            return position;
        }


    }
}
