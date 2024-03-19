using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public SceneController sceneController;

    [Header("Panels")]
    public Canvas mainCanvas;
    public TopBarPanel topBarPanel;
    public GameObject settingsPanel;
    public Profil profil;
    public Shop shop;
    public InventoryUI inventoryUI;

    [Header("Builder")]
    public GameObject itemContener;
    public Transform floorContener;
    public GameObject gameContent;

    [Header("UI elements")]
    [SerializeField] private GameObject fingerIcon;
    public Success notificationSuccess;
    public UnlockTitle unlockTitles = new();
    public GameObject boatObject;
    public Sprite boatSpriteGoAway;


    [Header("Button")]
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonBuilder;
    public Button buttonProfil;

    [Header("Datas")]
    public DatabaseItems itemsList;
    #region Ground
    public GroundRuleTile[] groundRules;
    [Serializable]
    public struct GroundRuleTile
    {
        public int id;
        public RuleTile tiles;
    }
    public static RuleTile GetGroundRule(int id)
    {
        foreach (var groundType in current.groundRules)
        {
            if (groundType.id == id) return groundType.tiles;
        }
        return current.groundRules[0].tiles;
    }
    #endregion

    public static UIManager current;
    public static Camera _cam;
    public static Coroutine pointer;

    private void Awake()
    {
        current = this;
        _cam = Camera.main;
        //if(PlayAudio.Instance != null) PlayAudio.Instance.PlayMusic(PlayAudio.Instance.bank.beach, "fx");

        //Things to do if we are in local
        if (Database.Instance.IsPublicAccount())
        {
            //SaveDataInventory.current.ReadInventoryFile();
        }
        //Database.Instance?.StartCoroutine(Database.Instance.GetSpecialScenarios());
    }

    private void Start()
    {
        

        //Application.targetFrameRate = 60;


        finger = Instantiate(fingerIcon);
        finger.SetActive(false);

        InitPanels(); //open and close right panels

        #region Buttons Init
        if (buttonSettings != null)
        {
            buttonSettings.onClick.AddListener(() => OpenClosePanel(Panel.Settings));
        }
        if (buttonBuilder != null)
        {
            buttonBuilder.onClick.AddListener(() => OpenClosePanel(Panel.Builder));
        }
        if (buttonProfil != null)
        {
            buttonProfil.onClick.AddListener(() => OpenClosePanel(Panel.Profil));
        }
        #endregion

        #region Place Island Tile
        if (Database.Instance.userData.rockPlacementJson == "") Database.Instance.userData.rockPlacementJson = "[]";

        List<UserData.TilePos> list = JsonConvert.DeserializeObject<List<UserData.TilePos>>(Database.Instance.userData.rockPlacementJson);
        foreach (UserData.TilePos tile in list)
        {
            IslandBuilder.current.islandTiles.Add(new Vector3Int(tile.x_pos, tile.y_pos, 0), tile.style_tile);
        }
        IslandBuilder.current.LoadIslandTiles();

        #endregion

        #region Fill Inventory & Placement
        //building placement
        //floorContener.position = IslandBuilder.current.islandTilemap.CellToLocalInterpolated(Database.Instance.userData.buildingPlacement) + new Vector3(0, 0, Database.Instance.userData.buildingPlacement.x + Database.Instance.userData.buildingPlacement.y - 1);
        List<ItemFloor> placedFloors = new();
        foreach (UserData.ItemPlacement item in Database.Instance.userData.inventory.OrderBy(item => item.z_pos))
        {
            //TO DO : ajouter les photos de profil et les titres
            Item refitem = itemsList.GetItemData(item.id);
            if (!refitem)
            {
                Debug.LogWarning("This prefab's item (" + item.id + ") doesn't exist. You must create one");
                continue;
            }
            refitem.id_inventory = item.id_inventory;
            refitem.variante = item.variante;
            refitem.area.position = new Vector3Int(item.x_pos, item.y_pos, item.z_pos);

            if (!item.placed || !GridBuildingSystem.current.AreaHaveRocks(refitem.area))
            {
                // Item is not placed
                
                // TODO : Change the state of refItem to not placed

                // Add it to inventory
                inventoryUI.AddNewSlot(refitem);
            }
            else
            {
                // Item is placed
                // Instantiate it
                GameObject tempObj = Instantiate(itemsList.GetItem(item.id), itemContener.transform);
                Item tempItem = tempObj.GetComponent<Item>();
                tempItem.placed = true;
                tempItem.area.position = refitem.area.position;

                if (refitem.type == ItemType.Decoration)
                {
                    if (item.variante  > 0 && tempItem.spriteMirror.Count > 0)
                    {
                        if ( item.variante <= tempItem.spriteMirror.Count )
                        {
                            tempItem.spriteMirror[item.variante -1].gameObject.SetActive(true);
                            tempItem.spriteRenderer.gameObject.SetActive(false);
                        }
                    }
                    tempObj.transform.localPosition = GridBuildingSystem.current.gridLayout.CellToLocalInterpolated(refitem.area.position) + new Vector3(0, 0, refitem.area.position.x + refitem.area.position.y - 1);
                    GridBuildingSystem.current.TakeArea(TileType.TakenByItem, refitem.area);
                }
                else if (refitem.type == ItemType.Building)
                {
                    ItemFloor itemFloor = tempItem.GetComponent<ItemFloor>();
                    
                    if (item.z_pos == 0)
                    {
                        if (itemFloor != null)
                        {
                            placedFloors.Add(itemFloor);
                        }
                        tempObj.transform.localPosition = GridBuildingSystem.current.gridLayout.CellToLocalInterpolated(refitem.area.position) + new Vector3(0, 0, refitem.area.position.x + refitem.area.position.y - 1);
                        GridBuildingSystem.current.TakeArea(TileType.TakenByFloor, refitem.area);
                    }
                    else
                    {
                        ItemFloor parentFloor = placedFloors.Find(floor => floor.area.position == new Vector3Int(item.x_pos, item.y_pos, 0));
                        if (parentFloor)
                        {
                            parentFloor.PlaceFloorOnTop(itemFloor);
                        }
                    }
                }

            }

        }

        //Building.current.ResetTower(); //place floors
        //if(Building.current.BottomFloor != null) GridBuildingSystem.current.TakeArea(new BoundsInt(Database.Instance.userData.buildingPlacement, Vector3Int.right * 2 + Vector3Int.up * 2 + Vector3Int.forward));

        inventoryUI.rocksRemainingText.text = Database.Instance.userData.rocksRemaining + "";

        #endregion


        StartCoroutine(CameraBehavior.Instance.ResetCameraAtStartCoroutine());

        #region Fill Shop

        ///Key : tuple of the name of the item style and the item type.
        ///Value : category handler (transform)
        Dictionary<(Style, ItemType), Transform> allCategories = new();

        GameObject tempCaseShop = shop.caseShop;
        int nbrColumns = 4;
        float heightCase = shop.GetContentByType(ItemType.Decoration).gameObject.GetComponent<RectTransform>().rect.width / nbrColumns;

        foreach (var itemGO in itemsList.items.Keys)
        {
            Item item = itemsList.GetItemData(itemGO);

            if (item.price == 0) continue; //if not buyable, not put it in shop
            if (shop.GetContentByType(item.type) == null) continue; //cannot place item of this type in shop.

            Transform currentCategory;
            //create a new category if doesn't exist already
            if (!allCategories.ContainsKey((item.style, item.type)))
            {
                currentCategory = Instantiate(shop.categoryObject, shop.GetContentByType(item.type)).transform;
                allCategories.Add((item.style, item.type), currentCategory);
                currentCategory.GetComponentInChildren<TMPro.TMP_Text>().text = item.style.Label;
                currentCategory.GetComponent<RectTransform>().sizeDelta += new Vector2(0, heightCase);
            }
            else
            {
                currentCategory = allCategories[(item.style, item.type)];
            }

            //instantiate object in shop in category
            tempCaseShop = Instantiate(shop.caseShop, currentCategory.Find("Items"));
            tempCaseShop.GetComponent<ItemDragHandler>().FillInfos(item);
            Item.CopyItem(item, tempCaseShop.GetComponent<Item>());

        }

        #endregion

       

      
        

    }

    #region Panels
    public enum Panel
    {
        NULL,
        Builder,
        Settings,
        Profil,
        Shop,
        Gifts,
        Expedition
    }
    [HideInInspector] public Panel currentOpenPanel = Panel.NULL;

    private void InitPanels()
    {
        if (!topBarPanel.gameObject.activeInHierarchy) topBarPanel.gameObject.SetActive(true);
        if (settingsPanel.activeInHierarchy) settingsPanel.SetActive(false);
        if (profil.gameObject.activeInHierarchy) profil.gameObject.SetActive(false);
        if (shop.gameObject.activeInHierarchy) shop.gameObject.SetActive(false);
        if (!inventoryUI.gameObject.activeInHierarchy) inventoryUI.gameObject.SetActive(true);
        if (inventoryUI.sideBarOpen) inventoryUI.ShowBuildingPanel();

        if (!gameContent.activeInHierarchy) gameContent.SetActive(true);
    }
    public void OpenClosePanel(Panel window)
    {
        if (currentOpenPanel == Panel.NULL) OpenPanel(window);
        else if (currentOpenPanel == window) ClosePanel(window);
        else
        {
            CloseCurrentPanel();
            OpenPanel(window);
        }
    }
    public void CloseCurrentPanel()
    {
        ClosePanel(currentOpenPanel);
    }
    private void OpenPanel(Panel window)
    {
        currentOpenPanel = window;
        PlayAudio.Instance.bank.PressButtonNormal();
        switch (window)
        {
            case Panel.Builder:
                if (!inventoryUI.sideBarOpen) inventoryUI.ShowBuildingPanel();
                break;

            case Panel.Profil:
                profil.ReorganizeTabs();
                topBarPanel.TriggerPanel(profil.gameObject);
                ActivateUIColliders(false);
                break;

            case Panel.Settings:
                topBarPanel.TriggerSettingsPanel();
                ActivateUIColliders(false);
                break;

            case Panel.Expedition:
                if (boatObject != null)
                {
                    boatObject.GetComponent<SpriteRenderer>().sprite = boatSpriteGoAway;
                    boatObject.GetComponent<GoingTo>().ChangeDirection(new Vector3(7, -2, 0));
                    boatObject.GetComponentInChildren<ParticleSystem>().Play();
                }
                LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.MissionScene, "Voyage vers la zone de mission en cours..."));
                break;

            case Panel.Shop:
                topBarPanel.TriggerPanel(shop.gameObject);
                ActivateUIColliders(false);
                break;

            default:
                break;

        }
    }
    private void ActivateUIColliders(bool activate = true)
    {
        foreach (Collider2D collider in gameContent.GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = activate;
        }
    }

    private void ClosePanel(Panel window)
    {
        PlayAudio.Instance.bank.PressButtonReturn();
        switch (window)
        {
            case Panel.Builder:
                if (inventoryUI.sideBarOpen) inventoryUI.ShowBuildingPanel();
                break;

            case Panel.Profil:
                if (profil.gameObject.activeInHierarchy)
                {
                    profil.ReorganizeTabs();
                    topBarPanel.TriggerPanel(profil.gameObject);
                    ActivateUIColliders(true);
                }
                break;

            case Panel.Settings:
                if (settingsPanel.activeInHierarchy)
                {
                    topBarPanel.TriggerSettingsPanel();
                    ActivateUIColliders(true);
                }
                break;

            case Panel.Shop:
                StartCoroutine(SaveDataInventory.Instance.UpdateItemBuyDatabase());
                if (shop.gameObject.activeInHierarchy)
                {
                    topBarPanel.TriggerPanel(shop.gameObject);
                    ActivateUIColliders(true);
                }
                break;

            default:
                break;
        }

        currentOpenPanel = Panel.NULL;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           
                LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.HomeScene, "Retour au bercail"));
               
            
        }
    }

    public void HideAllButtons(bool hide = true)
    {
        if (buttonProfil != null) buttonProfil.gameObject.SetActive(!hide);
        if (buttonSettings != null) buttonSettings.gameObject.SetActive(!hide);
        if (buttonBuilder != null) buttonBuilder.gameObject.SetActive(!hide);
    }

    #endregion

    #region Pop Up

    [Header("Pop Up")]
    [SerializeField] private PopUp confirmationDialog;
    private static PopUp currentPopUp = null;
    public GameObject infoDialog;
    public GameObject notifDialog;
    public GameObject fullScreenDialog;
    private static GameObject infoDialogTemp = null;
    private static GameObject notifDialogTemp = null;
    private static GameObject fullScreenDialogTemp = null;
    /// <summary>
    /// Show a Pop Up window, and get the user choice as result <br/>
    /// Use it as follow :<br/><code>  bool result;<br/>  StartCoroutine(ShowConfirmationDialog("test", value => result = value)); </code>
    /// </summary>
    public static IEnumerator ShowConfirmationDialog(string text, PopupStyle style, Action<bool> choice)
    {
        PlayAudio.Instance.PlayOneShot(PlayAudio.Instance.bank.bipUnvalid);
        if (currentPopUp != null) Destroy(currentPopUp.gameObject);
        currentPopUp = current.confirmationDialog.Instance(text, style);
        while (currentPopUp.result == currentPopUp.NONE)
        {
            yield return null; // wait
        }

        if (currentPopUp.result == currentPopUp.YES)
        {
            //Debug.Log("Builder.ShowConfirmationDialog(): Yes");
            choice(true);
        }
        else if (currentPopUp.result == currentPopUp.CANCEL)
        {
            //Debug.Log("Builder.ShowConfirmationDialog(): Cancel");
            choice(false);
        }

        currentPopUp.Destroy();
    }

    public static void ShowInfoMessage(string textInfo)
    {
        if (infoDialogTemp != null) Destroy(infoDialogTemp);
        current.infoDialog.GetComponent<TMPro.TMP_Text>().text = textInfo;
        if (textInfo != "")
            infoDialogTemp = Instantiate(current.infoDialog, current.mainCanvas.transform);
    }

    public static void ShowBigNotification(string textNotif)
    {
        if (notifDialogTemp != null) Destroy(notifDialogTemp);
        current.notifDialog.GetComponentInChildren<TMPro.TMP_Text>().SetText(textNotif);
        if (textNotif != "")
        {
            notifDialogTemp = Instantiate(current.notifDialog, current.topBarPanel.transform);
        }
    }

    public static void ShowFullScreenDialog(string textDialog)
    {
        if (fullScreenDialogTemp != null) Destroy(fullScreenDialogTemp);
        current.fullScreenDialog.GetComponentInChildren<TMPro.TMP_Text>().SetText(textDialog);
        if (!string.IsNullOrWhiteSpace(textDialog))
        {
            fullScreenDialogTemp = Instantiate(current.fullScreenDialog, current.mainCanvas.transform);
            fullScreenDialogTemp.GetComponentInChildren<UIAnimation>()?.AnimateFromStartToEndAsync();
        }
    }
    #endregion

    #region Animations
    [HideInInspector] public GameObject finger;
    public IEnumerator DragFinger(Vector3 from, Vector3 to, float speed = 1.0f)
    {
        finger.transform.position = from;
        finger.transform.localScale = Vector3.one * _cam.orthographicSize / 3;
        finger.SetActive(true);
        //finger.transform.localScale = new Vector3(Screen.width / 900.0f, Screen.width / Screen.width*9.0f, Screen.width / 9.0f);
        yield return new WaitForSeconds(0.2f);
        float startTime = Time.time;
        float journeyTime = 1.0f;

        Vector3 center = (from + to) * 0.5F;
        center -= new Vector3(0, 2.0f, 0);

        // Interpolate over the arc relative to center
        Vector3 riseRelCenter = from - center;
        Vector3 setRelCenter = to - center;

        while (Vector3.Distance(finger.transform.position, to) > 0.001f)
        {
            float frac = (Time.time - startTime) / journeyTime * speed;
            finger.transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, frac);
            finger.transform.position += center;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        finger.SetActive(false);
        pointer = null;
    }
    #endregion

    #region Get Pos
    /// <summary>
    /// Mouse world position from main camera.
    /// </summary>
    public static Vector3 GetMousePos()
    {
        Vector3 cursorPosition = Vector3.zero;
#if UNITY_IOS || UNITY_ANDROID

        //Get Touch position
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            cursorPosition = _cam.ScreenToWorldPoint(touch.position);
        }
#else
        // Get Mouse position
        cursorPosition = _cam.ScreenToWorldPoint(Input.mousePosition);
#endif

        cursorPosition.z = 0;
        return cursorPosition;
    }

    public static Vector3 GetMousePosOnScreen()
    {
        Vector3 cursorPosition = Vector3.zero;
#if UNITY_IOS || UNITY_ANDROID

        //Get Touch position
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            cursorPosition = touch.position;
        }
        else return -Vector3.one;
#else
        // Get Mouse position
        cursorPosition = Input.mousePosition;
#endif

        return cursorPosition;
    }

    public Vector3 CenterScreen()
    {
#if UNITY_EDITOR
        string[] res = UnityStats.screenRes.Split('x');
        //Debug.Log(int.Parse(res[0]) + " " + int.Parse(res[1]));
        return _cam.ScreenToWorldPoint(new Vector3(int.Parse(res[0]) / 2, int.Parse(res[1]) / 2, 0));
#else
        return _cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
#endif
    }
    #endregion

}
