using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarPanel : MonoBehaviour
{
    #region Avatar
    [Header("Profil")]
    public Image avatarImage;
    public DotNotification dotNotification;
    #endregion

    #region Logout
    [Header("Logout")]
    [SerializeField] private SceneController sceneController;

   

    public void ExitApp()
    {
        Application.Quit();
    }
    #endregion

    #region Code
    [Header("Code")]
    [SerializeField] private TMP_InputField inputCode;

    public void SubmitCode()
    {
        if (string.IsNullOrEmpty(inputCode.text)) return;
        StartCoroutine(SubmitCodeCoroutine());
    }

    private IEnumerator SubmitCodeCoroutine()
    {
        List<Reward> codeRewards = new();
        List<string> scenariosUnlocked = new();
        int resultCode = 0;
     //   yield return StartCoroutine(Database.Instance?.SubmitCode(inputCode.text, (result, rewards, scenarios) => { resultCode = result; codeRewards = rewards; scenariosUnlocked = scenarios; }));

        CleanCode();

        // Code not working for whatever the reason
        if (resultCode != 1)
        {
            string text = "";
            if (resultCode == 0) text = "Code invalide";
            bool choice;
            yield return StartCoroutine(UIManager.ShowConfirmationDialog(text, PopupStyle.Ok, value => choice = value));
            yield break;
        }
        // Code working
        // Scenario unlocked
        if (scenariosUnlocked.Count > 0)
        {
            foreach (string scenarioName in scenariosUnlocked)
            {
                UIManager.ShowInfoMessage($"Tu as débloqué le scénario {scenarioName}");
                yield return new WaitForSeconds(1f);
            }
        }

        // Reward item animation
        if (codeRewards.Count > 0)
        {
            CrossSceneInformation.Rewards = new List<Reward>(codeRewards);
            sceneController.LoadRewardScene();
        }
    }

    public void CleanCode()
    {
        inputCode.text = "";
    }

    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel;

    public void CloseSettingWindow()
    {
        TriggerSettingsPanel();
        UIManager.current.currentOpenPanel = UIManager.Panel.NULL;
    }
    public async void TriggerSettingsPanel()
    {
        PlayAudio.Instance?.bank.PressButtonReturn();
        foreach (WindowLink item in windowSettings)
        {
            if (item.panel.activeInHierarchy)
                await ClosePanelAsync(item.panel, false);
        }
        await TriggerPanelAsync(settingsPanel);
        CheckButtonsState();
    }
    public async void TriggerPanel(GameObject panel)
    {
        await TriggerPanelAsync(panel);
        CheckButtonsState();
    }
    #region Sound Settings
    [SerializeField] private Toggle soundTog;
    [SerializeField] private Toggle musicTog;

    public void NoMusic()
    {
        PlayAudio.Instance.EnableMusic(musicTog.isOn);
        PlayerPrefs.SetInt("MusicPref", musicTog.isOn ? 1 : 2); //0 is default preference
        for (int i = 0; i < musicTog.graphic.transform.childCount; i++)
        {
            musicTog.graphic.transform.GetChild(i).gameObject.SetActive(musicTog.isOn);
        }
    }

    public void NoSound()
    {
        PlayAudio.Instance.EnableSfx(soundTog.isOn);
        PlayerPrefs.SetInt("SoundPref", soundTog.isOn ? 1 : 2); //0 is default preference
        for (int i = 0; i < soundTog.graphic.transform.childCount; i++)
        {
            soundTog.graphic.transform.GetChild(i).gameObject.SetActive(soundTog.isOn);
        }
    }

    #endregion

    #endregion

    #region Open & Close Panels
    private async Task ClosePanelAsync(GameObject panel, bool animate = true)
    {
        if (panel.activeInHierarchy)
        {
            if (animate)
            {
                List<Task> tasks = new();
                if (panel.GetComponent<UIAnimation>())
                    tasks.Add(panel.GetComponent<UIAnimation>().AnimateFromEndToStartAsync());
                foreach (UIAnimation item in panel.GetComponentsInChildren<UIAnimation>(false))
                {
                    if (item != null) tasks.Add(item.AnimateFromEndToStartAsync());
                }

                await Task.WhenAll(tasks); //wait animation to be finished before close menu
            }
            panel.SetActive(false);
        }
    }

    private async Task CloseAllPanelsAsync(bool animate = true)
    {
        List<Task> tasks = new List<Task>();
        if (settingsPanel != null)
        {
            tasks.Add(ClosePanelAsync(settingsPanel, animate));
        }
        await Task.WhenAll(tasks);
    }

    [Serializable]
    public struct WindowLink
    {
        public GameObject panel;
        public Image buttonImage;
    }
    [SerializeField] private List<WindowLink> windowSettings = new();
    [SerializeField] private Color buttonHighlightColor;
    [SerializeField] private Color buttonNormalColor = Color.white;
    private void CheckButtonsState()
    {
        foreach (WindowLink item in windowSettings)
        {
            if (item.panel.activeInHierarchy)
            {
                item.buttonImage.color = buttonHighlightColor;
            }
            else
            {
                item.buttonImage.color = buttonNormalColor;
            }
        }
    }
    private async Task TriggerPanelAsync(GameObject panel)
    {
        if (!panel.activeInHierarchy)
        {
            List<Task> tasks = new List<Task>();
            //open panel
            panel.SetActive(true);
            if (panel.GetComponent<UIAnimation>())
                tasks.Add(panel.GetComponent<UIAnimation>().AnimateFromStartToEndAsync());
            foreach (UIAnimation item in panel.GetComponentsInChildren<UIAnimation>(false))
            {
                if (item != null) tasks.Add(item.AnimateFromStartToEndAsync());
            }

            //close the other ones
            if (panel.transform.parent.gameObject == settingsPanel)
            {
                foreach (WindowLink item in windowSettings)
                {
                    if (item.panel.activeInHierarchy && item.panel != panel)
                        tasks.Add(ClosePanelAsync(item.panel));
                }
            }
            else if (settingsPanel != null && settingsPanel != panel)
            {
                tasks.Add(ClosePanelAsync(settingsPanel));
            }
            await Task.WhenAll(tasks);
        }
        else
        {
            await ClosePanelAsync(panel);
            //await CloseAllPanelsAsync();
        }
    }
    #endregion

    private async void Start()
    {
        //InitSettings(); //postprocess values
;

        //sound buttons
        if (soundTog != null) soundTog.onValueChanged.AddListener(delegate { NoSound(); });
        if (musicTog != null) musicTog.onValueChanged.AddListener(delegate { NoMusic(); });
        if (PlayerPrefs.GetInt("MusicPref") == 2) musicTog.isOn = false;
        if (PlayerPrefs.GetInt("SoundPref") == 2) soundTog.isOn = false;

        //avatarName.text = Database.Instance?.userData.namePlayer;
      //  if (avatarImage != null) avatarImage.sprite = Database.Instance.profilPictures.GetPicture(Database.Instance.userData.profilPicture);
        await CloseAllPanelsAsync(false);
        if (dotNotification != null)
        {
            dotNotification.gameObject.SetActive(false);
            HandleNotification();
        }

        // Check first time
        if (Database.Instance?.userData.tutorialStep == 0 && helpPanel != null)
        {
            UIManager.ShowFullScreenDialog($"Bienvenue!\r\n\r\n\r\nPour commencer, tu peux regarder le tutoriel en appuyant sur <size=200%><sprite=0></size> puis sur <size=200%><sprite=1></size>\r\n\r\n\r\n<size=80%><i>Continuer...</i></size> ");
            Database.Instance.userData.tutorialStep++;
          //  Database.Instance.StartCoroutine(Database.Instance.UpdateTutorialProgression());
        }
    }

    #region Dot Notification
    public void HandleNotification()
    {
        StartCoroutine(HandleNotificationCoroutine());
    }

    [HideInInspector] public int nbOfRewardsWaiting = 0;
    [SerializeField] private FloatingReward floatingRewardPrefab;
    [SerializeField] private Sprite progressSprite;
    private IEnumerator HandleNotificationCoroutine()
    {
        if (Database.Instance == null) yield break;

        yield return StartCoroutine(Database.Instance.UpdateNumberOfCompletedQuiz());
        nbOfRewardsWaiting = 0;
        foreach (LevelReward lvlReward in Database.Instance.LevelRewards)
        {
            if (lvlReward.Acquired == false && lvlReward.RewardReached(Database.Instance.userData.quizCompleted))
            {
                // Reward that can be acquired
                nbOfRewardsWaiting++;
            }
        }
        dotNotification.SetNotificationCount(nbOfRewardsWaiting);
        if (nbOfRewardsWaiting > 0)
        {
            UIManager.ShowBigNotification("Une nouvelle récompense est disponible !\nTu peux la récupérer depuis ton profil");
        }
    }

    public void CheckFloatingReward()
    {
        if (CrossSceneInformation.CompletedQuizSinceLastTime > 0)
        {
            Instantiate(floatingRewardPrefab, transform).PlayRewardAnimation(UIManager.current.boatObject.transform, CrossSceneInformation.CompletedQuizSinceLastTime, UIManager.current.buttonProfil.transform, progressSprite);

            CrossSceneInformation.CompletedQuizSinceLastTime = 0;
        }
    }
    #endregion

    #region Capture
    [SerializeField] private GameObject photo;
    internal void PermissionCallbacks__PermissionGranted(string permissionName)
    {
        // Authorization granted
        StartCoroutine(CaptureCountdown());
    }
    public void Capture()
    {
        if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            // Authorization granted
            StartCoroutine(CaptureCountdown());
        }
        else
        {
            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted += PermissionCallbacks__PermissionGranted;
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite, callbacks);
        }

    }
    private WaitForSeconds wait1Second = new(1);
    private IEnumerator CaptureCountdown()
    {

        UIManager.current.CloseCurrentPanel();
        UIManager.current.HideAllButtons(true);

        UIManager.ShowInfoMessage("<size=300%>3</size>");
        PlayAudio.Instance.bank.PressButtonBip();
        yield return wait1Second;
        UIManager.ShowInfoMessage("<size=300%>2</size>");
        PlayAudio.Instance.bank.PressButtonBip();
        yield return wait1Second;
        UIManager.ShowInfoMessage("<size=300%>1</size>");
        PlayAudio.Instance.bank.PressButtonBip();
        yield return wait1Second;
        UIManager.ShowInfoMessage("");


        yield return new WaitForEndOfFrame();

        //animation
        Texture2D image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        image.Apply();
        photo.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(image, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
        Instantiate(photo, UIManager.current.mainCanvas.transform);
        PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.photoSaved);

        //save image
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        string temp = Application.persistentDataPath.Replace("/AppData/LocalLow/Archipel/Archipel", "/Downloads");
#elif UNITY_ANDROID
        //packagename = com.arc.wearepirate.com.Archipelv2
        string temp = Application.persistentDataPath.Replace("/Android/data/com.arc.wearepirate.com.Archipelv2/files", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures));
#elif UNITY_IOS
        string temp = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures); //TO DO: see if this work.
#else
        string temp = Application.persistentDataPath + "/Photos";
        if (!System.IO.Directory.Exists(temp))
            System.IO.Directory.CreateDirectory(temp);
#endif
        var bytes = image.EncodeToPNG();
        //var file = new FileStream(temp + "/Ile_" + Database.Instance.userData.username + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png", FileMode.Create, FileAccess.Write);
       // var binary = new BinaryWriter(file);
       // binary.Write(bytes);
       // file.Close();
        //ScreenCapture.CaptureScreenshot(temp + "/Ile_" + Database.Instance.userData.namePlayer + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
        yield return new WaitForSeconds(0.15f);

        UIManager.current.HideAllButtons(false);
        UIManager.ShowInfoMessage("Image enregistrée dans" + " " + temp);

    }
    #endregion
}
