using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MouseTooltipBhv : MonoBehaviour
{
    [Header("Inputs")]
    public Vector2 offset = new Vector2(24f, -24f);
    public LayerMask applicableMasks;

    [Header("Object Idenfitication")]
    public Camera canvasCam;
    private InputAction uiInputsCursorPoint;
    private RectTransform rTransform;
    private TextMeshProUGUI tooltipTxt;
    private PointerEventData ped;
    private GraphicRaycaster graphicsRaycaster;

    void Start()
	{
        if (canvasCam == null) canvasCam = GameObject.Find("CanvasCam").GetComponent<Camera>();
        if (GameMainframe.GetInstance() != null) uiInputsCursorPoint = GameMainframe.GetInstance().playerContrScr.GetInputSystem().UI.Point;
        if (rTransform == null) rTransform = GetComponent<RectTransform>();
        if (tooltipTxt == null) tooltipTxt = GetComponentInChildren<TextMeshProUGUI>();
        if (graphicsRaycaster == null) graphicsRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
    }

	// Update is called once per frame
	void LateUpdate()
    {
        // If game is not paused, skip the code below
        if (!GameMainframe.GetInstance().GetGameSuspendState())
            return;

        // --- APPEARANCE OF TOOLTIP ---
        Vector3 mousePosConv = uiInputsCursorPoint.ReadValue<Vector2>();
        //mousePosConv.z = canvasCam.transform.position.z;
        //mousePosConv = canvasCam.ScreenToWorldPoint(mousePosConv);

        ped = new PointerEventData(EventSystem.current);
        ped.position = mousePosConv;

        // Count how many things the mouse is hovering over and pick out only highlightable ones
        List<RaycastResult> allRayHits = new List<RaycastResult>();
        graphicsRaycaster.Raycast(ped, allRayHits);
        List<GameObject> highlightableRayHits = new List<GameObject>();
        foreach (RaycastResult result in allRayHits)
        {
            if (result.gameObject.tag == "Highlightable")
                highlightableRayHits.Add(result.gameObject);
            //Debug.Log("Hit " + result.gameObject.name);
        }

        // As long as the mouse is hovering over a highlightable thing, show the tooltip
        if (highlightableRayHits.Count > 0 && highlightableRayHits[0].TryGetComponent(out InvTagInfo iti))
        {
            GetComponent<CanvasGroup>().alpha = Mathf.Lerp(GetComponent<CanvasGroup>().alpha, 1f, Time.deltaTime * 20f);

            // --- POSITION OF TOOLTIP ---

            float mPosX, mPosY;

            // x Position (right of mouse by default, shifts to the left if mouse is too far to the right)
            if (uiInputsCursorPoint.ReadValue<Vector2>().x < Screen.currentResolution.width * 0.65f)
                mPosX = uiInputsCursorPoint.ReadValue<Vector2>().x + offset.x;
            else
                mPosX = uiInputsCursorPoint.ReadValue<Vector2>().x - offset.x - rTransform.rect.width;

            // y Position (below mouse by default, shifts to the top if mouse is too low on the screen)
            if (uiInputsCursorPoint.ReadValue<Vector2>().y > rTransform.rect.height)
                mPosY = uiInputsCursorPoint.ReadValue<Vector2>().y - Screen.currentResolution.height + offset.y;
            else
                mPosY = uiInputsCursorPoint.ReadValue<Vector2>().y - Screen.currentResolution.height - offset.y + rTransform.rect.height;

            // All together now
            rTransform.anchoredPosition = Vector3.Lerp(rTransform.anchoredPosition, new Vector3(mPosX, mPosY, -800f), Time.deltaTime * 20f);

            // --- TEXT FOR TOOLTIP ---
            if (GameMainframe.GetInstance().plantSpawningScr.TagDescs.ContainsKey(iti.GetAssignedTag()))
                tooltipTxt.text = GameMainframe.GetInstance().plantSpawningScr.TagDescs[iti.GetAssignedTag()].ToString();
            else
                tooltipTxt.text = highlightableRayHits[0].GetComponentInChildren<TextMeshProUGUI>().text;
        }
        else
            GetComponent<CanvasGroup>().alpha = Mathf.Lerp(GetComponent<CanvasGroup>().alpha, 0f, Time.deltaTime * 15f);

        Debug.DrawRay(mousePosConv, Vector3.forward * canvasCam.transform.position.z * -1.05f, Color.yellow);
    }
}
