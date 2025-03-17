using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VillagerBhv : MonoBehaviour
{
    public int currentPOIIndex;
    public NavMeshAgent agent;
    public Animator stateAnimator;
    public float timeIdlingMin = 60f * 2f, timeIdlingMax = 60f * 4f; // this value is in seconds
    public float idlingTimer;

    // Determine several Points of Interest for a villager to "patrol" around
    // They'll move into a random coordinate in the POI every time they would change locations
	#region Points of Interest Logic
	[System.Serializable]
    public class OnePOI
    {
        public Vector3 poiCentre;
        public float poiRadius;

        [Tooltip("This variable doesn't change anything, only for visual aid")]
        public float poiHeight = 2f;
    }
    [SerializeField]
    public OnePOI[] poiDeets;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (stateAnimator == null) stateAnimator = transform.Find("Bird").GetComponent<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (idlingTimer <= 0f)
		{
            GetCoordInNextPOI();
		}
        else
		{
            idlingTimer -= Time.deltaTime;
		}

        if (agent.remainingDistance <= agent.stoppingDistance)
		{
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                stateAnimator.SetBool("isMoving", false);
            }
		}
        else
        {
            stateAnimator.SetBool("isMoving", true);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (poiDeets.Length > 0)
            foreach (OnePOI thisPOI in poiDeets)
            {
                if (thisPOI.poiRadius > 0f)
                    Gizmos.color = new Color(0f, 0.9f, 0.2f, 0.5f);
                else
                    Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.8f);

                // Draw a semitransparent green cylinder at the transforms position
                DrawCylinder(thisPOI.poiCentre, Quaternion.Euler(90f, 0f, 0f), -Mathf.Abs(thisPOI.poiHeight), thisPOI.poiRadius);
            }
    }

    public void GetCoordInNextPOI()
	{
        // Move to the next POI defined - it should always be a new one unless you're exceptionally unlucky/there's not enough POIs to make a good random
        int chosenPOIindex = currentPOIIndex, strikes = 0;
        while (chosenPOIindex == currentPOIIndex && strikes < 4)
		{
            chosenPOIindex = Random.Range(0, poiDeets.Length);
            strikes += 1;
		}
        
        if (chosenPOIindex == currentPOIIndex && strikes >= 4)
            Debug.LogWarning("Couldn't randomize to a new POI, remaining in same POI.");

        float xInPOI = Random.Range(poiDeets[chosenPOIindex].poiCentre.x - poiDeets[chosenPOIindex].poiRadius, poiDeets[chosenPOIindex].poiCentre.x + poiDeets[chosenPOIindex].poiRadius);
        float zInPOI = Random.Range(poiDeets[chosenPOIindex].poiCentre.z - poiDeets[chosenPOIindex].poiRadius, poiDeets[chosenPOIindex].poiCentre.z + poiDeets[chosenPOIindex].poiRadius);
        Vector3 randomCoordInPOI = new Vector3(xInPOI, poiDeets[chosenPOIindex].poiCentre.y, zInPOI);

        currentPOIIndex = chosenPOIindex;
        idlingTimer = Random.Range(timeIdlingMin, timeIdlingMax);
        agent.SetDestination(randomCoordInPOI);
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
