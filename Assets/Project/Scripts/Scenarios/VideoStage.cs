public class VideoStage : ScenarioStage
{
    public string URL { get; private set; }
    public string URLEmbed { get; private set; }

    public VideoStage() : base()
    {

    }

    public VideoStage(VideoStage vs) : base(vs)
    {
        URL = vs.URL;
        URLEmbed = vs.URLEmbed;
    }

    public VideoStage(ScenarioStageType type, string title, string url, string urlEmbed, int stageId, int scenarioId) : base(type, title, stageId, scenarioId)
    {
        URL = url;
        if (string.IsNullOrEmpty(urlEmbed))
        {
            URLEmbed = url;
        }
        else
        {
            URLEmbed = urlEmbed;
        }
    }
}
