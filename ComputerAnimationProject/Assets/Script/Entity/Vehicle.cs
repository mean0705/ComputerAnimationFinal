using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleData
{
    public const float cf_ReachPointThreshold = 0.25f;
    public Vector3 v3_Destination = Vector3.zero;
    public float f_Speed = 0;
    // ---------------------------------------------------

    public float alpha = 100.0f; // drag coefficient

}

public class Vehicle : MonoBehaviour
{
    public VehicleData vehicleData;
    // Start is called before the first frame update
    void Start()
    {
        vehicleData = new VehicleData();
    }

    // Update is called once per frame
    void Update()
    {
        // transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
