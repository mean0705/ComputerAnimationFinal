using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "params_SocialForceCrossing", menuName = "ScriptableObjects/params_SocialForceCrossing", order = 1)]
public class params_SocialForceCrossing : ScriptableObject
{

	public float le = 0.2f; // buffer length
	public float R = 0.36f; // human collider radius

	public float veh_A = 200.0f;
	public float veh_b = 2.6f;
	public float des_sigma = 1.0f;
	public float des_k = 300.0f;
	public float mu_gap = 4.0f;
	public float sigma_gap = 2.5f;
	// todo: add different pedestrian type
	public float mu_vd = 1.4f;
	public float sigma_vd = 0.2f;
	public float thr_gap = 0;
	public float k_des = -0.5f;
}

