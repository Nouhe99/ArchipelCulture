using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TextContent
{

    public TextContent(int id, string key, string comment, string frContent, string enContent)
    {
        ID = id;
        Key = key;
        Comment = comment;
        FRcontent = frContent;
        //CAcontent = cAcontent;
        ENcontent = enContent;
    }
    public int ID { get; set; }
    public string Key { get; set; }
    public string Comment { get; set; }
    public string FRcontent { get; set; }
    //public string CAcontent { get; set; }
    public string ENcontent { get; set; }
}
public enum Language
{
    French,
    Canadan,
    English
}

public class Library
{
    public readonly Dictionary<int, TextContent> _dataList = new();
    public bool isLoad = false;

    public Library()
    {
        AsyncOperationHandle<TextAsset> loadOp = Addressables.LoadAssetAsync<TextAsset>("Archipel_RemoteTexts");
        loadOp.Completed += handle =>
        {
            Addressables.Release(handle);
            string[] rows = loadOp.Result.text.Split("\n");
            rows = rows.Skip(1).ToArray();
            foreach (var line in rows)
            {
                string[] values = line.Split(';');
                if (int.TryParse(values[0], out int id))
                {
                    _dataList.Add(id, new TextContent(id, values[1], values[2], values[3], values[4]));
                }
            }
            isLoad = true;
        };
    }
}

public static class ExternalTexts
{
    private static readonly Library library = new();
    private static Language language = Language.French; 

    public static string GetContent(string refName)
    {
        foreach (TextContent item in library._dataList.Values)
        {
            if (item.Key == refName) return GetContent(item.ID);
        }
        return "###";
    }

    public static string GetContent(int id)
    {
        if (!library._dataList.ContainsKey(id)) return "###";
        TextContent temp = library._dataList[id];
        if (language == Language.English && !string.IsNullOrEmpty(temp.ENcontent))
            return temp.ENcontent.Replace("\\n", "\n").Replace("\\r", "\r");
        return temp.FRcontent.Replace("\\n", "\n").Replace("\\r", "\r");
    }

    public static bool IsLoaded()
    {
        return library.isLoad;
    }
}
