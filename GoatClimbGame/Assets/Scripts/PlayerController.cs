using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Component Inputs")]
    private GoatControls goatControls;
    private Transform camPivot, orientation, model;
    private Rigidbody rb;

    [Header("Variables")]
    public int herbCount = 0;
    public Vector3 sensitivity = new Vector3(20f, 15f, 8f); // x is Left/Right, y is Up/Down, z is Scroll wheel THIS DOES NOT CORRELATE TO CAM ROTATION THOUGH
    public Vector2 lookLimit = new Vector2(-40f, 10f); // x = upwards, y = downwards
    public Vector2 zoomLimit = new Vector2(0.12f, 1.5f); // x = lowest, y = highest
    public Vector2 camYPos = new Vector2(0.55f, 4.5f); // x = lowest, y = highest
    private float rotX, rotY, rotLerpX, rotLerpY, zoomVal = 0.5f, zoomLerp = 0.5f;
    public Vector3 moveDir, modelRotLerp;
    private Vector2 kbInputs;
    public float speedDefault = 5f, speedMax = 4f;

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
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        orientation = GameObject.Find("Orientation").transform;
        model = GameObject.Find("PlayerModel").transform;

        camPivot = GameObject.Find("CameraPivot").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse Inputs to code
        float mouseLR = goatControls.Defaults.CaMovement.ReadValue<Vector2>().x * Time.deltaTime * sensitivity.x;
        float mouseUD = goatControls.Defaults.CaMovement.ReadValue<Vector2>().y * Time.deltaTime * sensitivity.y;

        rotY += mouseLR;
        rotX -= mouseUD;
        rotX = Mathf.Clamp(rotX, lookLimit.x, lookLimit.y);

        float mouseW = goatControls.Defaults.CamZoomer.ReadValue<float>();
        zoomVal = Mathf.Clamp(zoomVal + (mouseW * Time.deltaTime * sensitivity.z), zoomLimit.x, zoomLimit.y);

        MoveCamBasedOnInputs();

        // Keyboard Inputs to Code
        kbInputs = goatControls.Defaults.Movement.ReadValue<Vector2>();
    }

	private void FixedUpdate()
    {
        MovePlayerBasedOnInputs();
    }

    void MovePlayerBasedOnInputs()
    {
        // Calculate Movement Direction
        moveDir = (orientation.forward * kbInputs.y) + (orientation.right * kbInputs.x);
        moveDir.y = 0f;
        rb.AddForce(moveDir.normalized * speedDefault * 10f, ForceMode.Force);

        // Rotate model based on movement direction
        modelRotLerp = Vector3.Lerp(modelRotLerp, moveDir.normalized, Time.deltaTime * 2.7f);
        if (moveDir != Vector3.zero)
        {
            model.transform.forward = modelRotLerp;
        }
    }

	void MoveCamBasedOnInputs()
	{
        // Lerps for smoother camera movement
        if (Mathf.Abs(rotLerpX - rotX) <= 0.08f)
            rotLerpX = rotX;
        else
            rotLerpX = Mathf.Lerp(rotLerpX, rotX, Time.deltaTime * 6.9f);

        if (Mathf.Abs(rotLerpY - rotY) <= 0.08f)
            rotLerpY = rotY;
        else
            rotLerpY = Mathf.Lerp(rotLerpY, rotY, Time.deltaTime * 6.9f);

        // The actual camera rotating parts
        camPivot.transform.localRotation = Quaternion.Euler(rotLerpX, rotLerpY, 0f);

        // Set orientation based on camera's current position
        orientation.rotation = Quaternion.Euler(0, camPivot.rotation.eulerAngles.y, 0);

        // Zoom based on value
        if (Mathf.Abs(zoomLerp - zoomVal) < 0.002f)
            zoomLerp = zoomVal;
        else
            zoomLerp = Mathf.Lerp(zoomLerp, zoomVal, Time.deltaTime * 6.9f);

        camPivot.localScale = new Vector3(zoomLerp, zoomLerp, zoomLerp);

        // Camera position based on zoom
        // y = mx + b, x is zoomLimit, y is camYPos
        float m = (camYPos.y - camYPos.x) / (zoomLimit.y - zoomLimit.x); // slope
        float b = camYPos.y - (m * zoomLimit.y);
        camPivot.localPosition = new Vector3(0f, (m * zoomLerp) + b, 0f);
    }
}
