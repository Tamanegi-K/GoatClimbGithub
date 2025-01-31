using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZLockUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ZLock();
    }

    void ZLock()
	{
        Vector3 asd = transform.localPosition;

        if (asd.z >= 10f || asd.z < 0f)
            asd.z = 0;

        transform.localPosition = asd;
        enabled = false;
	}
}
