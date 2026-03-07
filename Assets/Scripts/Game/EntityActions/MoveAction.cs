



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
		public Vector2Int TargetLocation;

		public float LerpFactor = 1.0f;

		public MoveAction(GameObject entity, int tickDuration, AdjacentDirection direction) : base(entity, tickDuration)
		{
			
			this


			//calculate target location


		}

		public MoveAction(GameObject entity, int tickDuration, Vector2Int newLocation) : base(entity, tickDuration)
		{
			this.MoveType = MoveActionType.Null;


		}





		public override void OnStart()
		{
			base.OnStart();

			//..


		}

		public override void PerTick()
		{


			//...

			base.PerTick();
		}

		public override void OnEnd()
		{


			//...

			base.OnEnd();

			//...


		}
	}
}
