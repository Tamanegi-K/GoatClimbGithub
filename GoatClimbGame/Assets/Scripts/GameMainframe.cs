using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class GameMainframe : MonoBehaviour
{
    [Header("Object Idenfitication")]
    public PlayerController playerContrScrpt;
    public SkinnedMeshRenderer goteMesh;
    public AudioManager audioMngr;
    public GameObject inventoryDisplay;
    public bool inTitle = false;

    [Header("Variables")]
    private bool titleAnimStarted = false, gameStarted = false;
    private List<GameObject> invHudObjs = new List<GameObject>();
    private bool setupComplete = false, firstPickToggled = false;

    [Header("Prefab Housing")]
    public GameObject hudPopupPrefab;
    public GameObject hudInventoryPrefab;

    [Header("Pause Menu Interactivity")]
    public bool gameSuspended = false;
    public enum PauseTabs { KNAPSACK, ASSEMBLY, SETTINGS };
    public PauseTabs currentTab = PauseTabs.KNAPSACK;
    public CanvasGroup uiGroupTitle, uiGroupWhite, uiGroupPause, uiGroupHUD;
    private RectTransform pauseUp, pauseLeft, pauseRightSack, pauseRightAss, pauseRightSttngs;
    private ToggleGroup pauseUpTG;
    private GameObject pauseTabSack, pauseTabAss, pauseTabSttngs;
    private Transform sackObjHolder;

    #region OBJECT POOLING
    public static Dictionary<string, List<GameObject>> objectPools = new Dictionary<string, List<GameObject>>();

    // Dynamic Pool Creator
    public static List<GameObject> GetPool(string poolName)
    {
        if (!objectPools.ContainsKey(poolName))
        {
            objectPools.Add(poolName, new List<GameObject>());
        }

        return objectPools[poolName];
    }

    // Object Pooling version of GameObject.Instantiate()
    // To use, do GameMainframe.GetInstance().ObjectUse()
    // Don't forget to use SetActive(true)!!
    public GameObject ObjectUse(string poolName, System.Action<GameObject> objLoaded, GameObject objPrefab)
    {
        GameObject objChosen;

        if (objectPools.TryGetValue(poolName, out List<GameObject> z) && objectPools[poolName].Find(obj => obj.name.Contains(poolName)))
        {
            objChosen = objectPools[poolName][0];
            objectPools[poolName].Remove(objChosen);
            objLoaded?.Invoke(objChosen);
        }
        else
        {
            objChosen = Instantiate(objPrefab);
            objLoaded?.Invoke(objChosen);
        }
        //objChosen.SetActive(true);
        return objChosen;
    }

    // Object pooling version of GameObject.Destroy()
    // To use, do GameMainframe.GetInstance().ObjectEnd()
    // Don't forget to use SetActive(false)!!
    public void ObjectEnd(string poolName, GameObject obj)
    {
        GetPool(poolName).Add(obj);
        //obj.SetActive(false);
    }
    #endregion

    #region SINGLETON
    // Singleton for GameMainframe
    private static GameMainframe itsMe;
    public static GameMainframe GetInstance() => itsMe;

    private void Awake()
    {
        if (itsMe != null)
        // if another GameMainframe already exist, kms
        {
            Destroy(gameObject);
        }
        else
        {
            // if the GameMainframe haven't existed, this instance becomes the OG
            itsMe = this;
            //DontDestroyOnLoad(gameObject); // This line makes it so that the GameMainframe persists between scenes - disable if not needed 
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SetUpObjs();
    }

    void Update()
    {
        if (!setupComplete)
            return;

        // Pause menu opened
        if (gameSuspended)
        {
            uiGroupPause.gameObject.SetActive(true);

            if (Mathf.Abs(uiGroupPause.alpha - 1f) <= 0.05f)
            {
                uiGroupPause.alpha = 1f;

                pauseUp.anchoredPosition = Vector3.zero;
                pauseLeft.anchoredPosition = new Vector3(0f, -64f, 0f);
                // TO DO: Each pauseRight grp to move out based on what tab is toggled
                pauseRightSack.anchoredPosition = Vector3.zero;
            }
            else
            {
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 1f, Time.deltaTime * 6.9f);

                pauseUp.anchoredPosition = Vector3.Lerp(pauseUp.anchoredPosition, Vector3.zero, Time.deltaTime * 14f);
                pauseLeft.anchoredPosition = Vector3.Lerp(pauseLeft.anchoredPosition, new Vector3(0f, -64f, 0f), Time.deltaTime * 14f);
                pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, Vector3.zero, Time.deltaTime * 14f);
            }
        }
        // Pause menu closed
        else
        {
            if (Mathf.Abs(uiGroupPause.alpha - 0f) <= 0.05f)
            {
                uiGroupPause.alpha = 0f;
                //uiGroupPause.gameObject.SetActive(false);

                pauseUp.anchoredPosition = new Vector3(0f, 192f, 0f);
                pauseLeft.anchoredPosition = new Vector3(-1024f, -64f, 0f);
                pauseRightSack.anchoredPosition = new Vector3(1024f, 0f, 0f);
            }
            else
            {
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 0f, Time.deltaTime * 6.9f);

                pauseUp.anchoredPosition = Vector3.Lerp(pauseUp.anchoredPosition, new Vector3(0f, 192f, 0f), Time.deltaTime * 8f);
                pauseLeft.anchoredPosition = Vector3.Lerp(pauseLeft.anchoredPosition, new Vector3(-1024f, -64f, 0f), Time.deltaTime * 8f);
                pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, new Vector3(1024f, -0, 0f), Time.deltaTime * 8f);
            }
        }
    }

    void FixedUpdate()
    {
        if (!gameSuspended)
            return;

        // If Knapsack tab is selected, the display obj on the right will speeeen slowly
        if (pauseUpTG.GetFirstActiveToggle() == pauseTabSack.GetComponent<Toggle>())
        {
            Vector3 speeen = sackObjHolder.localEulerAngles;
            speeen.y = speeen.y > 360f ? speeen.y + (Time.fixedDeltaTime * 18f) - 360f : speeen.y + (Time.fixedDeltaTime * 18f);
            sackObjHolder.localEulerAngles = speeen;
        }
    }

	public bool GetTitleStartedState()
    {
        return titleAnimStarted;
    }

    public bool GetGameStartedState()
    {
        return gameStarted;
    }

    public bool GetGameSuspendState()
    {
        return gameSuspended;
    }

    public void ToggleGameSuspendState()
    {
        gameSuspended = !gameSuspended;
    }

    public IEnumerator ToggleTitleFade()
    {
        if (titleAnimStarted)
            yield return null;

        audioMngr.StopBGMCurrent();
        audioMngr.ReflushInitAmbiences();

        titleAnimStarted = true;

        audioMngr.PlayAMBPersistent("gamestart");

        // White fading in
        uiGroupWhite.alpha = 0f;
        uiGroupWhite.gameObject.SetActive(true);
        for (float i = 0; i < 1f; i += Time.deltaTime)
        {
            uiGroupWhite.alpha += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return new WaitForSeconds(0.5f);

        goteMesh.enabled = true;

        // Title fading out
        for (float i = 0; i < 1f; i += Time.deltaTime)
        {
            uiGroupTitle.alpha -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        uiGroupTitle.alpha = 0f;
        yield return new WaitForSeconds(0.5f);

        playerContrScrpt.controlGiven = true;
        playerContrScrpt.TogglePlayerControl();
        audioMngr.ForceBGMCD(Random.Range(audioMngr.bgmCDmin * 10f, audioMngr.bgmCDmax * 10f));
        gameStarted = true;
        gameSuspended = false;

        // White fading out
        for (float i = 0; i < 1f; i += Time.deltaTime)
        {
            uiGroupWhite.alpha -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return new WaitForSeconds(0.5f);


        yield return null;
    }

    public void SetUpObjs()
    {
        if (audioMngr == null && GameObject.Find("AudioManager").TryGetComponent(out AudioManager amg))
            audioMngr = amg;

        if (uiGroupTitle == null && GameObject.Find("Canvas/TitleUIGroup").TryGetComponent(out CanvasGroup t))
            uiGroupTitle = t;

        if (uiGroupWhite == null && GameObject.Find("Canvas/WhiteOut").TryGetComponent(out CanvasGroup wo))
            uiGroupWhite = wo;

        if (uiGroupPause == null && GameObject.Find("Canvas/PauseUIGroup").TryGetComponent(out CanvasGroup p))
        {
            uiGroupPause = p;
            inventoryDisplay = p.transform.Find("InventoryQtyGroup/InventoryScrollBounds/InventoryDisplay").gameObject;

            // Other Pause Menu stuff
            pauseUp = uiGroupPause.transform.Find("PauseTitleGroup").GetComponent<RectTransform>();
            pauseLeft = uiGroupPause.transform.Find("InventoryQtyGroup").GetComponent<RectTransform>();
            pauseRightSack = uiGroupPause.transform.Find("InvDescGroup").GetComponent<RectTransform>();

            pauseUpTG = pauseUp.GetComponent<ToggleGroup>();

            pauseTabSack = pauseUp.transform.Find("PauseBtnKnapsack").gameObject;
            sackObjHolder = pauseRightSack.Find("ObjHolder").transform;

            pauseTabAss = pauseUp.transform.Find("PauseBtnAssembly").gameObject;
            pauseTabSttngs = pauseUp.transform.Find("PauseBtnSettings").gameObject;
        }

        if (uiGroupHUD == null && GameObject.Find("Canvas/HUD").TryGetComponent(out CanvasGroup h))
            uiGroupHUD = h;

        if (!inTitle)
        {
            if (playerContrScrpt == null && GameObject.Find("Player").TryGetComponent(out PlayerController pcs))
            {
                playerContrScrpt = pcs;
                goteMesh = playerContrScrpt.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            uiGroupWhite.gameObject.SetActive(true); uiGroupWhite.alpha = 0f;
            uiGroupPause.gameObject.SetActive(true); uiGroupPause.alpha = 0f;
        }

        setupComplete = true;
    }

    public void PauseTabKS()
    {
        currentTab = PauseTabs.KNAPSACK;
    }

    public void PauseTabAss()
    {
        currentTab = PauseTabs.ASSEMBLY;
    }

    public void PauseTabSttngs()
    {
        currentTab = PauseTabs.SETTINGS;
    }

    public void UpdateInventoryQuantities() // Displays of the right side of the inventory screen and also runs right side stuff (see bottom for individual functions based on tab toggles)
    {
        //InventoryItemBhv[] children = inventoryDisplay.GetComponentsInChildren<InventoryItemBhv>(true);
        // Flush the InvItem grid
        foreach (GameObject i in invHudObjs)
        {
            ObjectEnd("InvItem", i);
            i.SetActive(false);
        }

        invHudObjs.Clear();

        // Recreate the InvItem grid
        if (playerContrScrpt.collectedPlants.Count > 0)
        {
            foreach (KeyValuePair<string, int> thing in playerContrScrpt.collectedPlants)
            {
                ObjectUse("InvItem", (ii) =>
                {
                    InventoryItemBhv iiIib = ii.GetComponent<InventoryItemBhv>();
                    ii.name = "InvItem";

                    // Find the display object's prefab (A LITTLE BACK ASSWARDS BUT FUCK IT
                    GameObject foundDispObj = null;
                    foreach (PlantSpawning.OnePlantInfo pi in GetComponent<PlantSpawning>().plantMasterlist)
                    {
                        if (pi.plantName == thing.Key)
                        {
                            if (pi.plantPickedup != null) foundDispObj = pi.plantPickedup;
                            else foundDispObj = pi.plantOnfield;
                            break;
                        }
                    }
                    iiIib.SetupInvDisplay(thing.Key, thing.Value, foundDispObj);

                    ii.transform.SetParent(inventoryDisplay.transform);
                    ii.SetActive(true);


                    if (!firstPickToggled)
                    {
                        ii.GetComponent<Toggle>().isOn = true;
                        firstPickToggled = true;
                    }

                    invHudObjs.Add(ii);
                }, hudInventoryPrefab);
            }
        }
    }

    public void UpdateInventoryTabSelect(float value = 0) // Displays the top buttons and its selections
    {
        // Leftwards switch
        if (value < 0)
        {
            currentTab = (int)currentTab <= 0 ? 0 : currentTab -= 1;
        }
        // Rightwards switch
        else if (value > 0)
        {
            // if the currentTab is already the last one, remain on the last one. Otherwise, move rightwards once (don't be intimidated by the length of this ternary operator lmfao)
            currentTab = (int)currentTab >= System.Enum.GetNames(typeof(PauseTabs)).Length - 1 ? (PauseTabs)System.Enum.GetNames(typeof(PauseTabs)).Length - 1 : currentTab += 1;
        }

        // Display Setting
        pauseTabSack.GetComponentInChildren<TextMeshProUGUI>().text = "Knapsack";
        pauseTabAss.GetComponentInChildren<TextMeshProUGUI>().text = "Assembly";
        pauseTabSttngs.GetComponentInChildren<TextMeshProUGUI>().text = "Settings";
        GameObject selectedToggle = null;

        switch (currentTab)
        {
            case PauseTabs.KNAPSACK:
                pauseTabSack.GetComponentInChildren<TextMeshProUGUI>().text = "• Knapsack •";
                break;
            case PauseTabs.ASSEMBLY:
                pauseTabAss.GetComponentInChildren<TextMeshProUGUI>().text = "• Assembly •";
                break;
            case PauseTabs.SETTINGS:
                pauseTabSttngs.GetComponentInChildren<TextMeshProUGUI>().text = "• Settings •";
                break;
        }

        if (value != 0) selectedToggle.GetComponent<Toggle>().isOn = true;
    }

    public void InvOnSelect(string input) // Does various things based on what tab is currently selected when clicking on an inventory item
	{
        // TO DO : FOR OTHER RIGHT SIDE INVENTORY SHITS

        // Description appearance
        if (pauseUpTG.GetFirstActiveToggle() == pauseTabSack.GetComponent<Toggle>())
        {
            GameObject dispObj = null;
            foreach (PlantSpawning.OnePlantInfo pi in GetComponent<PlantSpawning>().plantMasterlist)
            {
                // Search the plantMasterlist and grab its display obj and desc
                if (pi.plantName == input)
                {
                    // --- DESCRIPTION DISPLAYING ---
                    pauseRightSack.Find("InvDescBG").GetComponentInChildren<TextMeshProUGUI>().text = pi.plantDesc;

                    // --- OBJECT DISPLAYING ---
                    // Check if object has a display object, otherwise use the whole damn plant
                    if (pi.plantPickedup != null) dispObj = pi.plantPickedup;
                    else dispObj = pi.plantOnfield;

                    // Erasing the display object in desc area
                    foreach (Transform c in sackObjHolder)
                    {
                        GetInstance().ObjectEnd(c.name, c.gameObject);
                        c.gameObject.SetActive(false);
                    }

                    // Spawning the display object in desc area
                    GetInstance().ObjectUse(input + "Disp", (pickedDisplay) =>
                    {
                        pickedDisplay.name = pickedDisplay.name.Contains("Disp") ? pickedDisplay.name : input + "Disp";
                        pickedDisplay.transform.SetParent(sackObjHolder);

                        pickedDisplay.transform.localPosition = Vector3.zero;
                        pickedDisplay.transform.localEulerAngles = Vector3.zero;
                        pickedDisplay.transform.localScale = Vector3.one;
                        pickedDisplay.gameObject.SetActive(true);
                    }, dispObj);
                    break;
                }
            }
        }
    }
}
