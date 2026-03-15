using UnityEngine;

namespace GameLogic
{
	public abstract class EntityAction
	{
		public GameObject Entity;

		public bool complete = false;
		public int currentTick;
		public int TickDuration;

		public EntityAction(GameObject entity, int tickDuration)
		{
			Entity = entity;
			TickDuration = tickDuration;
		}

		public virtual void OnStart()
		{
			complete = false;
			currentTick = 0;

			if (TickDuration <= 0)
			{
				complete = true;
				OnEnd();
			}
		}

		public virtual void PerTick()
		{
			if (complete) return;

			if (currentTick >= TickDuration)
			{
				complete = true;
			}

			if (complete)
			{
				OnEnd();
				return;
			}

			currentTick++;
		}

		public virtual void OnEnd()
		{
		}
	}
}