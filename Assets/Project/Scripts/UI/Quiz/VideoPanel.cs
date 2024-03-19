using UnityEngine;

public class VideoPanel : MonoBehaviour
{
    [SerializeField] private WebViewer webViewer;

    private VideoStage activeVideoStage;
    private bool quizVideo;

    public void FillPanel(VideoStage video, bool fromQuiz)
    {
        activeVideoStage = video;
        activeVideoStage.State = StageState.CompletedSuccessfully;
        quizVideo = fromQuiz;
        //videoController.PrepareVideo(activeVideoStage.URL);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_OSX

        OpenURL();
        return;
#else

        webViewer.UrlWebsite = activeVideoStage.URL;
        webViewer.UrlMobile = activeVideoStage.URLEmbed;
        ScenarioManager.Instance?.StartCoroutine(webViewer.StartVideo());
        //CloseVideo();
#endif
    }

    public void OpenURL()
    {
        if (activeVideoStage.URL != "")
            Application.OpenURL(activeVideoStage.URL);
        else
            Application.OpenURL(activeVideoStage.URLEmbed);

        gameObject.SetActive(false);

    }

    private void OnEnable()
    {
        if (PlayAudio.Instance != null)
        {
            PlayAudio.Instance.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (PlayAudio.Instance != null)
        {
            PlayAudio.Instance.gameObject.SetActive(true);
        }
    }
}
