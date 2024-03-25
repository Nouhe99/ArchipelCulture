using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private Transform gameContentParent;
    [SerializeField] private StagePanel stagePanel;
    [SerializeField] private TeacherTalk helperDialog;
    private UIAnimation helperDialogAnimation;
    private bool helperDialogHidden = true;
    [SerializeField] private int dialogDelayInMilliseconds = 5000;
    //[SerializeField] private Button retryButton;
    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private Canvas mainCanvas;
    public StagePanel StagePanel
    {
        get
        {
            return stagePanel;
        }
    }

    public Scenario ActiveScenario { get; private set; }
    public ScenarioIsle ScenarioIsle { get; private set; }

    [SerializeField] private SceneController sceneController;

    public delegate void RewardOpenedEventHandler();
    public static event RewardOpenedEventHandler OnRewardOpened;
    private bool logoTransitionInProgress = false;

    private bool inRewardScene = false;

    #region Singleton
    public static ScenarioManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    private IEnumerator Start()
    {
        //sound
        PlayAudio.Instance.PlayMusic(PlayAudio.Instance.bank.musicScenario);
        //PlayAudio.Instance.PlayMusic(PlayAudio.Instance.bank.beach, "fx");

        stagePanel.gameObject.SetActive(false);
        if (CrossSceneInformation.SelectedScenario != null)
        {
            ActiveScenario = CrossSceneInformation.SelectedScenario;
            GameObject islePrefab = null;
            yield return StartCoroutine(Database.Instance?.GetPrefabByAddressAsync(ActiveScenario.Isle.Address, value => islePrefab = value));
            if (islePrefab == null)
            {
                yield break;
            }
            ScenarioIsle = Instantiate(islePrefab, gameContentParent).GetComponent<ScenarioIsle>();
            if (ScenarioIsle != null)
            {
                ScenarioIsle.InitializeIsle(ActiveScenario);
                ScenarioIsle.SetActiveChestVFX(ActiveScenario.Successes > 0 && ActiveScenario.RewardReceived == 0);
            }
        }
        if (helperDialog != null)
        {
            helperDialogAnimation = helperDialog.GetComponent<UIAnimation>();
        }
    }

    public void LaunchStage(Quiz quiz)
    {
        stagePanel.gameObject.SetActive(true);
        stagePanel.FillPanel(quiz);
    }

    public void CloseStage()
    {
        ScenarioIsle?.UpdateSpots();
        stagePanel.gameObject.SetActive(false);

        if (ActiveScenario.Tried() && ActiveScenario.Successes == 1 && ActiveScenario.RewardReceived == 1)
        {
            // All Scenario's steps completed without failure
            if (ActiveScenario.Completed())
            {
                // Completed for the first time
                if (ActiveScenario.Successes == 1)
                {
                    PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.celebration);
                    ActiveScenario.Successes++;
                }

            }
            // At least one failure
            else
            {
                ActiveScenario.Failures++;
            }

            // Update scenario state online
            StartCoroutine(Database.Instance?.UpdateStudentScenario(ActiveScenario));

        }

        // All Scenario's steps tried | Scenario finished
        if (ActiveScenario.Tried() && ActiveScenario.Successes == 0 && ActiveScenario.RewardReceived == 0)
        {
            // All Scenario's steps completed without failure
            if (ActiveScenario.Completed())
            {
                // Completed for the first time
                if (ActiveScenario.Successes == 0)
                {
                    PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.celebration);
                    ActiveScenario.Successes++;
                }

            }
            // At least one failure
            else
            {
                ActiveScenario.Failures++;
            }
            // Update database table


            // Update scenario state online
            StartCoroutine(Database.Instance?.UpdateStudentScenario(ActiveScenario));
        }

    }

    public async void HideDialog()
    {
        if (helperDialogAnimation == null)
        {
            return;
        }
        await helperDialogAnimation.AnimateFromEndToStartAsync();
        helperDialogHidden = true;
    }

    public async void ShowDialog(string text)
    {
        if (helperDialogHidden)
        {
            helperDialogHidden = false;
            await helperDialogAnimation.AnimateFromStartToEndAsync();
            if (helperDialog != null)
            {
                //helperDialog.say(text);
            }
            await Task.Delay(dialogDelayInMilliseconds);
            if (helperDialog != null)
            {
               // helperDialog.stop();
                HideDialog();
            }
        }
    }

    public void OpenReward()
    {
        if (ActiveScenario.Successes <= 1 && ActiveScenario.RewardReceived == 0)
        {
            //PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.celebration);
            ScenarioIsle.SetActiveChestVFX(true);
            StartCoroutine(OpenRewardCoroutine());

        }
        else if (ActiveScenario.Successes == 2 && ActiveScenario.RewardReceived == 1)
        {
            ScenarioIsle.SetActiveChestVFX(true);
            StartCoroutine(OpenRewardCoroutine());

        }
        else if (ActiveScenario.Successes == 3 && ActiveScenario.RewardReceived == 2)
        {
            ScenarioIsle.SetActiveChestVFX(true);
            StartCoroutine(OpenRewardCoroutine());

        }
        else
        {
            if (ActiveScenario.Successes <= 0)
            {
                if (helperDialogHidden) PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.fail);
                ShowDialog("Avant de pouvoir ouvrir ce coffre, tu dois répondre correctement à tous les quiz !");
            }
        }
    }



    private IEnumerator OpenRewardCoroutine()
    {

        inRewardScene = true;

        // Notify that the reward has been opened
        OnRewardOpened?.Invoke();


        // Wait for logo transition to complete
        yield return new WaitUntil(() => !logoTransitionInProgress);

        // Load reward scene
        CrossSceneInformation.Rewards = new List<Reward>(ActiveScenario.Rewards);
        yield return sceneController.LoadRewardScene();


        ScenarioIsle.SetActiveChestVFX(false);
        ActiveScenario.RewardReceived++;

        // Wait until the player leaves the reward scene
        yield return new WaitUntil(() => !SceneManager.GetSceneByName(sceneController.RewardScene).isLoaded);

        // Set Restarted
        ActiveScenario.Restarted++;

        // Remove quiz progress for the current scenario
        StartCoroutine(Database.Instance?.RemoveQuizProgress(GetActiveScenarioId()));

        // Call the method to handle actions after leaving the reward scene
        HandleAfterRewardScene();

        // Save and return
        SaveAndReturn();
    }




    public void SaveAndReturn()
    {

        logoTransitionInProgress = true;
        StartCoroutine(SaveAndReturnCoroutine());
    }

    private IEnumerator SaveAndReturnCoroutine()
    {

        yield return StartCoroutine(Database.Instance?.UpdateStudentScenario(ActiveScenario));

        // Check if the player is in the reward scene
        if (inRewardScene)
        {
            // Call the method to handle actions after leaving the reward scene
            HandleAfterRewardScene();
        }

        // Reset the flag after the logo transition is complete
        logoTransitionInProgress = false;

        LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.PreviousScene, "Embarquement immédiat !"));
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !popupOpen)
        {
            StartCoroutine(ReturnToMyIsland());
        }
    }
    private bool popupOpen = false;
    private IEnumerator ReturnToMyIsland()
    {
        popupOpen = true;
        PopUp dialog = Database.Instance.errorDialog.Instance("Quitter l'île ?", PopupStyle.YesNo);
        yield return null;
        while (dialog.result == dialog.NONE)
        {
            yield return null;
        }

        if (dialog.result == dialog.YES)
        {
            SaveAndReturn();
            inRewardScene = false;
        }

        dialog.Destroy();
        popupOpen = false;
    }

    public void HandleAfterRewardScene()
    {
        // Update scenario state online
        StartCoroutine(Database.Instance?.UpdateStudentScenario(ActiveScenario));
    }

    public int GetActiveScenarioId()
    {
        return ActiveScenario != null && int.TryParse(ActiveScenario.ID, out int id) ? id : -1;
    }

}
