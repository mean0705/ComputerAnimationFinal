using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle_StartSide : MonoBehaviour
{
	// Start is called before the first frame update
	[SerializeField] Transform t_AnotherSide;
	[SerializeField] Transform t_Cars;
	[SerializeField] Color color;
	public bool isDirectionInversed = false;

	static float sf_North => 2.0f;
	static float sf_South => -2.0f;
	void Start()
	{
		StartCoroutine(IUpdate());
	}

	// Update is called once per frame
	IEnumerator IUpdate()
	{
		yield return new WaitForEndOfFrame();

		while (true)
		{
			for (int i = 0; i < 1; i++)
			{
				float randPos = Random.Range(sf_South, sf_North);
				GameObject gobj = VehiclePool.GetModel();

				gobj.transform.SetParent(t_Cars);

				gobj.transform.position = new Vector3(
					transform.position.x,
					t_Cars.position.y,
					transform.position.z);

				// gobj.transform.position = transform.position;

				VehicleHandler ph = gobj.GetComponent<VehicleHandler>();
				ph.isDirectionInversed = isDirectionInversed;
				ph.SetTarget(t_AnotherSide);
			}
			yield return new WaitForSeconds(Random.Range(12, 16));
		}
	}
}
