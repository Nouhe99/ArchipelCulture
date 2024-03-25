using System.Collections.Generic;

[System.Serializable]
public class DataInventory
{
    public List<UserData.ItemPlacement> placedObj = new();
    public List<Item> buyedObj = new(); //list of ids
    public int remainingGold;
    public string jsonRockPlacement;
}