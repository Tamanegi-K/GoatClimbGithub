using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    // Start is called before the first frame update
    void Start()
    {
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
            spawnedCount = 0;

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
                            if (thisPlant.useCircleAreaInstead && (new Vector2(x, z) - thisPlant.circlePoint).magnitude <= thisPlant.circleRadius && thisPlant.spawnChancePercent > Random.Range(0f, 100f))
							{
                                // Using object pooling to spawn plants - see GameMainframe for object pooling usage
                                GameMainframe.GetInstance().ObjectUse(thisPlant.plantName, (singlePlant) =>
                                {
                                    // Defining this one spawned object's properties
                                    singlePlant.name = thisPlant.plantName;
                                    singlePlant.transform.position = hit.point + new Vector3 (Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                                    singlePlant.transform.eulerAngles = new Vector3(0f, Random.Range(-65f, 65f), 0f);
                                    singlePlant.transform.parent = this.gameObject.transform;

                                    singlePlant.SetActive(true);
                                }, thisPlant.plantPrefab);

                                spawnedCount += 1;
                                //Debug.LogWarning("spawned, no. " + spawnedCount);
                            }

                            // Spawn frequency of plant based on the percent chance defined
                            else if (!thisPlant.useCircleAreaInstead && thisPlant.spawnChancePercent > Random.Range(0f, 100f))
                            {
                                // Using object pooling to spawn plants - see GameMainframe for object pooling usage
                                GameMainframe.GetInstance().ObjectUse(thisPlant.plantName, (singlePlant) =>
                                {
                                    // Defining this one spawned object's properties
                                    singlePlant.name = thisPlant.plantName;
                                    singlePlant.transform.position = hit.point + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                                    singlePlant.transform.eulerAngles = new Vector3(0f, Random.Range(-65f, 65f), 0f);
                                    singlePlant.transform.parent = this.gameObject.transform;

                                    singlePlant.SetActive(true);
                                }, thisPlant.plantPrefab);

                                spawnedCount += 1;
                                //Debug.LogWarning("spawned, no. " + spawnedCount);
                            }
                        }
                    }
                }

                if (spawnedCount == 0)
                {
                    if (strikes >= 3)
					{
                        Debug.LogError("Loop broken for " + thisPlant.plantName + " spawning - either the spawnChance & minClusterAmt is too small, or the spawn area is unable to find the terrain.");
                        break;
					}
                    else
                        strikes += 1;
                }
            }
        }
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
