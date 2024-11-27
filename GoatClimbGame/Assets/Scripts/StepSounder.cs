using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSounder : MonoBehaviour
{
	public void StepMade()
	{
		GameMainframe.GetInstance().audioMngr.PlaySFXStep("grass" + Random.Range(1, 5));
	}
}
