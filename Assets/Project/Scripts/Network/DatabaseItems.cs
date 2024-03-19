using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Items")]
public class DatabaseItems : ScriptableObject
{
    [SerializeField] private List<GameObject> itemsReferences;
    public readonly Dictionary<string, GameObject> items = new();

    public void UpdateDictionnary()
    {
        foreach (GameObject item in itemsReferences)
        {
            if (!GetItem(item.GetComponent<Item>().id)) items.Add(item.GetComponent<Item>().id, item);
        }
    }

    private void OnEnable()
    {
        UpdateDictionnary();
    }

    public GameObject GetItem(string id)
    {
        if (id == null) return null;
        if (items.ContainsKey(id)) return items[id];
        return null;
    }

    public Item GetItemData(string id)
    {
        if (id == null) return null;
        if (items.ContainsKey(id)) return items[id].GetComponent<Item>();
        return null;
    }

    public Item GetRandomObjectOfRarityAndType(Rarity rarity, ItemType type) //can only win object that can be buy (no chest, no special items)
    {
        Dictionary<int, Item> listItem = new();
        int i = 0;
        foreach (GameObject item in itemsReferences)
        {
            Item it = item.GetComponent<Item>();
            if (it.rarity == rarity && it.type == type && it.price > 0)
            {
                listItem.Add(i, it);
                i++;
            }
        }
        if (listItem.Count < 1) return null;
        return listItem[RandomNumber(0, listItem.Count - 1)];
    }
    private readonly RandomNumberGenerator rng = new RNGCryptoServiceProvider();
    /// <summary>
    /// Random int between min and max value.
    /// </summary>
    private int RandomNumber(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException();
        }

        var data = new byte[sizeof(uint)];
        rng.GetBytes(data);
        double rngvalue = (BitConverter.ToUInt32(data, 0) / (uint.MaxValue + 1.0));
        return (int)Math.Floor((minValue + ((double)maxValue - minValue) * rngvalue));
    }

}
