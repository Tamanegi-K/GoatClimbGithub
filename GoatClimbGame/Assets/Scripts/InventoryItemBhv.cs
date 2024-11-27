using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryItemBhv : MonoBehaviour
{
    public TextMeshProUGUI tmpTxt, tmpNum;

    // Start is called before the first frame update
    void Start()
    {
        if (tmpTxt == null) tmpTxt = transform.Find("NameBG").GetComponent<TextMeshProUGUI>();
        if (tmpNum == null) tmpNum = transform.Find("NumBG").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetInvText(string name, int num)
	{
        tmpTxt.text = name;
        tmpNum.text = num.ToString();
	}
}
