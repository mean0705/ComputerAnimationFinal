using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightSelfControl : MonoBehaviour
{
	[SerializeField] GameObject gobj_LightObject;
	[SerializeField] GameObject gobj_LightMesh;

	// Start is called before the first frame update
	void Start()
	{
		StartCoroutine(IStart());
	}

	IEnumerator IStart()
	{
		yield return new WaitUntil(() => SystemHandler.current != null);
		SystemHandler.current.ev_TimeCycle += LightControl;
	}

	public void LightControl(object s, EventArgs e)
	{
		if (gobj_LightObject != null)
			gobj_LightObject.SetActive((e as Ev_DateTimeCycleEventArgs).isNight);

		if (gobj_LightMesh != null)
			gobj_LightMesh.SetActive((e as Ev_DateTimeCycleEventArgs).isNight);
	}

}
