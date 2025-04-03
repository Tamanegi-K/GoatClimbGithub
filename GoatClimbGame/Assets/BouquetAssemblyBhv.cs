using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BouquetAssemblyBhv : MonoBehaviour
{
    public ColWheelBhv colourWheelScr;
    public Button btnCancel, btnFinish;
    public GameObject[] objHolderBouquetArray = new GameObject[7], objHolderListArray = new GameObject[7];
    public GameObject bouquetObj;
    private string[] plantNameArray = new string[7] { "", "", "", "", "", "", "" };
    private int currentSlot = 0;
    public Dictionary<PlantSpawning.PlantColour, int> ccc = new Dictionary<PlantSpawning.PlantColour, int>(); // short for currentColourCount
    public Dictionary<PlantSpawning.PlantColour, int> cccNoC = new Dictionary<PlantSpawning.PlantColour, int>(); // short for currentColourCountNoCentre
    private int bouquetID = 1;
    private Vector3 bqDefaultLocalRotation;
    private Coroutine shakeyCoro;

    // Start is called before the first frame update
    void Start()
    {
        ResetColourCount();
        bouquetObj = transform.Find("InvItemBG/Bouquet").gameObject;
        bqDefaultLocalRotation = bouquetObj.transform.localEulerAngles;
    }

    public void InvClickBouquet(string input)
    {
        if (currentSlot > 6)
            return;

        shakeyCoro = StartCoroutine(ShakeBouquet(1f));

        // If the current tab isn't the Assembly screen, skip all this
        if (!GameMainframe.GetInstance().GetPauseTabToggleGrp().GetFirstActiveToggle() == GameMainframe.GetInstance().GetPauseTabAss().GetComponent<Toggle>())
            return;
        
        GameObject dispObj = null;
        foreach (PlantSpawning.OnePlantInfo pi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist)
        {
            // Search the plantMasterlist and grab its display obj and desc
            if (pi.plantName == input && GameMainframe.GetInstance().playerContrScr.GetInventoryQty(input) > 0)
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
                GameMainframe.GetInstance().playerContrScr.UpdateInventory(input, -1);
                GameMainframe.GetInstance().UpdateInventoryQuantities();
                
                currentSlot += 1;
                CheckComplete();

                break;
            }
        }
    }

    public void RestartBouquetBtn() // attached to button in scene
    {
        shakeyCoro = StartCoroutine(ShakeBouquet(2f));
        foreach (string s in plantNameArray)
		{
            if (s != "")
            {
                GameMainframe.GetInstance().playerContrScr.UpdateInventory(s, 1);
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
        shakeyCoro = StartCoroutine(ShakeBouquet(3f));
        List<PlantSpawning.PlantType> inBouquetTyps = new List<PlantSpawning.PlantType>(); // total types in the plantNameArray
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
                    inBouquetTyps.Add(opi.plantTyp);
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

        //GameMainframe.GetInstance().playerContrScr.UpdateInventory(pbhv.name, pbhv.PickMeUp());

        //Debug.Log(inBouquetVals.Count + " | " + inBouquetCols.Count + " | " + centreSpcs.Count);

        // Colour Counter
        ResetColourCount();
        CountColours(inBouquetCols);

        // Making the bouquet entry and adding it to the player's inventory
        GameObject madeBQ = Instantiate(bouquetObj);
        madeBQ.SetActive(false);
        madeBQ.transform.localScale = Vector3.one;
        //madeBQ.transform.position = new Vector3(madeBQ.transform.position.x, madeBQ.transform.position.y - 500f, madeBQ.transform.position.z);

        PlantSpawning.OneBouquetMade newBouquet = new PlantSpawning.OneBouquetMade(madeBQ, bouquetID, plantNameArray, "Bouquet " + bouquetID, AssignBouquetHarmony(), AssignBouquetCentre(inBouquetCols[0]), AssignBouquetSpecials(inBouquetTyps, inBouquetVals), "A beautiful bouquet you made! \n \n WIP: Soon you'll be able to name your own bouquets.");
        GameMainframe.GetInstance().GetComponent<PlantSpawning>().bouquetsMade.Add(newBouquet);
        GameMainframe.GetInstance().playerContrScr.UpdateInventory(newBouquet.bqName, 1);
        bouquetID += 1;

        // Flushing the array so I can run this function again without giving the items back
        plantNameArray = new string[7] { "", "", "", "", "", "", "" };
        RestartBouquetBtn();
    }

    public PlantSpawning.BouquetHarmony AssignBouquetHarmony()
	{
        PlantSpawning.BouquetHarmony resultHarm = PlantSpawning.BouquetHarmony.NONE;

        // We're checking the stuff in order of most "points"

        // MULTICOLOURED
        int multicolourStrikes = 0;
        foreach (KeyValuePair<PlantSpawning.PlantColour, int> kvp in ccc)
		{
            //Debug.Log(kvp.Key + " | " + kvp.Value);
            if (kvp.Value == 1)
                multicolourStrikes += 1;
		}
        if (multicolourStrikes >= 6)
        {
            resultHarm = PlantSpawning.BouquetHarmony.MULTICOLOURED;
        }

        // TRIADIC
        else if (
            (cccNoC[PlantSpawning.PlantColour.RED] >= 2 && cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 2) ||
            (cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 2 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2)
            )
        {
            resultHarm = PlantSpawning.BouquetHarmony.TRIADIC;
        }

        // ANALOGOUS
        else if (
            (cccNoC[PlantSpawning.PlantColour.RED] >= 2 && cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2 && cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2) ||
            (cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2 && cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 2) ||
            (cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 2 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 2) ||
            (cccNoC[PlantSpawning.PlantColour.GREEN] >= 2 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 2 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2) ||
            (cccNoC[PlantSpawning.PlantColour.BLUE] >= 2 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2 && cccNoC[PlantSpawning.PlantColour.RED] >= 2) ||
            (cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2 && cccNoC[PlantSpawning.PlantColour.RED] >= 2 && cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2)
            )
        {
            resultHarm = PlantSpawning.BouquetHarmony.ANALOGOUS;
        }

        // CONTRASTING
        else if (
            (cccNoC[PlantSpawning.PlantColour.RED] >= 3 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 3) || 
            (cccNoC[PlantSpawning.PlantColour.ORANGE] >= 3 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 3) || 
            (cccNoC[PlantSpawning.PlantColour.YELLOW] >= 3 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 3)
            )
        {
            resultHarm = PlantSpawning.BouquetHarmony.CONTRASTING;
        }

        // SOLID
        else if (
            cccNoC[PlantSpawning.PlantColour.RED] >= 6 ||
            cccNoC[PlantSpawning.PlantColour.ORANGE] >= 6 ||
            cccNoC[PlantSpawning.PlantColour.YELLOW] >= 6 ||
            cccNoC[PlantSpawning.PlantColour.GREEN] >= 6 ||
            cccNoC[PlantSpawning.PlantColour.BLUE] >= 6 ||
            cccNoC[PlantSpawning.PlantColour.PURPLE] >= 6
            )
        {
            resultHarm = PlantSpawning.BouquetHarmony.SOLID;
        }

        return resultHarm;
	}

    public PlantSpawning.BouquetCentres AssignBouquetCentre(PlantSpawning.PlantColour centreCol)
    {
        PlantSpawning.BouquetCentres resultCntr = PlantSpawning.BouquetCentres.NONE;

        // Again we're checking the stuff in order of most "points"

        // PARTITION (TRIADIC)
        if (
            ((cccNoC[PlantSpawning.PlantColour.RED] >= 2 && cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 2)
            && (centreCol == PlantSpawning.PlantColour.RED || centreCol == PlantSpawning.PlantColour.YELLOW || centreCol == PlantSpawning.PlantColour.BLUE))
            ||
            ((cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 2 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2)
            && (centreCol == PlantSpawning.PlantColour.ORANGE || centreCol == PlantSpawning.PlantColour.GREEN || centreCol == PlantSpawning.PlantColour.PURPLE))
            )
        {
            resultCntr = PlantSpawning.BouquetCentres.TROVE;
        }


        // SPECTRUM (ANALOGOUS)
        else if (
            ((cccNoC[PlantSpawning.PlantColour.RED] >= 2 && cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2 && cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2)
            && (centreCol == PlantSpawning.PlantColour.RED || centreCol == PlantSpawning.PlantColour.ORANGE || centreCol == PlantSpawning.PlantColour.YELLOW))
            ||
            ((cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2 && cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 2)
            && (centreCol == PlantSpawning.PlantColour.ORANGE || centreCol == PlantSpawning.PlantColour.YELLOW || centreCol == PlantSpawning.PlantColour.GREEN))
            ||
            ((cccNoC[PlantSpawning.PlantColour.YELLOW] >= 2 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 2 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 2)
            && (centreCol == PlantSpawning.PlantColour.YELLOW || centreCol == PlantSpawning.PlantColour.GREEN || centreCol == PlantSpawning.PlantColour.BLUE))
            ||
            ((cccNoC[PlantSpawning.PlantColour.GREEN] >= 2 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 2 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2)
            && (centreCol == PlantSpawning.PlantColour.GREEN || centreCol == PlantSpawning.PlantColour.BLUE || centreCol == PlantSpawning.PlantColour.PURPLE))
            ||
            ((cccNoC[PlantSpawning.PlantColour.BLUE] >= 2 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2 && cccNoC[PlantSpawning.PlantColour.RED] >= 2)
            && (centreCol == PlantSpawning.PlantColour.BLUE || centreCol == PlantSpawning.PlantColour.PURPLE || centreCol == PlantSpawning.PlantColour.RED))
            ||
            ((cccNoC[PlantSpawning.PlantColour.PURPLE] >= 2 && cccNoC[PlantSpawning.PlantColour.RED] >= 2 && cccNoC[PlantSpawning.PlantColour.ORANGE] >= 2)
            && (centreCol == PlantSpawning.PlantColour.PURPLE || centreCol == PlantSpawning.PlantColour.RED || centreCol == PlantSpawning.PlantColour.ORANGE))
            )
        {
            resultCntr = PlantSpawning.BouquetCentres.SPECTRUM;
        }

        // JEWELBED (CONTRAST)
        else if (
            ((cccNoC[PlantSpawning.PlantColour.RED] >= 3 && cccNoC[PlantSpawning.PlantColour.GREEN] >= 3)
            && (centreCol == PlantSpawning.PlantColour.RED || centreCol == PlantSpawning.PlantColour.GREEN))
            ||
            ((cccNoC[PlantSpawning.PlantColour.ORANGE] >= 3 && cccNoC[PlantSpawning.PlantColour.BLUE] >= 3)
            && (centreCol == PlantSpawning.PlantColour.ORANGE || centreCol == PlantSpawning.PlantColour.BLUE))
            ||
            ((cccNoC[PlantSpawning.PlantColour.YELLOW] >= 3 && cccNoC[PlantSpawning.PlantColour.PURPLE] >= 3)
            && (centreCol == PlantSpawning.PlantColour.YELLOW || centreCol == PlantSpawning.PlantColour.PURPLE))
            ||
            // alternate conditions - solid bouquet harmony but contrasting centrepiece
            ((cccNoC[PlantSpawning.PlantColour.RED] >= 6)
            && centreCol == PlantSpawning.PlantColour.GREEN)
            ||
            ((cccNoC[PlantSpawning.PlantColour.ORANGE] >= 6)
            && centreCol == PlantSpawning.PlantColour.BLUE)
            ||
            ((cccNoC[PlantSpawning.PlantColour.YELLOW] >= 6)
            && centreCol == PlantSpawning.PlantColour.PURPLE)
            ||
            ((cccNoC[PlantSpawning.PlantColour.GREEN] >= 6)
            && centreCol == PlantSpawning.PlantColour.RED)
            ||
            ((cccNoC[PlantSpawning.PlantColour.BLUE] >= 6)
            && centreCol == PlantSpawning.PlantColour.ORANGE)
            ||
            ((cccNoC[PlantSpawning.PlantColour.PURPLE] >= 6)
            && centreCol == PlantSpawning.PlantColour.YELLOW)
            )
        {
            resultCntr = PlantSpawning.BouquetCentres.JEWELBED;
        }

        // TROVE (SOLID)
        else if (
            (cccNoC[PlantSpawning.PlantColour.RED] >= 6
            && centreCol == PlantSpawning.PlantColour.RED)
            ||
            (cccNoC[PlantSpawning.PlantColour.ORANGE] >= 6
            && centreCol == PlantSpawning.PlantColour.ORANGE)
            ||
            (cccNoC[PlantSpawning.PlantColour.YELLOW] >= 6
            && centreCol == PlantSpawning.PlantColour.YELLOW)
            ||
            (cccNoC[PlantSpawning.PlantColour.GREEN] >= 6
            && centreCol == PlantSpawning.PlantColour.GREEN)
            ||
            (cccNoC[PlantSpawning.PlantColour.BLUE] >= 6
            && centreCol == PlantSpawning.PlantColour.BLUE)
            ||
            (cccNoC[PlantSpawning.PlantColour.PURPLE] >= 6
            && centreCol == PlantSpawning.PlantColour.PURPLE)
            )
        {
            resultCntr = PlantSpawning.BouquetCentres.TROVE;
        }

        return resultCntr;
    }

    public List<PlantSpawning.BouquetSpecials> AssignBouquetSpecials(List<PlantSpawning.PlantType> pTs, List <PlantSpawning.PlantValue> pVs)
    {
        List<PlantSpawning.BouquetSpecials> resultSpcs = new List<PlantSpawning.BouquetSpecials>();

        // RADIANT
        foreach (PlantSpawning.OnePlantInfo pi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist)
        {
            if (pi.plantName == plantNameArray[0])
            {
                foreach (PlantSpawning.PlantSpecials piPS in pi.plantSpc)
				{
                    if (piPS == PlantSpawning.PlantSpecials.RARE)
					{
                        resultSpcs.Add(PlantSpawning.BouquetSpecials.RADIANT);
                        break;
					}
				}
            }
        }

        // MONOSPECIES - all accents are the same plant type
        //Debug.LogWarning("loop begins");
        for (int i = 0; i < pTs.Count; i += 1)
		{
            //Debug.LogError(pTs[1] + " vs " + pTs[i]);
            if (pTs[1] != pTs[i])
                break;

            //Debug.Log("success, keep going");

            if (i == pTs.Count - 1)
			{
                resultSpcs.Add(PlantSpawning.BouquetSpecials.MONOSPECIES);
                //Debug.LogWarning("monospecies awarded");
			}
		}

        // DELICATE
        int pales = 0;
        foreach (PlantSpawning.PlantValue value in pVs)
		{
            if (value == PlantSpawning.PlantValue.PALE)
                pales += 1;
		}
        if (pales >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.DELICATE);

        // BOLD
        int brights = 0;
        foreach (PlantSpawning.PlantValue value in pVs)
        {
            if (value == PlantSpawning.PlantValue.BRIGHT)
                brights += 1;
        }
        if (brights >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.BOLD);

        // REFINED
        int vibrants = 0;
        foreach (PlantSpawning.PlantValue value in pVs)
        {
            if (value == PlantSpawning.PlantValue.VIBRANT)
                vibrants += 1;
        }
        if (vibrants >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.REFINED);

        // ELEGANT
        int darks = 0;
        foreach (PlantSpawning.PlantValue value in pVs)
        {
            if (value == PlantSpawning.PlantValue.DARK)
                darks += 1;
        }
        if (darks >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.ELEGANT);

        // DOMINANTS
        if (ccc[PlantSpawning.PlantColour.RED] >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.RED_DOMINANT);
        if (ccc[PlantSpawning.PlantColour.ORANGE] >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.ORANGE_DOMINANT);
        if (ccc[PlantSpawning.PlantColour.YELLOW] >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.YELLOW_DOMINANT);
        if (ccc[PlantSpawning.PlantColour.GREEN] >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.GREEN_DOMINANT);
        if (ccc[PlantSpawning.PlantColour.BLUE] >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.BLUE_DOMINANT);
        if (ccc[PlantSpawning.PlantColour.PURPLE] >= 4)
            resultSpcs.Add(PlantSpawning.BouquetSpecials.PURPLE_DOMINANT);

        return resultSpcs;
    }

    public void CountColours(List<PlantSpawning.PlantColour> inBouquetCols)
    {
        // Count all colours available in bouquet
        foreach (PlantSpawning.PlantColour item in inBouquetCols)
        {
            ccc[item] += 1;
        }

        // Same as above but it excludes the centrepiece
        for (int i = 1;  i < inBouquetCols.Count; i += 1)
		{
            cccNoC[inBouquetCols[i]] += 1;
		}
    }

    public void ResetColourCount()
    {
        ccc.Clear();
        ccc.Add(PlantSpawning.PlantColour.RED, 0);
        ccc.Add(PlantSpawning.PlantColour.ORANGE, 0);
        ccc.Add(PlantSpawning.PlantColour.YELLOW, 0);
        ccc.Add(PlantSpawning.PlantColour.GREEN, 0);
        ccc.Add(PlantSpawning.PlantColour.BLUE, 0);
        ccc.Add(PlantSpawning.PlantColour.PURPLE, 0);

        cccNoC.Clear();
        cccNoC.Add(PlantSpawning.PlantColour.RED, 0);
        cccNoC.Add(PlantSpawning.PlantColour.ORANGE, 0);
        cccNoC.Add(PlantSpawning.PlantColour.YELLOW, 0);
        cccNoC.Add(PlantSpawning.PlantColour.GREEN, 0);
        cccNoC.Add(PlantSpawning.PlantColour.BLUE, 0);
        cccNoC.Add(PlantSpawning.PlantColour.PURPLE, 0);
    }

    public IEnumerator ShakeBouquet(float intensityMult)
	{
        // Reset anim
        if (shakeyCoro != null) StopCoroutine(shakeyCoro);
        bouquetObj.transform.localEulerAngles = bqDefaultLocalRotation;

        // Tilting Bouquet
        bouquetObj.transform.localEulerAngles = new Vector3(
            bouquetObj.transform.localEulerAngles.x + Random.Range(-3f, 3f) * intensityMult,
            bouquetObj.transform.localEulerAngles.y + Random.Range(-3f, 3f) * intensityMult,
            bouquetObj.transform.localEulerAngles.z + Random.Range(-3f, 3f) * intensityMult
            );

        // Reorienting bouquet back to normal
        float t = 0.34f;
		while (t > 0f)
		{
            bouquetObj.transform.localEulerAngles = new Vector3(
                Mathf.LerpAngle(bouquetObj.transform.localEulerAngles.x, bqDefaultLocalRotation.x, 12f * Time.deltaTime),
                Mathf.LerpAngle(bouquetObj.transform.localEulerAngles.y, bqDefaultLocalRotation.y, 12f * Time.deltaTime),
                Mathf.LerpAngle(bouquetObj.transform.localEulerAngles.z, bqDefaultLocalRotation.z, 12f * Time.deltaTime)
                );

            t -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
		}
        bouquetObj.transform.localEulerAngles = bqDefaultLocalRotation;
    }
}
