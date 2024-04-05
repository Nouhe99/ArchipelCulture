using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public enum ItemType
{
    Building,
    Decoration,
    Boat,
    Rock,
    Resource,
    ImageProfil,
    Title,
    Ground,
    NULL
}

[System.Serializable]
public class Item : ReplaceableOnMap
{
    public List<SpriteRenderer> spriteMirror;
    public string id;
    [HideInInspector] public string id_inventory;
    [Header("Appearance")]
    public string nameItem;
    //public ItemType type; (hérité)
    public Rarity rarity;
    public Style style;
    public Size size;
    public int variante;
    private int currentVariantIndex = 0;
    [Header("Placement")]
    public BoundsInt area; //size z should be !=0
    [Header("Shop")]
    public int price;
    [Header("Sfx")]
    public AudioClip soundClick;

    




    protected Collider2D thisCollider;
    public Collider2D GetCollider() { return thisCollider; }

    #region Unity Methods
    public override string ToString()
    {
        return nameItem + " (ITEM: id:" + id + ", id_inventory:" + id_inventory + ", type:" + type + ", rarity:" + rarity.name + ", style:" + style.Label + ", price:" + price + ")\n(PLACEMENT: placed:" + placed + ", area" + area + ")";
    }

    private void Start()
    {
        //calculate placement depending of size
        if (type == ItemType.Rock)
        {
            if (GridBuildingSystem.current.objSelected != gameObject)
                placed = true; //tiles updated by the selection of the one next to it.
        }

        //add collider
        thisCollider = gameObject.GetComponent<Collider2D>();
        if (thisCollider == null)
        {
            if (spriteRenderer != null && spriteRenderer.gameObject != gameObject)
            {
                SpriteRenderer temp = gameObject.AddComponent<SpriteRenderer>();
                temp.sprite = spriteRenderer.sprite;
                thisCollider = gameObject.AddComponent<PolygonCollider2D>();
                Destroy(temp);
            }
            else
                thisCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

    }

    public ItemData Data()
    {
        ItemData data = new();
        return CopyItem(this, data);
    }
    #endregion

    #region Build Methods

    protected override bool CanBePlaced()
    {
        if (MouseOverMenu(UIManager.current.inventoryUI.gameObject))
        { //over ui, cannot be placed
            return false;
        }

        Vector3Int positionInt = GridBuildingSystem.current.prevPos;
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;
        return GridBuildingSystem.current.ItemCanTakeArea(areaTemp);

    }

    protected override void Place()
    {
        Vector3Int positionInt = GridBuildingSystem.current.prevPos;
        Instantiate(GridBuildingSystem.current.smokeParticles, GridBuildingSystem.current.gridLayout.CellToWorld(positionInt), GridBuildingSystem.current.smokeParticles.transform.rotation, gameObject.transform);
        gameObject.transform.localPosition = (Vector3)((Vector2)GridBuildingSystem.current.gridLayout.CellToLocalInterpolated(positionInt)) + new Vector3(0, 0, positionInt.x + positionInt.y - 1);
        PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.placeItem);
        placed = true;
        thisCollider.enabled = true;
    
        if (type == ItemType.Decoration)
        {
            BoundsInt areaTemp = area;
            areaTemp.position = positionInt;
            GridBuildingSystem.current.TakeArea(TileType.TakenByItem, areaTemp);
            SaveDataInventory.Instance.AddPlacedObj(id, area.position.x, area.position.y, area.position.z, id_inventory);

            //verify new title
        }
    }

    #endregion

    #region Replacement

    
    public override void Follow()
    {
        Vector2 touchPos = UIManager.GetMousePos();
        Vector3Int cellPos = IslandBuilder.current.placeholderTilemap.LocalToCell(touchPos);
        GridBuildingSystem builder = GridBuildingSystem.current;
        transform.localPosition = (Vector3)((Vector2)builder.gridLayout.CellToLocalInterpolated(cellPos)) + new Vector3(0, 0, cellPos.x + cellPos.y - 1);

        if (builder.prevPos != cellPos)
        {
            builder.prevPos = cellPos;

            area.position = builder.prevPos;
            builder.HighlightGrid(area, type);
        }
    }

    protected override void SpecificSelect()
    {
        if (UIManager.current.inventoryUI.menuSelection.GetFirstActiveToggle().name != "Deco - Toggle") return;

        GridBuildingSystem.current.FreeArea(area); //reinitialize tile
        GridBuildingSystem.current.Select(this);
        GridBuildingSystem.current.prevPos = area.position;
        GridBuildingSystem.current.HighlightGrid(this);
    }

    public bool IsMirrorActive()
    {
        foreach (SpriteRenderer sr in spriteMirror)
        {
            if (sr.gameObject.activeInHierarchy)
            {
                return true;
            }

        }
        return false;
    }
    public void UnactiveMirror()
    {
        foreach (SpriteRenderer sr in spriteMirror)
        {
            sr.gameObject.SetActive(false);

        }

    }

    protected override void SpecificClick()
    {

        if (placed == true && UIManager.current.inventoryUI.sideBarOpen && UIManager.current.inventoryUI.menuSelection.GetFirstActiveToggle().name == "Deco - Toggle" && spriteMirror != null)
        {
            
            PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.placeItem);
            if ((variante == 0 || variante <= spriteMirror.Count - 1) && spriteMirror.Count > 0) //Activate mirrors 
            {
                UnactiveMirror();
                variante++;
                spriteMirror[variante - 1].gameObject.SetActive(true);
                spriteRenderer.gameObject.SetActive(false);
                SaveDataInventory.Instance.AddPlacedObj(id, area.position.x, area.position.y, area.position.z, id_inventory, true, variante);
            }
            else
            {
                UnactiveMirror();
                variante = 0;
                spriteRenderer.gameObject.SetActive(true);
                SaveDataInventory.Instance.AddPlacedObj(id, area.position.x, area.position.y, area.position.z, id_inventory, true, variante);
            }
        }
        else if (placed == true && !UIManager.current.inventoryUI.sideBarOpen)
        {
            PlayAudio.Instance.PlayOneShot(soundClick);
        }
      //  SaveDataInventory.Instance.SaveInventory();
    }

    /// <summary>
    /// Remove item from scene and place it in inventory.
    /// </summary>
    public override void ReplaceInInventory()
    {
        UIManager.current.inventoryUI.AddNewSlot(this);
        SaveDataInventory.Instance.AddPlacedObj(id, 0, 0, 0, id_inventory, false);
        StartCoroutine(RemoveAnimation());



      //  SaveDataInventory.Instance.SaveInventory();
      //  SaveDataInventory.Instance.SavePlacedObjects();

    }

    public override void RemoveFromTilemap()
    {
        GridBuildingSystem.current.FreeArea(area);
    }

    #endregion

    #region Sprite
    public void Setimage(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public Sprite GetSprite()
    {
        if (IsMirrorActive() && spriteMirror.Count > 0 && variante >= 0 && variante < spriteMirror.Count)
        {
            return spriteMirror[variante].sprite;
        }
        else if (spriteRenderer != null)
        {
            return spriteRenderer.sprite;
        }
        else if (gameObject.transform.Find("Profil - Image").TryGetComponent(out Image i)) //in shop
        {
            return i.sprite;
        }
        return null;
    }
    
   public void CycleSprites(int direction)
{
    if (spriteMirror != null && spriteMirror.Count > 0)
    {
        int newVariantIndex = (variante + direction) % spriteMirror.Count;
        if (newVariantIndex < 0)
        {
            newVariantIndex = spriteMirror.Count - 1;
        }
        variante = newVariantIndex;
    }
}

    #endregion

    #region Static Methods

    public static ItemType GetTypeFromString(string type)
    {
        if (type == "Building") return ItemType.Building;
        else if (type == "Decoration") return ItemType.Decoration;
        else if (type == "Boat") return ItemType.Boat;
        else if (type == "Rock") return ItemType.Rock;
        else if (type == "Resource") return ItemType.Resource;
        else if (type == "PhotoProfil") return ItemType.ImageProfil;
        else if (type == "Titre") return ItemType.Title;
        Debug.LogWarning("There is an item with a type not allowed : " + type);
        return ItemType.NULL;
    }
    public static string GetStringFromType(ItemType type)
    {
        if (type == ItemType.Building) return "Building";
        else if (type == ItemType.Decoration) return "Decoration";
        else if (type == ItemType.Boat) return "Boat";
        else if (type == ItemType.Rock) return "Rock";
        else if (type == ItemType.Resource) return "Resource";
        else if (type == ItemType.ImageProfil) return "PhotoProfil";
        else if (type == ItemType.Title) return "Titre";
        Debug.LogWarning("There is an item with a type not allowed : " + type);
        return null;
    }

    public static void CopyItem(Item copy, Item to)
    {
        to.id = copy.id;
        to.name = copy.name;
        to.nameItem = copy.nameItem;
        to.type = copy.type;
        to.area = copy.area;
        to.rarity = copy.rarity;
        to.style = copy.style;
        to.price = copy.price;
        to.size = copy.size;
        to.variante = copy.variante;
        to.currentVariantIndex = copy.currentVariantIndex;
        to.spriteRenderer = copy.spriteRenderer;
        to.spriteMirror = copy.spriteMirror;
        to.id_inventory = copy.id_inventory;
        to.placed = copy.placed;
    }

    public static void CopyItem(ItemData copy, Item to)
    {
        to.id = copy.ID;
        to.name = copy.Name;
        to.nameItem = copy.Name;
        to.Setimage(copy.Sprite);
        to.type = copy.Type;
        if (copy.zSize >= 1) to.area.size = new(copy.xSize, copy.ySize, copy.zSize);
        else to.area.size = new(copy.xSize, copy.ySize, 1);
        to.rarity = copy.Rarity;
        to.style = copy.Style;
        to.price = copy.Buyable;
    }

    public static ItemData CopyItem(Item copy, ItemData to)
    {
        to.ID = copy.id;
        to.Name = copy.nameItem;
        to.Sprite = copy.GetSprite();
        to.Type = copy.type;
        to.xSize = copy.area.size.x;
        to.ySize = copy.area.size.y;
        to.zSize = copy.area.size.z;
        to.Rarity = copy.rarity;
        to.Style = copy.style;
        to.Buyable = copy.price;
        return to;
    }

    #endregion

}