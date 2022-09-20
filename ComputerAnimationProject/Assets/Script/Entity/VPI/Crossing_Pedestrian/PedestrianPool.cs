using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianPool : MonoBehaviour
{
    public static PedestrianPool Get;

    /// <summary>
    /// �s�񨤦�Ϊ��Ū���
    /// </summary>
    [SerializeField] Transform t_Pool;

    /// <summary>
    /// ���a�P�Ǫ��ҫ��Ҧb��m
    /// </summary>
    [SerializeField] GameObject prefab_Pedestrain;

    private void Awake()
    {
        Get = this;
    }
    private void OnDestroy()
    {
        Get = null;
    }

    Queue<GameObject> queue_gobj_ObjList = new Queue<GameObject>();
    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < t_Pool.childCount; i++)
        {
            Destroy(t_Pool.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    public static GameObject GetModel()
    {
        // Check
        if (Get.queue_gobj_ObjList.Count > 0)
        {
            GameObject gobj = Get.queue_gobj_ObjList.Dequeue();
            gobj.SetActive(true);
            return gobj;
        }
        else
        {
            return Instantiate(Get.prefab_Pedestrain);
        }
    }

    public static void Recycle(GameObject gobj)
    {
        gobj.transform.SetParent(Get.t_Pool);
        gobj.SetActive(false);
        gobj.transform.localPosition = Vector3.zero;
    }
}
