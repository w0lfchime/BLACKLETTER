using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
	[Serializable]
	public class GridEntityData
	{
		public Vector2Int Position;
		public bool Stackable;
		public int visualDataIndex;
	}

	public abstract class GridEntity : MonoBehaviour, IOnClock
	{
		public GridEntityData Data;
		public EntityView View;
		public float Height = 0f;

		protected readonly Queue<EntityAction> actionQueue = new();
		protected EntityAction currentAction;

		public EntityAction CurrentAction => currentAction;
		public int QueuedActionCount => actionQueue.Count;
		public bool IsBusy => currentAction != null;

		protected virtual void OnEnable()
		{
			Clock.Register(this);
		}

		protected virtual void OnDisable()
		{
			Clock.Unregister(this);
		}

		protected virtual void Start()
		{
			if (View != null)
			{
				View.GoToPosition(new Vector3Int(Data.Position.x, Mathf.RoundToInt(Height), Data.Position.y), 0f);
			}
		}

		public virtual void Tick(float dt, int tick)
		{
			if (currentAction == null)
			{
				StartNextAction();
			}

			if (currentAction == null) return;

			currentAction.PerTick();

			if (currentAction.complete)
			{
				currentAction = null;
				StartNextAction();
			}
		}

		void StartNextAction()
		{
			if (actionQueue.Count == 0)
			{
				currentAction = null;
				return;
			}

			currentAction = actionQueue.Dequeue();
			currentAction.OnStart();

			if (currentAction.complete)
			{
				currentAction = null;
				StartNextAction();
			}
		}

		public virtual bool CanAcceptAction(EntityAction action)
		{
			return action != null;
		}

		public virtual bool EnqueueAction(EntityAction action)
		{
			if (!CanAcceptAction(action)) return false;

			actionQueue.Enqueue(action);
			return true;
		}

		public virtual void ClearQueuedActions()
		{
			actionQueue.Clear();
		}

		public virtual void CancelAllActions()
		{
			currentAction = null;
			actionQueue.Clear();
		}

		public void SingleStepInDirection(AdjacentDirection direction, bool wrapAroundEnabled = false)
		{
			Data.Position += GetDirectionVector(direction);
		}

		public Vector2Int GetDirectionVector(AdjacentDirection direction) => direction switch
		{
			AdjacentDirection.North => new Vector2Int(0, 1),
			AdjacentDirection.NorthEast => new Vector2Int(1, 1),
			AdjacentDirection.East => new Vector2Int(1, 0),
			AdjacentDirection.SouthEast => new Vector2Int(1, -1),
			AdjacentDirection.South => new Vector2Int(0, -1),
			AdjacentDirection.SouthWest => new Vector2Int(-1, -1),
			AdjacentDirection.West => new Vector2Int(-1, 0),
			AdjacentDirection.NorthWest => new Vector2Int(-1, 1),
			_ => Vector2Int.zero
		};
	}
}