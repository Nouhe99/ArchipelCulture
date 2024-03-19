using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string Name;
    public string ID;
    public Sprite Sprite;
    public Rarity Rarity;
    public ItemType Type;
    public Style Style;
    public int Buyable;
    public int xSize;
    public int ySize;
    public int zSize;

    public ItemData()
    {
    }

    public ItemData(string id, string name, Sprite sprite, ItemType type, Rarity rarity)
    {
        ID = id;
        Name = name;
        Sprite = sprite;
        Type = type;
        Rarity = rarity;
    }

    public ItemData(string id, string name, Sprite sprite, ItemType type, Rarity rarity, Style style, int buyable, int xSize, int ySize, int zSize) : this(id, name, sprite, type, rarity)
    {
        Style = style;
        Buyable = buyable;
        this.xSize = xSize;
        this.ySize = ySize;
        this.zSize = zSize;
    }
}
