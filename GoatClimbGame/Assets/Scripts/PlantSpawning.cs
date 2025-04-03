using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlantSpawning : MonoBehaviour
{
    // Tutorial here:
    // https://www.youtube.com/watch?v=gfD8S32xzYI

    // PLANT TAGS
    public enum PlantType { HYACINTH, HYDRANGEA, HIBISCUS, TULIP, ORCHID, LILY, SNOWPRISM } // species of flower
    public enum PlantValue { PALE, BRIGHT, VIBRANT, DARK } // from most faded to most saturated, one only
    public enum PlantColour { RED, ORANGE, YELLOW, GREEN, BLUE, PURPLE }; // colour of flower, one only
    public enum PlantSpecials { NONE, LUSTROUS, INFLORESCENT, NIGHTBLOOM, RARE } // other characteristics that makes this flower special, can have multiple

    // BOUQUET TAGS - only for bouquets
    public enum BouquetHarmony { NONE, CONTRASTING, ANALOGOUS, TRIADIC, SOLID, MULTICOLOURED }; // colour harmonies existing in the bouquet, one only
    public enum BouquetCentres { NONE, JEWELBED, SPECTRUM, PARTITION, TROVE }; // characteristics of the centrepiece in the bouquet, one only
    public enum BouquetSpecials { NONE, RADIANT, MONOSPECIES, DELICATE, BOLD, REFINED, ELEGANT, // other characteristics that makes the bouquet special, can have multiple
                RED_DOMINANT, ORANGE_DOMINANT, YELLOW_DOMINANT, GREEN_DOMINANT, BLUE_DOMINANT, VIOLET_DOMINANT, PURPLE_DOMINANT };

    // DESCRIPTIONS FOR EVERY TAG, see the TagDescFiller() function
    public Hashtable TagDescs = new Hashtable();

    // Handling the number check on how many plants that spawned in the world
    public Dictionary<string, int> spawnedPlants = new Dictionary<string, int>();
    private bool firstSpawn = true;

    #region Plant Masterlist
    [System.Serializable]
    public class OnePlantInfo
    {
        public GameObject plantOnfield, plantPickedup;
        public string plantName;
        public PlantType plantTyp;
        public PlantValue plantVal;
        public PlantColour plantCol;
        public List<PlantSpecials> plantSpc;
        public string plantDesc;
    }

    [SerializeField]
    public OnePlantInfo[] plantMasterlist;
    #endregion

    #region Plant Spawning Details
    // WARNING - IF ANY PARTS OF THE CODE IN THIS REGION HAS BEEN EDITED, THE INFO INPUTTED IN THE INSPECTOR WILL RESET
    [System.Serializable]
    public class OnePlantSpawn
	{
        [Header("Spawning Settings")]
        //public GameObject plantPrefab;
        //public string plantName;
        [Tooltip("Based off the Element Num written Plant Masterlist")] public int indexOfPlant;
        [Tooltip("Amount received when picked")] public int amtWhenPicked;
        public float spawnChancePercent;
        public int minClusterAmt;

        [Header("Raycast Variables")]
        public LayerMask layerToCheck;
        public float distBtwnPsMin, distBtwnPsMax;
        public float heightOfCheck = 10f, rangeOfCheck = 30f; // height is the highest point where plants can be spawned, raycasted DOWNWARDS to range
        public Vector2 positivePos, negativePos; // Area where plant would be spawned

        [Header("Circle Raycast (only toggle the boolean)")]
        public bool useCircleAreaInstead = false;
        public Vector2 circlePoint;
        public float circleRadius;
    }
    [SerializeField]
    public OnePlantSpawn[] plantSpawnDeets;
    #endregion

    #region Making Bouquets
    [System.Serializable]
    public class OneBouquetMade
    {
        public GameObject bqObj;
        public int bqID;
        public string[] flowerNameArray = new string[7] { "", "", "", "", "", "", "" };
        public string bqName;
        public BouquetHarmony bqHarm;
        public BouquetCentres bqCntr;
        public List<BouquetSpecials> bqSpcs;
        public string bqDesc;

        // Making a new class with given variables
        public OneBouquetMade(GameObject iBQObj, int iBQid, string[] iFlowerNameArray, string iBQName, BouquetHarmony iBQHarm, BouquetCentres iBQCntr, List<BouquetSpecials> iBQSpcs, string iBQDesc)
        {
            bqObj = iBQObj; bqID = iBQid;
            flowerNameArray = iFlowerNameArray;
            bqName = iBQName; bqDesc = iBQDesc;
            bqHarm = iBQHarm; bqCntr = iBQCntr; bqSpcs = iBQSpcs;
        }
    }

    [SerializeField]
    public List<OneBouquetMade> bouquetsMade;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        TagDescFiller();
        GetRandomQuestTag();

        // every time the time of day changes, spawn plants again
        GameMainframe.DayHasChanged += SpawnPlants;
        SpawnPlants();
    }

    void OnDrawGizmosSelected()
    {
        foreach (OnePlantSpawn thisPlant in plantSpawnDeets)
        {
            if (thisPlant.positivePos.x > thisPlant.negativePos.x && thisPlant.positivePos.y > thisPlant.negativePos.y)
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            else
                Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.8f);

            // Draw a semitransparent green cylinder at the transforms position
            if (thisPlant.useCircleAreaInstead)
			{
                // Auto circle centre finder
                thisPlant.circlePoint = new Vector2(Mathf.Lerp(thisPlant.positivePos.x, thisPlant.negativePos.x, 0.5f), Mathf.Lerp(thisPlant.positivePos.y, thisPlant.negativePos.y, 0.5f));
                Vector3 centre = new Vector3(thisPlant.circlePoint.x, thisPlant.heightOfCheck, thisPlant.circlePoint.y);

                // Auto circle radius finder
                float diffX = Mathf.Abs(thisPlant.positivePos.x - thisPlant.negativePos.x), diffY = Mathf.Abs(thisPlant.positivePos.y - thisPlant.negativePos.y);
                thisPlant.circleRadius = diffX > diffY ? diffY / 2f : diffX / 2f;
                
                DrawCylinder(centre, Quaternion.Euler(90f, 0f, 0f), thisPlant.rangeOfCheck, thisPlant.circleRadius);
            }
            
            // Draw a semitransparent green cuboid at the transforms position
            else 
            {
                float   length = Mathf.Abs(Mathf.Abs(thisPlant.positivePos.x) - Mathf.Abs(thisPlant.negativePos.x));
                float  breadth = Mathf.Abs(Mathf.Abs(thisPlant.positivePos.y) - Mathf.Abs(thisPlant.negativePos.y));
                float   height = Mathf.Abs(thisPlant.rangeOfCheck);
                Vector3 centre = new Vector3(Mathf.Lerp(thisPlant.positivePos.x, thisPlant.negativePos.x, 0.5f), thisPlant.heightOfCheck - (thisPlant.rangeOfCheck / 2f), Mathf.Lerp(thisPlant.positivePos.y, thisPlant.negativePos.y, 0.5f));

                Gizmos.DrawWireCube(centre, new Vector3(length, height, breadth));
            }
        }

    }

    void SpawnPlants()
	{
        //Debug.Log("spawning begins now - " + plantSpawnDeets.Length + " plants detected");

        int spawnedCount = 0, strikes = 0;

        // For loop within for loop
        foreach (OnePlantSpawn thisPlant in plantSpawnDeets)
		{
            //Debug.LogError("spawning: " + thisPlant.plantName);
            // Spawns the plant x amount of times based on the minClusterAmt set in the inspector
            if (firstSpawn)
            {
                spawnedCount = 0;
            }
            // Once this function has ran once the amount spawned will decrease every day/night
            else
            {
                // if this plant has been spawned before, get the amount currently in the scene and add that amount to spawnedCount (so that it spawns less), otherwise it'll spawn a little more (but still less)
                spawnedCount = spawnedPlants.TryGetValue(plantMasterlist[thisPlant.indexOfPlant].plantName, out int onField) ? (thisPlant.minClusterAmt / 5) + onField : thisPlant.minClusterAmt / 5;
            }

            // For loop to spawn plants until minimum number is hit
            for (int i = 0; spawnedCount < thisPlant.minClusterAmt; i += 0)
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
                            // Spawn frequency of plant based on the percent chance defined - for circular area
                            if (thisPlant.useCircleAreaInstead && (new Vector2(x, z) - thisPlant.circlePoint).magnitude <= thisPlant.circleRadius && thisPlant.spawnChancePercent > Random.Range(0f, 100f))
							{
                                // Using object pooling to spawn plants - see GameMainframe for object pooling usage
                                GameMainframe.GetInstance().ObjectUse(plantMasterlist[thisPlant.indexOfPlant].plantName /*thisPlant.plantName*/, (singlePlant) =>
                                {
                                    // Defining this one spawned object's properties
                                    singlePlant.transform.position = hit.point + new Vector3 (Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                                    singlePlant.transform.eulerAngles = new Vector3(0f, Random.Range(-65f, 65f), 0f);
                                    singlePlant.transform.parent = this.gameObject.transform;

                                    PlantBhv singlePlantPBHV = singlePlant.GetComponentInChildren<PlantBhv>(); // code shortening
                                    singlePlantPBHV.SetPickupQty(thisPlant.amtWhenPicked); // setting amount to give when picked up
                                    singlePlantPBHV.SetPickedPrefab(plantMasterlist[thisPlant.indexOfPlant].plantPickedup); // setting object displays
                                    singlePlantPBHV.SetPlantTag(plantMasterlist[thisPlant.indexOfPlant].plantSpc[0]); // setting plant tag

                                    singlePlantPBHV.gameObject.name = plantMasterlist[thisPlant.indexOfPlant].plantName /*thisPlant.plantName*/; // set up plant name (only applies to object that's affected by animation)

                                    singlePlantPBHV.ObjReuse();
                                    singlePlant.SetActive(true);
                                }, plantMasterlist[thisPlant.indexOfPlant].plantOnfield /*thisPlant.plantPrefab*/);

                                // increment plant spawn stuff
                                spawnedCount += 1;
                                IncrementSpawns(plantMasterlist[thisPlant.indexOfPlant].plantName, 1);
                                //Debug.LogWarning("spawned, no. " + spawnedCount);
                            }

                            // same thing as above but for non circular areas
                            else if (!thisPlant.useCircleAreaInstead && thisPlant.spawnChancePercent > Random.Range(0f, 100f))
                            {
                                // Using object pooling to spawn plants - see GameMainframe for object pooling usage
                                GameMainframe.GetInstance().ObjectUse(plantMasterlist[thisPlant.indexOfPlant].plantName /*thisPlant.plantName*/, (singlePlant) =>
                                {
                                    // Defining this one spawned object's properties
                                    singlePlant.transform.position = hit.point + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                                    singlePlant.transform.eulerAngles = new Vector3(0f, Random.Range(-65f, 65f), 0f);
                                    singlePlant.transform.parent = this.gameObject.transform;

                                    PlantBhv singlePlantPBHV = singlePlant.GetComponentInChildren<PlantBhv>(); // code shortening
                                    singlePlantPBHV.SetPickupQty(thisPlant.amtWhenPicked); // setting amount to give when picked up
                                    singlePlantPBHV.SetPickedPrefab(plantMasterlist[thisPlant.indexOfPlant].plantPickedup); // setting object displays
                                    singlePlantPBHV.SetPlantTag(plantMasterlist[thisPlant.indexOfPlant].plantSpc[0]); // setting plant tag

                                    singlePlantPBHV.gameObject.name = plantMasterlist[thisPlant.indexOfPlant].plantName /*thisPlant.plantName*/; // set up plant name (only applies to object that's affected by animation

                                    singlePlantPBHV.ObjReuse();
                                    singlePlant.SetActive(true);
                                }, plantMasterlist[thisPlant.indexOfPlant].plantOnfield /*thisPlant.plantPrefab*/);

                                // increment plant spawn stuff
                                spawnedCount += 1;
                                IncrementSpawns(plantMasterlist[thisPlant.indexOfPlant].plantName, 1 /*thisPlant.amtWhenPicked*/);
                                //Debug.LogWarning("spawned, no. " + spawnedCount);
                            }
                        }
                    }
                }

                if (spawnedCount == 0)
                {
                    if (strikes >= 5)
					{
                        Debug.LogError("Loop broken for " + plantMasterlist[thisPlant.indexOfPlant].plantName + " spawning - either the spawnChance & minClusterAmt is too small, or the spawn area is unable to find the terrain.");
                        break;
					}
                    else
                        strikes += 1;
                }
            }
        }

        // 
        firstSpawn = false;
    }

    public void IncrementSpawns(string plantName, int quantity)
    {
        if (!spawnedPlants.ContainsKey(plantName))
        {
            spawnedPlants.Add(plantName, 0);
        }

        spawnedPlants[plantName] += quantity;
        //Debug.LogWarning("amount of " + plantName + " rn : " + spawnedPlants[plantName]);
    }

    private void TagDescFiller()
	{
        // PlantValues
        TagDescs.Add(PlantValue.PALE, "The colour on this flower is faded and pastel-like.");
        TagDescs.Add(PlantValue.BRIGHT, "The colour on this flower is bright and healthy."); // this is otherwise the "default"
        TagDescs.Add(PlantValue.VIBRANT, "The colour on this flower is deep and saturated.");
        TagDescs.Add(PlantValue.DARK, "The colour on this flower is dark and refined.");

        // PlantColours
        TagDescs.Add(PlantColour.RED, "A rosy colour representing affection, passion, and love.");
        TagDescs.Add(PlantColour.ORANGE, "A cheery colour representing enthusiasm, fascination, and confidence.");
        TagDescs.Add(PlantColour.YELLOW, "A lively colour representing friendship, happiness, and optimism.");
        TagDescs.Add(PlantColour.GREEN, "A vigourous colour representing renewal, longevity, and hope.\n\nGreen flowers are uncommon, considering how flowers adapted and evolved to attract pollinators with striking colours.");
        TagDescs.Add(PlantColour.BLUE, "A prudent colour representing beauty, inspiration, and serenity.");
        TagDescs.Add(PlantColour.PURPLE, "A majestic colour representing respect, luxury, and elegance.\n\nBoth reddish and blueish purples fit under this tag.");

        // PlantSpecials
        TagDescs.Add(PlantSpecials.LUSTROUS, "This flower has a shiny and crystal-like appearance.");
        TagDescs.Add(PlantSpecials.INFLORESCENT, "Many clusters of flowers grow from this plant's stem.");
        TagDescs.Add(PlantSpecials.NIGHTBLOOM, "The flower only blooms at night.");
        TagDescs.Add(PlantSpecials.RARE, "This flower is considered special and may be harder to find than most.");

        // BouquetHarmonies
        TagDescs.Add(BouquetHarmony.CONTRASTING, "The accents of the bouquet has a Contrasting colour harmony.");
        TagDescs.Add(BouquetHarmony.ANALOGOUS, "The accents of the bouquet has a Analogous colour harmony.");
        TagDescs.Add(BouquetHarmony.TRIADIC, "The accents of the bouquet has a Triadic colour harmony.");
        TagDescs.Add(BouquetHarmony.SOLID, "The accents of the bouquet has a Solid colour harmony.");
        TagDescs.Add(BouquetHarmony.MULTICOLOURED, "Every flower that accents the bouquet is a different colour.");

        // BouquetCentres
        TagDescs.Add(BouquetCentres.JEWELBED, "The centrepiece of the bouquet shares a Constrating colour harmony with its accents.");
        TagDescs.Add(BouquetCentres.SPECTRUM, "The centrepiece of the bouquet shares an Analogous colour harmony with its accents.");
        TagDescs.Add(BouquetCentres.PARTITION, "The centrepiece of the bouquet shares a Triadic colour harmony with its accents.");
        TagDescs.Add(BouquetCentres.TROVE, "The centrepiece of the bouquet shares a Solid colour harmony with its accents.");

        // BouquetSpecials
        TagDescs.Add(BouquetSpecials.RADIANT, "The centrepiece of the bouquet is a Rare flower.");
        TagDescs.Add(BouquetSpecials.MONOSPECIES, "Every flower in the bouquet are the same type.");
        TagDescs.Add(BouquetSpecials.DELICATE, "4 or more Pale flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.BOLD, "4 or more Bright flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.REFINED, "4 or more Vibrant flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.ELEGANT, "4 or more Dark flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.RED_DOMINANT, "4 or more Red flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.ORANGE_DOMINANT, "4 or more Orange flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.YELLOW_DOMINANT, "4 or more Yellow flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.GREEN_DOMINANT, "4 or more Green flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.BLUE_DOMINANT, "4 or more Blue flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.VIOLET_DOMINANT, "4 or more Violet flowers adorn the accents of the bouquet.");
        TagDescs.Add(BouquetSpecials.PURPLE_DOMINANT, "4 or more Purple flowers adorn the accents of the bouquet.");

        // How to use the hashtable
        //Debug.Log(TagDescs[BouquetSpecials.BLACK_DOMINANT]);
    }

    public string GetRandomQuestTag(int randomStyle = 0)
	{
        // randomStyle index (tailored based on requestDiff):
        // 0 - any one
        // 1 - bouquet harmonies only
        // 2 - bouquet centres only
        // 3 - any special
        // 4 - 4 or more value conditions
        // 5 - 4 or more colour conditions
        // 6 - radiant/monospecies
        List<string> tempTagList = new List<string>();

        // HARMONIES
        if (randomStyle == 0 || randomStyle == 1)
        {
            string[] bHarmonies = System.Enum.GetNames(typeof(BouquetHarmony));
            for (int i = 0; i < bHarmonies.Length; i += 1)
            {
                if (bHarmonies[i] != "NONE")
                    tempTagList.Add(bHarmonies[i]);
            }
        }

        // CENTRES
        if (randomStyle == 0 || randomStyle == 2)
        {
            string[] bCentres = System.Enum.GetNames(typeof(BouquetCentres));
            for (int i = 1; i < bCentres.Length; i += 1)
            { 
                tempTagList.Add(bCentres[i]);
            }
        }

        // EVERY SPECIAL
        if (randomStyle == 0 || randomStyle == 3)
		{
            string[] bSpecials = System.Enum.GetNames(typeof(BouquetSpecials));
            for (int i = 1; i < bSpecials.Length; i += 1)
            {
                tempTagList.Add(bSpecials[i]);
            }
        }

        // VALUE CONDITIONS
        if (randomStyle == 4)
        {
            string[] bSpecials = System.Enum.GetNames(typeof(BouquetSpecials));
            for (int i = 3; i <= 6; i += 1)
            {
                tempTagList.Add(bSpecials[i]);
            }
        }

        // COLOUR CONDITIONS
        if (randomStyle == 5)
        {
            string[] bSpecials = System.Enum.GetNames(typeof(BouquetSpecials));
            for (int i = 7; i <= 13; i += 1)
            {
                tempTagList.Add(bSpecials[i]);
            }
        }

        // LAST TWO
        if (randomStyle == 6)
        {
            string[] bSpecials = System.Enum.GetNames(typeof(BouquetSpecials));
            for (int i = 1; i <= 2; i += 1)
            {
                tempTagList.Add(bSpecials[i]);
            }
        }

        // Shuffles the list, shoutouts to the Fisher-Yates Shuffle Algorithm
        // https://www.wayline.io/blog/how-to-shuffle-a-list-in-unity
        for (int i = tempTagList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = tempTagList[i];
            tempTagList[i] = tempTagList[j];
            tempTagList[j] = temp;
        }

        return tempTagList[Random.Range(0, tempTagList.Count)];
    }

    public static void DrawCircle(Vector3 position, Quaternion rotation, float radius)
    {
        if (radius <= 0.0f || 8 <= 0)
        {
            return;
        }

        float angleStep = (360.0f / 16);

        angleStep *= Mathf.Deg2Rad;

        Vector3 lineStart = Vector3.zero;
        Vector3 lineEnd = Vector3.zero;

        for (int i = 0; i < 16; i++)
        {
            lineStart.x = Mathf.Cos(angleStep * i);
            lineStart.y = Mathf.Sin(angleStep * i);
            lineStart.z = 0.0f;

            lineEnd.x = Mathf.Cos(angleStep * (i + 1));
            lineEnd.y = Mathf.Sin(angleStep * (i + 1));
            lineEnd.z = 0.0f;

            lineStart *= radius;
            lineEnd *= radius;

            lineStart = rotation * lineStart;
            lineEnd = rotation * lineEnd;

            lineStart += position;
            lineEnd += position;

            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }

    public static void DrawCylinder(Vector3 position, Quaternion orientation, float height, float radius)
    {
        Vector3 topPosition = position;
        Vector3 basePosition = new Vector3(position.x, position.y - height, position.z);

        Vector3 pointA = topPosition + Vector3.right * radius;
        Vector3 pointB = topPosition + Vector3.forward * radius;
        Vector3 pointC = topPosition - Vector3.right * radius;
        Vector3 pointD = topPosition - Vector3.forward * radius;

        Gizmos.DrawLine(pointA, pointA + Vector3.down * height);
        Gizmos.DrawLine(pointB, pointB + Vector3.down * height);
        Gizmos.DrawLine(pointC, pointC + Vector3.down * height);
        Gizmos.DrawLine(pointD, pointD + Vector3.down * height);

        Gizmos.DrawLine(pointA, pointC);
        Gizmos.DrawLine(pointB, pointD);

        DrawCircle(basePosition, orientation, radius);
        DrawCircle(topPosition, orientation, radius);
    }
}
