using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour, IPointerMoveHandler, IPointerClickHandler
{
    public Slider Slider;
    public Button PlayButton;
    public Button PauseButton;

    public Image ControllerOverlay;

    public bool HideCoroutineRunning = false;
    public float HideTimer = 3f;
    private float timer = 0f;

    public VideoPlayer MyVideoPlayer;

    public bool VideoLoaded = false;

    public void PrepareVideo(string url)
    {
        MyVideoPlayer.source = VideoSource.Url;
        MyVideoPlayer.url = url;
        MyVideoPlayer.waitForFirstFrame = true;
        MyVideoPlayer.playOnAwake = true;
        MyVideoPlayer.isLooping = false;
        MyVideoPlayer.prepareCompleted += SetupControls;
        MyVideoPlayer.Prepare();
    }

    public void SetupControls(VideoPlayer videoPlayer)
    {
        Slider.onValueChanged.AddListener(TrySkip);
        MyVideoPlayer.StepForward();
        Play();
    }

    private void Update()
    {
        if (MyVideoPlayer.isPrepared /*&& VideoPlayer.frameCount > 0*/)
        {
            if (Input.touchCount <= 0 || !Input.GetMouseButtonDown(0))
            {
                Slider.value = (float)NTime;
            }
        }
    }

    public void TrySkip(float nTime)
    {
        nTime = Mathf.Clamp01(nTime);
        MyVideoPlayer.time = nTime * Duration;
    }

    public double VideoTime
    {
        get { return MyVideoPlayer.time; }
    }

    public ulong Duration
    {
        get { return (ulong)(MyVideoPlayer.frameCount / MyVideoPlayer.frameRate); }
    }

    //Time of the video between 0 and 1
    public double NTime
    {
        get { return VideoTime / Duration; }
    }

    public void Play()
    {
        PlayButton.interactable = false;
        PlayButton.gameObject.SetActive(false);

        PauseButton.interactable = true;
        PauseButton.gameObject.SetActive(true);

        MyVideoPlayer.Play();
    }

    public void Pause()
    {
        PauseButton.interactable = false;
        PauseButton.gameObject.SetActive(false);

        PlayButton.interactable = true;
        PlayButton.gameObject.SetActive(true);

        MyVideoPlayer.Pause();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        timer = HideTimer;
        if (!ControllerOverlay.gameObject.activeInHierarchy)
        {
            ControllerOverlay.gameObject.SetActive(true);
        }
        if (!HideCoroutineRunning)
        {
            StartCoroutine(HideAfterTimer());
        }
    }

    private IEnumerator HideAfterTimer()
    {
        HideCoroutineRunning = true;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        ControllerOverlay.gameObject.SetActive(false);
        HideCoroutineRunning = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        timer = HideTimer;
        if (!ControllerOverlay.gameObject.activeInHierarchy)
        {
            ControllerOverlay.gameObject.SetActive(true);
        }
        if (!HideCoroutineRunning)
        {
            StartCoroutine(HideAfterTimer());
        }
    }
}
