using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The boat controller.
/// </summary>

public class BoatController : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rigidbody2d;
    private Collider2D collider2d;
    private Vector2 moveDirection;
    [SerializeField] private MovementJoystick movementJoystick;

    //private NavMeshAgent agent;
    [SerializeField] private Sprite_EightDirection eightDirSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem trail;
    private CustomAiPath customAiPath;
    private AudioSource audioSource;

    public static BoatController Instance;
    [Header("Miscellaneous")]
    [SerializeField] private SceneController sceneController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        rigidbody2d = GetComponent<Rigidbody2D>();
        //collider2d = GetComponent<Collider2D>();
        customAiPath = GetComponent<CustomAiPath>();
#if UNITY_STANDALONE
        movementJoystick.GetComponentInParent<Canvas>().gameObject.SetActive(false);
#endif
    }

    private void OnDisable()
    {
        SaveMyPosition_ScenarioWorld();
    }

    private void Start()
    {
       // audioSource = GetComponent<AudioSource>();
       // audioSource.enabled = PlayAudio.Instance.IsSfxEnabled();

        if (sceneController.ActiveScene == sceneController.MissionScene)
        {
            transform.position = CrossSceneInformation.BoatPosition_ScenarioWorld;
        }
        GiveControlsToPlayer();
    }

    private void Update()
    {
        // Processing Inputs
        ProcessInputs();
    }

    private void FixedUpdate()
    {
        // Physics Calculations
        Move();
    }

    private void ProcessInputs()
    {
#if UNITY_STANDALONE
        /*************** Keyboard input ***************/
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            GoTo(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
#else
        /*************** Touch input ***************/
        moveDirection = movementJoystick.joystickVector;
#endif
        /*************** AI To Player ***************/
        if (moveDirection != Vector2.zero)
        {
            GiveControlsToPlayer();
        }
    }

    private void Move()
    {
        if (customAiPath.enabled == false)
        {
            rigidbody2d.velocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
        }
        var emission = trail.emission;
        if (rigidbody2d.velocity != Vector2.zero && !emission.enabled)
        {
            emission.enabled = true;
            //audioSource.mute = false;
        }
        else if (rigidbody2d.velocity == Vector2.zero && emission.enabled)
        {
            emission.enabled = false;
            //audioSource.mute = true;
        }

        eightDirSprite.SetSpriteForRendererWithDirection(rigidbody2d.velocity, spriteRenderer, false);
    }

    public void GoTo(Vector2 targetPosition)
    {
        GiveControlsToAI();
        customAiPath.SetDestination(targetPosition);
    }

    private void GiveControlsToAI()
    {
        customAiPath.enabled = true;
        // Disable Collider to avoid cases where the AI hit the edge of an object
        //collider2d.enabled = false;
    }

    private void GiveControlsToPlayer()
    {
        //collider2d.enabled = true;
        customAiPath.StopPath();
        // Disable AI Behavior to avoid useless call to Update()
        customAiPath.enabled = false;
    }

    public void SaveMyPosition_ScenarioWorld()
    {
        CrossSceneInformation.BoatPosition_ScenarioWorld = transform.position;
    }

    public void ResetMyPosition_ScenarioWorld()
    {
        CrossSceneInformation.BoatPosition_ScenarioWorld = Vector3.zero;
    }
}
