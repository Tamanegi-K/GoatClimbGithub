using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyGravSet : MonoBehaviour
{
    [Header("Hi Ocean if you see this dw about it :)")]
    [Header("This script is necessary to make life easy")]
    public float timeBeforeSet = 1f;
    private BoxCollider bc;
    private Rigidbody rb;
    private MeshCollider mc;

    // Start is called before the first frame update
    void Start()
    {
        if (bc == null) bc = transform.GetComponent<BoxCollider>();
        if (rb == null) rb = transform.GetComponent<Rigidbody>();
        if (mc == null) mc = transform.GetComponentInChildren<MeshCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timeBeforeSet > 0f)
            timeBeforeSet -= Time.fixedDeltaTime;
        else
		{
            if (bc != null) Destroy(bc);
            if (rb != null) Destroy(rb);
            if (bc == null && rb == null)
            {
                mc.enabled = true;
                Destroy(this);
            }
        }
    }
}
