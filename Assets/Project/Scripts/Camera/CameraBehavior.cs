using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraBehavior : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private LayerMask uiLayer;

    #region Pan & Zoom
    [Header("Pan")]
    [SerializeField] private float panSpeed = 10.0f;
    private Vector3 lastPanPosition;
    private int panFingerId;
    public bool startedPan;
    public bool CanPan { get; set; }
    [HideInInspector] public bool panOnX = true;

    [Range(1, 25)]
    [SerializeField]
    private int distanceToBorderInPercentageBeforeAutoPan = 5;
    [SerializeField] private float borderPanSpeed = 1f;
    public bool CanPanOnBorders { get; set; }

    [Header("Zoom")]
    [SerializeField] private float mouseZoomSpeed = 10.0f;
    [SerializeField] private float touchZoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 1.0f;
    [SerializeField] private float maxZoom = 100.0f;
    private bool wasZoomingLastFrame;
    private Vector2[] lastZoomPositions;
    private Vector3 zoomToPosition;
    public bool CanZoom { get; set; }

    public void SetCameraSize(float value)
    {
        cam.orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
    }
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField] private float secondsToReachTarget = 1.0f;
    #endregion

    #region Bounds
    [Header("Bounds")]
    private float[] BoundsX = new float[] { -10f, 10f };
    private float[] BoundsY = new float[] { -10f, 10f };
    private float[] ZoomBounds = new float[] { 4f, 12f };
    [SerializeField] private GameObject gameContentParent;
    private Bounds contentBounds = new();
    #endregion

    #region Singleton
    public static CameraBehavior Instance = null;

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
        cam = GetComponent<Camera>();
    }

    #endregion


    private void Start()
    {
        CanPan = true;
        CanZoom = true;
        CanPanOnBorders = true;
    }

    private void Update()
    {
#if UNITY_IOS || UNITY_ANDROID
        HandleTouch();
#else
        HandleMouse();
#endif
    }

    public void UpdateBounds()
    {
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        Renderer[] renderers = gameContentParent.GetComponentsInChildren<Renderer>(false);
        if (renderers.Length <= 0)
        {
            contentBounds.SetMinMax(Vector3.zero, Vector3.zero);
            return;
        }
        foreach (var obj in renderers)
        {
            // Skip UI Renderers
            if ((uiLayer & 1 << obj.gameObject.layer) == 1 << obj.gameObject.layer)
            {
                continue;
            }
            // Skip tilemaps
            if (obj is TilemapRenderer)
            {
                continue;
            }
            // Skip itself
            if (obj.gameObject == gameContentParent)
            {
                continue;
            }

            min = Vector3.Min(obj.bounds.min, min);
            max = Vector3.Max(obj.bounds.max, max);
        }

        contentBounds.SetMinMax(min, max);

        // Min and Max Zoom of the camera
        // Uncomment to get a min zoom bound depending on the content 
        //ZoomBounds[0] = (contentBounds.size.y / 4f)<=minZoom?minZoom: (contentBounds.size.y / 4f);
        //float maxBound = (contentBounds.size.x + contentBounds.size.y) / 2;
        //ZoomBounds[1] = maxBound >= maxZoom?maxZoom: maxBound;
        ZoomBounds[0] = minZoom;
        ZoomBounds[1] = maxZoom;

        // Left and Right Space in which we can move the camera
        BoundsX[0] = contentBounds.center.x - contentBounds.extents.x - (cam.orthographicSize * cam.aspect / 2f);
        BoundsX[1] = contentBounds.center.x + contentBounds.extents.x + (cam.orthographicSize * cam.aspect / 2f);

        // Top and Bottom Space in which we can move the camera
        BoundsY[0] = contentBounds.center.y - contentBounds.extents.y - (cam.orthographicSize / 2f);
        BoundsY[1] = contentBounds.center.y + contentBounds.extents.y + (cam.orthographicSize / 2f);
    }

    private Bounds GetGameObjectBounds(GameObject targetGO)
    {
        Bounds targetBounds = new();
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        Renderer[] renderers = targetGO.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length <= 0)
        {
            targetBounds.SetMinMax(Vector3.zero, Vector3.zero);
            return targetBounds;
        }
        foreach (var obj in renderers)
        {
            min = Vector3.Min(obj.bounds.min, min);
            max = Vector3.Max(obj.bounds.max, max);
        }

        targetBounds.SetMinMax(min, max);
        return targetBounds;
    }

    public void CenterCameraOnContent()
    {
        //StopAllCoroutines();
        StopCoroutine(moveTo);
        moveTo = StartCoroutine(MoveToInSeconds(contentBounds.center, secondsToReachTarget));
    }

    public Vector3 GetContentCenter()
    {
        Vector3 sumVector = new Vector3(0f, 0f, 0f);

        foreach (Transform child in gameContentParent.transform)
        {
            sumVector += child.position;
        }
        Vector3 groupCenter = sumVector / gameContentParent.transform.childCount;
        groupCenter.z = transform.position.z;
        return groupCenter;
    }

    private Coroutine moveTo;
    private Coroutine zoomTo;
    /// <param name="targetGameObject">Target object to center.</param>
    /// <param name="zoom">Will it zoom to target ?</param>
    /// <param name="time">Time that it should take to move and zoom.</param>
    public void CenterCameraOnGameObject(GameObject targetGameObject, bool zoom = false, float time = 1)
    {
        //StopAllCoroutines();
        if (moveTo != null) StopCoroutine(moveTo);
        if (zoomTo != null) StopCoroutine(zoomTo);
        moveTo = StartCoroutine(MoveToInSeconds(GetGameObjectBounds(targetGameObject).center, time));
        if (zoom)
        {
            zoomTo = StartCoroutine(ZoomToInSeconds(targetGameObject, time));
        }
    }

    /// <summary>
    /// Get Bounds based on current content and place the camera at the center
    /// </summary>
    public IEnumerator ResetCameraAtStartCoroutine()
    {
        UpdateBounds();
        yield return null;
        ResetCameraPosition();
    }

    private void ResetCameraPosition()
    {
        if (GridBuildingSystem.current?.mainTilemap != null)
        {
            transform.position = new Vector3(transform.position.x, GridBuildingSystem.current.mainTilemap.localBounds.center.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
        SetCameraSize((maxZoom + minZoom) / 2f);
    }

    public IEnumerator MoveToInSeconds(Vector3 target, float seconds)
    {
        target.z = transform.position.z;
        if (seconds == 0f)
        {
            transform.position = target;
            yield break;
        }
        Vector3 startPosition = transform.position;
        float timeElapsed = 0.0f;
        while (timeElapsed < seconds)
        {
            timeElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, target, timeElapsed / seconds);
            yield return null;
        }
    }

    private IEnumerator ZoomToInSeconds(GameObject targetGO, float seconds)
    {
        float startSize = cam.orthographicSize;
        float timeElapsed = 0.0f;
        float targetSize = GetGameObjectBounds(targetGO).size.y / 2.0f;
        while (timeElapsed < seconds)
        {
            timeElapsed += Time.deltaTime;
            SetCameraSize(Mathf.Lerp(startSize, targetSize, timeElapsed / seconds)); // Change ZoomBounds[0] by targetContent.size ?

            yield return null;
        }
        SetCameraSize(targetSize);
    }
    private IEnumerator ZoomToInSeconds(float[] targetBounds, float seconds)
    {
        float startSize = cam.orthographicSize;
        float timeElapsed = 0.0f;
        float targetSize = Mathf.Abs(targetBounds[0] - targetBounds[1]);
        while (timeElapsed < seconds)
        {
            timeElapsed += Time.deltaTime;
            SetCameraSize(Mathf.Lerp(startSize, targetSize, timeElapsed / seconds)); // Change ZoomBounds[0] by targetContent.size ?

            yield return null;
        }
        SetCameraSize(targetSize);
    }

    private void HandleTouch()
    {
        switch (Input.touchCount)
        {

            case 1: // Panning
                wasZoomingLastFrame = false;

                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                }
                HandleScreenCorners(touch.position);
                break;

            case 2: // Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };

                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    zoomToPosition = cam.ScreenToWorldPoint((newPositions[0] + newPositions[1]) / 2);

                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    Zoom(offset, touchZoomSpeed);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }
    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            panSpeed = cam.orthographicSize * 2f;
            lastPanPosition = Input.mousePosition;
            if (CanPan)
            {
                startedPan = true;
            }
            else
            {
                startedPan = false;
            }
        }
        else if (Input.GetMouseButton(0) && startedPan)
        {
            PanCamera(Input.mousePosition);
        }
        HandleScreenCorners(Input.mousePosition);

        // Check for scrolling to zoom the camera
        zoomToPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        Zoom(scrollWheelInput, mouseZoomSpeed);
    }

    private void PanCamera(Vector3 cursorPosition)
    {
        if (!CanPan) return;

        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - cursorPosition);
        Vector3 move;
        if (panOnX) move = new(offset.x * panSpeed * cam.aspect, offset.y * panSpeed, 0);
        else move = new(0, offset.y * panSpeed, 0);

        // Perform the movement
        transform.Translate(move, Space.World);

        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        if (panOnX) pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.y = Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]);
        //pos.z = transform.position.z;
        transform.position = pos;

        // Cache the position
        lastPanPosition = cursorPosition;
    }

    private void Zoom(float offset, float speed)
    {
        if (offset == 0 || CanZoom == false)
        {
            return;
        }

        if (cam.orthographic)
        {
            float newSize = cam.orthographicSize - (offset * speed);
            if (newSize < ZoomBounds[0] || newSize > ZoomBounds[1] || newSize < minZoom || newSize > maxZoom)
            {
                return;
            }
            // Move to Zoom Position -- TODO : The distance thing is just a workaround, Reflect is actually changing the position too often which cause a bad behavior
            if (offset < 0 && Vector3.Distance(cam.transform.position, zoomToPosition) > 1f)
            {
                zoomToPosition = Vector3.Reflect(zoomToPosition, cam.transform.position);
            }
            zoomToPosition.z = transform.position.z;
            zoomToPosition.x = Mathf.Clamp(zoomToPosition.x, BoundsX[0], BoundsX[1]);
            zoomToPosition.y = Mathf.Clamp(zoomToPosition.y, BoundsY[0], BoundsY[1]);
            transform.position = Vector3.MoveTowards(transform.position, zoomToPosition, Mathf.Abs(offset) * speed);

            // Zoom
            SetCameraSize(Mathf.Clamp(newSize, ZoomBounds[0], ZoomBounds[1]));
        }
        if (!panOnX)
        {
            transform.position = Vector3.Scale(transform.position, Vector3.up + Vector3.forward);
        }
    }

    private void HandleScreenCorners(Vector3 cursorPosition)
    {
        if (CanPanOnBorders == false) { return; }

        Vector3 move = Vector3.zero;
        // Mouse position in screen percentage
        Vector2 viewportMousePosition = new(cursorPosition.x / Screen.width, cursorPosition.y / Screen.height);
        // Converting percentage (x%) to float (x.xx)
        float distanceToBorder = distanceToBorderInPercentageBeforeAutoPan / 100f;
        if (viewportMousePosition.x <= distanceToBorder && panOnX)
        {
            move.x -= Time.deltaTime * cam.aspect * borderPanSpeed;
        }
        if (viewportMousePosition.x >= (1 - distanceToBorder) && panOnX)
        {
            move.x += Time.deltaTime * cam.aspect * borderPanSpeed;
        }
        if (viewportMousePosition.y <= distanceToBorder)
        {
            move.y -= Time.deltaTime * borderPanSpeed;
        }
        if (viewportMousePosition.y >= (1 - distanceToBorder))
        {
            move.y += Time.deltaTime * borderPanSpeed;
        }
        transform.Translate(move, Space.World);
        Vector3 clampVector;
        if (panOnX)
        {
            clampVector = new Vector3(Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]),
                                      Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]),
                                      transform.position.z);
        }
        else
        {
            clampVector = new Vector3(transform.position.x,
                                      Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]),
                                      transform.position.z);
        }
        transform.position = clampVector;
    }


    public void ZoomOnHome(bool zoomHome = true)
    {
        panOnX = !zoomHome;
        if (zoomHome)
        {
            CenterCameraOnGameObject(UIManager.current.floorContener.gameObject, true);
            BoundsY = new float[] { 0f, 10f };
        }
        else
        {
            UpdateBounds();
            if (moveTo != null) StopCoroutine(moveTo);
            if (zoomTo != null) StopCoroutine(zoomTo);
        }
    }

    public void ZoomGlobal()
    {
        if (moveTo != null) StopCoroutine(moveTo);
        if (zoomTo != null) StopCoroutine(zoomTo);
        UpdateBounds();
        //ResetCameraPosition();
        //moveTo = StartCoroutine(MoveToInSeconds(contentBounds.center, 1));
        moveTo = StartCoroutine(MoveToInSeconds(new Vector3(0, -2.75f, -10), 1));
        zoomTo = StartCoroutine(ZoomToInSeconds(ZoomBounds, 1));
    }
}
