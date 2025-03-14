using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BouquetAssemblyBhv : MonoBehaviour
{
    public Button btnCancel, btnFinish;
    public GameObject[] objHolderBouquetArray = new GameObject[7], objHolderListArray = new GameObject[7];
    private string[] plantNameArray = new string[7] { "", "", "", "", "", "", "" };
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
        if (currentSlot > 6)
            return;

        // If the current tab isn't the Assembly screen, skip all this
        if (!GameMainframe.GetInstance().GetPauseTabToggleGrp().GetFirstActiveToggle() == GameMainframe.GetInstance().GetPauseTabAss().GetComponent<Toggle>())
            return;
        
        GameObject dispObj = null;
        foreach (PlantSpawning.OnePlantInfo pi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist)
        {
            // Search the plantMasterlist and grab its display obj and desc
            if (pi.plantName == input && GameMainframe.GetInstance().playerContrScr.GetPlantQty(input) > 0)
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
                GameMainframe.GetInstance().playerContrScr.PlantCollection(input, -1);
                GameMainframe.GetInstance().UpdateInventoryQuantities();
                
                currentSlot += 1;
                CheckComplete();

                break;
            }
        }
    }

    public void RestartBouquetBtn() // attached to button in scene
    {
        foreach (string s in plantNameArray)
		{
            if (s != "")
            {
                GameMainframe.GetInstance().playerContrScr.PlantCollection(s, 1);
            }
		}

        plantNameArray = new string[7] { "", "", "", "", "", "", ""};

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
        if (currentSlot > 6) 
            btnFinish.interactable = true;
        else 
            btnFinish.interactable = false;
	}

    public void FinishBouquet() // attached to button in scene
	{
        // TO DO - PUT ASSEMBLED BOUQUET IN INVENTORY
        List<PlantSpawning.PlantValue> inBouquetVals = new List<PlantSpawning.PlantValue>(); // total values in the plantNameArray
        List<PlantSpawning.PlantColour> inBouquetCols = new List<PlantSpawning.PlantColour>(); // total colours in the plantNameArray
        List<PlantSpawning.PlantSpecials> centreSpcs = new List<PlantSpawning.PlantSpecials>(); // special tags in the centrepiece ONLY
        bool isOnCentrepiece = true; // first index of the plantNameArray is always the centrepiece

        // Loop through every flower in the plantNamtArray and tabulate every plant tag
        foreach (string s in plantNameArray)
        {
            foreach (PlantSpawning.OnePlantInfo opi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist) 
            {
                // Loop through name of flower with the plantMasterlist and only execute if it matches
                if (opi.plantName == s)
				{
                    inBouquetVals.Add(opi.plantVal);
                    inBouquetCols.Add(opi.plantCol);

                    if (isOnCentrepiece)
                    {
                        foreach (PlantSpawning.PlantSpecials tag in opi.plantSpc)
                        {
                            centreSpcs.Add(tag);
                        }
                    }

                    isOnCentrepiece = false;
                    break;
				}
            }
        }

        Debug.Log(inBouquetVals.Count + " | " + inBouquetCols.Count + " | " + centreSpcs.Count);

        // Flushing the array so I can run this function again without giving the items back
        plantNameArray = new string[7] { "", "", "", "", "", "", "" };
        RestartBouquetBtn();


    }
}
