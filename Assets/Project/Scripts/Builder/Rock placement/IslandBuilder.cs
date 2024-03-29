using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using System.IO;



/// <summary>
/// Handler for rock tiles and boat placement.
/// </summary>
/// 
[System.Serializable]
public class RockData
{
    public List<RockInfo> rocks = new List<RockInfo>();
}

[System.Serializable]
public class RockInfo
{
    public int x;
    public int y;
    public int rockType;

    public RockInfo(int x, int y, int rockType)
    {
        this.x = x;
        this.y = y;
        this.rockType = rockType;
    }
}


public class IslandBuilder : MonoBehaviour
{

    public static IslandBuilder current;
    string rocksRemainingFilePath;
    private void Awake()
    {
        current = this;
        rocksRemainingFilePath = Path.Combine(Application.persistentDataPath, "rocksRemainingJson.txt");
    }


    #region Rock Placement
    [Header("Island")]
    public Tilemap islandTilemap;

    public readonly Dictionary<Vector3Int, int> islandTiles = new();

    [Header("UI")]
    [SerializeField] private AudioClip audioSplash;
    [SerializeField] private GameObject fxSplash;
    [SerializeField] private UnityEngine.UI.Image imgRocks;
    [SerializeField] private RectTransform remainingRocksCounter;
    [SerializeField] private TileBase plusTile;
    [SerializeField] private TileBase minusTile;

    private int stateNumber; //-1 is remove, 0 is nothing, 1 and more are ground numbers.
    public int StateNumber
    {
        set
        {
            stateNumber = value;
            if (value < 0)
            {
                PlaceMinusTiles();
            }
            else if (value > 0)
            {
                PlacePlusTiles();
            }
            else
            {
                RemovePlusTiles();
            }
        }
        get => stateNumber;
    }

   


    #region Plus tiles

    private void PlacePlusTiles()
    {
        RemovePlusTiles();
        if (Database.Instance.userData.rocksRemaining <= 0) GridBuildingSystem.current.tempTilemap.color = new Color(1, 1, 1, 0.3f);
        for (int i = 0; i > -sizeTilemap; i--)
        {
            for (int j = 0; j > -sizeTilemap; j--)
            {
                Vector3Int pos = new(i, j);
                if (!islandTiles.ContainsKey(pos))
                {
                    GridBuildingSystem.SetTilesBlock(new BoundsInt(pos, Vector3Int.one), TileType.Plus, GridBuildingSystem.current.tempTilemap);
                }
            }
        }
    }
    private void PlaceMinusTiles()
    {
        RemovePlusTiles();
        for (int i = 0; i > -sizeTilemap; i--)
        {
            for (int j = 0; j > -sizeTilemap; j--)
            {
                Vector3Int pos = new(i, j);
                if (islandTiles.ContainsKey(pos))
                {
                    GridBuildingSystem.SetTilesBlock(new BoundsInt(pos, Vector3Int.one), TileType.Minus, GridBuildingSystem.current.tempTilemap);
                }
            }
        }
    }

    private void RemovePlusTiles()
    {
        GridBuildingSystem.current.tempTilemap.color = Color.white;
        GridBuildingSystem.current.tempTilemap.ClearAllTiles();
    }

    #endregion

    public void AddIslandTile(Vector3Int position, int numberTile)
    {
        RuleTile tile = UIManager.GetGroundRule(numberTile);

        if (islandTiles.ContainsKey(position))
        {
            //NOTE : previons condition must never be satified
            //change tile type to newer type : remove it and then place it.
            islandTiles.Remove(position);
        }

        //Place rock
        islandTiles.Add(position, numberTile);
        RemoveRockCounter(islandTiles.Count);
        islandTilemap.SetTile(position, tile);
        GridBuildingSystem.current.tempTilemap.SetTile(position, null);
        islandTilemap.GetInstantiatedObject(position).GetComponent<ItemRock>().rockType = numberTile;
        SaveDataInventory.Instance.AddRockSave(position.x, position.y, numberTile);

        //int z = position.z;
        //for (int x = position.x - 1; x <= position.x + 1; x++)
        //{
        //    for (int y = position.y - 1; y <= position.y + 1; y++)
        //    {
        //        Vector3Int thisPos = new(x, y, z);
        //        if (islandTilemap.HasTile(thisPos))
        //        {
        //            if (islandTilemap.GetInstantiatedObject(thisPos).TryGetComponent<ItemRock>(out var thisTile)) //thsTile could be null if it's one of the "building tiles".
        //            {
        //                thisTile.placed = true;
        //                thisTile.rockType = islandTiles[thisPos];
        //            }
        //        }
        //    }
        //}

        

        Instantiate(fxSplash, islandTilemap.CellToWorld(position), fxSplash.transform.rotation, gameObject.transform);
        if (PlayAudio.Instance != null) PlayAudio.Instance.PlayOneShot(audioSplash);
    }

    //to do : use this method to initialise island at start
    public void LoadIslandTiles()
    {
        islandTilemap.ClearAllTiles();

        foreach (Vector3Int position in islandTiles.Keys)
        {
            islandTilemap.SetTile(position, UIManager.GetGroundRule(islandTiles[position]));
            islandTilemap.GetInstantiatedObject(position).GetComponent<ItemRock>().rockType = islandTiles[position];
        }
        foreach (var tile in islandTilemap.gameObject.GetComponentsInChildren<ReplaceableOnMap>())
        {
            if (!tile.placed) tile.placed = true;
        }


     /*   #region 2x2 tiles on top of the island  TO CHECK, TILE WORKS ON THE CORNER
       // keep the 2x2 tiles on top for building //NOTE: using FillBow doesn't work :'(
        islandTilemap.SetTile(Vector3Int.zero, UIManager.GetGroundRule(1));
        if (!islandTiles.ContainsKey(Vector3Int.zero)) islandTiles.Add(Vector3Int.zero, 1);
        islandTilemap.SetTile(Vector3Int.left, UIManager.GetGroundRule(1));
        if (!islandTiles.ContainsKey(Vector3Int.left)) islandTiles.Add(Vector3Int.left, 1);
        islandTilemap.SetTile(Vector3Int.down, UIManager.GetGroundRule(1));
        if (!islandTiles.ContainsKey(Vector3Int.down)) islandTiles.Add(Vector3Int.down, 1);
        islandTilemap.SetTile(Vector3Int.down + Vector3Int.left, UIManager.GetGroundRule(1));
        if (!islandTiles.ContainsKey(Vector3Int.down + Vector3Int.left)) islandTiles.Add(Vector3Int.down + Vector3Int.left, 1);
        GridBuildingSystem.current.TakeArea(TileType.White , new BoundsInt(Vector3Int.left + Vector3Int.down, Vector3Int.one * 2));
        #endregion
     */

        UIManager.current.inventoryUI.changeBoundsEvent.Raise();
    }

    /// <summary>
    /// Remove a rock from the grid. Also remove object on top if there is any.
    /// </summary>
    public void RemoveRock(Vector3Int posOnTilemap)
    {
        AddRock(islandTiles.Count); //count in inventory
        islandTiles.Remove(posOnTilemap);
        islandTilemap.SetTile(posOnTilemap, null);
        GridBuildingSystem.current.tempTilemap.SetTile(posOnTilemap, null);
        SaveDataInventory.Instance.RemoveRockSave(posOnTilemap.x, posOnTilemap.y);

        //int z = posOnTilemap.z;
        //for (int x = posOnTilemap.x - 1; x <= posOnTilemap.x + 1; x++)
        //{
        //    for (int y = posOnTilemap.y - 1; y <= posOnTilemap.y + 1; y++)
        //    {
        //        Vector3Int thisPos = new(x, y, z);
        //        if (islandTilemap.HasTile(thisPos))
        //        {
        //            ItemRock thisTile = islandTilemap.GetInstantiatedObject(thisPos).GetComponent<ItemRock>();
        //            if (thisTile == null) continue; //no itemrock, this is a tile which cannot be moved.
        //            thisTile.placed = true;
        //            thisTile.rockType = islandTiles[thisPos];
        //        }
        //    }
        //}

        //remove item on top if there is any
        //NOTE for debug: if the right tile is not found, check if error is not about z value.
        if (GridBuildingSystem.current.takenTilemap.GetTile(posOnTilemap) == GridBuildingSystem.tileBases[TileType.TakenByItem] || GridBuildingSystem.current.takenTilemap.GetTile(posOnTilemap) == GridBuildingSystem.tileBases[TileType.TakenByFloor])
        {
            Item objToRemove = null;
            foreach (Item item in UIManager.current.itemContener.GetComponentsInChildren<Item>())
            {
                if (item.area.Contains(posOnTilemap))
                {
                    objToRemove = item;
                    break;
                }
            }

            if (objToRemove == null)
            {
                Debug.LogError("No gameobject found above the rock tile.");
                GridBuildingSystem.current.FreeArea(new BoundsInt(posOnTilemap, Vector3Int.one));
                return;
            }
            GridBuildingSystem.current.FreeArea(objToRemove.area);

            //animate & destroy
            objToRemove.ReplaceInInventory();

            //to do : animation ? plouf ?
        }
    }

    public void Following(ItemRock rock)
    {
        StartCoroutine(FollowingCorout(rock));
    }
    private IEnumerator FollowingCorout(ItemRock rock)
    {
        yield return null;

#if UNITY_IOS || UNITY_ANDROID
        while (Input.touchCount > 0)
#else
        while (!Input.GetMouseButtonUp(0))
#endif
        {
            rock.Follow();
            yield return null;
        }

        CameraBehavior.Instance.CanPan = true;
        //GridBuildingSystem.current.ClearArea();
        rock.gameObject.transform.localPosition += Vector3.up * 0.25f;
        rock.PlaceIt();
    }

    #region Placeholder (white tiles)

    [Header("Placeholder")]
    public Tilemap placeholderTilemap;
    [SerializeField] private TileBase placeholderTile;

    private const int sizeTilemap = 10;
    private void InitializePlaceholderTilemap()
    {
        placeholderTilemap.BoxFill((Vector3Int.up + Vector3Int.right) * -(sizeTilemap - 1), placeholderTile, -(sizeTilemap - 1), -(sizeTilemap - 1), 0, 0);
        //placeholderTilemap.BoxFill((Vector3Int.down + Vector3Int.left), null, -1, -1, 0, 0); //cannot place on the top 4 tiles
        //placeholderTilemap.BoxFill((Vector3Int.up + Vector3Int.right) * (sizeTilemap - 1), placeholderTile, 0, 0, sizeTilemap - 1, sizeTilemap - 1);
        //placeholderTilemap.FloodFill(new Vector3Int(sizeTilemap, 0), placeholderTile);
        //placeholderTilemap.FloodFill(new Vector3Int(0, sizeTilemap), placeholderTile);
        placeholderTilemap.gameObject.SetActive(false);
    }
    #endregion

    //placement
    [HideInInspector] public Vector3Int prevPos;
    private Vector3Int cursorPositionOnTilemap;

    void Start()
    {
        InitializePlaceholderTilemap();
        UpdateRocks();
        LoadRocks();
        if (!GridBuildingSystem.tileBases.ContainsKey(TileType.Plus))
            GridBuildingSystem.tileBases.Add(TileType.Plus, plusTile);
        if (!GridBuildingSystem.tileBases.ContainsKey(TileType.Minus))
            GridBuildingSystem.tileBases.Add(TileType.Minus, minusTile);
    }

    #region Pen or erase mode
    private void Update()
    {
        if (stateNumber == 0) return;

#if UNITY_IOS || UNITY_ANDROID
        //Get Touch position
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
#endif
        // Get Mouse position
        cursorPositionOnTilemap = placeholderTilemap.LocalToCell(UIManager.GetMousePos());

        #region Edit island with Pen and Eraser

#if UNITY_IOS || UNITY_ANDROID
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
#else
        if (Input.GetMouseButton(0) /*&& !currentTile*/)
#endif
        {
            // Is mouse in bounds ?
            if (!InBounds(cursorPositionOnTilemap)) return;


            //add rock to island
            if (stateNumber > 0)
            {
#if UNITY_IOS || UNITY_ANDROID
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
#else
                if (EventSystem.current.IsPointerOverGameObject()) return;
#endif
                if (Database.Instance.userData.rocksRemaining > 0)
                {
                    CameraBehavior.Instance.CanPan = false;
                    CameraBehavior.Instance.startedPan = false;

                    if (islandTilemap.GetTile(cursorPositionOnTilemap) == null && !ReplaceableOnMap.MouseOverMenu(UIManager.current.inventoryUI.gameObject))
                    {
                        AddIslandTile(cursorPositionOnTilemap, stateNumber);
                    }
                }
                else if (Database.Instance.userData.rocksRemaining <= 0)
                {
                    StartCoroutine(AnimationsScript.Notif(remainingRocksCounter, 1));
                }
            }

            //remove rock
            else if (stateNumber == -1)
            {
                CameraBehavior.Instance.CanPan = false;
                CameraBehavior.Instance.startedPan = false;

                if (islandTilemap.GetTile(cursorPositionOnTilemap) != null)
                {
                    RemoveRock(cursorPositionOnTilemap);
                }
            }

            return;
        }
        #endregion

#if UNITY_IOS || UNITY_ANDROID

        }
#endif
    }
    #endregion

    #endregion

    /// <summary>
    /// Verify if a cell position in included in the construction perimeter.
    /// </summary>
    private bool InBounds(Vector3Int pos)
    {
        //placeholderTilemap.LocalToCell(UIManager.GetMousePos());
        return placeholderTilemap.HasTile(pos);
    }

    #region UI Interactions

    public void ShowGrid(bool open = true)
    {
        placeholderTilemap.gameObject.SetActive(open);
    }

    [SerializeField] private List<Sprite> listImgRocks = new();

    /// <summary>
    /// Add rock to counter <i>rocksRemaining</i>.
    /// </summary>
    private void AddRock(int rocksRemaining)
    {
       
        rocksRemaining++;
        
        UpdateRocks();
        SaveRocks();
    }

    /// <summary>
    /// Remove rock to counter <i>rocksRemaining</i>.
    /// </summary>
    private void RemoveRockCounter(int rocksRemaining)
    {
        
       // int rocksRemaining = islandTiles.Count;
        rocksRemaining--; 
            // Additional logic as needed, for example:
            if (islandTiles.Count <= 0) GridBuildingSystem.current.tempTilemap.color = new Color(1, 1, 1, 0.3f);

            UpdateRocks();
            SaveRocks();
         
        
    }

    internal void UpdateRocks()
    {
        int nbrRocks = islandTiles.Count;
        UIManager.current.inventoryUI.rocksRemainingText.text = nbrRocks + "";
        if (nbrRocks >= listImgRocks.Count) imgRocks.sprite = listImgRocks[^1];
        else imgRocks.sprite = listImgRocks[nbrRocks];
        SaveRocks();
    }


    public void SaveRocks()
    {
        RockData rockData = new RockData();
        foreach (var item in islandTiles)
        {
            rockData.rocks.Add(new RockInfo(item.Key.x, item.Key.y, item.Value));
        }

        string json = JsonConvert.SerializeObject(rockData, Formatting.Indented);
        File.WriteAllText(rocksRemainingFilePath, json);
    }

    public void LoadRocks()
    {
        if (File.Exists(rocksRemainingFilePath))
        {
            string json = File.ReadAllText(rocksRemainingFilePath);
            RockData rockData = JsonConvert.DeserializeObject<RockData>(json);

            islandTiles.Clear();
            foreach (var rock in rockData.rocks)
            {
                Vector3Int position = new Vector3Int(rock.x, rock.y, 0); // Assuming Z is always 0 for your tiles
                islandTiles[position] = rock.rockType;
            }

            // Now you can use LoadIslandTiles() or similar to reflect these changes in the game world
            LoadIslandTiles();
        }
    }

    #endregion

}
