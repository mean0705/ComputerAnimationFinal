using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassingRoadHandler : MonoBehaviour
{
	public static PassingRoadHandler current = null;

	private void Awake()
	{
		current ??= this;
	}

	private void OnDestroy()
	{
		if (current == this)
			current = null;
	}

	public PassageHandler[] passageHandlers;

	public void Passing(GameObject pedestrian, Collider colliderThatCollision)
	{
		var pd = pedestrian.GetComponent<Pedestrain>();
		if (pd.isCrossedRoad)
			return;
		else
			pd.isCrossedRoad = true;

		foreach (var p in passageHandlers)
		{
			var thatway = p.CheckIfInArea(colliderThatCollision);
			if (thatway != PassageHandler.enum_PassingDirection.None)
				p.Passing_In(pedestrian, thatway);
		}
	}
}
