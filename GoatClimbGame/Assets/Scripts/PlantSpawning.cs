using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawning : MonoBehaviour
{
    // Tutorial here:
    // https://www.youtube.com/watch?v=gfD8S32xzYI

    #region Plant Spawning Details
    // WARNING - IF ANY PARTS OF THE CODE IN THIS REGION HAS BEEN EDITED, THE INFO INPUTTED IN THE INSPECTOR WILL RESET
    [System.Serializable]
    public class OnePlantSpawn
	{
        [Header("Spawning Settings")]
        public GameObject plantPrefab;
        public string plantName;
        public float spawnChancePercent;

        [Header("Raycast Variables")]
        public LayerMask layerToCheck;
        public float distBtwnPsMin, distBtwnPsMax;
        public float heightOfCheck = 10f, rangeOfCheck = 30f; // height is the highest point where plants can be spawned, raycasted DOWNWARDS to range
        public Vector2 positivePos, negativePos; // Area where plant would be spawned
    }
    [SerializeField]
    public OnePlantSpawn[] plantSpawnDeets;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlants();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnPlants()
	{
        //Debug.Log("spawning begins now - " + plantSpawnDeets.Length + " plants detected");

        // For loop within for loop
        foreach (OnePlantSpawn thisPlant in plantSpawnDeets)
		{
            // For loop in for loop to spawn plants within area
            for (float x = thisPlant.negativePos.x; x < thisPlant.positivePos.x; x += Random.Range(thisPlant.distBtwnPsMin, thisPlant.distBtwnPsMax))
            {
                //Debug.Log("loop 1");
                for (float z = thisPlant.negativePos.y; z < thisPlant.positivePos.y; z += Random.Range(thisPlant.distBtwnPsMin, thisPlant.distBtwnPsMax))
                {
                    //Debug.Log("loop 2");
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(x, thisPlant.heightOfCheck, z), Vector3.down, out hit, thisPlant.rangeOfCheck, thisPlant.layerToCheck))
					{
                        //Debug.LogWarning("raycast'd");
                        // Spawn frequency of plant based on the percent chance defined
                        if (thisPlant.spawnChancePercent > Random.Range(0f, 100f))
						{
                            //Debug.LogWarning("chance success'd");
                            // Using object pooling to spawn plants - see GameMainframe for object pooling usage
                            GameMainframe.GetInstance().ObjectUse(thisPlant.plantName, (singlePlant) =>
							{
                                // Defining this one spawned object's properties
                                singlePlant.name = thisPlant.plantName;
                                singlePlant.transform.position = hit.point;
                                singlePlant.transform.eulerAngles = new Vector3(0f, Random.Range(-65f, 65f), 0f);
                                singlePlant.transform.parent = this.gameObject.transform;
                                //Debug.LogError("spawn'd");
							}, thisPlant.plantPrefab);
						}
					}
                }

            }
        }
    }
}
