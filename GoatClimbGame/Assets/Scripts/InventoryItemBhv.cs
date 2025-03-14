using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shoutouts
// https://www.youtube.com/watch?v=8yzpjkoE0YA

public class InventoryItemBhv : MonoBehaviour
{
    public GameObject objHolder, objNum;
    public TextMeshProUGUI tmpNum;
    private string invName;
    public bool isNotPlant = false;

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
        if (GameMainframe.GetInstance().uiGroupPause.alpha <= 0f)
		{
            objHolder.SetActive(false);
        }
        else
        {
            objHolder.SetActive(true);
            //objHolder.transform.localScale = new Vector3(50f, GameMainframe.GetInstance().uiGroupPause.alpha * 50f, 50f);
        }
    }

    public void SetupInvDisplay(string input, int num, GameObject go)
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
        GameMainframe.GetInstance().ObjectUse(input + "Inv", (pickedDisplay) =>
        {
            pickedDisplay.name = pickedDisplay.name.Contains("Inv") ? pickedDisplay.name : input + "Inv";
            pickedDisplay.transform.SetParent(objHolder.transform);

            pickedDisplay.transform.localPosition = Vector3.zero;
            pickedDisplay.transform.localEulerAngles = Vector3.zero;
            pickedDisplay.transform.localScale = Vector3.one;
            pickedDisplay.gameObject.SetActive(true);
        }, go);
    }

    public string GetInvName()
	{
        return invName;
	}

    public void InvItemClick() // attached to button in scene
    {
        GameMainframe.GetInstance().InvOnSelect(invName);

        // The next part is for bouquet assembly - if the item clicked is not a flower/plant, skip the rest
        if (isNotPlant)
            return;

        // if the isOn condition didn't exist, it would fire twice because of the Toggle "On value changed" condition (bruh)
        if (GameMainframe.GetInstance().currentTab == GameMainframe.PauseTabs.ASSEMBLY && GetComponent<Toggle>().isOn)
        {
            GameMainframe.GetInstance().GetAssRight().InvOnClick(invName);
        }
	}
}
