using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrainData
{
	public const float cf_ReachPointThreshold = 0.25f;
	public Vector3 v3_Destination = Vector3.zero;
	public float f_Speed = 0;
}

[SelectionBase]
public class Pedestrain : MonoBehaviour
{
	public enum Phase
	{
		Initializing,
		Approaching,
		Waitng,
		Crossing,
		Finishing
	}
	public Phase phase;
	public float f_TouGap;
	public float f_TGap;

	bool isCrossingRoad = false;
	[SerializeField] bool isHeuristic = false;

	[SerializeField] Animator animator;

	[Space(10)]
	[SerializeField] float f_RunSpeedThreshold;


	public PedestrainData pedestrainData;

	Crossing_PedestrianHandler c_ph;
	PedestrianHandler ph;

	[SerializeField] Rigidbody rigidbody_This;


	public bool isCrossedRoad = false;

	Vector3 v3_PositionNow => gameObject.transform.position;



	bool isInsideWait;


	// Start is called before the first frame update
	void Start()
	{
		pedestrainData = new PedestrainData();
		animator.SetInteger("arms", 5);
		animator.SetInteger("legs", 5);


		c_ph = gameObject.GetComponent<Crossing_PedestrianHandler>();
		ph = gameObject.GetComponent<PedestrianHandler>();
	}

	// Update is called once per frame
	void Update()
	{
		pedestrainData.f_Speed = rigidbody_This.velocity.magnitude;
		if (pedestrainData.f_Speed > PedestrianHandler.sf_SpeedMax)
			rigidbody_This.velocity = PedestrianHandler.sf_SpeedMax * rigidbody_This.velocity.normalized;

		if (pedestrainData.f_Speed < f_RunSpeedThreshold)
		{
			if (Vector3.Distance(transform.position, pedestrainData.v3_Destination) > PedestrainData.cf_ReachPointThreshold)
			{
				animator.SetInteger("arms", 1);
				animator.SetInteger("legs", 1);
			}
			else
			{
				animator.SetInteger("arms", 5);
				animator.SetInteger("legs", 5);
			}

		}
		else
		{

			if (Vector3.Distance(transform.position, pedestrainData.v3_Destination) > PedestrainData.cf_ReachPointThreshold)
			{
				animator.SetInteger("arms", 2);
				animator.SetInteger("legs", 2);
			}
			else
			{
				animator.SetInteger("arms", 5);
				animator.SetInteger("legs", 5);
			}

		}

	}

	IEnumerator StateMachine_NextState()
	{
		switch (phase)
		{
			case Phase.Initializing:
				phase = Phase.Approaching;
				break;
			case Phase.Approaching:
				ph.isIgnoreBorder = true;
				yield return new WaitUntil(() => InsideWait());
				phase = Phase.Waitng;
				break;
			case Phase.Waitng:
				yield return new WaitUntil(() => CompareGap());
				phase = Phase.Crossing;
				break;
			case Phase.Crossing:
				yield return new WaitWhile(() => InsideLane());
				phase = Phase.Finishing;
				break;
			case Phase.Finishing:
				break;
			default:
				throw new System.Exception();
		}
		yield return new WaitForFixedUpdate();
		StateMachine_StartState();
	}

	public void StateMachine_StartState()
	{
		switch (phase)
		{
			case Phase.Initializing:

				isInsideWait = false;

				break;
			case Phase.Approaching:
				break;
			case Phase.Waitng:
				rigidbody_This.velocity *= 0.1f;
				ph.isSFMRunning = false;
				ph.isIgnoreBorder = false;
				c_ph.isVPIRunning = false;
				break;
			case Phase.Crossing:
				ph.isSFMRunning = false;
				ph.isIgnoreBorder = false;
				c_ph.isVPIRunning = true;
				break;
			case Phase.Finishing:
				phase = Phase.Initializing;
				return;
			default:
				throw new System.Exception();
		}

		StartCoroutine(StateMachine_NextState());
	}


	private void OnTriggerEnter(Collider collision)
	{
		// Waiting
		if (collision.gameObject.tag == "Passing_Wait")
		{
			isInsideWait = true;
		}
	}

	bool InsideWait() => isInsideWait;



	bool CompareGap()
	{
		f_TGap = c_ph.f_TGap;
		f_TouGap = c_ph.f_TouGap;
		return f_TGap > f_TouGap;
	}

	bool InsideLane() => c_ph.isInsideLane;
}
