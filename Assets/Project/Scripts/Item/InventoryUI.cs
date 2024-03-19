using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameEvent changeBoundsEvent;

    public ToggleGroup menuSelection;
    public GameObject contentCustom;
    public GameObject contentHome;
    [SerializeField] private InventorySlot itemEntry; // prefab of the slot in inventory
    public TMPro.TMP_Text rocksRemainingText;
    [SerializeField] private AnimationCurve inventoryShowing;
    [SerializeField] private GameObject outOfWindow;
    [SerializeField] private ToggleGroup togglesRocks;

    #region Unity Methods

    private void OnApplicationPause(bool pauseStatus)
    {
        bool paused = pauseStatus;
        //to do : sign in and sign out work when it's a class version ?
        if (paused)
        {
            //backup on database
            if(SaveDataInventory.Instance != null)
            {
                StartCoroutine(SaveDataInventory.Instance.UpdateItemPlacedDatabase());
            }
            //Debug.LogFormat("<i>PAUSED !</i>");
        }
        //else
        //Debug.LogFormat("<i>Is not in pause,</i> welcome back !");
    }

    #endregion

    #region Open and Close Panels

    [HideInInspector] public bool sideBarOpen = false;
    [HideInInspector] public bool changeFloorOpen = false;
    private Coroutine panelCoroutine;

    public void ShowBuildingPanel()
    {
        //if the side bar panel is open, close it, otherwise open it.
        if (sideBarOpen)
        {
            if (panelCoroutine != null) StopCoroutine(panelCoroutine);
            panelCoroutine = StartCoroutine(ClosePanel(gameObject));

            //hide grid, selection floor images and change state for rocks (remove "+"s).
            IslandBuilder.current.ShowGrid(false);
            Building.current.ShowConstructionBlocks(false);
            IslandBuilder.current.StateNumber = 0;

            //backup on database
            if (SaveDataInventory.Instance != null) //TO DO : change this for local version.
                StartCoroutine(SaveDataInventory.Instance.UpdateItemPlacedDatabase());

            //change bounds for the camera
            if (changeBoundsEvent != null)
            {
                changeBoundsEvent.Raise();
            }
            //unlock camera
            CameraBehavior.Instance.panOnX = true;
        }
        else
        {
            OpenToggle();
            if (panelCoroutine != null) StopCoroutine(panelCoroutine);
            panelCoroutine = StartCoroutine(OpenPanel(gameObject));
        }

        sideBarOpen = !sideBarOpen;

    }

    public void OpenToggle(Toggle tog)
    {
        if (tog.isOn) OpenToggle();
    }
    private void OpenToggle()
    {
        //if (menuSelection.GetFirstActiveToggle().name == "Home - Toggle") //open floor menu
        //{
        //    //CameraBehavior.Instance.ZoomOnHome(true);
        //    Building.current.ShowConstructionBlocs(true);
        //    //IslandBuilder.current.ShowGrid(false);
        //}
        //else
        //{
        //    //CameraBehavior.Instance.ZoomOnHome(false); //unlock pan on X and update bounds
        //    Building.current.ShowConstructionBlocs(false);
        //    //IslandBuilder.current.ShowGrid(true);
        //    //CameraBehavior.Instance.ZoomGlobal();
        //}

        if (menuSelection.GetFirstActiveToggle().name == "Island - Toggle") //open rock menu
        {
            //CameraBehavior.Instance.ZoomGlobal();
            IslandBuilder.current.ShowGrid(true);
            togglesRocks.GetFirstActiveToggle().onValueChanged.Invoke(true); //apply the event of the rock state selected
            GridBuildingSystem.current.MaskItemType(ItemType.NULL);
            GridBuildingSystem.current.HighlightFloorsRoof(false);
        }
        else 
        {
            IslandBuilder.current.ShowGrid(false);
            IslandBuilder.current.StateNumber = 0;
            
        }
        if (menuSelection.GetFirstActiveToggle().name == "Home - Toggle") //open floor menu
        {
            GridBuildingSystem.current.HighlightValidPositions();
            GridBuildingSystem.current.MaskItemType(ItemType.Decoration);
            GridBuildingSystem.current.HighlightFloorsRoof(true);
        }
        if (menuSelection.GetFirstActiveToggle().name == "Deco - Toggle") //open decoration menu
        {
            GridBuildingSystem.current.HighlightValidPositions();
            GridBuildingSystem.current.MaskItemType(ItemType.Building);
            GridBuildingSystem.current.HighlightFloorsRoof(false);
        }
        CameraBehavior.Instance.ZoomGlobal();

        //Remove pointer if existed
        if (UIManager.pointer != null)
        {
            UIManager.current.finger.SetActive(false);
            UIManager.current.StopCoroutine(UIManager.pointer);
            UIManager.pointer = null;
        }
    }

    private IEnumerator OpenPanel(GameObject panel)
    {
        float timeSecond = 0.8f;
        float timeElapsed = 0f;
        Vector2 from = panel.GetComponent<RectTransform>().anchoredPosition;
        Vector2 to = Vector3.zero;
        //element out of the panel
        Vector2 from2 = outOfWindow.GetComponent<RectTransform>().anchoredPosition;
        Vector2 to2 = Vector3.up * -39;
        while (timeElapsed < timeSecond)
        {
            outOfWindow.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(from2, to2, inventoryShowing.Evaluate(timeElapsed / timeSecond) /*Mathf.Cos(((timeElapsed/timeSecond)-1)*Mathf.PI)/2 + .5f*/);
            panel.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(from, to, inventoryShowing.Evaluate(timeElapsed / timeSecond) /*Mathf.Cos(((timeElapsed/timeSecond)-1)*Mathf.PI)/2 + .5f*/);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        outOfWindow.GetComponent<RectTransform>().anchoredPosition = to2;
    }

    private IEnumerator ClosePanel(GameObject panel)
    {
        float timeSecond = 0.8f;
        float timeElapsed = 0f;
        Vector2 from = panel.GetComponent<RectTransform>().anchoredPosition;
        Vector2 to = -new Vector2(0, panel.GetComponent<RectTransform>().rect.height);
        //Vector2 to = -new Vector2(panel.GetComponent<RectTransform>().rect.width, 0);
        //element of the panel
        Vector2 from2 = outOfWindow.GetComponent<RectTransform>().anchoredPosition;
        Vector2 to2 = -new Vector2(0, outOfWindow.GetComponent<RectTransform>().rect.height - 25);
        GridBuildingSystem.current.HighlightFloorsRoof(false);
        GridBuildingSystem.current.MaskItemType(ItemType.NULL);
        
        while (timeElapsed < timeSecond)
        {
            outOfWindow.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(from2, to2, inventoryShowing.Evaluate(timeElapsed / timeSecond) /*Mathf.Cos(((timeElapsed/timeSecond)-1)*Mathf.PI)/2 + .5f*/);
            panel.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(from, to, inventoryShowing.Evaluate(timeElapsed / timeSecond) /*Mathf.Cos(((timeElapsed/timeSecond)-1)*Mathf.PI)/2 + .5f*/);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        panel.GetComponent<RectTransform>().anchoredPosition = to;
        outOfWindow.GetComponent<RectTransform>().anchoredPosition = to2;
        
    }

    #endregion

    #region Manage SideBar

    /// <summary>
    /// Create a new slot if item doesn't exist, increment number if item of this type already exist.
    /// </summary>
    public InventorySlot AddNewSlot(Item item)
    {
        InventorySlot slot = FindItemSlot(item.id);
        if (slot == null)
        {
            if (item.type == ItemType.Building) slot = itemEntry.Instance(item.Data(), contentHome.transform);
            else slot = itemEntry.Instance(item.Data(), contentCustom.transform);
        }
        slot.AddItem(item);
        return slot;
    }
    /// <summary>
    /// Remove completely the item from the game, but not from database and/or save.
    /// </summary>
    public void RemoveItemFromAnywhere(string id_inventory)
    {
        UserData.ItemPlacement itemFound = Database.Instance.userData.RemoveItemInInventory(id_inventory);
        if (itemFound == null) return; //item was not in inventory :(
        Item refitem = Database.Instance.itemsList.GetItemData(itemFound.id);

        if (itemFound.placed)
        {
            //find placed objet to remove it.
            //if (refitem.type == ItemType.Building)
            //    Building.current.RemoveFloorNumber(refitem.id_inventory, false);
            //else
            GridBuildingSystem.current.RemoveById(refitem.id_inventory, false);

        }
        else
        {
            //remove inventory slot
            GameObject inventory;
            if (refitem.type == ItemType.Building) inventory = contentHome;
            else inventory = contentCustom;
            foreach (InventorySlot slot in inventory.GetComponentsInChildren<InventorySlot>(true))
            {
                if (slot.inventory_id.Contains(id_inventory))
                {
                    slot.RemoveItem();
                    return;
                }
            }
            Debug.Log("cannot find slot from this id_inventory in inventory :(");
        }
    }


    public InventorySlot FindItemSlot(string item_id, ItemType type = ItemType.NULL) //if type NULL, search it everywhere.
    {
        GameObject inventory;
        if (type == ItemType.Building) inventory = contentHome;
        else inventory = contentCustom;
        foreach (InventorySlot slot in inventory.GetComponentsInChildren<InventorySlot>(true))
        {
            if (slot.item.ID == item_id) return slot;
        }
        if (type == ItemType.NULL)
        {
            foreach (InventorySlot slot in contentHome.GetComponentsInChildren<InventorySlot>(true))
            {
                if (slot.item.ID == item_id) return slot;
            }
        }

        //Debug.Log("Item "+ item_id + " doesn't have a slot, must create a new one !");
        return null;
    }

    #endregion

}