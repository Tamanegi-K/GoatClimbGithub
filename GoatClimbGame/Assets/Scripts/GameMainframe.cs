using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMainframe : MonoBehaviour
{
    [Header("Object Idenfitication")]
    public PlayerController playerContrScrpt;
    public SkinnedMeshRenderer goteMesh;
    public AudioManager audioMngr;
    public CanvasGroup uiGroupTitle, uiGroupWhite, uiGroupPause, uiGroupHUD;
    private RectTransform pauseUp, pauseLeft, pauseRight;
    public GameObject inventoryDisplay;
    public bool inTitle = false;

    [Header("Variables")]
    private bool titleAnimStarted = false, gameStarted = false, gameSuspended = false;
    private List<GameObject> invHudObjs = new List<GameObject>();

    [Header("Prefab Housing")]
    public GameObject hudPopupPrefab;
    public GameObject hudInventoryPrefab;

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

        // Pause Menu stuff
        pauseUp = uiGroupPause.transform.Find("PauseTitle").GetComponent<RectTransform>();
        pauseLeft = uiGroupPause.transform.Find("InventoryBG").GetComponent<RectTransform>();
        pauseRight = uiGroupPause.transform.Find("InvDescGroup").GetComponent<RectTransform>();
    }

	void Update()
	{
        // Pause menu appearance
        if (gameSuspended)
        {
            uiGroupPause.gameObject.SetActive(true);

            if (Mathf.Abs(uiGroupPause.alpha - 1f) <= 0.05f)
			{
                uiGroupPause.alpha = 1f;

                pauseUp.anchoredPosition = Vector3.zero;
                pauseLeft.anchoredPosition = new Vector3(0f, -64f, 0f);
                pauseRight.anchoredPosition = Vector3.zero;
            }
            else
			{
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 1f, Time.deltaTime * 6.9f);

                pauseUp.anchoredPosition = Vector3.Lerp(pauseUp.anchoredPosition, Vector3.zero, Time.deltaTime * 14f);
                pauseLeft.anchoredPosition = Vector3.Lerp(pauseLeft.anchoredPosition, new Vector3(0f, -64f, 0f), Time.deltaTime * 14f);
                pauseRight.anchoredPosition = Vector3.Lerp(pauseRight.anchoredPosition, Vector3.zero, Time.deltaTime * 14f);
            }
        }
        else
        {
            if (Mathf.Abs(uiGroupPause.alpha - 0f) <= 0.05f)
            {
                uiGroupPause.alpha = 0f;
                //uiGroupPause.gameObject.SetActive(false);

                pauseUp.anchoredPosition = new Vector3(0f, 192f, 0f);
                pauseLeft.anchoredPosition = new Vector3(-1024f, -64f, 0f);
                pauseRight.anchoredPosition = new Vector3(1024f, 0f, 0f);
            }
            else
			{
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 0f, Time.deltaTime * 6.9f);

                pauseUp.anchoredPosition = Vector3.Lerp(pauseUp.anchoredPosition, new Vector3(0f, 192f, 0f), Time.deltaTime * 8f);
                pauseLeft.anchoredPosition = Vector3.Lerp(pauseLeft.anchoredPosition, new Vector3(-1024f, -64f, 0f), Time.deltaTime * 8f);
                pauseRight.anchoredPosition = Vector3.Lerp(pauseRight.anchoredPosition, new Vector3(1024f, -0, 0f), Time.deltaTime * 8f);
            }
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
        for (float i = 0; i < 1f;  i += Time.deltaTime)
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
            inventoryDisplay = p.transform.Find("InventoryBG/InventoryScrollBounds/InventoryDisplay").gameObject;
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
    }

    public void UpdateInventoryDisplay()
	{
        //InventoryItemBhv[] children = inventoryDisplay.GetComponentsInChildren<InventoryItemBhv>(true);
        // Flush
        foreach (GameObject i in invHudObjs)
        {
            ObjectEnd("InvItem", i);
            i.SetActive(false);
        }

        invHudObjs.Clear();

        // Recreate
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

                    invHudObjs.Add(ii);
                }, hudInventoryPrefab);
            }
        }
    }
}
