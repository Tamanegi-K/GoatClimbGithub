using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMainframe : MonoBehaviour
{
    [Header("Object Idenfitication")]
    public PlayerController playerContrScrpt;
    public AudioManager audioMngr;

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
            objChosen = objectPools[poolName].Find(obj => obj.name.Contains(poolName));
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
        if (GameObject.Find("Player").TryGetComponent(out PlayerController pcs))
            playerContrScrpt = pcs;

        if (TryGetComponent(out AudioManager amg))
            audioMngr = amg;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
