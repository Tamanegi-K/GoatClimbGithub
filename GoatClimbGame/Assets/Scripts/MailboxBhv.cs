using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MailboxBhv : MonoBehaviour
{
    [Header("Component Inputs")]
    public GameObject billboardUI;
    public List<GameObject> questScrollObjs;
    public bool requestAvailable = false, scrollsActive = false;
    public float timeToNextReqMin = 60f, timeToNextReqMax = 360f; // this is in seconds
    public float timeToNextReq;

    void Awake()
    {
        if (billboardUI == null) billboardUI = transform.Find("BBUI").gameObject;

        billboardUI.SetActive(false);
        billboardUI.GetComponent<TextMeshPro>().text = "No requests available." + "\n" + "Check back later!";

        timeToNextReq = 10f;
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);

        if (other.gameObject.tag == "Player" && GameMainframe.GetInstance().playerContrScr.objCloseTo == null)
        {
            billboardUI.SetActive(true);
            GameMainframe.GetInstance().playerContrScr.objCloseTo = this.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            billboardUI.SetActive(false);
            GameMainframe.GetInstance().playerContrScr.objCloseTo = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (requestAvailable)
            return;

        if (timeToNextReq <= 0f)
        {
            GameMainframe.GetInstance().GenerateRequest();
            timeToNextReq = Random.Range(timeToNextReqMin, timeToNextReqMax);
        }
        else
        {
            timeToNextReq -= Time.deltaTime;
        }
    }

	void LateUpdate()
	{
        if (GameMainframe.GetInstance().requestList.Count > 0)
        {
            requestAvailable = true;
            billboardUI.GetComponent<TextMeshPro>().text = "You have a request!" + "\n" + GameMainframe.GetInstance().requestList[0].requesteeName + " wants a bouquet!";
        }
        else
        {
            requestAvailable = false;
            billboardUI.GetComponent<TextMeshPro>().text = "No requests available." + "\n" + "Check back later!";
        }
        ToggleScrolls();
    }

    void ToggleScrolls()
	{
        if (requestAvailable == scrollsActive)
            return;

        foreach (GameObject oneScroll in questScrollObjs)
        {
            oneScroll.SetActive(requestAvailable);
        }
        scrollsActive = requestAvailable;
    }
}
