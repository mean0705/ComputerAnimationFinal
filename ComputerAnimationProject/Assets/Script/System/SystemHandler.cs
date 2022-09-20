using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ev_DateTimeCycleEventArgs : EventArgs
{
	public Ev_DateTimeCycleEventArgs(bool isNight)
	{
		this.isNight = isNight;
	}
	public bool isNight;
}

public class SystemHandler : MonoBehaviour
{
	// singleton //
	public static SystemHandler current = null;
	private void Awake()
	{
		if (current is null)
			current = this;
	}
	private void OnDestroy()
	{
		if (current == this)
			current = null;
	}
	// singleton end //

	[SerializeField] public params_SocialForceCrossing params_SocialForceCrossing;

	[Space(10)]
	[SerializeField] GameObject gobj_DirectionalLight;
	Light light_Directional;
	[SerializeField] int int_Time = 0;
	[SerializeField] int int_TimeStep = 1;
	[SerializeField] int int_DawnTime = 6000;
	[SerializeField] int int_NightTime = 18000;
	[SerializeField] int int_DayTimeTotal = 24000;
	bool isNight = true;
	public EventHandler ev_TimeCycle;



	// Start is called before the first frame update
	void Start()
	{
		light_Directional = gobj_DirectionalLight.GetComponent<Light>();
		SetTime(int_DawnTime);
	}


	// Update is called once per frame
	void Update()
	{
		SetTime(int_Time + int_TimeStep);

		if (int_Time > int_NightTime && !isNight)
		{
			isNight = true;
			ev_TimeCycle?.Invoke(this, new Ev_DateTimeCycleEventArgs(true));
		}
		else if (int_Time <= int_NightTime && int_Time > int_DawnTime && isNight)
		{
			isNight = false;
			ev_TimeCycle?.Invoke(this, new Ev_DateTimeCycleEventArgs(false));
		}
		else if (int_Time > int_DayTimeTotal)
		{
			int_Time = 0;
		}
	}

	void SetTime(int time)
	{
		int_Time = time;
		float angle = ((time + int_DayTimeTotal - int_DawnTime) % int_DayTimeTotal) * 1.0f / int_DayTimeTotal * 360.0f;

		if (angle > 0 && angle < 180)
		{
			light_Directional.shadows = LightShadows.Soft;
		}
		else
		{
			light_Directional.shadows = LightShadows.None;
		}


		light_Directional.intensity = (angle > 0 && angle < 180) ? MathF.Sin(angle / 180 * MathF.PI) : 0;

		gobj_DirectionalLight.transform.rotation =
			Quaternion.Euler(new Vector3(angle, 170, 0));
	}
}
