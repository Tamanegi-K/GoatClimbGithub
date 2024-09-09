using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Component Inputs")]
    public GoatControls goatControls;
    public Transform camPivot;

    [Header("Variables")]
    public int herbCount = 0;
    public Vector2 sensitivity = new Vector2(1f, 1f); // x is Left/Right, y is Up/Down. THIS DOES NOT CORRELATE TO CAM ROTATION THOUGH
    public float lookLimitUp = -40f, lookLimitDown = 10f;
    public float rotX, rotY, rotLerpX, rotLerpY;
    private Vector3 moveDir;

    void OnEnable()
    {
        if (goatControls == null) goatControls = new GoatControls();
        goatControls.Enable();
    }
    void OnDisable()
    {
        goatControls.Disable();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse Inputs to code
        float mouseLR = goatControls.Defaults.CaMovement.ReadValue<Vector2>().x * Time.deltaTime * sensitivity.x;
        float mouseUD = goatControls.Defaults.CaMovement.ReadValue<Vector2>().y * Time.deltaTime * sensitivity.y;

        rotY += mouseLR;
        rotX -= mouseUD;
        rotX = Mathf.Clamp(rotX, lookLimitUp, lookLimitDown);

        // - - - - - Camera Movement - - - - -

        // Lerps for smoother camera movement
        if (Mathf.Abs(rotLerpX - rotX) <= 0.08f)
            rotLerpX = rotX;
        else
            rotLerpX = Mathf.Lerp(rotLerpX, rotX, Time.deltaTime * 5f);

        if (Mathf.Abs(rotLerpY - rotY) <= 0.08f)
            rotLerpY = rotY;
        else
            rotLerpY = Mathf.Lerp(rotLerpY, rotY, Time.deltaTime * 5f); 

        // The actual camera rotating parts
        camPivot.transform.localRotation = Quaternion.Euler(rotLerpX, rotLerpY, 0f);
    }
}
