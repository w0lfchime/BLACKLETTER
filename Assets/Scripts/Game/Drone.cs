using UnityEngine;

namespace GameLogic
{
	public class Drone : GridEntity, IOnClock
	{
		[Header("Action Limits")]
		[SerializeField] private int maxQueuedMoveActions = 1;

		protected override void Start()
		{
			maxQueuedMoveActions = 1;
			base.Start();
		}

		public void SingleStepInDirection(AdjacentDirection direction, float time = 0f, bool wrapAroundEnabled = false)
		{
			Debug.Log("Stepping");
			base.SingleStepInDirection(direction, wrapAroundEnabled);
		}

		public override bool CanAcceptAction(EntityAction action)
		{
			if (!base.CanAcceptAction(action)) return false;

			if (action is MoveAction)
			{
				int queuedMoveActions = 0;

				foreach (EntityAction queuedAction in actionQueue)
				{
					if (queuedAction is MoveAction)
					{
						queuedMoveActions++;
					}
				}

				bool currentActionIsMove = currentAction is MoveAction;

				if (currentActionIsMove)
				{
					return false;
				}

				if (queuedMoveActions >= maxQueuedMoveActions)
				{
					return false;
				}
			}

			return true;
		}

		public override void Tick(float dt, int tick)
		{
			base.Tick(dt, tick);
		}
	}
}