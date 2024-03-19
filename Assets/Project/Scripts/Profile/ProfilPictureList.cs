using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ProfilPicturesList")]
public class ProfilPictureList : ScriptableObject
{
    public List<ItemObjectData> pictureReferences;

    public Sprite GetPicture(string id)
    {
        if (id == null) return null;
        return GetProfilPictureData(id).Sprite;
    }

    public ItemData GetProfilPictureData(string id)
    {
        if (id == null) return null;
        foreach (ItemObjectData item in pictureReferences)
        {
            if (item.datas.ID == id) return item.datas;
        }
        return pictureReferences[0].datas; //return first in list if picture cannot be found.
    }
}