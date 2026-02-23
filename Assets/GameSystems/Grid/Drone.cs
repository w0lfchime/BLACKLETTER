using UnityEngine;
using DG.Tweening;


namespace BL_Grid
{

    public class Drone : GridEntity
    {
        public DroneView View;

        void Start()
        {
            Grid.I.AddDrone(this);
            View.GoToPosition(new Vector3Int(Position.x, (int)height, Position.y), 0f);
        }

        public void SingleStepInDirection(GridDirection direction, float time = 0f, bool wrapAroundEnabled = false) //ignore wrap for now
        {
            Vector2Int newPosition = Position + GetDirectionVector(direction);
            newPosition = Grid.I.WrapPosition(newPosition);
            View.GoToPosition(new Vector3Int(newPosition.x, (int)height, newPosition.y), time);

            Position = newPosition;
        }

    }
}

