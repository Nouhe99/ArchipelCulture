using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("World Map")]
    [SerializeField] private GameObject worldMap;
    [SerializeField] private Camera worldMapCamera;
    [SerializeField] private SceneController sceneController;
    [SerializeField] private UnityEngine.UI.Button ButtonReturnHome;
    [SerializeField] private UnityEngine.UI.Button ButtonKraken;

    Vector3 boatPosition;
    
    private UIAnimation worldMapAnimator;

    //#if UNITY_ANDROID || UNITY_IOS
    //    private Vector2? initialTouch1 = null;
    //    private Vector2? initialTouch2 = null;
    //    private float initialDistance = 0.0f;
    //#endif

    //[Header("Mini Map")]
    //[SerializeField] private GameObject miniMap;

    public static MapManager Instance;

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
    }

    private void Start()
    {
        worldMapAnimator = worldMap.GetComponent<UIAnimation>();
        worldMap.SetActive(false);
        worldMapCamera.gameObject.SetActive(false);

        ButtonReturnHome.onClick.AddListener(() =>
        {
            BoatController.Instance.transform.position = Vector3.zero;
            LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.HomeScene, "Retour au bercail..."));
        });

        //ButtonKraken.onClick.AddListener(() =>
        //{
        //    BoatController.Instance.transform.position = new Vector3(70,37);

        //});
    }

    /*
    //Zoom in zoom out for open map
    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
       
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                initialTouch1 = touch1.position;
                initialTouch2 = touch2.position;
                initialDistance = Vector2.Distance(initialTouch1.Value, initialTouch2.Value);
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                float distanceDifference = currentDistance - initialDistance;
                if ((distanceDifference < 0 && !open) || (distanceDifference > 0 && open))
                {
                    ToggleWorldMap();
                }
            }
        }
        else
        {
            initialTouch1 = null;
            initialTouch2 = null;
            initialDistance = 0.0f;
        }
#else
        if (Input.GetKeyUp(KeyCode.M))
        {
            ToggleWorldMap();
        }
        else if ((Input.mouseScrollDelta.y < 0 && !open) || (Input.mouseScrollDelta.y > 0 && open))
        {
            ToggleWorldMap();
        }
#endif
    }
    */

    public void ToggleWorldMap()
    {
        if (worldMap.activeInHierarchy && worldMapCamera.gameObject.activeInHierarchy)
        {
            CloseWorldMap();
        }
        else
        {
            OpenWorldMap();
        }
    }

    public async void CloseWorldMap()
    {
        await worldMapAnimator.AnimateFromEndToStartAsync();
        worldMap.SetActive(false);
        worldMapCamera.gameObject.SetActive(false);

        //miniMap.SetActive(true);
    }

    public async void OpenWorldMap()
    {
        
        worldMap.SetActive(true);
        worldMapCamera.gameObject.SetActive(true);
        await worldMapAnimator.AnimateFromStartToEndAsync();
        

        //miniMap.SetActive(false);
    }
}
