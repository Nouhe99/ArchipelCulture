using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, /*IDropHandler,*/ IPointerDownHandler
{
    [SerializeField] private Image picture;
    [SerializeField] private Sprite questionMark;
    [SerializeField] private GameObject hide;
    [HideInInspector] public GameObject selected;

    [SerializeField] private GameEventListener observer;

    /*
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            selected = eventData.pointerDrag;
        }

        if(selected != null) { 

            if (selected.TryGetComponent(out ItemDragHandler idh))
            {
                picture.sprite = idh.imageRef.sprite;
            }
            observer.OnEventRaised();
        }
    }
    */
    public void PlaceIt(ItemDragHandler itemPlaced)
    {
        selected = itemPlaced.gameObject;
        picture.sprite = itemPlaced.imageRef.sprite;
        observer.OnEventRaised();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ResetSlot();
    }

    public void ResetSlot()
    {
        picture.sprite = questionMark;
        hide.SetActive(false);
    }
}
