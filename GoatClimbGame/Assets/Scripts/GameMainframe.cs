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
    public bool inTitle = false;

    [Header("Variables")]
    private bool titleAnimStarted = false, gameStarted = false, gameSuspended = false;

    [Header("Prefab Housing")]
    public GameObject hudPopupPrefab;

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
        // Pause menu appearance
        if (gameSuspended)
        {
            uiGroupPause.gameObject.SetActive(true);

            if (Mathf.Abs(uiGroupPause.alpha - 1f) <= 0.05f)
                uiGroupPause.alpha = 1f;
            else
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 1f, Time.deltaTime * 6.9f);
        }
        else
        {
            if (Mathf.Abs(uiGroupPause.alpha - 0f) <= 0.05f)
            {
                uiGroupPause.alpha = 0f;
                uiGroupPause.gameObject.SetActive(false);
            }
            else
                uiGroupPause.alpha = Mathf.Lerp(uiGroupPause.alpha, 0f, Time.deltaTime * 6.9f);
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
        yield return new WaitForSeconds(0.5f);

        playerContrScrpt.controlGiven = true;
        playerContrScrpt.TogglePlayerControl();
        gameStarted = true;

        // White fading out
        for (float i = 0; i < 1f; i += Time.deltaTime)
        {
            uiGroupWhite.alpha -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return new WaitForSeconds(0.5f);

        gameSuspended = false;

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
            uiGroupPause = p;

        if (uiGroupHUD == null && GameObject.Find("Canvas/HUD").TryGetComponent(out CanvasGroup h))
            uiGroupHUD = h;

        if (!inTitle)
        {
            if (playerContrScrpt == null && GameObject.Find("Player").TryGetComponent(out PlayerController pcs))
            {
                playerContrScrpt = pcs;
                goteMesh = playerContrScrpt.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            uiGroupWhite.gameObject.SetActive(false);
            uiGroupPause.gameObject.SetActive(false);
        }
    }
}
