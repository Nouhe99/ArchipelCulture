using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/User Data")]
public class UserData : ScriptableObject
{
    [Header("Profil")]
    public AccountType accountType;
    public string id;
    public string code;
    public string username;
    public string profilPicture;
    public byte tutorialStep; //0 mean not started ; 1 mean finished first step ; ...

    public event OnVariableChangeDelegate OnVariableChange = delegate { };
    public delegate void OnVariableChangeDelegate(int gold);
    [SerializeField] private int Gold;
    public int gold
    {
        get => Gold;
        set
        {
            if (value == Gold) return;
            int difference = value - Gold;
            Gold = value;
            OnVariableChange(difference);
        }
    }
    public string title;
    public List<string> codesUsed;

    [Header("Builder")]
    public Vector3Int buildingPlacement;
    public List<ItemPlacement> inventory = new();
    public int rocksRemaining; //rocks availables for building
    //public Vector3Int homePlacement;
    [TextArea(4, 200)]
    public string rockPlacementJson;



    [Header("Stats")]
    public int quizCompleted;
    public int totalGoldSpent;

    public class TilePos
    {
        public int x_pos;
        public int y_pos;
        public int style_tile;
        public TilePos(int posx, int posy, int type)
        {
            x_pos = posx;
            y_pos = posy;
            style_tile = type;
        }
    }

    [System.Serializable]
    public class ItemPlacement
    {
        public string id_inventory;
        public string id;
        public bool placed;
        public int x_pos;
        public int y_pos;
        public int z_pos;
        public int variante;

        public ItemPlacement(string i, bool p, int xp, int yp, int zp, string idinv, int var)
        {
            id_inventory = idinv;
            id = i;
            placed = p;
            x_pos = xp;
            y_pos = yp;
            z_pos = zp;
            variante = var;
        }
    }

    /// <summary>
    /// Remove an item from user inventory if exist. Return null if not found.
    /// </summary>
    public ItemPlacement RemoveItemInInventory(string id_inv)
    {
        ItemPlacement itemFound = null;
        foreach (ItemPlacement item in inventory)
        {
            if (item.id_inventory == id_inv)
            {
                itemFound = item;
                inventory.Remove(item);
                break;
            }
        }
        return itemFound;
    }

    public int GetNumberItemPlaced()
    {
        int numberPlaced = 0;
        foreach (ItemPlacement item in inventory)
        {
            if (item.placed && Database.Instance.itemsList.GetItemData(item.id).type == ItemType.Decoration)
                numberPlaced++;
        }
        return numberPlaced;
    }

    public int GetNumberDecorationInInventory()
    {
        int numberInventory = 0;
        foreach (ItemPlacement item in inventory)
        {
            if (!item.placed && Database.Instance.itemsList.GetItemData(item.id).type == ItemType.Decoration)
                numberInventory++;
        }
        return numberInventory;
    }

    public int GetTotalDecoration()
    {
        int number = 0;
        foreach (ItemPlacement item in inventory)
        {
            if (Database.Instance.itemsList.GetItemData(item.id).type == ItemType.Decoration)
                number++;
        }
        return number;
    }

    public int GetTotalBuildings()
    {
        int number = 0;
        foreach (ItemPlacement item in inventory)
        {
            if (Database.Instance.itemsList.GetItemData(item.id).type == ItemType.Building)
                number++;
        }
        return number;
    }
}
