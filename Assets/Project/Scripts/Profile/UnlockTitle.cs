using System.Collections.Generic;
using UnityEngine;

public class UnlockTitle
{
    //TOREMOVE
    //public class Title
    //{
    //    public string id;
    //    public TitleCategory cat;
    //    public string name;
    //    public string method;
    //    public bool unlock;

    //    public Title(string id, string cat, string name, string method, bool unlock)
    //    {
    //        this.id = id;

    //        if (cat == "Aventure") this.cat = TitleCategory.Aventure;
    //        else if (cat == "Construction") this.cat = TitleCategory.Construction;
    //        else if (cat == "Cadeaux") this.cat = TitleCategory.Cadeaux;
    //        else if (cat == "Magasin") this.cat = TitleCategory.Magasin;
    //        else if (cat == "Anciennete") this.cat = TitleCategory.Anciennete;
    //        else this.cat = TitleCategory.Debloque;

    //        this.name = name;
    //        this.method = method;
    //        this.unlock = unlock;
    //    }
    //}
    //public enum TitleCategory
    //{
    //    Aventure,
    //    Construction,
    //    ConstructionIle,
    //    ConstructionDeco,
    //    ConstructionMaison,
    //    Cadeaux,
    //    Magasin,
    //    Anciennete,
    //    Debloque,
    //    Kraken
    //}

    ////UIManager.current.unlockTitles.Verify(TopFloor, UnlockTitle.TitleCategory.ConstructionMaison);
    //public void Verify(int quantity, TitleCategory category)
    //{
    //    HashSet<string> idToUnlock = new();
    //    switch (category)
    //    {
    //        case TitleCategory.Aventure:
    //            if (quantity >= 1) //(1) Marin d'eau douce
    //                idToUnlock.Add("1");

    //            if (quantity >= 10) //(2) Incollable
    //                idToUnlock.Add("2");

    //            if (quantity >= 25) //(3) Vent en poupe
    //                idToUnlock.Add("3");

    //            if (quantity >= 50) //(4) En haute mer
    //                idToUnlock.Add("4");

    //            if (quantity >= 80) //(5) As du gouvernail
    //                idToUnlock.Add("5");

    //            break;

    //        case TitleCategory.ConstructionIle:
    //            if (quantity >= 96) //(15) La folie des grandeurs
    //                idToUnlock.Add("15");

    //            if (quantity >= 9) //(14) Petit poucet
    //                idToUnlock.Add("14");

    //            break;

    //        case TitleCategory.ConstructionDeco:
    //            if (quantity >= 20) //(8) La main verte
    //                idToUnlock.Add("8");

    //            if (quantity >= 10) //(7) Apprenti
    //                idToUnlock.Add("7");

    //            if (quantity >= 5) //(6) Jeune Pousse
    //                idToUnlock.Add("6");
    //            break;
    //        case TitleCategory.ConstructionMaison:
    //            if (quantity >= 20) //(13) Vers l'infini et l'eau del�
    //                idToUnlock.Add("13");

    //            if (quantity >= 15) //(12) Ing�nieur
    //                idToUnlock.Add("12");

    //            if (quantity >= 10) //(11) Equilibriste
    //                idToUnlock.Add("11");

    //            if (quantity >= 5) //(10) Architecte en herbe
    //                idToUnlock.Add("10");

    //            if (quantity >= 3) //(9) Aux quatre vents
    //                idToUnlock.Add("9");
    //            break;
    //        case TitleCategory.Construction:
    //            Debug.Log("Must select a specific type of construction (island, deco or house ?)");
    //            break;

    //        case TitleCategory.Cadeaux:
    //            if (quantity >= 50) //(18) Coeur sur la main
    //                idToUnlock.Add("18");

    //            if (quantity >= 25) //(17) Pipelette
    //                idToUnlock.Add("17");

    //            if (quantity >= 10) //(16) Merci
    //                idToUnlock.Add("16");
    //            break;

    //        case TitleCategory.Magasin:
    //            if (quantity >= 15) //(19) Econome
    //                idToUnlock.Add("19");

    //            if (quantity >= 50) //(21) Client fidéle
    //                idToUnlock.Add("21");

    //            if (quantity >= 100) //(20) Habitué
    //                idToUnlock.Add("20");

    //            if (quantity >= 200) //(22) Plein aux as
    //                idToUnlock.Add("22");

    //            break;

    //        case TitleCategory.Kraken:
    //            if (quantity >= 1) //(46) Monstrueux
    //                idToUnlock.Add("46");
    //            break;

    //        //NOTE: seniority can't be verify for class version because profil can be created before the student actually play. Otherwise, must keep date of first connection in db ?
    //        case TitleCategory.Anciennete:
    //            if (quantity >= 365) //(25) Vieille branche
    //                idToUnlock.Add("25");

    //            if (quantity >= 30) //(24) Comme d'habitude
    //                idToUnlock.Add("24");
    //            break;

    //        default:
    //            Debug.Log("Cannot find this category.");
    //            break;
    //    }
    //    foreach (string t in idToUnlock)
    //    {
    //        try
    //        {
    //          //  if (!Database.Instance.myTitles[t].unlock) UnlockID(t);
    //        }
    //        catch (KeyNotFoundException e)
    //        {
    //            Debug.LogError("This title " + t + " doesn't exist (" + e.Message + ").");
    //        }
    //    }
    //    //NOTE: si il y a une erreur lors du d�bloquage de plusieurs titres en m�me temps,
    //    //      cela pourrait venir des multirequ�tes. 
    //}

    //private void UnlockID(string id)
    //{
    //   // Database.Instance.SaveUnlockTitle(id);
    //   // UIManager.current.notificationSuccess.Open(Database.Instance.myTitles[id].name, Database.Instance.myTitles[id].method);
    //    foreach (ChangeTitle ct in UIManager.current.profil.titleContent.GetComponentsInChildren<ChangeTitle>())
    //    {
    //        if (ct.id == id)
    //        {
    //            ct.UnlockTitle();
    //            return;
    //        }
    //    }
    //}
}
