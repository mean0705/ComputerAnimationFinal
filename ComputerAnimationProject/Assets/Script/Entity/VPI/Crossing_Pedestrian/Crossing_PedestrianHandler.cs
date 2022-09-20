using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Crossing_PedestrianHandler : MonoBehaviour
{
	[SerializeField] Pedestrain pedestrain = null;

	[SerializeField] Rigidbody rigidbody_This;
	Collider collider_Target;
	Vector3 v3_Target;

	Vector3 v3_SpeedReality;
	static float sf_SpeedExpect = 1f * 2.25f;
	static float sf_SpeedMax = 1.34f * sf_SpeedExpect;

	static float sf_TDelta => TerrainHandler.sf_TDelta * 0.5f;
	static float sf_RelaxTime => TerrainHandler.sf_RelaxTime;
	static float sf_PedstrainVision => TerrainHandler.sf_PedstrainVision;
	static float sf_PedstrainVision_OutsideRate => TerrainHandler.sf_PedstrainVision_OutsideRate;

	public static List<Crossing_PedestrianHandler> list_ped = new List<Crossing_PedestrianHandler>();

	public bool isActivate => gameObject.activeInHierarchy;

	PassageHandler passageHandler;
	PassageHandler.enum_PassingDirection passingDirection;

	static float Vab0 = 2.1f * 2;
	static float sigma = 0.3f * 2;

	public float f_TGap; // gap between ped and nearest car
	public float f_TouGap;


	public bool isInsideLane => isVPIRunning;

	[SerializeField] bool isDebugLogOutput = false;
	// MinHua Added 
	// Pedestrian interaction
	public bool isVPIRunning = false;

	public float le => SystemHandler.current.params_SocialForceCrossing.le;
	public float R => SystemHandler.current.params_SocialForceCrossing.R;
	public float veh_A => SystemHandler.current.params_SocialForceCrossing.veh_A;
	public float veh_b => SystemHandler.current.params_SocialForceCrossing.veh_b;
	public float des_sigma => SystemHandler.current.params_SocialForceCrossing.des_sigma;
	public float des_k => SystemHandler.current.params_SocialForceCrossing.des_k;

	public float mu_gap => SystemHandler.current.params_SocialForceCrossing.mu_gap;
	public float sigma_gap => SystemHandler.current.params_SocialForceCrossing.sigma_gap;
	public float thr_gap => SystemHandler.current.params_SocialForceCrossing.thr_gap;

	public float k_des => SystemHandler.current.params_SocialForceCrossing.k_des;


	// 吸引力(暫不實作)
	// Dictionary<object, float> list_int_ThingsThatAttracted = new List<int>();

	public string Info =>
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
		Vector3 pedAccel = Vector3.zero;

		foreach (var ped in list_ped)
		{
			if (ped != this)
				if (isWatching(ped.transform.position))
					pedAccel += AwayFromOtherPedestrain(ped);
				else
					pedAccel += AwayFromOtherPedestrain(ped) * sf_PedstrainVision_OutsideRate;
		}
		// pedAccel = pedAccel.normalized;
		Debug.DrawLine(transform.position, transform.position + pedAccel, Color.yellow, sf_TDelta);

		if (isDebugLogOutput)
			Debug.Log($"{v3_SpeedReality} / {selfAccel} + {pedAccel}");
		return selfAccel + pedAccel;
	}

	/// <summary>
	/// 自我提速
	/// </summary>
	Vector3 AccelSelfSpeed(float? ExpectVelocity = null)
	{
		if (ExpectVelocity == null)
			ExpectVelocity = sf_SpeedExpect;

		Vector3 ret = (1 / sf_RelaxTime) * ((float)ExpectVelocity * GetDesiredDirection() - v3_SpeedReality);
		ret = Quaternion.Euler(0, UnityEngine.Random.Range(-.5f, .5f), 0) * ret;
		//Debug.DrawLine(transform.position + v3_SpeedReality, transform.position + v3_SpeedReality + ret, Color.cyan, sf_TDelta);
		Debug.DrawLine(transform.position, transform.position + sf_SpeedExpect * GetDesiredDirection(), Color.magenta, sf_TDelta);
		return ret;
	}

	/// <summary>
	/// 遠離行人
	/// </summary>
	Vector3 AwayFromOtherPedestrain(Crossing_PedestrianHandler beta)
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
	public void SetTarget(Transform t, PassageHandler ph, PassageHandler.enum_PassingDirection dir)
	{
		this.passageHandler = ph;
		this.passingDirection = dir;

		StopCoroutine(IUpdate());

		collider_Target = t.GetComponent<BoxCollider>();
		StartCoroutine(IUpdate());
	}

	void OnDrawGizmosSelected()
	{
		if (!isVPIRunning)
			return;

		Gizmos.color = Color.cyan;
		foreach (var veh in VehicleHandler.list_veh)
		{
			Vector3 PedPos = transform.position;
			Vector3 VehPos = veh.GetComponent<Collider>().ClosestPoint(PedPos);
			VehPos = new Vector3(VehPos.x, transform.position.y, VehPos.z);

			Vector3 v2p = PedPos - VehPos;
			if (Vector3.Dot(v2p, veh.v3_SpeedReality) <= 0) continue;

			Gizmos.DrawWireSphere(VehPos, .5f);
			Gizmos.DrawLine(transform.position, VehPos);
		}
	}


	IEnumerator IUpdate()
	{
		float posBias = rigidbody_This.velocity.z > 0 ? 1 : -1;

		v3_Target = collider_Target.ClosestPoint(transform.position);
		v3_SpeedReality = rigidbody_This.velocity = GetDesiredDirection() * UnityEngine.Random.Range(0.9f, 1.1f);
		yield return new WaitForFixedUpdate();
		while (true)
		{

			while (!isVPIRunning)
			{
				// waiting
				v3_Target = collider_Target.ClosestPoint(transform.position);
				f_TouGap = 0;

				Vector3 fv = Vector3.zero;

				float distance = f_TGap = float.MaxValue;
				Vector3 PedPos = transform.position;
				float d_v2p;
				Vector3 v2p;
				Vector3 VehPos;

				foreach (var veh in VehicleHandler.list_veh)
				{
					VehPos = veh.GetComponent<Collider>().ClosestPoint(PedPos);
					VehPos = new Vector3(VehPos.x, transform.position.y, VehPos.z);


					v2p = PedPos - VehPos;

					if (v2p.z * veh.v3_SpeedReality.normalized.z < -5) continue;

					d_v2p = v2p.magnitude; // direction veh to ped

					if (d_v2p < distance)
					{
						f_TGap = distance = d_v2p;

						float d_rem = Mathf.Abs(v3_Target.x - PedPos.x) + R; // remaining distance
						f_TouGap = d_rem / sf_SpeedMax * veh.rigidbody_This.velocity.magnitude;
					}
				}
				yield return new WaitForFixedUpdate();
			}

			Compute_T_gap();
			Vector3 old_next = transform.position + v3_SpeedReality;
			Debug.DrawLine(transform.position, old_next, Color.green, sf_TDelta);

			v3_Target = collider_Target.ClosestPoint(transform.position);
			v3_Target = new Vector3(v3_Target.x, v3_Target.y, v3_Target.z + posBias);

			//Debug.DrawLine(transform.position, v3_Target, Color.red, sf_TDelta);

			Vector3 force = DestinationForce();
			force += new Vector3(0, 0, 1) * UnityEngine.Random.Range(-0.1f, 0.1f);
			//Debug.Log("VPI : force : " + force);

			(Vector3 vec_force, Vector3 desiredAccelInDirection) = VehicleEffect();
			//Debug.Log("VPI : (veh)force : " + force);

			force += vec_force;

			Vector3 ped_force = PedEffect();
			force += ped_force;

			if (force.magnitude > 100)
				force = Vector3.zero;
			rigidbody_This.AddForce(force);
			//rigidbody_This.AddForce(force + desiredAccelInDirection);

			yield return new WaitForFixedUpdate();

			v3_SpeedReality = rigidbody_This.velocity;

			transform.LookAt(transform.position + v3_SpeedReality);

			Debug.DrawLine(transform.position, transform.position + v3_SpeedReality, Color.red, sf_TDelta);
			Debug.DrawLine(old_next, old_next + force, Color.blue, sf_TDelta);

			if (pedestrain)
			{
				pedestrain.pedestrainData.v3_Destination = transform.position + v3_SpeedReality;
				pedestrain.pedestrainData.f_Speed = v3_SpeedReality.magnitude;
			}

			yield return new WaitForSeconds(sf_TDelta);
		}

	}

	private void OnTriggerEnter(Collider collision)
	{
		// End
		if (collision == collider_Target)
		{
			StopAllCoroutines();
			//ModelPool.Recycle(gameObject);
			passageHandler.Passing_Out(gameObject, passingDirection);
		}
	}

	// MinHua Add

	Vector3 DestinationForce()
	{

		float v0 = 1.28f;
		Vector3 s_des = new Vector3(this.v3_Target.x, this.transform.position.y, this.v3_Target.z);

		Vector3 s_p = this.transform.position;
		float des_sigma = 1.0f;

		Vector3 v_ped = this.v3_SpeedReality;
		Vector3 v_desired_ped = v0 * ((s_des - s_p)
			/ Mathf.Sqrt(Mathf.Pow((s_des - s_p).magnitude, 2) + Mathf.Pow(des_sigma, 2)));

		Vector3 f_des = k_des * (v_ped - v_desired_ped);
		// Vector3 f_des = k_des * (v_desired_ped - v_ped );

		Debug.DrawLine(transform.position, transform.position + f_des, Color.green, sf_TDelta);
		return f_des;
	}

	Vector3 PedEffect()
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
		Debug.DrawLine(transform.position, transform.position + pedAccel, Color.yellow, sf_TDelta);
		return pedAccel;
	}

	(Vector3, Vector3) VehicleEffect()
	{
		//float veh_A = 200.0f;
		//float veh_b = 0.26f;
		Vector3 fv = Vector3.zero;

		float distance = float.MaxValue;
		VehicleHandler nearest_Veh = null;
		Vector3 PedPos = transform.position;
		float d_v2p;
		Vector3 v2p;
		Vector3 VehPos;

		foreach (var veh in VehicleHandler.list_veh)
		{

			if (!veh.isActivate)
				continue;

			VehPos = veh.GetComponent<Collider>().ClosestPoint(PedPos);
			VehPos = new Vector3(VehPos.x, transform.position.y, VehPos.z);


			v2p = PedPos - VehPos;

			float effectFactor = Vector3.Dot(v2p.normalized, veh.v3_SpeedReality.normalized);
			if (effectFactor <= 0) continue;

			d_v2p = v2p.magnitude; // direction veh to ped

			if (d_v2p < distance)
			{
				d_v2p = distance;
				nearest_Veh = veh;
			}
		}

		if (nearest_Veh == null)
			return (Vector3.zero, Vector3.zero);

		VehPos = nearest_Veh.GetComponent<Collider>().ClosestPoint(PedPos);
		VehPos = new Vector3(VehPos.x, transform.position.y, VehPos.z);

		v2p = PedPos - VehPos;

		d_v2p = v2p.magnitude; // direction veh to ped

		Vector3 n_v2p = (PedPos - VehPos) / d_v2p;
		float temp = Mathf.Abs(d_v2p);// - self.ped.R;

		fv = veh_A * Mathf.Exp(-veh_b * (d_v2p - le - R)) * n_v2p;

		// dot to desired direction
		int dir = 0;
		switch (this.passingDirection)
		{
			case PassageHandler.enum_PassingDirection.None:
				break;
			case PassageHandler.enum_PassingDirection.Forward:
				dir = ((VehPos - PedPos).x > 0) ? 1 : -1;
				break;
			case PassageHandler.enum_PassingDirection.Reverse:
				dir = ((VehPos - PedPos).x > 0) ? -1 : 1;
				break;
			default:
				break;
		}
		float mag = fv.magnitude * dir;
		fv = mag * GetDesiredDirection();

		// time to collision
		float spd = nearest_Veh.GetComponent<Rigidbody>().velocity.z;
		float d_lon = PedPos.z - R - VehPos.z;
		float ttc = d_lon / spd; // time to collision

		float d_rem = v3_Target.x - PedPos.x + R; // remaining distance
		float ttf = d_rem / rigidbody_This.velocity.x; // time to clear the crosswalk for pedestrian


		float vd_adjust = d_rem / ttc;
		Vector3 fd_adjust = (0 < ttc && ttc < ttf && vd_adjust < PedestrianHandler.sf_SpeedMax) ?
			AccelSelfSpeed(vd_adjust) : Vector3.zero; // self-accel

		Debug.DrawLine(transform.position, transform.position + fv, Color.magenta, sf_TDelta);
		return (fv, fd_adjust);

	}

	void Compute_T_gap()
	{
		float min_dist = 99999;
		VehicleHandler closetVeh = null;
		foreach (var veh in VehicleHandler.list_veh)
		{
			Vector3 VehPos = veh.transform.position;
			Vector3 PedPos = transform.position;
			float dist = Mathf.Abs((VehPos - PedPos).magnitude);

			if (dist < min_dist)
			{
				min_dist = dist;
				closetVeh = veh;
			}
		}

		if (closetVeh != null) this.pedestrain.f_TGap = min_dist / closetVeh.v3_SpeedReality.magnitude;
		else this.pedestrain.f_TGap = 1.0f;

		// Debug.Log("[Min Debug], f_TGap : " + this.pedestrain.f_TGap);
	}

}
