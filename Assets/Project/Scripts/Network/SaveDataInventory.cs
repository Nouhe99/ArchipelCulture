using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Numerics;
using UnityEditorInternal.Profiling.Memory.Experimental;
using System;
using static UnityEditor.Progress;

public class SaveDataInventory : MonoBehaviour
{
    public static SaveDataInventory Instance;
    //TEMPORARY : rock placements save is in Userdata.rockPlacementJson

    [SerializeField] private DataInventory inventoryData = new();

    public DataInventory GetDataInventory()
    {
        return inventoryData;
    }

    public void ResetInventory()
    {
      //  inventoryData.buyedObj.Clear();
        inventoryData.placedObj.Clear();
        SaveInventoryToLocal();
        
    }
    public void SaveInventoryToLocal()
    {
        string json = JsonUtility.ToJson(inventoryData, true);
        string filePath = Application.persistentDataPath + "inventoryData.json";
        File.WriteAllText(filePath, json);

    }

    public void LoadInventoryFromLocal()
    {
        string filePath = Application.persistentDataPath + "inventoryData.json";
        if (File.Exists(Path.Combine(filePath)))
           
        {
            string json = File.ReadAllText(filePath);
            inventoryData = JsonUtility.FromJson<DataInventory>(json);
        }
        else
        {
            inventoryData = new DataInventory();
            Debug.Log("SaveInventoryToLocal creating");
        }
    }

    public void UpdateItemBuyDatabaseLocal(Item purchasedItem)
    {
        string newInventoryId = GenerateNewInventoryId();

        // Create a new ItemPlacement for the purchased item.
        UserData.ItemPlacement newItemPlacement = new UserData.ItemPlacement(
            purchasedItem.id, // ID of the item.
            false, // Assuming the item is not placed immediately upon purchase.
            0, 0, 0, // Initial position set to 0,0,0 as it's not yet placed.
            newInventoryId, // The newly generated unique inventory ID.
            0); // Variant, assuming default variant for simplicity.

        // Add the new ItemPlacement to the user's inventory.
        Database.Instance.userData.inventory.Add(newItemPlacement);

        //foreach (Item item in inventoryData.buyedObj)
        //{
        //    string idTest = GenerateNewInventoryId();
        //      UserData.ItemPlacement newItemPlacement = new UserData.ItemPlacement(item.id, item.placed, 0, 0, 0, idTest, 0);
        //    idTest = item.id_inventory;
        //    Database.Instance.userData.inventory.Add(newItemPlacement);
            

        //    Debug.Log("item  " + item.id_inventory);
        //    Debug.Log("generating  " + GenerateNewInventoryId());


        //}

        // Clear the list of bought items as they've been processed
       // inventoryData.buyedObj.Clear();

        // Save the updated inventory data to local storage
        SaveInventoryToLocal();
    }

    private string GenerateNewInventoryId()
    {
        // Implement logic to generate a unique ID for new inventory items
        return System.Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

       // LoadInventoryFromLocal();
    }


    #region Save temp memory

    public void AddPlacedObj(string newid, int newposx, int newposy, int newposz, string id_inv, bool newplaced = true, int variante = 0)
    {
        if (!newplaced) { newposx = 0; newposy = 0; newposz = 0; }
        bool foundInInventory = false;

       // var items = Database.Instance.userData.inventory.FirstOrDefault(i => i.id_inventory == newid);

       


        foreach (UserData.ItemPlacement item in Database.Instance.userData.inventory)
        {
            if (item.id_inventory.Equals(id_inv, StringComparison.OrdinalIgnoreCase))
            {
                item.placed = newplaced;
                item.x_pos = newposx;
                item.y_pos = newposy;
                item.z_pos = newposz;
                item.variante = variante;
                foundInInventory = true;
                SaveInventoryToLocal();
                break;
            }
        }
        if (!foundInInventory)
        {
            Debug.LogError($"Item with inventory ID {id_inv} not found in inventory.");
            return; // Exit if we don't find the item in inventory, as something went wrong.
        }

        // Check if the item already exists in the placedObj list.
        var existingPlacedItem = inventoryData.placedObj.FirstOrDefault(obj => obj.id_inventory == id_inv);
        if (existingPlacedItem != null)
        {
            // If already in placedObj, update its position and placed status.
            existingPlacedItem.placed = newplaced;
            existingPlacedItem.x_pos = newposx;
            existingPlacedItem.y_pos = newposy;
            existingPlacedItem.z_pos = newposz;
            existingPlacedItem.variante = variante;
            Debug.Log("Placed item updated in placedObj list.");
        }
        else
        {
            // If not in placedObj, add it with the current status and position.
            inventoryData.placedObj.Add(new UserData.ItemPlacement(newid, newplaced, newposx, newposy, newposz, id_inv, variante));
            Debug.Log("New placed item added to placedObj list.");
        }

        ////find if item already exist in list
        //foreach (var obj in inventoryData.placedObj)
        //{
        //    if (obj.id_inventory == id_inv && obj.id == newid)
        //    {
        //        obj.placed = newplaced;
        //        obj.x_pos = newposx;
        //        obj.y_pos = newposy;
        //        obj.z_pos = newposz;
        //        obj.variante = variante;
        //        Debug.Log("placedy");
        //        return;
        //    }
        //}
        ////else, create a new one
        //inventoryData.placedObj.Add(new UserData.ItemPlacement(newid, newplaced, newposx, newposy, newposz, id_inv, variante));
        //UpdateItemAsPlaced(newid, newposx, newposy, newposz, newplaced);
        //Debug.Log("placedx");
    }
    public void RemovePlacedObj(string id_inv)
    {
        foreach (UserData.ItemPlacement obj in inventoryData.placedObj)
        {
            if (obj.id_inventory == id_inv) //compare id from table Student_Item_Inventory. It is unique for each item.
            {
                inventoryData.placedObj.Remove(obj);
                return;
            }
        }
        Debug.LogWarning("This object cannot be remove from list because it doesn't contain it.");
    }

    public void AddBuyObj(Item newid)
    {
       // inventoryData.buyedObj.Add(newid);
        // Database.Instance.userData.inventory.Add(newid);
        Debug.Log(newid + "id buy");
       


    }

    public void AddRockSave(int posx, int posy, int numTile)
    {
        if (RockExist(posx, posy) >= 0) return;

        JArray jsonRock = JArray.Parse(Database.Instance.userData.rockPlacementJson);
        JObject newRock = new(new JProperty("x_pos", posx), new JProperty("y_pos", posy), new JProperty("style_tile", numTile));
        jsonRock.Add(newRock);
        Database.Instance.userData.rockPlacementJson = jsonRock.ToString();
    }
    public void RemoveRockSave(int posx, int posy)
    {
        int posRock = RockExist(posx, posy);
        if (posRock < 0) return;

        JArray jsonRock = JArray.Parse(Database.Instance.userData.rockPlacementJson);
        jsonRock.RemoveAt(posRock);
        Database.Instance.userData.rockPlacementJson = jsonRock.ToString();
    }

    /// <returns>Position in array if exist, else -1. Don't compare rock type.</returns>
    private int RockExist(int posx, int posy)
    {
        JArray jsonRock = JArray.Parse(Database.Instance.userData.rockPlacementJson);
        for (int i = 0; i < jsonRock.Count; i++)
        {
            if (jsonRock[i]["x_pos"] + "" == posx + ""
                && jsonRock[i]["y_pos"] + "" == posy + "") return i;
        }
        return -1;
    }



    public void UpdateItemAsPlaced(string itemId, int posX, int posY, int posZ, bool placed)
    {
        // Update the placed object list with the new position and placed status
        foreach (var obj in inventoryData.placedObj)
        {
            if (obj.id == itemId) // Assuming 'id' is the identifier. Adjust as necessary.
            {
                obj.placed = true;
                obj.x_pos = posX;
                obj.y_pos = posY;
                obj.z_pos = posZ;
                break; // Assuming each item is unique, break once found and updated.
            }
        }

        // Update the inventory list to mark the item as placed
        foreach (var item in Database.Instance.userData.inventory)
        {
            if (item.id == itemId) // Match the item based on the identifier
            {
                item.placed = true;
                // Assuming you want to update the position in the inventory list as well
                item.x_pos = posX;
                item.y_pos = posY;
                item.z_pos = posZ;
                break; // Assuming each item is unique, break once found and updated.
            }
        }

        // Optionally, save the updated inventory to local storage if needed
        SaveInventoryToLocal();
    }

    #endregion

    #region Save On Database

    //TEMPORARY : must make a combined request
    /* public IEnumerator UpdateItemBuyDatabase()
     {
         WWWForm form = new();
         form.AddField("id", Database.Instance.userData.id);
         form.AddField("coinsRemaining", Database.Instance.userData.gold);
         for (int i = 0; i < inventoryData.buyedObj.Count; i++)
         {
             form.AddField("newitem_id[" + i + "]", inventoryData.buyedObj[i].id);
         }
         form.AddField("goldSpent", Database.Instance.userData.totalGoldSpent);

         string url = Database.Instance.API_ROOT + "/datas/set-new-items.php";
         UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
         webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
         yield return webRequest.SendWebRequest();

         // Process datas
         if (webRequest.result != UnityWebRequest.Result.Success)
         {
             Debug.LogError("Save items bought failed : " + webRequest.error);
             //TODO: keep it in memory for loading it later, instead of a error message.
             if (webRequest.result == UnityWebRequest.Result.ConnectionError)
             {
                 Debug.Log("Network error has occured: " + webRequest.GetResponseHeader(""));
                 bool result = false;
                 yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
                 if (result)
                 {
                     yield return StartCoroutine(UpdateItemBuyDatabase());
                     yield break;
                 }
             }
         }
         else
         {
             string[] items = webRequest.downloadHandler.text.Split(Database.Instance.rowSeparator);
             items = items.SkipLast(1).ToArray();
             webRequest.Dispose();
             foreach (var item in items)
             {
                 string[] subs = item.Split(Database.Instance.columnSeparator); //0:item id, 1:inventory id

                 //Adding to inventory
                 Item refitem = UIManager.current.itemsList.GetItemData(subs[0]);
                 refitem.id_inventory = subs[1];
                 Database.Instance.userData.inventory.Add(new UserData.ItemPlacement(refitem.id, false, 0, 0, 0, refitem.id_inventory, 0));
                 if (FindObjectOfType<UIManager>()) UIManager.current.inventoryUI.AddNewSlot(refitem);
             }
             inventoryData.buyedObj.Clear();
         }

         webRequest.Dispose();
     }*/

    //TEMPORARY
    /* public IEnumerator UpdateItemPlacedDatabase()
         //inventoryData.placedObj
     {
         WWWForm form = new();
         form.AddField("id", Database.Instance.userData.id);
         form.AddField("rocksJson", Database.Instance.userData.rockPlacementJson);
         form.AddField("rocksRemaining", Database.Instance.userData.rocksRemaining);

         for (int i = 0; i < inventoryData.placedObj.Count; i++)
         {
             form.AddField("newitem_id[" + i + "]", inventoryData.placedObj[i].id);
             form.AddField("newitem_idInv[" + i + "]", inventoryData.placedObj[i].id_inventory);
             form.AddField("newitem_placed[" + i + "]", inventoryData.placedObj[i].placed == true ? "1" : "0");
             form.AddField("newitem_xpos[" + i + "]", (inventoryData.placedObj[i].x_pos + "").Replace(",", "."));
             form.AddField("newitem_ypos[" + i + "]", (inventoryData.placedObj[i].y_pos + "").Replace(",", ".")); //NOTE : careful with commas, can be sources of errors.
             form.AddField("newitem_zpos[" + i + "]", (inventoryData.placedObj[i].z_pos + "").Replace(",", "."));
             form.AddField("newitem_variante[" + i + "]", inventoryData.placedObj[i].variante);
         }

         string url = Database.Instance.API_ROOT + "/datas/set-item-placement.php";
         UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
         webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
         yield return webRequest.SendWebRequest();

         // Process datas
         if (webRequest.result != UnityWebRequest.Result.Success)
         {
             Debug.LogError("Save items placed failed : " + webRequest.error);

             //TODO: keep it in memory for loading it later, instead of an error message.
             if (webRequest.result == UnityWebRequest.Result.ConnectionError)
             {
                 Debug.Log("Network error has occured: " + webRequest.GetResponseHeader(""));
                 bool result = false;
                 yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
                 if (result)
                 {
                     yield return StartCoroutine(UpdateItemPlacedDatabase());
                     yield break;
                 }
             }
         }
         else
         {
             inventoryData.placedObj.Clear();
         }

         webRequest.Dispose();
     }
    */
    #endregion

}

[System.Serializable]
public class DataInventory
{
    public List<UserData.ItemPlacement> placedObj = new();
   // public List<Item> buyedObj = new(); //list of ids
    public int remainingGold;
    public string jsonRockPlacement;
}