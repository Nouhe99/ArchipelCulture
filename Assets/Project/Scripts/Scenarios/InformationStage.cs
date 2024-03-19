using UnityEngine;

public class InformationStage : ScenarioStage
{
    public string Description { get; private set; }
    public string ResourceURL { get; private set; }
    public Sprite ResourcePicture { get; private set; }
    public bool ResourceDownloaded { get; set; }

    public InformationStage() : base()
    {

    }

    public InformationStage(ScenarioStageType type, string title, string description, string resourceUrl, Sprite resourcePicture, bool resourceDownloaded, int stageId, int scenarioId) : base(type, title, stageId, scenarioId)
    {
        Description = description;
        ResourceURL = resourceUrl;
        ResourcePicture = resourcePicture;
        ResourceDownloaded = resourceDownloaded;
    }

    //[SerializeField] private Sprite picture;
    //public Sprite ResourcePicture
    //{
    //    get { return picture; }
    //}

    //[SerializeField] private string resourceUrl;
    //public string ResourceUrl
    //{
    //    get { return resourceUrl; }
    //}

    //[SerializeField] private bool resourceDownloaded;
    //public bool ResourceDownloaded
    //{
    //    get { return resourceDownloaded; }
    //    set { resourceDownloaded = value; }
    //}

    //[SerializeField] private string title;
    //public string Title
    //{
    //    get { return title; }
    //}
    //[SerializeField] private string description;

    //public string Description
    //{
    //    get { return description; }
    //}
}
