using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PickupPopupBhv : MonoBehaviour
{
    public float lifetime;
    public CanvasGroup cvGrp;
    public TextMeshProUGUI tmp;

    // Start is called before the first frame update
    void Start()
    {
        cvGrp = GetComponent<CanvasGroup>();
        tmp = GameObject.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime > 0f)
        {
            if (GetComponent<RectTransform>().anchoredPosition.y > -600f) //NOTE: This doesn't work for some reason - it's supposed to not let popups outside of the screen decay so that we can see what's going on
            {
                lifetime -= Time.deltaTime;
            }

            if (cvGrp.alpha < 1f)
                cvGrp.alpha += Time.deltaTime * 6.9f;
        }
        else
        {
            if (cvGrp.alpha > 0f)
                cvGrp.alpha -= Time.deltaTime * 1.6f;

            else
            {
                GameMainframe.GetInstance().ObjectEnd("HUDPopup", this.gameObject);
                gameObject.SetActive(false);
            }
        }
    }

    public void SetPopupText(string input)
	{
        tmp.text = "";
        tmp.text = input;
        lifetime = 5f;
	}
}
