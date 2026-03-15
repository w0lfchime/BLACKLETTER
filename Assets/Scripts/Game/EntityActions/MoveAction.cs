using UnityEngine;

namespace GameLogic
{
	public enum AdjacentDirection
	{
		Null,
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest,
	}

	public enum MoveActionType
	{
		Null,
		Directional,
		Anywhere,
	}

	public class MoveAction : EntityAction
	{
		public MoveActionType MoveType;
		public AdjacentDirection Direction;
		public Vector2Int StartLocation;
		public Vector2Int TargetLocation;

		public float AccelerationFactor = 1.0f;
		public int MoveDuration;

		// constructor for adjacent movement
		public MoveAction(GameObject entity, int tickDuration, AdjacentDirection direction) : base(entity, tickDuration)
		{
			MoveType = MoveActionType.Directional;
			Direction = direction;
			MoveDuration = tickDuration;

			GridEntity gridEntity = entity.GetComponent<GridEntity>();
			if (gridEntity != null)
			{
				StartLocation = gridEntity.Data.Position;
				TargetLocation = gridEntity.Data.Position + gridEntity.GetDirectionVector(direction);
			}

			Debug.Log($"Move action created. Start: {StartLocation}, Target: {TargetLocation}");
		}

		// constructor for direct movement
		public MoveAction(GameObject entity, int tickDuration, Vector2Int newLocation) : base(entity, tickDuration)
		{
			MoveType = MoveActionType.Anywhere;
			MoveDuration = tickDuration;
			TargetLocation = newLocation;
		}

		public override void OnStart()
		{
			base.OnStart();

			if (complete) return;

			Debug.Log("Attempting move");

			GridEntity gridEntity = Entity.GetComponent<GridEntity>();
			if (gridEntity == null)
			{
				complete = true;
				return;
			}

			if (MoveType == MoveActionType.Directional)
			{
				Debug.Log("Attempting move");
				// backend move happens immediately
				gridEntity.SingleStepInDirection(Direction);

				// visual move lerps over the action duration
				if (gridEntity.View is DroneView droneView)
				{
					droneView.MoveBetween(
						StartLocation,
						TargetLocation,
						MoveDuration,
						AccelerationFactor,
						Mathf.RoundToInt(gridEntity.Height)
					);
				}
				else if (gridEntity.View != null)
				{
					// fallback if some other view type is used
					gridEntity.View.GoToPosition(
						new Vector3Int(TargetLocation.x, Mathf.RoundToInt(gridEntity.Height), TargetLocation.y),
						0f
					);
				}
			}
		}

		public override void PerTick()
		{
			if (complete) return;
			base.PerTick();
		}

		public override void OnEnd()
		{
			Debug.Log("MoveAction END");
			base.OnEnd();
		}
	}
}