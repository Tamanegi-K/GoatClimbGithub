using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Rendering;

public class GameMainframe : MonoBehaviour
{
    [Header("Object Idenfitication")]
    public PlayerController playerContrScr;
    public PlantSpawning plantSpawningScr;
    public SkinnedMeshRenderer goteMesh;
    public AudioManager audioMngr;
    public GameObject inventoryDisplay;
    public bool inTitle = false;
    public List<GameObject> villagersOnField;
    public MailboxBhv mailbox;

    [Header("Variables")]
    public Transform sunRotation;
    private LensFlareComponentSRP sunLensFlare;
    private bool titleAnimStarted = false, gameStarted = false;
    private List<GameObject> invHudObjs = new List<GameObject>();
    private bool setupComplete = false, firstPickToggled = false;
    private Vector3 pauseCoordsUpOpened = Vector3.zero, // Opened meaning game is suspended
                  pauseCoordsLeftOpened = new Vector3(0f, -64f, 0f),
                 pauseCoordsRightOpened = Vector3.zero,
                    pauseCoordsUpClosed = new Vector3(0f, 192f, 0f), // Closed meaning game is NOT suspended
                  pauseCoordsLeftClosed = new Vector3(-1024f, -64f, 0f),
                 pauseCoordsRightClosed = new Vector3(1024f, 0f, 0f);
    public static bool isCurrentlyDay = true;
    public static float daytimeSpeed = 0.24f;
    public static float daytimeSpeedInit;
    public static Action DayHasChanged;

    [Header("Prefab Housing")]
    public GameObject hudPopupPrefab;
    public GameObject hudInventoryPrefab;
    public GameObject pauseSackTagPrefab;

    [Header("Pause Menu Interactivity")]
    public bool gameSuspended = false, isGiving = false;
    public enum PauseTabs { KNAPSACK, ASSEMBLY, SETTINGS };
    public PauseTabs currentTab = PauseTabs.KNAPSACK;
    public CanvasGroup uiGroupTitle, uiGroupWhite, uiGroupPause, uiGroupHUD;
    private RectTransform pauseUp, pauseLeft, pauseRightSack, pauseRightAss, pauseRightSackTags, pauseRightSttngs, pauseRightTrade, pauseRightTradeTagsLeft, pauseRightTradeTagsRight;
    private ToggleGroup pauseUpTG;
    private GameObject pauseTabSack, pauseTabAss, pauseTabSttngs;
    private Transform sackObjHolder, assGridList, tradeObjHolder;
    private Button tradeBtnYes;
    private string thingToTrade = "";

    #region Request Details
    private int totalRequests = 0;
    public static float requestDiff = 0;
    [System.Serializable]
    public class Request
    {
        public string requesteeName;
        public int requesteeID;
        public PlantSpawning.BouquetHarmony requestedHarm;
        public PlantSpawning.BouquetCentres requestedCntr;
        public List<PlantSpawning.BouquetSpecials> requestedSpcs;

        public Request(string iRQName, int iRQId, PlantSpawning.BouquetHarmony iRQHarm, PlantSpawning.BouquetCentres iRQCntr, List<PlantSpawning.BouquetSpecials> iRQSpcs)
        {
            requesteeName = iRQName; requesteeID = iRQId;
            requestedHarm = iRQHarm; requestedCntr = iRQCntr; requestedSpcs = iRQSpcs;
        }
    }
    [SerializeField]
    public List<Request> requestList;
    #endregion

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
		{
            SetUpObjs();
            return;
        }
        
        PauseMenuUIAppearance(gameSuspended);
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
            tradeObjHolder.localEulerAngles = speeen;
        }
    }

	void LateUpdate()
	{
        if (!gameStarted || gameSuspended)
            return;

        //sunRotation.Rotate(-Time.deltaTime * 0.24f, 0f, 0f);
        sunRotation.RotateAround(sunRotation.position, Vector3.forward, -Time.deltaTime * daytimeSpeed);
        float lensScale = Mathf.Clamp(3f * Mathf.Sin(sunRotation.eulerAngles.x * Mathf.Deg2Rad), 0f, 3f);
        sunLensFlare.scale = lensScale;

        if ((lensScale > 0f && !isCurrentlyDay) || (lensScale <= 0f && isCurrentlyDay))
            ChangeTimeOfDay();

    }

    public IEnumerator ToggleTitleFade()
    {
        if (titleAnimStarted)
            yield return null;

        if (audioMngr == null)
		{
            SetUpObjs();
            audioMngr.SetAudioListener(playerContrScr.transform.Find("CameraPivot").gameObject);
		}

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

        playerContrScr.controlGiven = true;
        playerContrScr.TogglePlayerControl();
        audioMngr.ForceBGMCD(UnityEngine.Random.Range(audioMngr.bgmCDmin * 30f, audioMngr.bgmCDmax * 30f));
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

            // --- OTHER PAUSE MENU STUFF ---

            // Up - Tabs
            pauseUp = uiGroupPause.transform.Find("PauseTitleGroup").GetComponent<RectTransform>();
            pauseUpTG = pauseUp.GetComponent<ToggleGroup>();
            pauseTabSack = pauseUp.transform.Find("PauseBtnKnapsack").gameObject;
            pauseTabAss = pauseUp.transform.Find("PauseBtnAssembly").gameObject;
            pauseTabSttngs = pauseUp.transform.Find("PauseBtnSettings").gameObject;

            // Left - Inventory Grid
            pauseLeft = uiGroupPause.transform.Find("InventoryQtyGroup").GetComponent<RectTransform>();

            // Right 1 - Knapsack
            pauseRightSack = uiGroupPause.transform.Find("InvDescGroup").GetComponent<RectTransform>();
            pauseRightSackTags = pauseRightSack.Find("InvDescTops/InvDescTagsVP/InvDescTags").GetComponent<RectTransform>();
            sackObjHolder = pauseRightSack.Find("ObjHolder").transform;

            // Right 2 - Assembly
            pauseRightAss = uiGroupPause.transform.Find("InvAssGroup").GetComponent<RectTransform>();
            assGridList = pauseRightAss.transform.Find("AssGridList").transform;

            // Right 3 - Settings
            pauseRightSttngs = uiGroupPause.transform.Find("InvSettingsGroup").GetComponent<RectTransform>();

            // Right 4 - Trading
            pauseRightTrade = uiGroupPause.transform.Find("InvTradeGroup").GetComponent<RectTransform>();
            pauseRightTradeTagsLeft = pauseRightTrade.Find("InvTradeLefts/InvTradeLeftTagsVP/InvTradeLeftTags").GetComponent<RectTransform>();
            pauseRightTradeTagsRight = pauseRightTrade.Find("InvTradeRights/InvTradeRightTagsVP/InvTradeRightTags").GetComponent<RectTransform>();
            tradeObjHolder = pauseRightTrade.Find("ObjHolder").transform;
            tradeBtnYes = pauseRightTrade.Find("TradeGiftBtn").GetComponent<Button>();
            //Debug.Log(pauseRightTradeTagsRight);
        }

        if (uiGroupHUD == null && GameObject.Find("Canvas/HUD").TryGetComponent(out CanvasGroup h))
            uiGroupHUD = h;

        if (!inTitle)
        {
            if (playerContrScr == null && GameObject.Find("Player").TryGetComponent(out PlayerController pcs))
            {
                playerContrScr = pcs;
                goteMesh = playerContrScr.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (plantSpawningScr == null)
            {
                plantSpawningScr = GetComponent<PlantSpawning>();
            }

            uiGroupWhite.gameObject.SetActive(true); uiGroupWhite.alpha = 0f;
            uiGroupPause.gameObject.SetActive(true); uiGroupPause.alpha = 0f;
        }

        if (mailbox == null && GameObject.Find("MailboxObj").TryGetComponent(out MailboxBhv mb))
            mailbox = mb;

        if (sunRotation == null && GameObject.Find("Sun").TryGetComponent(out Transform sT))
        {
            sunRotation = sT;
            sunLensFlare = sunRotation.GetComponent<LensFlareComponentSRP>();
        }

        daytimeSpeedInit = daytimeSpeed;

        setupComplete = true;
    }

    public void PauseMenuUIAppearance(bool isSuspended)
	{
        if (!setupComplete)
            return;

        bool leCheckOpened = Mathf.Abs(uiGroupPause.alpha - 1f) <= 0.05f;
        bool leCheckClosed = Mathf.Abs(uiGroupPause.alpha - 0f) <= 0.05f;

        // Pause menu opened
        if (isSuspended)
        {
            // Left and ups
            if (leCheckOpened)
            {
                uiGroupPause.alpha = 1f;

                pauseUp.anchoredPosition = pauseCoordsUpOpened;
                pauseLeft.anchoredPosition = pauseCoordsLeftOpened;
            }
            else
            {
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 1f, Time.deltaTime * 6.9f);

                pauseUp.anchoredPosition = Vector3.Lerp(pauseUp.anchoredPosition, pauseCoordsUpOpened, Time.deltaTime * 14f);
                pauseLeft.anchoredPosition = Vector3.Lerp(pauseLeft.anchoredPosition, pauseCoordsLeftOpened, Time.deltaTime * 14f);
            }

            // Rights (only one should be onscreen)
            switch (currentTab)
			{
                case PauseTabs.KNAPSACK:
                    if (isGiving)
					{
                        pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                        pauseRightTrade.anchoredPosition = Vector3.Lerp(pauseRightTrade.anchoredPosition, pauseCoordsRightOpened, Time.deltaTime * 14f);
                    }
                    else
                    {
                        pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, pauseCoordsRightOpened, Time.deltaTime * 14f);
                        pauseRightTrade.anchoredPosition = Vector3.Lerp(pauseRightTrade.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    }
                    pauseRightAss.anchoredPosition = Vector3.Lerp(pauseRightAss.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    pauseRightSttngs.anchoredPosition = Vector3.Lerp(pauseRightSttngs.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    break;
                case PauseTabs.ASSEMBLY:
                    pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    pauseRightTrade.anchoredPosition = Vector3.Lerp(pauseRightTrade.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    pauseRightAss.anchoredPosition = Vector3.Lerp(pauseRightAss.anchoredPosition, pauseCoordsRightOpened, Time.deltaTime * 14f);
                    pauseRightSttngs.anchoredPosition = Vector3.Lerp(pauseRightSttngs.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    break;
                case PauseTabs.SETTINGS:
                    pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    pauseRightTrade.anchoredPosition = Vector3.Lerp(pauseRightTrade.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    pauseRightAss.anchoredPosition = Vector3.Lerp(pauseRightAss.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 14f);
                    pauseRightSttngs.anchoredPosition = Vector3.Lerp(pauseRightSttngs.anchoredPosition, pauseCoordsRightOpened, Time.deltaTime * 14f);
                    break;

            }
        }

        // Pause menu closed
        else
        {
            if (leCheckClosed)
            {
                uiGroupPause.alpha = 0f;

                pauseUp.anchoredPosition = pauseCoordsUpClosed;
                pauseLeft.anchoredPosition = pauseCoordsLeftClosed;

                pauseRightSack.anchoredPosition = pauseCoordsRightClosed;
                pauseRightTrade.anchoredPosition = pauseCoordsRightClosed;

                pauseRightAss.anchoredPosition = pauseCoordsRightClosed;
                pauseRightSttngs.anchoredPosition = pauseCoordsRightClosed;
            }
            else
            {
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 0f, Time.deltaTime * 6.9f);

                pauseUp.anchoredPosition = Vector3.Lerp(pauseUp.anchoredPosition, pauseCoordsUpClosed, Time.deltaTime * 8f);
                pauseLeft.anchoredPosition = Vector3.Lerp(pauseLeft.anchoredPosition, pauseCoordsLeftClosed, Time.deltaTime * 8f);
                
                pauseRightSack.anchoredPosition = Vector3.Lerp(pauseRightSack.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 8f);
                pauseRightTrade.anchoredPosition = Vector3.Lerp(pauseRightTrade.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 8f);

                pauseRightAss.anchoredPosition = Vector3.Lerp(pauseRightAss.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 8f);
                pauseRightSttngs.anchoredPosition = Vector3.Lerp(pauseRightSttngs.anchoredPosition, pauseCoordsRightClosed, Time.deltaTime * 8f);
            }
        }
    }

    #region Functions for Buttons (see inspector)
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
	#endregion

	public void UpdateInventoryQuantities() // Displays of the LEFT side of the inventory screen and also runs right side stuff (see bottom for individual functions based on tab toggles)
    {
        //InventoryItemBhv[] children = inventoryDisplay.GetComponentsInChildren<InventoryItemBhv>(true);
        // Flush the InvItem grid
        foreach (GameObject i in invHudObjs)
        {
            //// Erasing the display object INSIDE the grid
            //foreach (Transform c in i.transform.Find("IconArea/ObjHolder"))
            //{
            //    c.transform.SetParent(null);
            //    ObjectEnd(c.name, c.gameObject);
            //    c.gameObject.SetActive(false);
            //}

            i.transform.SetParent(null);
            ObjectEnd("InvGrid", i);
            i.SetActive(false);
        }

        invHudObjs.Clear();

        // Recreate the InvItem grid
        if (playerContrScr.collectedInventory.Count > 0)
        {
            foreach (KeyValuePair<string, int> thing in playerContrScr.collectedInventory)
            {
                ObjectUse("InvGrid", (ii) =>
                {
                    InventoryItemBhv iiIib = ii.GetComponent<InventoryItemBhv>();
                    ii.name = "InvGrid";

                    // Find the display object's prefab (A LITTLE BACK ASSWARDS BUT FUCK IT
                    GameObject foundDispObj = null;
                    foreach (PlantSpawning.OnePlantInfo pi in GetComponent<PlantSpawning>().plantMasterlist)
                    {
                        if (pi.plantName == thing.Key)
                        {
                            if (pi.plantPickedup != null) foundDispObj = pi.plantPickedup;
                            else foundDispObj = pi.plantOnfield;

                            iiIib.SetupInvDisplay(thing.Key, thing.Value, foundDispObj);
                            iiIib.isNotPlant = false;
                            break;
                        }
                    }

                    foreach (PlantSpawning.OneBouquetMade bm in GetComponent<PlantSpawning>().bouquetsMade)
                    {
                        if (bm.bqName == thing.Key)
                        {
                            foundDispObj = bm.bqObj;

                            iiIib.SetupInvDisplay(thing.Key, thing.Value, foundDispObj);
                            iiIib.isNotPlant = true;
                            break;
                        }
                    }

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
        audioMngr.PlaySFXUI("confirm");

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
                selectedToggle = pauseTabSack;
                break;
            case PauseTabs.ASSEMBLY:
                pauseTabAss.GetComponentInChildren<TextMeshProUGUI>().text = "• Assembly •";
                selectedToggle = pauseTabAss;
                break;
            case PauseTabs.SETTINGS:
                pauseTabSttngs.GetComponentInChildren<TextMeshProUGUI>().text = "• Settings •";
                selectedToggle = pauseTabSttngs;
                break;
        }

        if (value != 0) selectedToggle.GetComponent<Toggle>().isOn = true;
    }

    public void InvClick(string input) // Does various things based on what tab is currently selected when clicking on an inventory item
	{
        // (check InvItemClick() in InventoryItemBhv.cs)

        // Description appearance based on tab is currently active
        if (pauseUpTG.GetFirstActiveToggle() == pauseTabSack.GetComponent<Toggle>())
        {
            GameObject dispObj = null;

            // Logic for plants
            foreach (PlantSpawning.OnePlantInfo pi in GetComponent<PlantSpawning>().plantMasterlist)
            {
                // Search the plantMasterlist and grab its display obj and desc
                if (pi.plantName == input)
                {
                    pauseRightSack.Find("InvDescTops").GetComponent<CanvasGroup>().alpha = 1f;
   
                    // --- NAME DISPLAYING ---
                    pauseRightSack.Find("InvDescTops/InvDescTopTitle/InvNameBG").GetComponentInChildren<TextMeshProUGUI>().text = pi.plantName;
                    pauseRightTrade.Find("InvTradeLefts/InvTradeLeftName/InvNameBG").GetComponentInChildren<TextMeshProUGUI>().text = pi.plantName;

                    // --- TAGS AND COLOURS DISPLAYING ---
                    // Erasing the display tags in desc tag area
                    foreach (Transform pt in pauseRightSackTags)
                    {
                        GetInstance().ObjectEnd("InvTag", pt.gameObject);
                        pt.gameObject.SetActive(false);
                    }
                    foreach (Transform pt in pauseRightTradeTagsLeft)
                    {
                        GetInstance().ObjectEnd("InvTagL", pt.gameObject);
                        pt.gameObject.SetActive(false);
                    }

                    // Placing tag for plant's value
                    GetInstance().ObjectUse("InvTag", (pickedDisplay) =>
                    {
                        pickedDisplay.name = "InvTag";
                        pickedDisplay.transform.SetParent(pauseRightSackTags);

                        pickedDisplay.transform.localPosition = Vector3.zero;
                        pickedDisplay.GetComponent<InvTagInfo>().AssignTag(pi.plantVal);
                        pickedDisplay.gameObject.SetActive(true);
                    }, pauseSackTagPrefab);

                    GetInstance().ObjectUse("InvTagL", (pickedDisplay) =>
                    {
                        pickedDisplay.name = "InvTagL";
                        pickedDisplay.transform.SetParent(pauseRightTradeTagsLeft);

                        pickedDisplay.transform.localPosition = Vector3.zero;
                        pickedDisplay.GetComponent<InvTagInfo>().AssignTag(pi.plantVal);
                        pickedDisplay.gameObject.SetActive(true);
                    }, pauseSackTagPrefab);

                    // Placing tag for plant's value
                    GetInstance().ObjectUse("InvTag", (pickedDisplay) =>
                    {
                        pickedDisplay.name = "InvTag";
                        pickedDisplay.transform.SetParent(pauseRightSackTags);

                        pickedDisplay.transform.localPosition = Vector3.zero;
                        pickedDisplay.GetComponent<InvTagInfo>().AssignTag(pi.plantCol);
                        pickedDisplay.gameObject.SetActive(true);
                    }, pauseSackTagPrefab);

                    GetInstance().ObjectUse("InvTagL", (pickedDisplay) =>
                    {
                        pickedDisplay.name = "InvTagL";
                        pickedDisplay.transform.SetParent(pauseRightTradeTagsLeft);

                        pickedDisplay.transform.localPosition = Vector3.zero;
                        pickedDisplay.GetComponent<InvTagInfo>().AssignTag(pi.plantCol);
                        pickedDisplay.gameObject.SetActive(true);
                    }, pauseSackTagPrefab);

                    // Placing tag for plant's specials, one for each one
                    foreach (PlantSpawning.PlantSpecials pSpc in pi.plantSpc)
                    {
                        if (pSpc != PlantSpawning.PlantSpecials.NONE)
                        {
                            GetInstance().ObjectUse("InvTag", (pickedDisplay) =>
                            {
                                pickedDisplay.name = "InvTag";
                                pickedDisplay.transform.SetParent(pauseRightSackTags);

                                pickedDisplay.transform.localPosition = Vector3.zero;
                                pickedDisplay.GetComponent<InvTagInfo>().AssignTag(pSpc);
                                pickedDisplay.gameObject.SetActive(true);
                            }, pauseSackTagPrefab);

                            GetInstance().ObjectUse("InvTagL", (pickedDisplay) =>
                            {
                                pickedDisplay.name = "InvTagL";
                                pickedDisplay.transform.SetParent(pauseRightTradeTagsLeft);

                                pickedDisplay.transform.localPosition = Vector3.zero;
                                pickedDisplay.GetComponent<InvTagInfo>().AssignTag(pSpc);
                                pickedDisplay.gameObject.SetActive(true);
                            }, pauseSackTagPrefab);
                        }
                    }

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
                    foreach (Transform c in tradeObjHolder)
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

                        // change layer of display object
                        foreach (Transform tf in pickedDisplay.transform)
                        {
                            tf.gameObject.layer = LayerMask.NameToLayer("UI");
                        }

                        pickedDisplay.gameObject.SetActive(true);
                    }, dispObj);

                    GetInstance().ObjectUse(input + "DispL", (pickedDisplay) =>
                    {
                        pickedDisplay.name = pickedDisplay.name.Contains("DispL") ? pickedDisplay.name : input + "DispL";
                        pickedDisplay.transform.SetParent(tradeObjHolder);

                        pickedDisplay.transform.localPosition = Vector3.zero;
                        pickedDisplay.transform.localEulerAngles = Vector3.zero;
                        pickedDisplay.transform.localScale = Vector3.one;

                        // change layer of display object
                        foreach (Transform tf in pickedDisplay.transform)
                        {
                            tf.gameObject.layer = LayerMask.NameToLayer("UI");
                        }

                        pickedDisplay.gameObject.SetActive(true);
                    }, dispObj);
                    break;
                }
            }

            // Logic for non-plants
            foreach (PlantSpawning.OneBouquetMade bm in GetComponent<PlantSpawning>().bouquetsMade)
            {
                // Search the plantMasterlist and grab its display obj and desc
                if (bm.bqName == input)
                {
                    pauseRightSack.Find("InvDescTops").GetComponent<CanvasGroup>().alpha = 1f;

                    // --- NAME DISPLAYING ---
                    pauseRightSack.Find("InvDescTops/InvDescTopTitle/InvNameBG").GetComponentInChildren<TextMeshProUGUI>().text = bm.bqName;
                    pauseRightTrade.Find("InvTradeLefts/InvTradeLeftName/InvNameBG").GetComponentInChildren<TextMeshProUGUI>().text = bm.bqName;

                    // --- TAGS AND COLOURS DISPLAYING ---
                    // Erasing the display tags in desc tag area
                    foreach (Transform pt in pauseRightSackTags)
                    {
                        GetInstance().ObjectEnd("InvTag", pt.gameObject);
                        pt.gameObject.SetActive(false);
                    }
                    foreach (Transform pt in pauseRightTradeTagsLeft)
                    {
                        GetInstance().ObjectEnd("InvTagL", pt.gameObject);
                        pt.gameObject.SetActive(false);
                    }

                    // Placing tag for bouquet's harmony
                    if (bm.bqHarm != PlantSpawning.BouquetHarmony.NONE)
                    {
                        GetInstance().ObjectUse("InvTag", (pickedDisplay) =>
                        {
                            pickedDisplay.name = "InvTag";
                            pickedDisplay.transform.SetParent(pauseRightSackTags);

                            pickedDisplay.transform.localPosition = Vector3.zero;
                            pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bm.bqHarm);
                            pickedDisplay.gameObject.SetActive(true);
                        }, pauseSackTagPrefab);

                        GetInstance().ObjectUse("InvTagL", (pickedDisplay) =>
                        {
                            pickedDisplay.name = "InvTagL";
                            pickedDisplay.transform.SetParent(pauseRightTradeTagsLeft);

                            pickedDisplay.transform.localPosition = Vector3.zero;
                            pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bm.bqHarm);
                            pickedDisplay.gameObject.SetActive(true);
                        }, pauseSackTagPrefab);
                    }

                    // placing tag for bouquet's centre
                    if (bm.bqCntr != PlantSpawning.BouquetCentres.NONE)
                    {
                        GetInstance().ObjectUse("InvTag", (pickedDisplay) =>
                        {
                            pickedDisplay.name = "InvTag";
                            pickedDisplay.transform.SetParent(pauseRightSackTags);

                            pickedDisplay.transform.localPosition = Vector3.zero;
                            pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bm.bqCntr);
                            pickedDisplay.gameObject.SetActive(true);
                        }, pauseSackTagPrefab);

                        GetInstance().ObjectUse("InvTagL", (pickedDisplay) =>
                        {
                            pickedDisplay.name = "InvTagL";
                            pickedDisplay.transform.SetParent(pauseRightTradeTagsLeft);

                            pickedDisplay.transform.localPosition = Vector3.zero;
                            pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bm.bqCntr);
                            pickedDisplay.gameObject.SetActive(true);
                        }, pauseSackTagPrefab);
                    }

                    // Placing tag for bouquet's specials, one for each one
                    foreach (PlantSpawning.BouquetSpecials bSpc in bm.bqSpcs)
                    {
                        if (bSpc != PlantSpawning.BouquetSpecials.NONE)
                        {
                            GetInstance().ObjectUse("InvTag", (pickedDisplay) =>
                            {
                                pickedDisplay.name = "InvTag";
                                pickedDisplay.transform.SetParent(pauseRightSackTags);

                                pickedDisplay.transform.localPosition = Vector3.zero;
                                pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bSpc);
                                pickedDisplay.gameObject.SetActive(true);
                            }, pauseSackTagPrefab);

                            GetInstance().ObjectUse("InvTagL", (pickedDisplay) =>
                            {
                                pickedDisplay.name = "InvTagL";
                                pickedDisplay.transform.SetParent(pauseRightTradeTagsLeft);

                                pickedDisplay.transform.localPosition = Vector3.zero;
                                pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bSpc);
                                pickedDisplay.gameObject.SetActive(true);
                            }, pauseSackTagPrefab);
                        }
                    }

                    // --- DESCRIPTION DISPLAYING ---
                    pauseRightSack.Find("InvDescBG").GetComponentInChildren<TextMeshProUGUI>().text = bm.bqDesc;

                    // --- OBJECT DISPLAYING ---
                    // Check if object has a display object, otherwise use the whole damn plant
                    dispObj = bm.bqObj;

                    // Erasing the display object in desc area
                    foreach (Transform c in sackObjHolder)
                    {
                        GetInstance().ObjectEnd(c.name, c.gameObject);
                        c.gameObject.SetActive(false);
                    }
                    foreach (Transform c in tradeObjHolder)
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

                    GetInstance().ObjectUse(input + "DispL", (pickedDisplay) =>
                    {
                        pickedDisplay.name = pickedDisplay.name.Contains("DispL") ? pickedDisplay.name : input + "DispL";
                        pickedDisplay.transform.SetParent(tradeObjHolder);

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

    public string PascalCaseString(string input)
	{
        input = input.Replace("_", " ");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        //return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
	}

    public static void ChangeTimeOfDay()
    {
        isCurrentlyDay = !isCurrentlyDay;

        AudioManager.GetInstance().ReflushInitAmbiences();

        if (DayHasChanged != null)
            DayHasChanged();
    }

    public void GenerateRequest()
	{
        totalRequests += 1;

        // get a random villager and make the request
        int randomIndex = UnityEngine.Random.Range(0, villagersOnField.Count);
        string randomName = villagersOnField[randomIndex].GetComponent<VillagerBhv>().villagerName;

        // randomise request details
        PlantSpawning.BouquetHarmony randomHarm = PlantSpawning.BouquetHarmony.NONE;
        PlantSpawning.BouquetCentres randomCntr = PlantSpawning.BouquetCentres.NONE;
        List<PlantSpawning.BouquetSpecials> randomSpcs = new List<PlantSpawning.BouquetSpecials>();
        bool givenHarm = false, givenCntr = false, given4V = false, given4C = true;

        // TO DO - MAKE IT USE EVERY TAG, FOR NOW WE'RE TRUNCATING IT SINCE WE DON'T HAVE ENOUGH FLOWERS
        for (float i = requestDiff; i > 0 ; i -= 1)
        {
            string r = plantSpawningScr.GetRandomQuestTag(givenHarm, givenCntr, given4V, given4C);

            if (Enum.TryParse(r, out PlantSpawning.BouquetHarmony rH))
            {
                if (!givenHarm)
                {
                    if (givenCntr) // IMPOSSIBLE CASE PREVENTION
                    {
                        if (rH == PlantSpawning.BouquetHarmony.MULTICOLOURED)
						{
                            // nope
                            givenHarm = true;
                            givenCntr = true;
                            given4C = true;
						}
                        else if (randomCntr == PlantSpawning.BouquetCentres.SPECTRUM && rH == PlantSpawning.BouquetHarmony.ANALOGOUS)
                        {
                            randomHarm = rH;
                            givenHarm = true;
                        }
                        else if (randomCntr == PlantSpawning.BouquetCentres.PARTITION && rH == PlantSpawning.BouquetHarmony.TRIADIC)
                        {
                            randomHarm = rH;
                            givenHarm = true;
                        }
                        else if (randomSpcs.Count > 0)
                        {
                            foreach (PlantSpawning.BouquetSpecials b in randomSpcs)
                            {
                                if (b == PlantSpawning.BouquetSpecials.MONOSPECIES && rH == PlantSpawning.BouquetHarmony.SOLID)
                                {
                                    // literally cannot
                                    givenHarm = true;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (randomSpcs.Count > 0 && randomSpcs[0] == PlantSpawning.BouquetSpecials.UNIFORM)
                        {
                            // nope
                            givenHarm = true;
                        }
                        else
                        {
                            randomHarm = rH;
                            givenHarm = true;
                        }
                    }
                }
            }
            else if (Enum.TryParse(r, out PlantSpawning.BouquetCentres rC))
            {
                if (!givenCntr)
                {
                    if (givenHarm) // IMPOSSIBLE CASE PREVENTION
                    {
                        if (randomHarm == PlantSpawning.BouquetHarmony.MULTICOLOURED)
                        {
                            // can't get a centre this way
                            givenCntr = true;
                        }
                        else if (randomHarm == PlantSpawning.BouquetHarmony.ANALOGOUS && rC == PlantSpawning.BouquetCentres.SPECTRUM)
                        {
                            randomCntr = rC;
                            givenCntr = true;
                        }
                        else if (randomHarm == PlantSpawning.BouquetHarmony.TRIADIC && rC == PlantSpawning.BouquetCentres.PARTITION)
                        {
                            randomCntr = rC;
                            givenCntr = true;
                        }
                    }
                    else
                    {
                        if (randomSpcs.Count > 0 && randomSpcs[0] == PlantSpawning.BouquetSpecials.UNIFORM && (rC == PlantSpawning.BouquetCentres.SPECTRUM || rC == PlantSpawning.BouquetCentres.PARTITION))
                        {
                            // nope
                            givenCntr = true;
                        }
                        else
                        {
                            randomCntr = rC;
                            givenCntr = true;
                        }
                    }
                }
            }
            else if (Enum.TryParse(r, out PlantSpawning.BouquetSpecials rS))
            {
                // uniform is very very picky
                if (rS == PlantSpawning.BouquetSpecials.UNIFORM)
                {
                    if (randomSpcs.Count <= 0 &&
                        (randomCntr != PlantSpawning.BouquetCentres.PARTITION || randomCntr != PlantSpawning.BouquetCentres.SPECTRUM) &&
                        (randomHarm != PlantSpawning.BouquetHarmony.MULTICOLOURED || randomHarm != PlantSpawning.BouquetHarmony.CONTRASTING || randomHarm != PlantSpawning.BouquetHarmony.ANALOGOUS || randomHarm != PlantSpawning.BouquetHarmony.TRIADIC)
                        )
                    {
                        randomSpcs.Add(rS);
                        given4V = true;
                        given4C = true;
                    }
                }
                else
                {
                    if (!given4V &&
                        (rS == PlantSpawning.BouquetSpecials.DELICATE || rS == PlantSpawning.BouquetSpecials.BOLD ||
                        rS == PlantSpawning.BouquetSpecials.REFINED || rS == PlantSpawning.BouquetSpecials.MYSTERIOUS))
                    {
                        randomSpcs.Add(rS);
                        given4V = true;
                        given4C = true;
                    }
                    else if (!given4C && !givenHarm && !givenCntr &&
                        (rS == PlantSpawning.BouquetSpecials.LOVELY || rS == PlantSpawning.BouquetSpecials.CONFIDENT ||
                        rS == PlantSpawning.BouquetSpecials.JOYFUL || rS == PlantSpawning.BouquetSpecials.HOPEFUL ||
                        rS == PlantSpawning.BouquetSpecials.SERENE || rS == PlantSpawning.BouquetSpecials.ELEGANT))
                    {
                        randomSpcs.Add(rS);
                        given4C = true;
                        givenHarm = true;
                        givenCntr = true;
                    }
                }
            }
        }

        Request newRequest = new Request(randomName, totalRequests, randomHarm, randomCntr, randomSpcs);
        villagersOnField[randomIndex].GetComponent<VillagerBhv>().requestID = totalRequests;
        requestList.Add(newRequest);

        // DISPLAYING OF TAGS IN REQUEST WINDOW
        // Erasing the display tags in desc tag area
        foreach (Transform pt in pauseRightTradeTagsRight)
        {
            GetInstance().ObjectEnd("InvTagR", pt.gameObject);
            pt.gameObject.SetActive(false);
        }

        // --- TAGS AND COLOURS DISPLAYING ---
        // Erasing the display tags in desc tag area
        foreach (Transform pt in pauseRightTradeTagsRight)
        {
            GetInstance().ObjectEnd("InvTagR", pt.gameObject);
            pt.gameObject.SetActive(false);
        }

        // Placing tag for bouquet's harmony/centre
        if (newRequest.requestedHarm != PlantSpawning.BouquetHarmony.NONE)
        {
            GetInstance().ObjectUse("InvTagR", (pickedDisplay) =>
            {
                pickedDisplay.name = "InvTagR";
                pickedDisplay.transform.SetParent(pauseRightTradeTagsRight);

                pickedDisplay.transform.localPosition = Vector3.zero;
                pickedDisplay.GetComponent<InvTagInfo>().AssignTag(newRequest.requestedHarm);
                pickedDisplay.gameObject.SetActive(true);
            }, pauseSackTagPrefab);
        }

        if (newRequest.requestedCntr != PlantSpawning.BouquetCentres.NONE)
        {
            GetInstance().ObjectUse("InvTagR", (pickedDisplay) =>
            {
                pickedDisplay.name = "InvTagR";
                pickedDisplay.transform.SetParent(pauseRightTradeTagsRight);

                pickedDisplay.transform.localPosition = Vector3.zero;
                pickedDisplay.GetComponent<InvTagInfo>().AssignTag(newRequest.requestedCntr);
                pickedDisplay.gameObject.SetActive(true);
            }, pauseSackTagPrefab);
        }

        // Placing tag for bouquet's specials, one for each one
        foreach (PlantSpawning.BouquetSpecials bSpc in newRequest.requestedSpcs)
        {
            if (bSpc != PlantSpawning.BouquetSpecials.NONE)
            {
                GetInstance().ObjectUse("InvTagR", (pickedDisplay) =>
                {
                    pickedDisplay.name = "InvTagR";
                    pickedDisplay.transform.SetParent(pauseRightTradeTagsRight);

                    pickedDisplay.transform.localPosition = Vector3.zero;
                    pickedDisplay.GetComponent<InvTagInfo>().AssignTag(bSpc);
                    pickedDisplay.gameObject.SetActive(true);
                }, pauseSackTagPrefab);
            }
        }
    }

    public void CheckRequest(string clickedItem, bool ifFalseJustDontEvenDoAnything)
	{
        if (!ifFalseJustDontEvenDoAnything)
            return;


        if (currentTab == PauseTabs.KNAPSACK && GetGameGivingState())
        {
            foreach (PlantSpawning.OneBouquetMade bq in GetInstance().plantSpawningScr.bouquetsMade)
            {
                if (bq.bqName == clickedItem && playerContrScr.GetInventoryQty(clickedItem) > 0)
                {
                    bool checkHarm = false, checkCntr = false;
                    int checkSpcs = 0;

                    if (PlantSpawning.BouquetHarmony.NONE == requestList[0].requestedHarm ||
                        bq.bqHarm == requestList[0].requestedHarm)
                        checkHarm = true;

                    if (PlantSpawning.BouquetCentres.NONE == requestList[0].requestedCntr ||
                        bq.bqCntr == requestList[0].requestedCntr)
                        checkCntr = true;

                    foreach (PlantSpawning.BouquetSpecials reqBS in requestList[0].requestedSpcs)
                    {
                        if (PlantSpawning.BouquetSpecials.NONE != reqBS)
                        {
                            foreach (PlantSpawning.BouquetSpecials boqBS in bq.bqSpcs)
                            {
                                if (reqBS == boqBS)
                                    checkSpcs += 1;
                            }
                        }
                        else
                            checkSpcs += 1;
                    }

                    if (checkHarm && checkCntr && checkSpcs >= requestList[0].requestedSpcs.Count)
                    {
                        thingToTrade = clickedItem;
                        //tradeBtnYes.onClick.AddListener(delegate { FinishRequest(); });
                        tradeBtnYes.interactable = true;
                        break;
                    }
                    else
                    {
                        tradeBtnYes.interactable = false;
                    }
                }
            }

        }

    }

    public void FinishRequest()
	{
        // TO DO - SAFELY REMOVE REQUEST FROM LIST AND RELEASE VILLAGER'S REQUESTID, RN IT'LL JUST REMOVE THE FIRST ONE
        //foreach (GameMainframe.Request oneRequest in GameMainframe.GetInstance().requestList)
        //{
        audioMngr.PlaySFXUI("confirm");

        foreach (GameObject go in GetInstance().villagersOnField)
        {
            if (requestList[0].requesteeName == go.GetComponent<VillagerBhv>().villagerName)
            {
                go.GetComponent<VillagerBhv>().requestID = 0;
                requestList.RemoveAt(0);
                
                requestDiff = Mathf.Clamp(requestDiff + 0.32f, 1f, 6f); // requests to "get harder" the more you do them
                playerContrScr.UpdateInventory(thingToTrade, -1);
                thingToTrade = "";

                playerContrScr.TogglePlayerControl();
                tradeBtnYes.interactable = false;
                break;
            }
        }
        //}
    }

    public bool GetTitleStartedState() => titleAnimStarted;
    public bool GetGameStartedState() => gameStarted;
    public bool GetGameSuspendState() => gameSuspended;
    public void ToggleGameSuspendState() => gameSuspended = !gameSuspended;
    public bool GetGameGivingState() => isGiving;
    public void SetGameGivingState(bool b) => isGiving = b;
    public void ResetSetup(bool b) => setupComplete = b;
    public Toggle GetPauseTabSack() => pauseTabSack.GetComponent<Toggle>();
    public ToggleGroup GetPauseTabToggleGrp() => pauseUpTG.GetComponent<ToggleGroup>();
    public Toggle GetPauseTabAss() => pauseTabAss.GetComponent<Toggle>();
    public BouquetAssemblyBhv GetAssRight() => pauseRightAss.GetComponent<BouquetAssemblyBhv>();
    public Toggle GetPauseTabSttngs() => pauseRightSttngs.GetComponent<Toggle>();
}
