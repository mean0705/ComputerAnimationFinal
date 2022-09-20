using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian_StartCrossing : MonoBehaviour
{
    [SerializeField] Transform t_AnotherSide;
    [SerializeField] Transform t_Pedestrains;
    [SerializeField] Color color;

    static float sf_North => 2.0f;
    static float sf_South => -2.0f;
    // Start is called before the first frame update
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
				GameObject gobj = PedestrianPool.GetModel();

				gobj.transform.SetParent(t_Pedestrains);

				gobj.transform.position = new Vector3(
					transform.position.x + ((t_AnotherSide.position.x > transform.position.x) ? 1 : -1),
					t_Pedestrains.position.y,
					transform.position.z + randPos);

				// gobj.transform.position = transform.position;

				Crossing_PedestrianHandler ph = gobj.GetComponent<Crossing_PedestrianHandler>();

				throw new System.NotImplementedException();
				//ph.SetTarget(t_AnotherSide);
			}
			yield return new WaitForSeconds(Random.Range(1, 3));
		}
	}
}
