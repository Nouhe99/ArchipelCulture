using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, /*IDragHandler, IEndDragHandler, IBeginDragHandler,*/ IPointerClickHandler
{
    [SerializeField] private GameObject templateObject;
    public Image rarityImg;
    public Image imageRef;
    public TMPro.TMP_Text underlineText;

    /*
    //DRAGGING
    
    private GameObject currentItem;
    private Transform initialparent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (templateObject == null)
        {
            currentItem = gameObject;
            initialparent = gameObject.transform.parent;
            currentItem.transform.SetParent(UIManager.current.mainCanvas.transform);
            //currentItem.GetComponent<Image>().raycastTarget = false;
        }
        else
        {
            currentItem = Instantiate(templateObject);
            if (currentItem.TryGetComponent(out Item it)) it.Setimage(imageRef.sprite);
            //TO DO : change size

            if (currentItem.TryGetComponent(out SpriteRenderer r))
            {
                currentItem.transform.localScale = Vector3.one * UIManager._cam.orthographicSize / 3;
                r.sortingLayerName = "UI";
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentItem.transform.position = UIManager.GetMousePos() + new Vector3(0, 0, -5);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (templateObject == null)
        {
            FindObjectOfType<ItemSlot>().selected = gameObject;
            currentItem.transform.SetParent(initialparent);
            //currentItem.GetComponent<Image>().raycastTarget = true;
        }
        else
            Destroy(currentItem);
    }
    */

    public virtual void ClickEffect()
    {
        ItemSlot slotHere = FindObjectOfType<ItemSlot>();
        if (slotHere)
            slotHere.PlaceIt(this);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        ClickEffect();

        //FindObjectOfType<ItemSlot>().OnDrop(eventData);
    }

    public void FillInfos(Item item)
    {
        imageRef.sprite = item.GetSprite(); //change sprite
        underlineText.text = item.price + ""; //price
        if (item.rarity.Case == null) //rarity background
            rarityImg.color = Color.clear;
        else
        {
            rarityImg.sprite = item.rarity.Case;
            if (rarityImg.color == Color.clear) rarityImg.color = Color.white;
        }
    }
}