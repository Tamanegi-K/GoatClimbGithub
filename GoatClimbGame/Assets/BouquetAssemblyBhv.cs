using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BouquetAssemblyBhv : MonoBehaviour
{
    public Button btnCancel, btnFinish;
    public GameObject[] objHolderBouquetArray = new GameObject[6], objHolderListArray = new GameObject[6];
    private string[] plantNameArray = new string[6] { "", "", "", "", "", "" };
    private int currentSlot = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InvOnClick(string input)
    {
        if (currentSlot > 5)
            return;

        if (!GameMainframe.GetInstance().GetPauseTabToggleGrp().GetFirstActiveToggle() == GameMainframe.GetInstance().GetPauseTabAss().GetComponent<Toggle>())
            return;
        
        GameObject dispObj = null;
        foreach (PlantSpawning.OnePlantInfo pi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist)
        {
            // Search the plantMasterlist and grab its display obj and desc
            if (pi.plantName == input && GameMainframe.GetInstance().playerContrScrpt.GetPlantQty(input) > 0)
            {
                // --- OBJECT DISPLAYING ---
                // Check if object has a display object, otherwise use the whole damn plant
                if (pi.plantPickedup != null) dispObj = pi.plantPickedup;
                else dispObj = pi.plantOnfield;

                // --- BOUQUET LIST ---
                // Erasing the display object in queue grid
                foreach (Transform c in objHolderBouquetArray[currentSlot].transform)
                {
                    GameMainframe.GetInstance().ObjectEnd(c.name, c.gameObject);
                    c.transform.SetParent(this.transform);
                    c.gameObject.SetActive(false);
                }

                // Spawning the display object in desc area
                GameMainframe.GetInstance().ObjectUse(input + "BQ", (pickedDisplay) =>
                {
                    pickedDisplay.name = pickedDisplay.name.Contains("BQ") ? pickedDisplay.name : input + "BQ";
                    pickedDisplay.transform.SetParent(objHolderBouquetArray[currentSlot].transform);

                    pickedDisplay.transform.localPosition = Vector3.zero;
                    pickedDisplay.transform.localEulerAngles = Vector3.zero;
                    pickedDisplay.transform.localScale = Vector3.one;
                    pickedDisplay.gameObject.SetActive(true);
                }, dispObj);

                // --- QUEUE GRID -
                // Erasing the display object in queue grid
                foreach (Transform c in objHolderListArray[currentSlot].transform)
                {
                    GameMainframe.GetInstance().ObjectEnd(c.name, c.gameObject);
                    c.transform.SetParent(this.transform);
                    c.gameObject.SetActive(false);
                }

                // Spawning the display object in desc area
                GameMainframe.GetInstance().ObjectUse(input + "Ass", (pickedDisplay) =>
                {
                    pickedDisplay.name = pickedDisplay.name.Contains("Ass") ? pickedDisplay.name : input + "Ass";
                    pickedDisplay.transform.SetParent(objHolderListArray[currentSlot].transform);

                    pickedDisplay.transform.localPosition = Vector3.zero;
                    pickedDisplay.transform.localEulerAngles = Vector3.zero;
                    pickedDisplay.transform.localScale = Vector3.one;
                    pickedDisplay.gameObject.SetActive(true);
                }, dispObj);

                // Successful plant insert - move to next slot in bouquet assembly and remove 1 of that plant from inv
                plantNameArray[currentSlot] = input;
                GameMainframe.GetInstance().playerContrScrpt.PlantCollection(input, -1);
                GameMainframe.GetInstance().UpdateInventoryQuantities();
                
                currentSlot += 1;
                CheckComplete();

                break;
            }
        }
    }

    public void RestartBouquetBtn()
    {
        foreach (string s in plantNameArray)
		{
            if (s != "")
            {
                GameMainframe.GetInstance().playerContrScrpt.PlantCollection(s, 1);
            }
		}

        plantNameArray = new string[6] { "", "", "", "", "", "" };

        // Flush bouquet
        foreach (GameObject objH in objHolderBouquetArray)
        {
            // Erasing the display object in queue grid
            foreach (Transform c in objH.transform)
            {
                GameMainframe.GetInstance().ObjectEnd(c.name, c.gameObject);
                c.transform.SetParent(this.transform);
                c.gameObject.SetActive(false);
            }
        }

        // Flush queue grid
        foreach (GameObject objH in objHolderListArray)
        {
            // Erasing the display object in queue grid
            foreach (Transform c in objH.transform)
            {
                GameMainframe.GetInstance().ObjectEnd(c.name, c.gameObject);
                c.transform.SetParent(this.transform);
                c.gameObject.SetActive(false);
            }
        }

        currentSlot = 0;
        CheckComplete();
        GameMainframe.GetInstance().UpdateInventoryQuantities();
    }

    private void CheckComplete()
	{
        if (currentSlot > 5) 
            btnFinish.interactable = true;
        else 
            btnFinish.interactable = false;
	}

    public void FinishBouquet()
	{
        // TO DO - PUT ASSEMBLED BOUQUET IN INVENTORY
	}
}
