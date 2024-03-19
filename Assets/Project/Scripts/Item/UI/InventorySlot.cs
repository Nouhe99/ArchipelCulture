using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public ItemData item;
    public Queue<string> inventory_id = new();
    private bool newRelease = false;
    [Header("UI")]
    [SerializeField] private Image sprite;
    [SerializeField] private GameObject label;
    [SerializeField] private TMPro.TMP_Text nbrItem;
    [SerializeField] private Image rarityBackground;
    [SerializeField] private GameObject notification;

    private static GameObject thisGameObject;
    private void Awake()
    {
        thisGameObject = this.gameObject;
    }

    private void Start()
    {
        if (newRelease && notification != null)
        {
            notification.SetActive(true);
        }
    }

    public void AddItem(Item item)
    {
        inventory_id.Enqueue(item.id_inventory);
        UpdateLabel();
        if (gameObject.activeInHierarchy) StartCoroutine(SlotExtend());

        //debug
        string tempDebug = "";
        foreach (string inventoid in inventory_id)
        {
            tempDebug += inventoid + ", ";
        }
    }
    /// <summary>
    /// Remove Item from inventory.
    /// </summary>
    public void RemoveItem()
    {
        inventory_id.Dequeue();
        if (inventory_id.Count >= 1)
            UpdateLabel();
        else
            Destroy(gameObject);
    }
    public InventorySlot Instance(ItemData item, Transform parent)
    {
        InventorySlot temp = Instantiate(this, parent);
        temp.item = item;
        temp.inventory_id.Clear();
        temp.sprite.sprite = item.Sprite;
        temp.SetRaritySlot(item.Rarity);
        return temp;
    }

    private bool hasBeenMoved = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.pointer != null)
        {
            UIManager.current.finger.SetActive(false);
            // StopCoroutine(UIManager.pointer);
            //UIManager.pointer = null;
        }
        else if (UIManager.pointer == null && !hasBeenMoved)
        {
            Vector3 to;
            if (item.Type == ItemType.Building) to = (UIManager.current.CenterScreen() * 0.6f) + (Vector3.forward * 6);
            else to = UIManager.current.CenterScreen() + (Vector3.forward * 10);
            UIManager.pointer = UIManager.current.StartCoroutine(UIManager.current.DragFinger(gameObject.transform.position, to));
        }
        hasBeenMoved = false;
    }

    #region Drag
    public void OnPointerDown(PointerEventData eventData)
    {
        hasBeenMoved = false;
        if (item.Type == ItemType.Ground) return; //Note : what if we drag rocks?
        CameraBehavior.Instance.CanPan = false;
        StartCoroutine(Dragging());
        StartCoroutine(WaitForMove());
    }

    private IEnumerator Dragging()
    {
        yield return new WaitWhile(() => ReplaceableOnMap.MouseOverMenu(UIManager.current.inventoryUI.gameObject));
#if UNITY_IOS || UNITY_ANDROID
        if (UIManager.GetMousePosOnScreen().x <= 0 || UIManager.GetMousePosOnScreen().y <= 0 ||
            UIManager.GetMousePosOnScreen().x >= Screen.width || UIManager.GetMousePosOnScreen().y >= Screen.height ||
            Input.touchCount > 1) yield break;
#else
        if (!Input.GetMouseButton(0)) yield break;
#endif
        //creating item
        GameObject tempObj = GridBuildingSystem.current.InitializedWithItem(this);
        if (tempObj == null) yield break;

        //if (item.Type == ItemType.Building)
        //{
        //    Building.current.PlaceGhost();
        //}
        tempObj.GetComponent<PolygonCollider2D>().enabled = false; //desac collider

        //select
        if (tempObj.GetComponent<Item>())
        {
            Item tempItem = tempObj.GetComponent<Item>();
            tempItem.id_inventory = inventory_id.Peek();
            GridBuildingSystem.current.Select(tempItem);
            RemoveItem();
            yield break;
        }
        else if (tempObj.GetComponent<ItemRock>()) //Note: unused, but in case
        {
            GridBuildingSystem.current.Select(tempObj.GetComponent<ItemRock>());
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
        if (UIManager.GetMousePosOnScreen() != initialpos) hasBeenMoved = true;
    }

    #endregion


    #region UI Update Handler

    private void SetRaritySlot(Rarity rarity)
    {
        if (rarity.Case == null)
            rarityBackground.color = Color.clear;
        else
        {
            rarityBackground.sprite = rarity.Case;
            if (rarityBackground.color == Color.clear) rarityBackground.color = Color.white;
        }
    }
    private void UpdateLabel()
    {
        if (inventory_id.Count > 1)
        {
            label.SetActive(true);
            nbrItem.text = "X" + inventory_id.Count;
        }
        else
        {
            label.SetActive(false);
        }
    }
    #endregion


    #region Animation
    private bool isAnimated = false;
    private Vector3 initialScale = Vector3.one;
    private IEnumerator extend;
    /// <summary>
    /// Slot animation
    /// </summary> 
    public IEnumerator SlotExtend()
    {
        RectTransform rectTransform = sprite.gameObject.GetComponent<RectTransform>();
        if (isAnimated)
        {
            StopCoroutine(extend);
            rectTransform.localScale = initialScale;
        }
        else
        {
            isAnimated = true;
            initialScale = rectTransform.localScale;
        }
        extend = AnimationsScript.Extend(rectTransform);
        yield return StartCoroutine(extend);
        isAnimated = false;
    }

    #endregion


}