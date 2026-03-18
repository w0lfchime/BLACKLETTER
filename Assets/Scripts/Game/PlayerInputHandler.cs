using System;
using UnityEngine;

namespace GameLogic
{
	public sealed class PlayerInputHandler : MonoBehaviour, IOnClock
	{
		[Header("Controlled Entity")]
		[SerializeField] private Drone drone;

		[Header("Movement")]
		[SerializeField] private int moveTickDuration = 8;

		[Header("Current Input")]
		[SerializeField] private AdjacentDirection currentDirection = AdjacentDirection.Null;
		[SerializeField] private AdjacentDirection previousDirection = AdjacentDirection.Null;

		public AdjacentDirection CurrentDirection => currentDirection;

		void OnEnable()
		{
			Clock.Register(this);
		}

		void OnDisable()
		{
			Clock.Unregister(this);
		}

		public void Tick(float dt, int tick)
		{
			if (drone == null)
			{
				drone = GameGrid.I.Drones[0];

			}

			previousDirection = currentDirection;
			currentDirection = ReadArrowKeyDirection();

			bool newDirectionalPress =
				currentDirection != AdjacentDirection.Null &&
				currentDirection != previousDirection;

			if (!newDirectionalPress) return;



			bool accepted = drone.EnqueueAction(new MoveAction(drone.gameObject, moveTickDuration, currentDirection));
			

			if (!accepted)
			{
				Debug.Log("Move action rejected");
			}
		}


		public void MoveDrone(GridEntity gridEntity, AdjacentDirection direction, int duration = 0)
		{
			//data action
			RunAfterDelay(gridEntity.SingleStepInDirection(direction));

			// visual action
			if (gridEntity.View is DroneView droneView)
			{
				droneView.MoveBetween(
					gridEntity.Data.Position,
					gridEntity.Data.Position + gridEntity.GetDirectionVector(direction),
					duration,
					1f,
					Mathf.RoundToInt(gridEntity.Height)
				);
			}
		}

		//layout
		public void functionInIDE()
		{
			RunAfterDelay(Action);

			if (gridEntity.View is DroneView droneView)
			{
				droneView.doanimation();
			}
		}

		public void RunAfterDelay()
		{
			
		}

		AdjacentDirection ReadArrowKeyDirection()
		{
			bool up = Input.GetKey(KeyCode.UpArrow);
			bool down = Input.GetKey(KeyCode.DownArrow);
			bool left = Input.GetKey(KeyCode.LeftArrow);
			bool right = Input.GetKey(KeyCode.RightArrow);

			if (up)
			{
				if (right) return AdjacentDirection.NorthEast;
				if (left) return AdjacentDirection.NorthWest;
				return AdjacentDirection.North;
			}

			if (down)
			{
				if (right) return AdjacentDirection.SouthEast;
				if (left) return AdjacentDirection.SouthWest;
				return AdjacentDirection.South;
			}

			if (right) return AdjacentDirection.East;
			if (left) return AdjacentDirection.West;

			return AdjacentDirection.Null;
		}
	}
}