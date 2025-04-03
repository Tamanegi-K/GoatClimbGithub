using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZRotato : MonoBehaviour
{
	Transform tf;
	public float windSpeed = 4f;

	private void Awake()
	{
		if (tf == null) tf = transform.Find("Windmill/Windmill_Blades").transform;
	}

	// Update is called once per frame
	void LateUpdate()
    {
		tf.localEulerAngles = new Vector3(tf.localEulerAngles.x, tf.localEulerAngles.y, tf.localEulerAngles.z + (40f * windSpeed * Time.deltaTime));
    }
}
