using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PickupPopupBhv : MonoBehaviour
{
    public float lifetime;
    public CanvasGroup cvGrp;
    public TextMeshProUGUI tmp, tmpQ;
    public GameObject objHolder;

    // Start is called before the first frame update
    void Start()
    {
        if (cvGrp == null) cvGrp = transform.GetComponent<CanvasGroup>();
        if (tmp == null) tmp = transform.Find("PopupBG").Find("PopupTxt").GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = transform.Find("PopupQBG").Find("PopupQTxt").GetComponent<TextMeshProUGUI>();
        if (objHolder == null) objHolder = transform.Find("PopupDvd L/ObjHolder").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime > 0f)
        {
            if (GetComponent<RectTransform>().anchoredPosition.y > -600f)
            {
                lifetime -= Time.deltaTime;
            }

            if (cvGrp.alpha < 1f)
                cvGrp.alpha += Time.deltaTime * 6.9f;
        }
        else
        {
            if (cvGrp.alpha > 0.4f)
                cvGrp.alpha -= Time.deltaTime * 1.6f;

            else
            {
                GameMainframe.GetInstance().ObjectEnd("HUDPopup", this.gameObject);
                gameObject.SetActive(false);
            }
        }
    }

    public void SetupDisplay(string input, GameObject go, int qty = 1)
	{
        tmp.text = "";
        tmp.text = input;

        tmpQ.text = "";
        tmpQ.text = qty > 1 ? "×" + qty.ToString() : "";

        lifetime = 5f;

        // Erasing the display object
        foreach (Transform c in objHolder.transform)
		{
            c.transform.SetParent(null);
            GameMainframe.GetInstance().ObjectEnd(c.name, c.gameObject);
            c.gameObject.SetActive(false);
		}

        if (go == null) return; // if there's no object to display, skip the next bit

        // Reinserting the display object
        GameMainframe.GetInstance().ObjectUse(input + "Picked", (pickedDisplay) =>
        {
            pickedDisplay.name = pickedDisplay.name.Contains("Picked") ? pickedDisplay.name : input + "Picked";
            pickedDisplay.transform.SetParent(objHolder.transform);

            pickedDisplay.transform.localPosition = Vector3.zero;
            pickedDisplay.transform.localEulerAngles = Vector3.zero;
            pickedDisplay.transform.localScale = Vector3.one;
            pickedDisplay.gameObject.SetActive(true);
        }, go);

    }
}
