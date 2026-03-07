using UnityEngine;




public abstract class EntityAction
{
	public GameObject Entity;


	public bool complete = false;
	public int currentTick;
	public int TickDuration;




	public EntityAction(GameObject entity, int tickDuration)
	{
		this.Entity = entity;
		this.TickDuration = tickDuration;


	}



	public virtual void OnStart()
	{
		complete = false;
		currentTick = 0;

		if (TickDuration >= 0)
		{
			OnEnd();
		}

		//...
	}

	public virtual void PerTick()
	{
		//...





		if (currentTick >= TickDuration)
		{
			complete = true;
		}

		if (complete)
		{
			OnEnd();
		}

		currentTick++;
	}

	public virtual void OnEnd()
	{




	}

}