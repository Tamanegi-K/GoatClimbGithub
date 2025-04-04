using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Shoutouts
// https://www.youtube.com/watch?v=8yzpjkoE0YA

public class InventoryItemBhv : MonoBehaviour, IPointerEnterHandler
{
    public GameObject objHolder, objNum;
    public TextMeshProUGUI tmpNum;
    private string invName;
    public bool isNotPlant = false;
    public float localY, yScale;

    // Start is called before the first frame update
    void Start()
    {
        //if (tmpTxt == null) tmpTxt = transform.Find("NameBG").GetComponent<TextMeshProUGUI>();
        if (objHolder == null) objHolder = transform.Find("IconArea/ObjHolder").gameObject;
        if (objNum == null) objNum = transform.Find("IconArea/ObjHolder").gameObject;
        if (tmpNum == null) tmpNum = objNum.GetComponent<TextMeshProUGUI>();

        GetComponent<Toggle>().group = GameMainframe.GetInstance().inventoryDisplay.GetComponent<ToggleGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        // hiding the 3d display objects when unpaused (god why is this so fkn hard)
        if (GameMainframe.GetInstance().uiGroupPause.alpha <= 0.2f)
		{
            objHolder.SetActive(false);
        }
        else
        {
            // Scromching the 3d object when it's "out of view" in the inventory panel
            //float yScale = 50f;
            localY = transform.position.y;
            if (localY > 1251f)
            {
                yScale = Mathf.Clamp(Mathf.Lerp(0f, 50f, ((1251f + 90f + 16f + 25f) - localY) / 90f), 0f, 50f);
            }
            else if (localY < 601f)
            {
                yScale = Mathf.Clamp(Mathf.Lerp(0f, 50f, (localY - (601f - 90f - 16f + 25f)) / 90f), 0f, 50f);
            }
            else
			{
                yScale = 50f;
			}

            objHolder.transform.localScale = new Vector3(50f, yScale, 50f);

            if (yScale <= 5f)
            {
                objHolder.SetActive(false);
            }
            else
            {
                objHolder.SetActive(true);
            }

            //objHolder.transform.localScale = new Vector3(50f, GameMainframe.GetInstance().uiGroupPause.alpha * 50f, 50f);
        }
    }

    public void SetupInvDisplay(string input, int num = 1, GameObject go = null)
	{
        invName = input;
        tmpNum.text = num.ToString();

        // Erasing the display object
        foreach (Transform c in objHolder.transform)
        {
            c.transform.SetParent(null);
            GameMainframe.GetInstance().ObjectEnd(c.name, c.gameObject);
            c.gameObject.SetActive(false);
        }

        if (go == null) return; // if there's no object to display, skip the next bit

        // Reinserting the display object
        GameMainframe.GetInstance().ObjectUse("Inv_" + input, (pickedDisplay) =>
        {
            pickedDisplay.name = pickedDisplay.name.Contains("Inv_") ? pickedDisplay.name : "Inv_" + input;
            pickedDisplay.transform.SetParent(objHolder.transform);

            pickedDisplay.transform.localPosition = Vector3.zero;
            pickedDisplay.transform.localEulerAngles = Vector3.zero;
            pickedDisplay.transform.localScale = Vector3.one;

            // change layer of display object
            foreach (Transform tf in pickedDisplay.transform)
            {
                tf.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            pickedDisplay.gameObject.SetActive(true);
        }, go);
    }

    public string GetInvName()
	{
        return invName;
	}

    public void InvItemClick() // attached to button in scene
    {
        AudioManager.GetInstance().PlaySFXUI("select");
        GameMainframe.GetInstance().InvClick(invName);

        // The next part is for bouquet assembly - if the item clicked is not a flower/plant, skip the rest
        //if (isNotPlant)
            //return;

        // if the isOn condition didn't exist, it would fire twice because of the Toggle "On value changed" condition (bruh)
        if (GameMainframe.GetInstance().currentTab == GameMainframe.PauseTabs.ASSEMBLY && GetComponent<Toggle>().isOn && !isNotPlant)
        {
            GameMainframe.GetInstance().GetAssRight().InvClickBouquet(invName);
        }

        // ig player is giving something to a villager and the item clicked is a bouquet
        // TO DO - make the inventory properly show that they're giving something
        // MOVED TO GAMEMAINFRAME under CheckRequest()
        GameMainframe.GetInstance().CheckRequest(invName, isNotPlant);
	}

    public void OnPointerEnter(PointerEventData ped)
    {
        AudioManager.GetInstance().PlaySFXUI("shift");
    }
}
