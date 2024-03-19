public enum ScenarioStageType : byte
{
    Quiz = 1,
    Information = 2,
    Video = 3
}
public enum StageState
{
    Pending,
    Completed,
    CompletedSuccessfully
}

public class ScenarioStage
{
    public ScenarioStageType Type { get; private set; }
    public string Title { get; private set; }
    public StageState State { get; set; }
    public int ID { get; private set; }
    //public int ScenarioID { get; private set; }
    public int Order { get; private set; }


    public ScenarioStage()
    {
        State = StageState.Pending;
    }

    public ScenarioStage(ScenarioStage ss)
    {
        Type = ss.Type;
        Title = ss.Title;
        State = ss.State;
        ID = ss.ID;
        //ScenarioID = ss.ScenarioID;
    }

    public ScenarioStage(ScenarioStageType type, string title, int id, int scenarioId)
    {
        Type = type;
        Title = title;
        State = StageState.Pending;
        ID = id;
        //ScenarioID = scenarioId;
    }
}