using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainHandler : MonoBehaviour
{
    public static float sf_SaveGap = 2;

    public static float sf_TDelta = .2f;
    public static float sf_RelaxTime = .5f;
    public static float sf_PedstrainVision = 200f;
    public static float sf_PedstrainVision_OutsideRate = .5f;

    [SerializeField] Text text;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            Time.timeScale = 10;
        else if (Input.GetKeyUp(KeyCode.LeftControl))
            Time.timeScale = 1;
    }
}
