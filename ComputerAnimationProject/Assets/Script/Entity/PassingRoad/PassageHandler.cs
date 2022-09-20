using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassageHandler : MonoBehaviour
{
	[SerializeField] Collider collider_Entry;
	[SerializeField] Collider collider_WaitingEntry;
	[SerializeField] Collider collider_Exit;
	[SerializeField] Collider collider_WaitingExit;

	[SerializeField] Transform[] t_EntrySide;
	[SerializeField] Transform[] t_ExitSide;

	public enum enum_PassingDirection
	{
		None,
		Forward,
		Reverse
	}

	public enum_PassingDirection CheckIfInArea(Collider colliderThatCollision)
	{
		if (colliderThatCollision == collider_Entry)
			return enum_PassingDirection.Forward;
		else if (colliderThatCollision == collider_Exit)
			return enum_PassingDirection.Reverse;
		return enum_PassingDirection.None;
	}

	public void Passing_In(GameObject gobj_Pedestrian, enum_PassingDirection dir)
	{
		Crossing_PedestrianHandler c_ph = gobj_Pedestrian.GetComponent<Crossing_PedestrianHandler>();

		Pedestrain p = gobj_Pedestrian.GetComponent<Pedestrain>();
		p.StateMachine_StartState();

		PedestrianHandler ph = gobj_Pedestrian.GetComponent<PedestrianHandler>();

		// switch mode
		Pedestrain pedestrain = gobj_Pedestrian.GetComponent<Pedestrain>();

		Vector3 v3_targetPos;
		switch (dir)
		{
			case enum_PassingDirection.Forward:
				v3_targetPos = collider_Exit.transform.position;
				c_ph.SetTarget(collider_Exit.transform, this, dir);


				ph.SetTarget(collider_WaitingEntry.transform);
				break;
			case enum_PassingDirection.Reverse:
				v3_targetPos = collider_Entry.transform.position;
				c_ph.SetTarget(collider_Entry.transform, this, dir);


				ph.SetTarget(collider_WaitingExit.transform);
				break;
			case enum_PassingDirection.None:
			default:
				return;
		}
		pedestrain.pedestrainData.v3_Destination = v3_targetPos;

		// StartCoroutine(Debug_Passing(gobj_Pedestrian, dir, v3_targetPos));
	}


	public void Passing_Out(GameObject gobj_Pedestrian, enum_PassingDirection dir)
	{
		Rigidbody rigidbody = gobj_Pedestrian.GetComponent<Rigidbody>();
		int side = rigidbody.velocity.z < 0 ? 0 : 1;

		PedestrianHandler ph = gobj_Pedestrian.GetComponent<PedestrianHandler>();
		ph.isSFMRunning = true;

		// switch mode
		Crossing_PedestrianHandler c_ph = gobj_Pedestrian.GetComponent<Crossing_PedestrianHandler>();
		c_ph.isVPIRunning = false;
		Pedestrain pedestrain = gobj_Pedestrian.GetComponent<Pedestrain>();
		switch (dir)
		{
			case enum_PassingDirection.Forward:
				ph.SetTarget(t_ExitSide[side]);
				break;
			case enum_PassingDirection.Reverse:
				ph.SetTarget(t_EntrySide[side]);
				break;
			case enum_PassingDirection.None:
			default:
				return;
		}
	}

	// Vehicle P
	IEnumerator Debug_Passing(GameObject gobj_Pedestrian, enum_PassingDirection dir, Vector3 v3_targetPos)
	{
		Rigidbody rb = gobj_Pedestrian.GetComponent<Rigidbody>();
		Vector3 dist;
		do
		{
			dist = (v3_targetPos - gobj_Pedestrian.transform.position);
			dist = new Vector3(dist.x, 0, dist.z);

			gobj_Pedestrian.transform.LookAt(gobj_Pedestrian.transform.position + dist);
			rb.velocity = dist.normalized;
			yield return new WaitForSeconds(.1f);
			// yield return new WaitUntil(isa);
		} while (dist.sqrMagnitude > 1);

		Passing_Out(gobj_Pedestrian, dir);
	}


}
