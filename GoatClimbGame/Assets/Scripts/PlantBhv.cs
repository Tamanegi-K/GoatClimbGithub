using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlantBhv : MonoBehaviour
{
    [Header("Inputs")]
    public GameObject billboardUI;

    // Start is called before the first frame update
    void Start()
    {
        billboardUI.SetActive(false);
        billboardUI.GetComponent<TextMeshPro>().text = name + "\n" + "LMB to pick up";
    }

	void OnTriggerEnter(Collider other)
	{
        //Debug.Log(other.tag);

        if (other.gameObject.tag == "Player" && GameMainframe.GetInstance().playerContrScrpt.plantLookAt == null)
        {
            billboardUI.SetActive(true);
            GameMainframe.GetInstance().playerContrScrpt.plantLookAt = this.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
		{
            billboardUI.SetActive(false);
            GameMainframe.GetInstance().playerContrScrpt.plantLookAt = null;
        }
    }

    public int PickMeUp()
    {
        StartCoroutine(PickUpAnim());
        GameMainframe.GetInstance().ObjectEnd(name, this.gameObject);
        GameMainframe.GetInstance().audioMngr.PlaySFX("pickup" + Random.Range(1, 3), transform.position);
        return 1;
	}

    IEnumerator PickUpAnim()
    {
        billboardUI.SetActive(false);
        GetComponent<SphereCollider>().enabled = false;

        float time = 0f, timeEnd = 0.32f;
        
        while (time < timeEnd)
        {
            Vector3 dest = GameMainframe.GetInstance().playerContrScrpt.gameObject.transform.position;
            transform.position = Vector3.Lerp(transform.position, dest, time / timeEnd);
            transform.localScale = new Vector3(1f - (time / timeEnd), 1f - (time / timeEnd), 1f - (time / timeEnd));
            time += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
		}

        gameObject.SetActive(false);
        yield return null;
	}
}
