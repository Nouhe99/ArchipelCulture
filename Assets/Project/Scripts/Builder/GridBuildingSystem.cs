using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handler for item placement.
/// </summary>
public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem current;

    [Header("Tilemaps")]
    public GridLayout gridLayout;
    public Tilemap mainTilemap; //show where you can place elements
    public Tilemap tempTilemap; //show if the element can be placed where it is
    public Tilemap takenTilemap; //tiles where there is already an object

    [Header("Tiles")]
    [SerializeField] private TileBase validBoxTile;
    [SerializeField] private TileBase greenTile;
    [SerializeField] private TileBase redTile;
    [SerializeField] private TileBase invisibleTileItem;
    [SerializeField] private TileBase invisibleTileFloor;
    public static readonly Dictionary<TileType, TileBase> tileBases = new();

    [Header("Item selection")]
    public ReplaceableOnMap objSelected;

    [Header("Others")]
    public GameObject smokeParticles;

    public Vector3Int prevPos;
    private BoundsInt prevArea;

    #region Unity Methods

    private void Awake()
    {
        current = this;
        if (!tileBases.ContainsKey(TileType.Empty))
        {
            tileBases.Add(TileType.Empty, null);
            tileBases.Add(TileType.Invalid, redTile);
            tileBases.Add(TileType.Valid, greenTile);
            tileBases.Add(TileType.TakenByItem, invisibleTileItem);
            tileBases.Add(TileType.TakenByFloor, invisibleTileFloor);
            tileBases.Add(TileType.ValidBox, validBoxTile);
        }
    }

    #endregion

    public void Select(ReplaceableOnMap selec)
    {
        if (objSelected != null) objSelected.PlaceIt();
        objSelected = selec;
        objSelected.placed = false;

        StartCoroutine(selec.Following());
    }

    /// <summary>
    /// Deselect an object and place it, or remove it if cannot be placed.
    /// </summary>
    public void DeselectObj()
    {
        ClearArea();
        if (objSelected == null) return;
        objSelected.PlaceIt();
        objSelected = null;
    }

    #region TileMap Management
    /// <summary>
    /// Get array of Tiles in the corresponding <b>area</b> of <b>tilemap</b>
    /// </summary>
    /// <param name="area"></param>
    /// <param name="tilemap"></param>
    /// <returns>array of tiles contained in the area</returns>
    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    public static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    /// <summary>
    /// Fill the table with tiles of type <i>TileType</i>
    /// </summary>
    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            try
            {
                arr[i] = tileBases[type];
            }
            catch (KeyNotFoundException e) { Debug.LogError("Cannot fill tiles with type " + type + " : " + e.Message); }
        }
    }

    /// <summary>
    /// Clear the <b>tempTilemap</b> which contain the green, red, and valid tiles
    /// </summary>
    public void ClearArea()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.Empty);
        tempTilemap.SetTilesBlock(prevArea, toClear);
    }

    public void HighlightGrid(Item i)
    {
        HighlightGrid(i.area, i.type);
    }

    public void HighlightGrid(BoundsInt buildingArea, ItemType iType)
    {
        if (iType != ItemType.Decoration && iType != ItemType.Rock && iType != ItemType.Building) return;

        ClearArea();
        HighlightValidPositions();

        buildingArea.size = new Vector3Int(buildingArea.size.x, buildingArea.size.y, 1); //highlight only on the first row

        TileBase[] baseArray = GetTilesBlock(buildingArea, mainTilemap);
        TileBase[] baseArray2 = GetTilesBlock(buildingArea, takenTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];
        List<TileBase> tilesInArea = new();
        for (int j = 0; j < size; j++)
        {
            if (!tilesInArea.Contains(baseArray2[j]))
            {
                tilesInArea.Add(baseArray2[j]);
            }
            if(iType == ItemType.Decoration && baseArray[j] != null && baseArray2[j] != tileBases[TileType.TakenByItem] && baseArray2[j] != tileBases[TileType.TakenByFloor] && tilesInArea.Count < 2)
            {
                tileArray[j] = tileBases[TileType.Valid];
            }
            else if(iType == ItemType.Building && baseArray[j] != null && baseArray2[j] != tileBases[TileType.TakenByItem] && tilesInArea.Count < 2)
            {
                tileArray[j] = tileBases[TileType.Valid];
                ItemFloor itemFloor = GetFloorAtMousePosition();
                if(itemFloor != null)
                {
                    return;
                }
            }
            else if(iType == ItemType.Rock && baseArray[j] == null && IslandBuilder.current.placeholderTilemap.HasTile(buildingArea.position))
            {
                tileArray[j] = tileBases[TileType.Valid];
            }
            else
            {
                FillTiles(tileArray, TileType.Invalid);
                break;
            }
        }

        tempTilemap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;
    }

    public void HighlightValidPositions()
    {
        Tilemap rockTilemap = IslandBuilder.current.islandTilemap;
        List<Vector3Int> rockPositions = new List<Vector3Int>();

        for (int x = rockTilemap.origin.x; x < rockTilemap.size.x; x++)
        {
            for (int y = rockTilemap.origin.y; y < rockTilemap.size.y; y++)
            {
                if (rockTilemap.HasTile(new Vector3Int(x, y, 0)) && !takenTilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    rockPositions.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        Vector3Int[] validArray = rockPositions.ToArray();
        TileBase[] tiles = new TileBase[validArray.Length];
        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tileBases[TileType.ValidBox];
        }

        tempTilemap.SetTiles(validArray, tiles);
    }

    public void HighlightFloorsRoof(bool highlight = true)
    {
        ItemFloor[] floors = UIManager.current.itemContener.GetComponentsInChildren<ItemFloor>();
        if (highlight)
        {
            foreach (ItemFloor floor in floors)
            {
                if (floor.selectionImage != null)
                {
                    if(floor.floorOnTop == null)
                    {
                        floor.selectionImage.SetActive(true);
                    }
                    else
                    {
                        floor.selectionImage.SetActive(false);
                    }
                }
            }
        }
        else
        {
            foreach (ItemFloor floor in floors)
            {
                if (floor.selectionImage != null)
                {
                    floor.selectionImage.SetActive(false);
                }
            }
        }

    }

    public void MaskItemType(ItemType type)
    {
        Item[] items = UIManager.current.itemContener.GetComponentsInChildren<Item>();

        foreach (var item in items)
        {
            if (item.type == type)
            {
                foreach (var sr in item.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    sr.color = new Color(255, 255, 255, 0.25f);

                }

                item.GetCollider().enabled = false;
                
            }
            else
            {
                foreach (var sr in item.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    sr.color = new Color(255, 255, 255, 1f);

                }

                item.GetCollider().enabled = true;
            }
        }
    }

    public bool ItemCanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, takenTilemap);
        foreach (var tileMain in baseArray)
        {
            if (tileMain == tileBases[TileType.TakenByFloor])
            {
                Debug.Log("Cannot place here (already an object) in " + area + " because " + tileMain);
                return false;
            }
            if (tileMain == tileBases[TileType.TakenByItem])
            {
                Debug.Log("Cannot place here (already an object) in " + area + " because " + tileMain);
                return false;
            }
        }

        baseArray = GetTilesBlock(area, mainTilemap);
        foreach (var tileMain in baseArray)
        {
            if (tileMain == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool FloorCanTakeArea(BoundsInt area)
    {
        // Check floor at mouse position
        if (GetFloorAtMousePosition() != null)
        {
            return true;
        }

        // Check area
        TileBase[] baseArray = GetTilesBlock(area, takenTilemap);
        List<TileBase> tilesInArea = new List<TileBase>();
        foreach (var tileMain in baseArray)
        {
            if (!tilesInArea.Contains(tileMain))
            {
                tilesInArea.Add(tileMain);
            }
            if (tileMain == tileBases[TileType.TakenByItem])
            {
                Debug.Log("Cannot place here (already an object) in " + area + " because " + tileMain);
                return false;
            }
        }
        if(tilesInArea.Count > 1)
        {
            // Different type of tiles in area
            return false;
        }

        baseArray = GetTilesBlock(area, mainTilemap);
        foreach (var tileMain in baseArray)
        {
            if (tileMain == null)
            {
                return false;
            }
        }

        return true;
    }

    public bool AreaHaveRocks(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, mainTilemap);
        foreach (var tileMain in baseArray)
        {
            if (tileMain == null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Set the area to taken
    /// </summary>
    public void TakeArea(TileType tileType, BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, tempTilemap);
        SetTilesBlock(area, tileType, takenTilemap);
    }

    /// <summary>
    /// Set the item area to Empty
    /// </summary>
    public void FreeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, takenTilemap);
    }

    #endregion

    #region Item Placement

    public GameObject InitializedWithItem(InventorySlot inventoryblock)
    {
        if (inventoryblock.item.Type is not ItemType.Rock
            and not ItemType.Building
            and not ItemType.Decoration)
        {
            Debug.Log(inventoryblock.item.Name + " cannot be instantiate this way ! (type: " + inventoryblock.item.Type + ")");
            return null;
        }

        if (inventoryblock.item.Type == ItemType.Rock)
        {
            Debug.LogWarning("Create Rock");
         //   InventoryUI.current.userData.islandTile.m_TilingRules[0].m_GameObject; //tile gameObject
            return Instantiate(UIManager.GetGroundRule(1).m_DefaultGameObject, //TO DO : temporary number
                                        gridLayout.CellToWorld(IslandBuilder.current.islandTilemap.WorldToCell(UIManager.GetMousePos())),
                                        Quaternion.identity,
                                        IslandBuilder.current.gameObject.transform);
        }

        GameObject tempObj = UIManager.current.itemsList.GetItem(inventoryblock.item.ID);
        if (tempObj)
        {
            //if (inventoryblock.item.Type == ItemType.Building) tempObj = Instantiate(tempObj, UIManager.GetMousePos() - (Vector3.up * 0.5f), Quaternion.identity, UIManager.current.floorContener);
            //else tempObj = Instantiate(tempObj, UIManager.GetMousePos(), Quaternion.identity, UIManager.current.itemContener.transform);
            tempObj = Instantiate(tempObj, UIManager.GetMousePos(), Quaternion.identity, UIManager.current.itemContener.transform);
        }
        else
        {
            Debug.LogWarning(inventoryblock.inventory_id.Peek() + "(type:" + inventoryblock.item.Type + ") This prefab's item doesn't exist. You must create one, or review the code of item creation (in ReplacableOnMap).");
        }
        return tempObj;
    }

    public void RemoveById(string id_inventory, bool placeItInInventory = true)
    {
        Item thisItem = null;
        foreach (Item it in UIManager.current.itemContener.GetComponentsInChildren<Item>(false))
        {
            if (it.id_inventory != id_inventory) continue;
            thisItem = it;
            break;
        }
        if (thisItem == null) { Debug.LogWarning("This item doesn't exist."); return; }

        thisItem.RemoveFromTilemap();
        if (placeItInInventory)
        {
            thisItem.ReplaceInInventory();
        }
        else 
        {
            Destroy(thisItem.gameObject); //NOTE: destroy item from scene but don't update in database.
        }
    }

    #endregion
    public ItemFloor GetFloorAtMousePosition()
    {
        Vector2 clickpos = UIManager.GetMousePos();
        Collider2D[] hits = Physics2D.OverlapPointAll(clickpos);
        foreach (Collider2D hit in hits)
        {
            ItemFloor itemFloor;
            if ((itemFloor = hit.GetComponent<ItemFloor>()) != null)
            {
                return itemFloor;
            }
        }
        return null;
    }
}

public enum TileType
{
    Empty,
    White,
    Valid,
    TakenByItem,
    TakenByFloor,
    Plus,
    Minus,
    Invalid,
    ValidBox,
}

