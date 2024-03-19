[System.Serializable]
public class Isle
{
    public string Name { get; private set; }
    public int MaxStages { get; private set; }
    public string Address { get; private set; }

    [System.NonSerialized] public AdventureIsle PreviewIsle;

    public Isle(string name, int maxStages, string address)
    {
        Name = name;
        MaxStages = maxStages;
        Address = address;
    }
}
