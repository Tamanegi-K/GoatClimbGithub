using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InvTagInfo : MonoBehaviour
{
    [Header("Object Inputs")]
    public TextMeshProUGUI txtDisplay;
    public GameObject iconHolder;
    public Image val, col, spc;

	private Hashtable givenTag = new Hashtable();

	public void SetTagAppearance()
    {
        Image chosenImage;

        iconHolder.SetActive(true);
        col.gameObject.SetActive(false); val.gameObject.SetActive(false); spc.gameObject.SetActive(false);
        txtDisplay.text = GameMainframe.GetInstance().PascalCaseString(givenTag[0].ToString());

        // Icon shape
        switch (givenTag[0])
        {
            case PlantSpawning.PlantValue.PALE: case PlantSpawning.PlantValue.BRIGHT:
            case PlantSpawning.PlantValue.VIBRANT: case PlantSpawning.PlantValue.DARK:
                val.gameObject.SetActive(true);
                chosenImage = val;
                break;
            case PlantSpawning.PlantColour.RED: case PlantSpawning.PlantColour.ORANGE: case PlantSpawning.PlantColour.YELLOW:
            case PlantSpawning.PlantColour.GREEN: case PlantSpawning.PlantColour.BLUE: case PlantSpawning.PlantColour.PURPLE:
                col.gameObject.SetActive(true);
                chosenImage = col;
                break;
            case PlantSpawning.PlantSpecials.INFLORESCENT: case PlantSpawning.PlantSpecials.LUSTROUS:
            case PlantSpawning.PlantSpecials.NIGHTBLOOM: case PlantSpawning.PlantSpecials.RARE:
                spc.gameObject.SetActive(true);
                chosenImage = spc;
                break;
            default:
                iconHolder.SetActive(false);
                chosenImage = null;
                break;
		}

        // If the tag is none, skip this next bit
        if (chosenImage == null)
            return;

        // Icon colour
        switch (givenTag[0])
        {
            case PlantSpawning.PlantColour.RED: case PlantSpawning.BouquetSpecials.RED_DOMINANT:
                chosenImage.color = new Color(246f / 255f, 46f / 255f, 46f / 255f);
                break;
            case PlantSpawning.PlantColour.ORANGE: case PlantSpawning.BouquetSpecials.ORANGE_DOMINANT:
                chosenImage.color = new Color(255f, 109f / 255f, 0f);
                break;
            case PlantSpawning.PlantColour.YELLOW: case PlantSpawning.BouquetSpecials.YELLOW_DOMINANT:
                chosenImage.color = new Color(251f / 255f, 188f / 255f, 4f / 255f);
                break;
            case PlantSpawning.PlantColour.GREEN: case PlantSpawning.BouquetSpecials.GREEN_DOMINANT:
                chosenImage.color = new Color(52f / 255f, 168f / 255f, 83f / 255f);
                break;
            case PlantSpawning.PlantColour.BLUE: case PlantSpawning.BouquetSpecials.BLUE_DOMINANT:
                chosenImage.color = new Color(17f / 255f, 85f / 255f, 204f / 255f);
                break;
            case PlantSpawning.PlantColour.PURPLE: case PlantSpawning.BouquetSpecials.PURPLE_DOMINANT:
                chosenImage.color = new Color(221f / 255f, 43f / 255f, 236f / 255f);
                break;
            case PlantSpawning.PlantValue.PALE:
                chosenImage.color = new Color(255f / 255f, 251f / 255f, 162f / 255f);
                break;
            case PlantSpawning.PlantValue.BRIGHT:
                chosenImage.color = new Color(255f / 255f, 205f / 255f, 0f);
                break;
            case PlantSpawning.PlantValue.VIBRANT:
                chosenImage.color = new Color(255f / 255f, 79f / 255f, 0f);
                break;
            case PlantSpawning.PlantValue.DARK:
                chosenImage.color = new Color(152f / 255f, 0f, 26f / 255f);
                break;
            default:
                chosenImage.color = new Color(1f, 1f, 1f);
                break;
        }

        //new Color(204f / 255f, 0f, 0f, 190f / 255f);
    }

    public void AssignTag(object k)
	{
        givenTag = new Hashtable();
        givenTag.Add(0, k);

        SetTagAppearance();
	}

	public object GetAssignedTag()
	{
		return givenTag[0];
	}
}
