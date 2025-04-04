using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Component Inputs")]
    private GoatControls goatControls;
    public Transform camPivot, orientation, camFarthestPoint;
    public Transform model;
    private Rigidbody rb;
    public Camera camActual;
    public Animator stateAnimator;

    [Header("Variables")]
    public Vector3 sensitivity = new Vector3(20f, 15f, 8f); // x is Left/Right, y is Up/Down, z is Scroll wheel THIS DOES NOT CORRELATE TO CAM ROTATION THOUGH
    public Vector2 lookLimit = new Vector2(-40f, 10f); // x = upwards, y = downwards
    public Vector2 zoomLimit = new Vector2(0.12f, 1.5f); // x = lowest, y = highest
    public Vector2 camYPos = new Vector2(0.55f, 4.5f); // x = lowest, y = highest
    private float rotX, rotY, rotLerpX, rotLerpY, zoomVal = 0.5f, zoomLerp = 0.5f;
    public Vector3 moveDir, modelRotLerp;

    private Vector2 kbInputsMvmnt;
    private InputAction kbInputsEsc;
    private InputAction uiInputsCursor, uiInputsKBNavig, uiInputsPauseTabs;

    public float speed = 4f, speedMax = 1f, groundDrag = 2f, airMult = 0.5f;
    private float speedInit, speedMaxInit;
    public bool controlGiven = false;
    private float lerpFactorCam = 0f, lerpFactorCamFinal = 6.9f, lerpFactorWalkrun;

    [Header("Gravity Response")]
    public LayerMask layersToCheck;
    public float sphereCastRadius = 0.2f, maxCastDist = 0.1f;
    public GameObject currentHitObj;
    public float currentHitDist;
    public bool touchingGrass;

    [Header("Camera Handling")]
    public GameObject objCloseTo;
    public RaycastHit camRC; // WIP: Activates when player physically LOOKS at the plant
    public float camPushRadius = 0.5f;
    public LayerMask camLayersToCheck;
    public Vector3 currentCamCastPoint;

    [Header("Inventory Management")]
    public Dictionary<string, int> collectedInventory = new Dictionary<string, int>();

    [Header("Debug Mode")]
    private InputAction kbInputsDebugToggle, kbInputsDebugH, kbInputsDebugJ, kbInputsDebugU, kbInputsDebugI;
    private bool debugModeOn = false, debugSpeed = false;

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
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (orientation == null) orientation = transform.Find("Orientation").transform;
        //model = GameObject.Find("PlayerModel").transform;

        if (camPivot == null) camPivot = transform.Find("CameraPivot").transform;

        if (stateAnimator == null) stateAnimator = transform.Find("Goat_Rigged").GetComponent<Animator>();

        kbInputsEsc = goatControls.Defaults.Escape;
        uiInputsPauseTabs = goatControls.UI.PauseTabs;
        speedInit = speed;
        speedMaxInit = speedMax;

        camActual = camPivot.GetComponentInChildren<Camera>();
        camFarthestPoint = camPivot.Find("FarthestPoint").transform;

        kbInputsDebugToggle = goatControls.Defaults.DebugToggle;
        kbInputsDebugH = goatControls.Defaults.DebugH;
        kbInputsDebugJ = goatControls.Defaults.DebugJ;
        kbInputsDebugU = goatControls.Defaults.DebugU;
        kbInputsDebugI = goatControls.Defaults.DebugI;
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse Inputs to code
        if (controlGiven)
        {
            float mouseLR = goatControls.Defaults.CaMovement.ReadValue<Vector2>().x * Time.deltaTime * sensitivity.x;
            float mouseUD = goatControls.Defaults.CaMovement.ReadValue<Vector2>().y * Time.deltaTime * sensitivity.y;

            rotY += mouseLR;
            rotX -= mouseUD;
            rotX = Mathf.Clamp(rotX, lookLimit.x, lookLimit.y);

            float mouseWhl = goatControls.Defaults.CamZoomer.ReadValue<float>();
            zoomVal = Mathf.Clamp(zoomVal + (mouseWhl * Time.deltaTime * sensitivity.z), zoomLimit.x, zoomLimit.y);
        }

        MoveCamBasedOnInputs(); CameraPushHandling();

        // Mouse Interactions
        InputAction mouseLMB = goatControls.Defaults.Interaction;
        mouseLMB.performed += InteractTriggerHandling;

        // Keyboard Inputs to Code
        kbInputsMvmnt = goatControls.Defaults.Movement.ReadValue<Vector2>(); // Movement

        // State Animation Crap
        stateAnimator.SetFloat("isMoving", kbInputsMvmnt.normalized.magnitude);

        if (Mathf.Abs(lerpFactorWalkrun - rb.velocity.magnitude) < 0.007f)

            lerpFactorWalkrun = rb.velocity.magnitude;
        else
            lerpFactorWalkrun = Mathf.Lerp(lerpFactorWalkrun, rb.velocity.magnitude, Time.deltaTime * 6.9f);
        stateAnimator.SetFloat("playerSpd", lerpFactorWalkrun);
    }

	private void FixedUpdate()
    {
        // Function that moves player via physics (do not put actual inputs here, only processing)
        MovePlayerBasedOnInputs();
        if (kbInputsEsc != null) kbInputsEsc.performed += TogglePlayerControlKB;
        if (uiInputsPauseTabs != null) uiInputsPauseTabs.performed += PauseTabSwitching;

        if (kbInputsDebugToggle != null) kbInputsDebugToggle.performed += ToggleDebugState;
        if (kbInputsDebugH != null)      kbInputsDebugH.performed += ToggleDebugRunListener;
        if (kbInputsDebugJ != null)      kbInputsDebugJ.performed += ToggleDebugPlantListener;
        if (kbInputsDebugU != null)      kbInputsDebugU.performed += ToggleDebugFastDaytimeListener;
        if (kbInputsDebugI != null)      kbInputsDebugI.performed += ToggleDebugReqGenerationListener;

        // Gravity related crap
        RaycastHit rc;
        if (Physics.SphereCast(model.transform.position, sphereCastRadius, Vector3.down, out rc, currentHitDist, layersToCheck, QueryTriggerInteraction.UseGlobal))
        { // Player is touching grass
            currentHitObj = rc.transform.gameObject;
            currentHitDist = rc.distance;

            rb.drag = groundDrag;
            touchingGrass = true;
        }
        else
        { // Player is not touching grass
            currentHitObj = null;
            currentHitDist = maxCastDist;

            rb.drag = groundDrag * airMult;
            touchingGrass = false;
        }

        // Camera related looking at shit
        //if (Physics.SphereCast(camPivot.transform.position, sphereCastRadius, camPivot.localEulerAngles, out camRC, 100f, LayerMask.GetMask(LayerMask.LayerToName(7)), QueryTriggerInteraction.Collide))
		//{
            
		//}
    }

    void OnDrawGizmosSelected()
    {
        // Displays the spherecast for gravity detection
        Gizmos.color = Color.red;
        Debug.DrawLine(model.transform.position, model.transform.position + Vector3.down * currentHitDist);
        Gizmos.DrawWireSphere(model.transform.position + Vector3.down * currentHitDist, sphereCastRadius);

        // Displays the spherecast for camera pushforward
        Gizmos.color = Color.cyan;
        Debug.DrawLine(camPivot.position, camFarthestPoint.position);
        Gizmos.DrawWireSphere(currentCamCastPoint, camPushRadius);

        //Gizmos.DrawLine(camPivot.transform.position, camPivot.transform.position + (camPivot.transform.forward) * 50f);
        //Gizmos.DrawWireSphere(camPivot.transform.position, sphereCastRadius);
    }

    void MovePlayerBasedOnInputs() // Modifying player movement inputs based on KB inputs
    {
        // If game is paused, skip all these
        if (!GameMainframe.GetInstance().GetGameStartedState() || GameMainframe.GetInstance().GetGameSuspendState())
            return;

        // Calculate Movement Direction
        moveDir = (orientation.forward * kbInputsMvmnt.y) + (orientation.right * kbInputsMvmnt.x);
        moveDir.y = 0f;

        if (touchingGrass)
            rb.AddForce(moveDir.normalized * speed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDir.normalized * speed * 10f * airMult, ForceMode.Force);

        // Ensure player doesn't go too fast
        Vector3 velCurr = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (velCurr.magnitude > speedMax && speedMax > 0f)
		{
            Vector3 curbedVel = velCurr.normalized * speedMax;
            rb.velocity = new Vector3(curbedVel.x, rb.velocity.y, curbedVel.z);
        }

        // Rotate model based on movement direction
        modelRotLerp = Vector3.Lerp(modelRotLerp, moveDir.normalized, Time.deltaTime * 2.7f);
        if (moveDir != Vector3.zero)
        {
            model.transform.forward = modelRotLerp;
        }
    }

    void MoveCamBasedOnInputs() // Modifying player camera values based on mouse inputs
    {
        // If game is paused or player cannot control game, skip all these
        if (!GameMainframe.GetInstance().GetGameStartedState())
            return;

        if (controlGiven)
            if (lerpFactorCam <= lerpFactorCamFinal)
                lerpFactorCam += Time.deltaTime * 2f;

        // Lerps for smoother camera movement
        if (Mathf.Abs(rotLerpX - rotX) <= 0.08f)
            rotLerpX = rotX;
        else
            rotLerpX = Mathf.Lerp(rotLerpX, rotX, Time.deltaTime * lerpFactorCam);

        if (Mathf.Abs(rotLerpY - rotY) <= 0.08f)
            rotLerpY = rotY;
        else
            rotLerpY = Mathf.Lerp(rotLerpY, rotY, Time.deltaTime * lerpFactorCam); 

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

        // Camera pivot position based on zoom
        // y = mx + b, x is zoomLimit, y is camYPos
        float m = (camYPos.y - camYPos.x) / (zoomLimit.y - zoomLimit.x); // slope
        float b = camYPos.y - (m * zoomLimit.y);
        camPivot.localPosition = new Vector3(0f, (m * zoomLerp) + b, 0f); // This is moving the PIVOT up/down based on zoom, it's still mostly centred on the player

        // Actual camera's location
        if (Mathf.Abs((camActual.transform.position - currentCamCastPoint).magnitude) <= 0.008f)
            camActual.transform.position = currentCamCastPoint;
        else
            camActual.transform.position = Vector3.Lerp(camActual.transform.position, currentCamCastPoint, Time.deltaTime * lerpFactorCam);
    }

    void InteractTriggerHandling(InputAction.CallbackContext context) // actions when LMB is clicked
    {
        // For starting the game
        if (!controlGiven && !GameMainframe.GetInstance().GetTitleStartedState())
        {
            StartCoroutine(GameMainframe.GetInstance().ToggleTitleFade());
        }
        // For picking up a flower
        else if (controlGiven && objCloseTo != null)
        {
            // If a flower can be picked up, pick it up
            if (objCloseTo.TryGetComponent(out PlantBhv pbhv))
            {
                if (pbhv.CheckPickable())
				{
                    UpdateInventory(pbhv.name, pbhv.PickMeUp());
                    objCloseTo = null;

                    GameMainframe.GetInstance().GetComponent<PlantSpawning>().IncrementSpawns(pbhv.name, /*pbhv.PickMeUp() * */ -1);
                }
            }

            // If a villager can be interactied with, do that
            else if (objCloseTo.TryGetComponent(out VillagerBhv vbhv))
            {
                vbhv.TalkToMe();
            }
        }
    }

    public void TogglePlayerControlKB(InputAction.CallbackContext context)
    {
        TogglePlayerControl();
    }

    public void ToggleDebugState(InputAction.CallbackContext context)
    {
        if (GameMainframe.GetInstance().GetGameSuspendState())
            return;

        debugModeOn = !debugModeOn;

        // Turn all active debugs off
        if (!debugModeOn)
		{
            if (debugSpeed) ToggleDebugRun();
            if (GameMainframe.daytimeSpeed != GameMainframe.daytimeSpeedInit) ToggleDebugFastDaytime();
		}

        GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
        {
            PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
            if (debugModeOn) hppPPB.SetupDisplay("Debug Buttons ENABLED", null);
            else hppPPB.SetupDisplay("Debug Buttons DISABLED", null);
            hpp.name = "HUDPopup";

            hpp.transform.SetParent(null);
            hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
            hpp.SetActive(true);
        }, GameMainframe.GetInstance().hudPopupPrefab);
    }

    public void ToggleDebugRunListener(InputAction.CallbackContext context)
    {
        if (GameMainframe.GetInstance().GetGameSuspendState() || !debugModeOn)
            return;

        ToggleDebugRun();
    }

    public void ToggleDebugRun()
    {
        debugSpeed = !debugSpeed;

        if (debugSpeed)
		{
            speed *= 2;
            speedMax *= 2;
        }
        else
		{
            speed = speedInit;
            speedMax = speedMaxInit;
        }

        GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
        {
            PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
            if (speed != speedInit) hppPPB.SetupDisplay("DEBUG: Nyoom ON", null);
            else hppPPB.SetupDisplay("DEBUG: Nyoom OFF", null);
            hpp.name = "HUDPopup";

            hpp.transform.SetParent(null);
            hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
            hpp.SetActive(true);
        }, GameMainframe.GetInstance().hudPopupPrefab);
    }

    public void ToggleDebugPlantListener(InputAction.CallbackContext context)
    {
        if (GameMainframe.GetInstance().GetGameSuspendState() || !debugModeOn)
            return;

        ToggleDebugPlant();
    }


    public void ToggleDebugPlant()
    {
        GameMainframe.GetInstance().audioMngr.PlaySFX("pickup" + Random.Range(1, 3), transform.position);
        
        foreach (PlantSpawning.OnePlantInfo pi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist)
        {
            UpdateInventory(pi.plantName, 5);
        }

        GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
        {
            PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
            hppPPB.SetupDisplay("DEBUG: All Plants", null, 5);
            hpp.name = "HUDPopup";

            hpp.transform.SetParent(null);
            hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
            hpp.SetActive(true);
        }, GameMainframe.GetInstance().hudPopupPrefab);

        GameMainframe.GetInstance().UpdateInventoryQuantities();
    }

    public void ToggleDebugFastDaytimeListener(InputAction.CallbackContext context)
    {
        if (GameMainframe.GetInstance().GetGameSuspendState() || !debugModeOn)
            return;

        ToggleDebugFastDaytime();
    }

    public void ToggleDebugFastDaytime()
    {
        if (GameMainframe.daytimeSpeed == GameMainframe.daytimeSpeedInit)
            GameMainframe.daytimeSpeed *= 70f;
        else
            GameMainframe.daytimeSpeed = GameMainframe.daytimeSpeedInit;
        
        GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
        {
            PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
            if (GameMainframe.daytimeSpeed != GameMainframe.daytimeSpeedInit) hppPPB.SetupDisplay("DEBUG: Fast Time ON", null);
            else hppPPB.SetupDisplay("DEBUG: Fast Time OFF", null);
            hpp.name = "HUDPopup";

            hpp.transform.SetParent(null);
            hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
            hpp.SetActive(true);
        }, GameMainframe.GetInstance().hudPopupPrefab);
    }

    public void ToggleDebugReqGenerationListener(InputAction.CallbackContext context)
    {
        if (GameMainframe.GetInstance().GetGameSuspendState() || !debugModeOn)
            return;

        ToggleDebugReqGeneration();
    }

    public void ToggleDebugReqGeneration()
    {
        if (GameMainframe.GetInstance().requestList.Count <= 0)
        {
            //GameMainframe.GetInstance().GenerateRequest();
            GameMainframe.GetInstance().mailbox.timeToNextReq = 0f;

            GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
            {
                PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
                hppPPB.SetupDisplay("DEBUG: Request Generated!", null);
                hpp.name = "HUDPopup";

                hpp.transform.SetParent(null);
                hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
                hpp.SetActive(true);
            }, GameMainframe.GetInstance().hudPopupPrefab);
        }
        else
        {
            GameMainframe.GetInstance().FinishRequest(0, true);

            GameMainframe.GetInstance().ObjectUse("HUDPopup", (hpp) =>
            {
                PickupPopupBhv hppPPB = hpp.GetComponent<PickupPopupBhv>();
                hppPPB.SetupDisplay("DEBUG: Request Cleared!", null);
                hpp.name = "HUDPopup";

                hpp.transform.SetParent(null);
                hpp.transform.SetParent(GameMainframe.GetInstance().uiGroupHUD.transform);
                hpp.SetActive(true);
            }, GameMainframe.GetInstance().hudPopupPrefab);
        }
    }

    void PauseTabSwitching(InputAction.CallbackContext context) // Selection of pause tabs values based on keyboard inputs ONLY when game is paused
    {
        // If game is NOT paused or player cannot control game, skip all these
        if (!GameMainframe.GetInstance().GetGameSuspendState())
            return;

        GameMainframe.GetInstance().UpdateInventoryTabSelect(uiInputsPauseTabs.ReadValue<float>());
    }

    public void TogglePlayerControl()
    {
        if (GameMainframe.GetInstance().GetGameStartedState())
        {
            GameMainframe.GetInstance().ToggleGameSuspendState();
            controlGiven = !GameMainframe.GetInstance().GetGameSuspendState();
        }

        GameMainframe.GetInstance().UpdateInventoryQuantities();
        if (controlGiven == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameMainframe.GetInstance().currentTab = GameMainframe.PauseTabs.KNAPSACK;
            GameMainframe.GetInstance().UpdateInventoryTabSelect();
            GameMainframe.GetInstance().GetPauseTabSack().GetComponent<UnityEngine.UI.Toggle>().isOn = true;
            EventSystem.current.SetSelectedGameObject(null);

            GameMainframe.GetInstance().SetGameGivingState(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CameraPushHandling()
    {
        RaycastHit rcCam;
        if (Physics.Linecast(camPivot.position, camFarthestPoint.position, out rcCam, camLayersToCheck, QueryTriggerInteraction.UseGlobal))
            currentCamCastPoint = rcCam.point;
        else
            currentCamCastPoint = camFarthestPoint.position;
    }

    public void UpdateInventory(string itemName, int quantity)
	{
        if (!collectedInventory.ContainsKey(itemName))
        {
            //Debug.Log(plantName + " not in dictionary, created new entry");
            collectedInventory.Add(itemName, 0);
        }

        collectedInventory[itemName] += quantity;
        //Debug.Log(plantName + " - " + collectedInventory[plantName]);

        if (collectedInventory[itemName] <= 0)
		{
            collectedInventory.Remove(itemName);

            // Remove "gaps" in inventory
            Dictionary<string, int> tempInv = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kvp in collectedInventory)
			{
                tempInv.Add(kvp.Key, kvp.Value);
			}
            collectedInventory.Clear();
            collectedInventory = tempInv;
		}
    }

    public int GetInventoryQty(string itemName)
    {
        if (collectedInventory.ContainsKey(itemName))
        {
            return collectedInventory[itemName];
        }
        else
            return 0;
    }

    public GoatControls GetInputSystem() => goatControls;
}
