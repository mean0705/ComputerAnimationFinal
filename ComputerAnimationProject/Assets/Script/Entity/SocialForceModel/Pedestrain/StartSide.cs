using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSide : MonoBehaviour
{
	[SerializeField] Transform t_AnotherSide;
	[SerializeField] Transform t_Pedestrains;
	[SerializeField] Color color;

	static float sf_North => 1.5f;
	static float sf_South => -1.5f;
	// Start is called before the first frame update
	void Start()
	{
		StartCoroutine(IUpdate());
	}

	IEnumerator IUpdate()
	{
		yield return new WaitForEndOfFrame();

		while (true)
		{
			for (int i = 0; i < 1; i++)
			{
				float randPos = Random.Range(sf_South, sf_North);
				GameObject gobj = ModelPool.GetModel();

				gobj.transform.SetParent(t_Pedestrains);
				gobj.transform.position = new Vector3(
					transform.position.x + randPos,
					t_Pedestrains.position.y,
					transform.position.z + ((t_AnotherSide.position.z > transform.position.z) ? 1 : -1));

				PedestrianHandler ph = gobj.GetComponent<PedestrianHandler>();
				ph.SetTarget(t_AnotherSide);
			}
			yield return new WaitForSeconds(Random.Range(1, 3));
		}
	}


}
