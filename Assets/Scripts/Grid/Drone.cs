using UnityEngine;
using DG.Tweening;


namespace BL_Grid
{

    public class Drone : GridEntity
    {
        public void SingleStepInDirection(GridDirection direction, float time = 0f, bool wrapAroundEnabled = false) //ignore wrap for now
        {
            Vector2Int newPosition = Data.Position + GetDirectionVector(direction);
            newPosition = Grid.I.WrapPosition(newPosition);
            View.GoToPosition(new Vector3Int(newPosition.x, (int)Height, newPosition.y), time);

            Data.Position = newPosition;
        }

    }
}

