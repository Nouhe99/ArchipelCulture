using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ReplaceableOnMap : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    //variables
    public ItemType type;
    [HideInInspector] public bool placed = false;
    public SpriteRenderer spriteRenderer;

    //abstract methods
    protected abstract void SpecificSelect();
    protected abstract void SpecificClick();
    public abstract void Follow();
    protected abstract void Place();
    protected abstract bool CanBePlaced();
    public abstract void ReplaceInInventory();
    public abstract void RemoveFromTilemap();

    #region Drag & Drop

    public void OnPointerDown(PointerEventData eventData)
    {
        if (UIManager.current.inventoryUI == null) return; //no inventory in current scene

        if (UIManager.current.inventoryUI.sideBarOpen
#if UNITY_IOS || UNITY_ANDROID
            && Input.touchCount == 1
#endif
        )
        {
            CameraBehavior.Instance.CanPan = false;
            StartCoroutine(WaitForMove());
        }
    }

    private IEnumerator WaitForMove()
    {
        Vector3 initialpos = UIManager.GetMousePosOnScreen();
        if (initialpos.x < 0) yield break;
        yield return new WaitUntil(() => UIManager.GetMousePosOnScreen() != initialpos ||
#if UNITY_IOS || UNITY_ANDROID
        UIManager.GetMousePosOnScreen().x < 0
#else
        !Input.GetMouseButton(0)
#endif
        );
        if (UIManager.GetMousePosOnScreen().x < 0) yield break;
        if (UIManager.GetMousePosOnScreen() != initialpos)
        {
            //select item only if building menu is open, it's not from the inventory, it is placed in the scene, and mouse has been moved.
            if (placed && IslandBuilder.current.StateNumber == 0 && !MouseOverMenu(UIManager.current.inventoryUI.gameObject))
            {
                SpecificSelect();
            }
        }
    }

    public IEnumerator Following()
    {
        // -- DRAG --

        //cannot scroll during follow
        if (type == ItemType.Decoration)
            UIManager.current.inventoryUI.contentCustom.GetComponentInParent<ScrollRect>().enabled = false;
        else if (type == ItemType.Building)
            UIManager.current.inventoryUI.contentHome.GetComponentInParent<ScrollRect>().enabled = false;

#if UNITY_IOS || UNITY_ANDROID
        while (Input.touchCount >= 1 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary))
#else
        while (!Input.GetMouseButtonUp(0))
#endif
        {
            Follow();
            yield return null;
        }

        // -- END DRAG --

        CameraBehavior.Instance.CanPan = true;
        GridBuildingSystem.current.DeselectObj();

        //can scroll again
        if (type == ItemType.Decoration)
            UIManager.current.inventoryUI.contentCustom.GetComponentInParent<ScrollRect>().enabled = true;
        else if (type == ItemType.Building)
            UIManager.current.inventoryUI.contentHome.GetComponentInParent<ScrollRect>().enabled = true;

    }
    /// <summary>
    /// If item can be place, place it, otherwise remove it.
    /// </summary>
    public void PlaceIt()
    {
        if (!CanBePlaced())
        {
            //can't be placed
            ReplaceInInventory();
        }
        else
        {
            //place it !
            Place();
        }
        GridBuildingSystem.current.HighlightValidPositions();
    }

    #endregion

    #region Click on It
    public void OnPointerClick(PointerEventData eventData)
    {
       // if (UIManager.current.inventoryUI == null) return; //in friend island !
        if (!MouseOverMenu(UIManager.current.inventoryUI.gameObject))
            SpecificClick();
    }

    public void ClickOnIt()
    {
        SpecificClick();
    }

    #endregion

    #region Mouse Over Things

    public static bool MouseOverMenu(GameObject menu)
    {
        PointerEventData eventDataCurrentPosition = new(EventSystem.current);
        eventDataCurrentPosition.position = UIManager.GetMousePosOnScreen();
#if UNITY_IOS || UNITY_ANDROID
        if (eventDataCurrentPosition.position.x < 0) return false;
#else
        if (!Input.GetMouseButton(0)) return false;
#endif


        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == menu)
            {
                return true;
            }
        }
        return false;
    }

    public bool ClickOverHimself()
    {
        Vector2 clickpos = UIManager.GetMousePos();
        Collider2D[] hits = Physics2D.OverlapPointAll(clickpos);
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject || h.gameObject.transform.IsChildOf(gameObject.transform))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Animations

    public IEnumerator RemoveAnimation()
    {
        if (gameObject.TryGetComponent(out Collider2D coll)) coll.enabled = false; //cannot be selected during disapear animation
        yield return StartCoroutine(AnimationsScript.Disapear(gameObject.transform));
        Destroy(gameObject);
    }
    #endregion

}
