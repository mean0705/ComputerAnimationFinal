using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public partial class VehicleHandler : MonoBehaviour
{
	[SerializeField] Vehicle vehicle = null;
	[SerializeField] GameObject vehicle_light = null;
	public bool isDirectionInversed = false;

	public Rigidbody rigidbody_This;
	Collider collider_Target;
	Vector3 v3_Target;

	public Vector3 v3_SpeedReality;
	static float sf_SpeedExpect = 1f * 20.25f;
	static float sf_SpeedMax = 1.34f * sf_SpeedExpect;

	static float sf_TDelta = 0.05f;


	public static List<VehicleHandler> list_veh = new List<VehicleHandler>();

	static float Vab0 = 2.1f * 2;
	static float sigma = 0.3f * 2;



	[SerializeField] bool isDebugLogOutput = false;

	public bool isActivate => gameObject.activeInHierarchy;

	// 吸引力(暫不實作)
	// Dictionary<object, float> list_int_ThingsThatAttracted = new List<int>();

	// Minhua Added
	// perception
	public List<int> t_traj = new List<int>();
	public List<List<float>> pred_traj;

	// interaction
	float K_P_dist = 1.0f;
	float K_P_speed = 1.0f;
	float K_I_speed = 0.1f;
	float K_D_speed = 0.0f;

	Vector3 v0_veh = new Vector3(0, 0, 6.0f);
	Vector3 speed_intergal = new Vector3(0, 0, 0);
	Vector3 last_velocity = new Vector3(0, 0, 0);

	float d_safe = 8.0f;
	float v_max = 22.5f;
	float v_min = 0.0f;
	float u_max = 20.0f;
	float u_min = -7.0f;
	float du_max = 5.0f;
	float du_min = -5.0f;

	public string control = "VKC"; // "OAC"

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
		Debug.DrawLine(transform.position, transform.position, Color.yellow, sf_TDelta);
		if (isDebugLogOutput)
			Debug.Log($"{v3_SpeedReality} / {selfAccel}");
		return selfAccel;
	}

	/// <summary>
	/// 自我提速
	/// </summary>
	Vector3 AccelSelfSpeed()
	{
		Vector3 ret = sf_SpeedExpect * GetDesiredDirection() - v3_SpeedReality;
		ret = Quaternion.Euler(0, UnityEngine.Random.Range(-.5f, .5f), 0) * ret;
		//Debug.DrawLine(transform.position + v3_SpeedReality, transform.position + v3_SpeedReality + ret, Color.cyan, sf_TDelta);
		Debug.DrawLine(transform.position, transform.position + sf_SpeedExpect * GetDesiredDirection(), Color.magenta, sf_TDelta);
		return ret;
	}

	/// <summary>
	/// 遠離行人
	/// </summary>

	//-------------------------------------------------------------------------//

	private void OnEnable()
	{
		list_veh.Add(this);
	}
	private void OnDisable()
	{
		list_veh.Remove(this);
	}
	public void SetTarget(Transform t)
	{
		StopCoroutine(IUpdate());

		collider_Target = t.GetComponent<BoxCollider>();
		StartCoroutine(IUpdate());
	}

	IEnumerator IUpdate()
	{
		if (isDirectionInversed)
		{
			v0_veh *= -1;
		}

		v3_Target = collider_Target.ClosestPoint(transform.position);
		v3_SpeedReality = v0_veh;
		rigidbody_This.velocity = v3_SpeedReality;
		vehicle_light.SetActive(false);
		// v3_SpeedReality = rigidbody_This.velocity = GetDesiredDirection() * UnityEngine.Random.Range(0.9f, 1.1f);
		yield return new WaitForFixedUpdate();
		while (true)
		{
			Vector3 old_next = transform.position + v3_SpeedReality;
			Debug.DrawLine(transform.position, old_next, Color.green, sf_TDelta);

			v3_Target = collider_Target.ClosestPoint(transform.position);

			Vector3 force = generateControl();

			// force += new Vector3(0, 0, 1) * UnityEngine.Random.Range(-0.1f, 0.1f);
			rigidbody_This.AddForce(force);

			yield return new WaitForFixedUpdate();

			v3_SpeedReality = rigidbody_This.velocity;
			// if (v3_SpeedReality.magnitude > sf_SpeedMax)
			//     v3_SpeedReality = rigidbody_This.velocity = sf_SpeedMax * v3_SpeedReality.normalized;

			transform.LookAt(transform.position + v3_SpeedReality);

			Debug.DrawLine(transform.position, transform.position + v3_SpeedReality, Color.red, sf_TDelta);
			Debug.DrawLine(transform.position, transform.position + force / rigidbody_This.mass, Color.blue, sf_TDelta);
			// Debug.DrawLine(old_next, old_next + force, Color.blue, sf_TDelta);

			if (vehicle)
			{
				vehicle.vehicleData.v3_Destination = transform.position + v3_SpeedReality;
				vehicle.vehicleData.f_Speed = v3_SpeedReality.magnitude;
			}

			// yield return new WaitForSeconds(sf_TDelta * UnityEngine.Random.Range(9.9f, 10.1f));
			yield return new WaitForSeconds(sf_TDelta);
		}

	}

	Vector3 generateControl()
	{
		Vector3 u = new Vector3(0, 0, 0);
		Vector3 vel_veh = this.v3_SpeedReality;
		Vector3 pos_veh = this.transform.position;



		if (isDebugLogOutput) Debug.Log("[MinHua]vel_veh = " + vel_veh);

		speed_intergal = speed_intergal + (v0_veh - vel_veh);
		if (isDebugLogOutput) Debug.Log("[MinHua]speed_intergal = " + speed_intergal);

		// 找到最近的行人 且正在crossing中 算與最近的行人的距離
		float min_dist = float.MaxValue;
		foreach (var ped in Crossing_PedestrianHandler.list_ped)
		{
			Vector3 ped_pos = ped.transform.position;

			float distance = (ped_pos - pos_veh).magnitude;
			bool isFront = false;
			float interval = isDirectionInversed ? -2 : 2.0f;
			if (ped_pos.z - interval > pos_veh.z && vel_veh.z > 0) isFront = true;
			else if (ped_pos.z + interval < pos_veh.z && vel_veh.z < 0) isFront = true;
			else isFront = false;

			// if (ped.isVPIRunning && isFront)
			//     Debug.DrawLine(pos_veh, ped_pos, Color.magenta, 1.0f);

			if (distance < min_dist && isFront && ped.isVPIRunning)
			{
				min_dist = distance;
			}
		}

		if (isDebugLogOutput) Debug.Log("[MinHua]min_dist = " + min_dist);
		// 算出 u 來調整vehicle 的速度
		float delta_t = Time.deltaTime;
		float u_z = 0;
		// if (min_dist == float.MaxValue) return new Vector3(0, 0, 0);

		if (min_dist < 17)
		{
			float d_dec = min_dist - d_safe;

			if (d_dec < 0)
			{
				u_z = -30.0f;
				if (isDebugLogOutput) Debug.Log("[MinHua]u_z -> u_min = " + u_z);
			}
			else
			{
				u_z = -1.0f * Mathf.Pow(Mathf.Abs(vel_veh.z), 2) / (2 * d_dec);
				if (isDebugLogOutput) Debug.Log("[MinHua]u_z -> u_compute = " + u_z);
			}
		}
		else
		{
			u_z = K_P_speed * (v0_veh.z - vel_veh.z) + K_I_speed * speed_intergal.z;
		}


		// u_z = Mathf.Max(u_z, vel_veh.z + du_min);
		// u_z = Mathf.Min(u_z, vel_veh.z + du_max);
		u_z = Mathf.Max(u_z, u_min);
		// u_z = Mathf.Min(u_z, u_max);


		u = new Vector3(0, 0, u_z);
		if (isDebugLogOutput) Debug.Log("[MinHua]u = " + u);

		float mass = this.rigidbody_This.mass;
		float alpha = 4.0f;


		Vector3 ForceNew = vel_veh * (-1 * (alpha / mass)) + u + new Vector3(0, 0, 0.35f);
		if (vel_veh.z < 0)
		{
			if (isDebugLogOutput) Debug.Log("[MinHua]Reverse!!!");
		}
		if (isDebugLogOutput) Debug.Log("[MinHua]ForceNew = " + ForceNew);
		ForceNew = ForceNew * 30;
		last_velocity = vel_veh;

		if (ForceNew.z < -1)
		{
			Debug.DrawLine(transform.position, transform.position + new Vector3(0, 1, 0) * (ForceNew.z), Color.red, sf_TDelta);
			vehicle_light.SetActive(true);
		}
		else
		{
			vehicle_light.SetActive(false);
		}


		if (isDirectionInversed)
		{
			//ForceNew *= -1;
		}

		return ForceNew;
	}

	private void OnTriggerEnter(Collider collision)
	{
		// End
		if (collision == collider_Target)
		{
			StopAllCoroutines();
			VehiclePool.Recycle(gameObject);
		}
	}
}
