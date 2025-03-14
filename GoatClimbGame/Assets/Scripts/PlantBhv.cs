using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlantBhv : MonoBehaviour
{
    [Header("Inputs")]
    public GameObject billboardUI;
    public GameObject pickedPrefab;
    public PlantSpawning.PlantSpecials myPlantTag;
    private int amtWhenPicked;

    // Start is called before the first frame update
    void Start()
    {
        billboardUI.SetActive(false);
        billboardUI.GetComponent<TextMeshPro>().text = name + "\n" + "LMB to pick up";

        GameMainframe.DayHasChanged += ChangeState;
        // https://discussions.unity.com/t/subscribing-to-a-function-method/695110/2
        ChangeState();
    }

    void OnTriggerEnter(Collider other)
	{
        //Debug.Log(other.tag);

        if (other.gameObject.tag == "Player" && GameMainframe.GetInstance().playerContrScr.plantLookAt == null)
        {
            billboardUI.SetActive(true);
            GameMainframe.GetInstance().playerContrScr.plantLookAt = this.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
		{
            billboardUI.SetActive(false);
            GameMainframe.GetInstance().playerContrScr.plantLookAt = null;
        }
    }

    public void SetPickupQty(int x)
	{
        // if pickup qty is less than 1, it'll be at least 1 (for me, who'll definitely forget)
        if (x < 1) x = 1;

        amtWhenPicked = x;
	}

    public void SetPickedPrefab(GameObject go)
	{
        if (go != null)
            pickedPrefab = go;
        else
            pickedPrefab = this.gameObject;
	}

    public GameObject GetPickedPrefab()
	{
        return pickedPrefab;
	}

    public void SetPlantTag(PlantSpawning.PlantSpecials ps)
	{
        myPlantTag = ps;
	}

    public void ChangeState()
    {
        // TO DO: CHANGE FLOWER BLOOM STATE HERE

        if (myPlantTag == PlantSpawning.PlantSpecials.NIGHTBLOOM)
        {
            if (GameMainframe.isCurrentlyDay)
            {
                billboardUI.GetComponent<TextMeshPro>().text = name + "\n" + "Unable to pick during Daytime";
                ToggleMeshes();
            }
            else
            {
                billboardUI.GetComponent<TextMeshPro>().text = name + "\n" + "LMB to pick up";
                ToggleMeshes();
            }
        }
	}

    public void ToggleMeshes()
	{
        foreach (Transform t in transform)
        {
            t.GetComponent<MeshRenderer>().enabled = !GameMainframe.isCurrentlyDay;
        }
	}

    public bool CheckPickable()
	{
        // To do every case here
        if (myPlantTag == PlantSpawning.PlantSpecials.NIGHTBLOOM && GameMainframe.isCurrentlyDay)
            return false;
        else
            return true;
    }

    public int PickMeUp()
    {
        StartCoroutine(PickUpAnim());
        GameMainframe.GetInstance().ObjectEnd(name, this.gameObject);
        GameMainframe.GetInstance().audioMngr.PlaySFX("pickup" + UnityEngine.Random.Range(1, 3), transform.position);
        return amtWhenPicked;
	}

    IEnumerator PickUpAnim()
    {
        billboardUI.SetActive(false);
        GetComponent<SphereCollider>().enabled = false;

        float time = 0f, timeEnd = 0.32f;
        
        while (time < timeEnd)
        {
            Vector3 dest = GameMainframe.GetInstance().playerContrScr.gameObject.transform.position;
            transform.position = Vector3.Lerp(transform.position, dest, time / timeEnd);
            transform.localScale = new Vector3(1f - (time / timeEnd), 1f - (time / timeEnd), 1f - (time / timeEnd));
            time += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
		}

        GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
        {
            PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
            hppPPB.SetupDisplay(gameObject.name, pickedPrefab, amtWhenPicked);
            hpp.name = "HUDPopup";

            hpp.transform.SetParent(null);
            hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
            hpp.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            hpp.SetActive(true);
        }, GameMainframe.GetInstance().hudPopupPrefab);
        
        gameObject.SetActive(false);
        yield return null;
	}

    public void ObjReuse()
	{
        transform.localScale = Vector3.one;
        GetComponent<SphereCollider>().enabled = true;
    }
}
