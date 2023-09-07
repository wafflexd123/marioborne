using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Humanoid
{
    //Inspector
    public static Player singlePlayer;
    public bool invincibility;
    public float maxInteractDistance;
    [SerializeField] LayerMask raycastIgnore;

    //Public
    public RaycastHit raycast;
    [HideInInspector] public bool hasDied;
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public PlayerCamera cameraController;
    [HideInInspector] public Fists fists;
    [HideInInspector] public new Camera camera;

    //Script
    Console.Line cnsRaycast;
    WickUI wickUI;
    GameObject escMenu;
    bool enableInput = true;
    Coroutine crtMoveToEnemy;
    Transform leftHand;

    public override Vector3 LookDirection => camera.transform.forward;
    public override Vector3 LookingAt => raycast.point;

    protected override void Awake()
    {
        base.Awake();
        leftHand = transform.Find("Left Hand");
        cameraController = transform.Find("Head").GetComponent<PlayerCamera>();
        camera = cameraController.transform.Find("Eyes").Find("Camera").GetComponent<Camera>();
        fists = transform.Find("Body").Find("Hand").GetComponent<Fists>();
        singlePlayer = this;
        cnsRaycast = Console.AddLine();
        movement = GetComponent<PlayerMovement>();
        Transform ui = transform.Find("UI");
        escMenu = ui.Find("Escape Menu").gameObject;
        wickUI = ui.Find("Wick Text").GetComponent<WickUI>();
        raycastIgnore = ~raycastIgnore;//invert layermask
    }

    //private void Start()
    //{
    //	input.AddListener("Mouse", InputType.OnPress, (float direction) => PickupObject(direction));
    //}

    void Update()
    {
        if (enableInput)
        {
            HandleInput();
            Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycast, Mathf.Infinity, raycastIgnore, QueryTriggerInteraction.Ignore);
            if (FindComponent(raycast.transform, out Raycastable hit)) hit.OnRaycast(this);
            if (Console.Enabled) cnsRaycast.text = $"Looking at: {(raycast.transform != null ? raycast.transform.name : null)}";
        }

        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (escMenu.activeSelf)
            {
                escMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                escMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    /// <summary>
    /// This script must have a negative value in the script execution order list for this to work reliably
    /// </summary>
    void HandleInput()
    {
        foreach (string name in Enum.GetNames(typeof(InputAxes)))//Loop through every input axis to see if it was pressed. This can be event driven (more efficient) if we use the newer Unity Input System, but it's fine for now.
        {
            if (Input.GetButtonDown(name)) input.Press(name, () => Input.GetAxis(name), () => Input.GetAxis(name) != 0);
        }
    }

    /// <summary>Method called by a weapon when it detects the player walking over it (do not call otherwise)</summary>
    /// <returns>True if object is picked up, sets parent of weapon</returns>
    public override bool PickupObject(WeaponBase weapon, out Action onDrop)
    {
        if (!this.weapon)//if nothing in hand
        {
            this.weapon = weapon;
            weapon.transform.SetParent(leftHand);
            onDrop = () => this.weapon = null;
            return true;
        }
        onDrop = null;
        return false;
    }

    public override void Kill(DeathType deathType = DeathType.General)
    {
        if (invincibility) Debug.Log("You would have died, but no one can kill John Matrix.");
        else if (!hasDied)
        {
            hasDied = true;
            wickUI.DisplayRandom(deathType);
            model.dying = true;
            Destroy(GetComponent<PlayerMovement>());
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.useGravity = true;
            Destroy(transform.Find("Head").GetComponent<PlayerCamera>());
            enableInput = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void TeleportToEnemy(Humanoid enemy, float teleportSpeed)
    {
        if (enemy.enabled && crtMoveToEnemy == null)//dont teleport to dead/disabled enemies; will cause issues otherwise
        {
            Instantiate(model.deathPosePrefab, transform.position, transform.rotation);
            cameraController.enabled = false;
            movement.enabled = false;
            movement.ResetVelocity();
            movement.EnableCollider(false);
            enemy.enabled = false;
            if (weapon) weapon.Drop(0);
            crtMoveToEnemy = StartCoroutine(LerpToPos(new Position(enemy.transform), Vector3.Distance(enemy.transform.position, transform.position) / teleportSpeed, transform, () =>
            {
                if (enemy.weapon) enemy.weapon.Pickup(this, true);
                cameraController.enabled = true;
                movement.EnableCollider(true);
                movement.enabled = true;
                crtMoveToEnemy = null;
                Destroy(enemy.gameObject);
            }, EasingFunction.EaseInSine));
        }
    }
}
