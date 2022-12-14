using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

static class BordersRepulsivePotential
{
	static float Uab0 = 10f;
	static float R = .2f; // 0.2

	public static Vector3 CountForce(Collider nearestBorder, Vector3 point, out bool isCollide, out Vector3 v3_NearestPositionToBoarder)
	{

		// magnitude of force
		v3_NearestPositionToBoarder = nearestBorder.ClosestPoint(point);
		float dist = Vector3.Distance(v3_NearestPositionToBoarder, point);
		isCollide = dist < R * 0.5;

		float mag(Vector3 point) => (float)(Uab0 * Math.Exp(-Vector3.Distance(nearestBorder.ClosestPoint(point), point) / R));

		float delta = 1e-2f;
		Vector3 dx = new Vector3(delta, 0, 0);
		Vector3 dy = new Vector3(0, 0, delta);

		float v = mag(point);
		float dvdx = (mag(point + dx) - v);
		float dvdy = (mag(point + dy) - v);
		Vector3 grad = new Vector3(dvdx, 0, dvdy);

		return -1 * grad * v;
		//float mag = Uab0* Mathf.Pow(Mathf.Exp(1), -Vector3.Distance(nearestBorder.ClosestPoint(point), point) / R);
		//Vector3 dir = point - nearestBorder.ClosestPoint(point);
		//return dir * mag;

	}
}

public class PedestrianHandler : MonoBehaviour
{
	[SerializeField] Pedestrain pedestrain = null;

	[SerializeField] Rigidbody rigidbody_This;
	Collider collider_Target;
	Vector3 v3_Target;

	Vector3 v3_SpeedReality;
	static float sf_SpeedExpect = 1f * 2.25f;
	public static float sf_SpeedMax { get; private set; } = 1.34f * sf_SpeedExpect;

	static float sf_TDelta => TerrainHandler.sf_TDelta;
	static float sf_RelaxTime => TerrainHandler.sf_RelaxTime;
	static float sf_PedstrainVision => TerrainHandler.sf_PedstrainVision;
	static float sf_PedstrainVision_OutsideRate => TerrainHandler.sf_PedstrainVision_OutsideRate;

	static List<PedestrianHandler> list_ped = new List<PedestrianHandler>();

	static float Vab0 = 2.1f * 2;
	static float sigma = 0.3f * 2;

	[SerializeField] bool isDebugLogOutput = false;

	Vector3 v3_NearestPositionToBoarder;

	public bool isSFMRunning = true;

	public bool isIgnoreOtherPed = false;
	public bool isIgnoreBorder = false;

	public bool isActivate => gameObject.activeInHierarchy;

	// 吸引力(暫不實作)
	// Dictionary<object, float> list_int_ThingsThatAttracted = new List<int>();

	public static string Info =>
		$"SpeedExpect(V_alpha) = {sf_SpeedExpect}\n" +
		$"SpeedMax(V_alpha_max) = {sf_SpeedMax}\n" +
		$"\n" +
		$"Vab0 = {Vab0}\n" +
		$"sigma = {sigma}\n" +
		$"delta T = {sf_TDelta}";

	Vector3 GetDesiredDirection()
	{
		return (v3_Target - transform.position).normalized;
	}
	Vector3 GetTotalForce()
	{
		Vector3 selfAccel = AccelSelfSpeed();

		bool isCollide = false;
		Vector3 borderAccel = isIgnoreBorder? Vector3.zero :AwayFromBorder(BorderMarker.list_col_AllBorder, out isCollide);

		if (!isCollide && !isIgnoreOtherPed)
		{
			Vector3 pedAccel = Vector3.zero;
			foreach (var ped in list_ped)
			{
				if (!ped.isActivate)
					continue;

				if (ped != this)
					if (isWatching(ped.transform.position))
						pedAccel += AwayFromOtherPedestrain(ped);
					else
						pedAccel += AwayFromOtherPedestrain(ped) * sf_PedstrainVision_OutsideRate;
			}
			// pedAccel = pedAccel.normalized;
			Debug.DrawLine(transform.position, transform.position + pedAccel, Color.yellow, sf_TDelta);
			if (isDebugLogOutput)
				Debug.Log($"{v3_SpeedReality} / {selfAccel} + {pedAccel} + {borderAccel}");
			return selfAccel + pedAccel + borderAccel;
		}
		else
		{
			return (selfAccel * 0.5f + borderAccel);
		}

	}

	/// <summary>
	/// 自我提速
	/// </summary>
	Vector3 AccelSelfSpeed()
	{
		Vector3 ret = (1 / sf_RelaxTime) * (sf_SpeedExpect * GetDesiredDirection() - v3_SpeedReality);
		ret = Quaternion.Euler(0, UnityEngine.Random.Range(-.5f, .5f), 0) * ret;
		//Debug.DrawLine(transform.position + v3_SpeedReality, transform.position + v3_SpeedReality + ret, Color.cyan, sf_TDelta);
		Debug.DrawLine(transform.position, transform.position + sf_SpeedExpect * GetDesiredDirection(), Color.magenta, sf_TDelta);
		return ret;
	}

	/// <summary>
	/// 遠離行人
	/// </summary>
	Vector3 AwayFromOtherPedestrain(PedestrianHandler beta)
	{
		Vector3 sBeta = beta.v3_SpeedReality;
		Vector3 rAB = transform.position - beta.transform.position;

		// magnitude of force
		float value_rAB(Vector3 rAB, Vector3 sBeta, Vector3 dir)
		{
			float b = (1 / 2f) * Mathf.Sqrt(
				Mathf.Pow(rAB.magnitude + (rAB - sBeta.magnitude * beta.GetDesiredDirection()).magnitude, 2)
				- Mathf.Pow(sBeta.magnitude, 2)
				);

			return Vab0 * Mathf.Pow(Mathf.Exp(1), -b / sigma);
		}

		Vector3 dir = beta.v3_SpeedReality.normalized;

		float delta = 1e-4f;
		Vector3 dx = new Vector3(delta, 0, 0);
		Vector3 dy = new Vector3(0, 0, delta);

		float v = value_rAB(rAB, sBeta, dir);
		float dvdx = (value_rAB(rAB + dx, sBeta, dir) - v) / delta;
		float dvdy = (value_rAB(rAB + dy, sBeta, dir) - v) / delta;
		Vector3 grad = new Vector3(dvdx, 0, dvdy);

		Vector3 fAB = -1 * grad * v;

		return fAB;
	}

	Vector3 AwayFromBorder(List<Collider> list_col_Border, out bool isCollide)
	{
		Vector3 pt_here = transform.position;

		var nearestBorder =
			list_col_Border.Aggregate(
				(a, b) => Vector3.Distance(a.ClosestPointOnBounds(pt_here), pt_here) < Vector3.Distance(b.ClosestPointOnBounds(pt_here), pt_here) ? a : b);

		var force = BordersRepulsivePotential.CountForce(nearestBorder, pt_here, out isCollide, out v3_NearestPositionToBoarder);

		Debug.DrawLine(transform.position, transform.position + force, Color.cyan, sf_TDelta);
		return force;
	}

	/// <summary>
	/// 被物件吸引
	/// </summary>
	Vector3 AttractBySomething()
	{
		return Vector3.zero;
	}

	bool isWatching(Vector3 point)
	{
		var angle = Mathf.Abs(Vector3.Angle(v3_SpeedReality, point));
		if (angle > 180)
			angle = 360 - angle;
		return angle < sf_PedstrainVision / 2;
	}

	//-------------------------------------------------------------------------//

	private void Awake()
	{
		list_ped.Add(this);
	}
	private void OnDestroy()
	{
		list_ped.Remove(this);
	}
	public void SetTarget(Transform t)
	{
		StopCoroutine(IUpdate());

		collider_Target = t.GetComponent<BoxCollider>();
		StartCoroutine(IUpdate());
	}
	void OnDrawGizmosSelected()
	{
		if (!isSFMRunning)
			return;

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(v3_NearestPositionToBoarder, .5f);
		Gizmos.DrawLine(transform.position, v3_NearestPositionToBoarder);
	}

	IEnumerator IUpdate()
	{
		v3_Target = collider_Target.ClosestPoint(transform.position);
		v3_SpeedReality = rigidbody_This.velocity = GetDesiredDirection() * UnityEngine.Random.Range(0.9f, 1.1f);
		yield return new WaitForFixedUpdate();
		while (true)
		{

			if (!isSFMRunning)
			{
				yield return new WaitUntil(() => isSFMRunning);
			}


			Vector3 old_next = transform.position + v3_SpeedReality;
			Debug.DrawLine(transform.position, old_next, Color.green, sf_TDelta);

			v3_Target = collider_Target.ClosestPoint(transform.position);

			Vector3 force = GetTotalForce();
			force += new Vector3(0, 0, 1) * UnityEngine.Random.Range(-0.1f, 0.1f);

			//Debug.Log("SFM : force : " + force);

			if (float.IsNaN( force.x))
				force = Vector3.zero;
			rigidbody_This.AddForce(force);

			yield return new WaitForFixedUpdate();

			v3_SpeedReality = rigidbody_This.velocity;
			if (v3_SpeedReality.magnitude > sf_SpeedMax)
				v3_SpeedReality = rigidbody_This.velocity = sf_SpeedMax * v3_SpeedReality.normalized;


			Debug.DrawLine(transform.position, transform.position + v3_SpeedReality, Color.red, sf_TDelta);
			// Debug.DrawLine(old_next, old_next + force, Color.blue, sf_TDelta);

			transform.LookAt(transform.position + v3_SpeedReality);

			if (pedestrain)
			{
				pedestrain.pedestrainData.v3_Destination = transform.position + v3_SpeedReality;
				pedestrain.pedestrainData.f_Speed = v3_SpeedReality.magnitude;
			}

			yield return new WaitForSeconds(sf_TDelta * UnityEngine.Random.Range(0.4f, 0.5f));
		}

	}

	private void OnTriggerEnter(Collider collision)
	{
		if (collision.gameObject.CompareTag("Passage"))
		{
			var passingRate = Random.Range(0, 100) <= 30;
			if (passingRate)
			{
				PassingRoadHandler.current.Passing(gameObject, collision);
			}
		}

		// End
		if (collision == collider_Target && collision.gameObject.tag != "Passing_Wait")
		{
			StopAllCoroutines();
			ModelPool.Recycle(gameObject);
		}
	}


}