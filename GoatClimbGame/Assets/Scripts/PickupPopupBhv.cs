using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PickupPopupBhv : MonoBehaviour
{
    public float lifetime;
    public CanvasGroup cvGrp;
    public TextMeshProUGUI tmp;
    public GameObject objHolder;

    // Start is called before the first frame update
    void Start()
    {
        if (cvGrp == null) cvGrp = transform.GetComponent<CanvasGroup>();
        if (tmp == null) tmp = transform.Find("PopupBG").Find("PopupTxt").GetComponent<TextMeshProUGUI>();
        if (objHolder == null) objHolder = transform.Find("PopupBG L/ObjHolder").gameObject;
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

    public void SetupDisplay(string input, GameObject go)
	{
        tmp.text = "";
        tmp.text = input;
        lifetime = 5f;

        // Erasing the display object
        foreach (Transform c in objHolder.transform)
		{
            GameMainframe.GetInstance().ObjectEnd(input + "Picked", c.gameObject);
		}

        if (go == null) return; // if there's no object to display, skip the next bit

        // Reinserting the display object
        GameMainframe.GetInstance().ObjectUse(input + "Picked", (pickedDisplay) =>
        {
            pickedDisplay.name = input + "Picked";
            pickedDisplay.transform.SetParent(null);
            pickedDisplay.transform.SetParent(objHolder.transform);

            pickedDisplay.transform.localPosition = Vector3.zero;
            pickedDisplay.transform.localEulerAngles = Vector3.zero;
            pickedDisplay.transform.localScale = Vector3.one;
        }, go);

    }
}
