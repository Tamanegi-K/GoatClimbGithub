using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shoutouts
// https://www.youtube.com/watch?v=8yzpjkoE0YA

public class InventoryItemBhv : MonoBehaviour
{
    public TextMeshProUGUI tmpNum;
    public GameObject objHolder;
    private string invName;

    // Start is called before the first frame update
    void Start()
    {
        //if (tmpTxt == null) tmpTxt = transform.Find("NameBG").GetComponent<TextMeshProUGUI>();
        if (tmpNum == null) tmpNum = transform.Find("NumBG").GetComponent<TextMeshProUGUI>();
        if (objHolder == null) objHolder = transform.Find("IconArea/ObjHolder").gameObject;

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

    public void InvItemClick()
	{
        GameMainframe.GetInstance().InvOnSelect(invName);
	}
}
