[System.Serializable]
public class Notion
{
    public string ID;
    public string Name;
    public string ParentID;

    public Notion GetParent()
    {
        if (Database.Instance != null)
        {
            if (Database.Instance.Notions.ContainsKey(ParentID))
            {
                return Database.Instance.Notions[ParentID];
            }
        }
        return null;
    }

    public Notion()
    {

    }

    public Notion(string id, string name)
    {
        ID = id;
        Name = name;
    }

    public Notion(string id, string name, string parentId) : this(id, name)
    {
        ParentID = parentId;
    }

    public static bool operator ==(Notion notion1, Notion notion2)
    {
        if ((object)notion1 == null)
            return (object)notion2 == null;
        return notion1.Equals(notion2);
    }

    public static bool operator !=(Notion notion1, Notion notion2)
    {
        return !(notion1 == notion2);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var notion2 = (Notion)obj;
        return ID == notion2.ID && Name == notion2.Name && ParentID == notion2.ParentID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode() ^ Name.GetHashCode() ^ ParentID.GetHashCode();
    }
}
