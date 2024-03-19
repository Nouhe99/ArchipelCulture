using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class StopCameraMovement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool StopPan, StopPanOnBorders, StopZoom;
    private bool expandToChildren = true;

    private void Start()
    {
        if (expandToChildren)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform)
                {
                    continue;
                }
                if (child.GetComponent<IEventSystemHandler>() != null)
                {
                    StopCameraMovement scm = child.gameObject.AddComponent<StopCameraMovement>();
                    if (scm != null)
                    {
                        scm.expandToChildren = false;
                        scm.StopPan = StopPan;
                        scm.StopPanOnBorders = StopPanOnBorders;
                        scm.StopZoom = StopZoom;
                    }
                }
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (CameraBehavior.Instance != null)
        {
            if (StopZoom)
            {
                CameraBehavior.Instance.CanZoom = false;
            }
            if (StopPan)
            {
                CameraBehavior.Instance.CanPan = false; // TODO : OnPointerEnter called after the start of CameraBehavior and before the object itself is disabled
            }
            if (StopPanOnBorders)
            {
                CameraBehavior.Instance.CanPanOnBorders = false;
            }
        }
    }

#if UNITY_IOS || UNITY_ANDROID
    private void Update()
    {
        if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
        {
            if (CameraBehavior.Instance != null)
            {
                if (StopZoom)
                {
                    CameraBehavior.Instance.CanZoom = true;
                }
                if (StopPan)
                {
                    CameraBehavior.Instance.CanPan = true;
                }
                if (StopPanOnBorders)
                {
                    CameraBehavior.Instance.CanPanOnBorders = true;
                }
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //nothing : for pc version only
    }
#else
    public void OnPointerUp(PointerEventData eventData)
    {
        if (CameraBehavior.Instance != null)
        {
            if (StopZoom)
            {
                CameraBehavior.Instance.CanZoom = true;
            }
            if (StopPan)
            {
                CameraBehavior.Instance.CanPan = true;
            }
            if (StopPanOnBorders)
            {
                CameraBehavior.Instance.CanPanOnBorders = true;
            }
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (CameraBehavior.Instance != null)
            {
                if (StopZoom)
                {
                    CameraBehavior.Instance.CanZoom = true;
                }
                if (StopPan)
                {
                    CameraBehavior.Instance.CanPan = true;
                }
                if (StopPanOnBorders)
                {
                    CameraBehavior.Instance.CanPanOnBorders = true;
                }
            }
        }
    }
#endif

}
