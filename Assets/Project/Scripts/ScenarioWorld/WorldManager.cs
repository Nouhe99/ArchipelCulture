using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private AdventureIsle islePrefab;
    [SerializeField] private List<Transform> islePositions;
    private List<AdventureIsle> isles;
    [SerializeField] private AstarPath pathfinder;
    [SerializeField] private ScenarioPanel scenarioPanel;
    [SerializeField] private int graphSize;

    private QuestManager questManager;
    public static WorldManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        scenarioPanel.gameObject.SetActive(false);
        questManager = GetComponent<QuestManager>();
    }

    private void Start()
    {
        if (Database.Instance == null)
        {
            return;
        }
        //sound
        PlayAudio.Instance.PlayMusic(PlayAudio.Instance.bank.musicAdventure);
        //PlayAudio.Instance.PlayMusic(PlayAudio.Instance.bank.boat, "fx");
        int scenarioInstantiated = 0;
        foreach (Scenario scenario in Database.Instance.Scenarios)
        {
            if (scenarioInstantiated >= islePositions.Count)
            {
                // TODO : Find a way to never come here
                Debug.LogError("Could not add this isle, not enough position available");
                continue;
            }
            if (scenario.Locked == false)
            {
                InstantiateScenario(islePositions[scenarioInstantiated], scenario);
                scenarioInstantiated++;
            }
        }

        pathfinder.Scan();
    }

    private void InstantiateScenario(Transform islePosition, Scenario scenario)
    {
        isles = new List<AdventureIsle>();

        if (scenario.Isle == null || scenario.Isle.PreviewIsle == null)
        {
            Debug.LogError("Could not create this isle, not found in assets");
            return;
        }

        AdventureIsle advIsle = Instantiate(scenario.Isle.PreviewIsle, new Vector3(islePosition.position.x, islePosition.position.y), Quaternion.identity, transform);

        // Assign scenario to the isle
        advIsle.Scenario = scenario;

        // Set completion state based on scenario.Successes
        AdventureIsle.CompletionState completionState = AdventureIsle.CompletionState.NotCompleted;

        // Update completion state based on CurrentCompletions
        if (scenario.CurrentCompletions >= 1)
        {
            if (scenario.CurrentCompletions == 1 && scenario.RewardReceived == 1)
            {
                completionState = AdventureIsle.CompletionState.CompletedOnce;
            }
            else if (scenario.CurrentCompletions == 2 && scenario.RewardReceived == 2)
            {
                completionState = AdventureIsle.CompletionState.CompletedTwice;
            }
            else if (scenario.CurrentCompletions >= 3 && scenario.RewardReceived >= 3)
            {
                completionState = AdventureIsle.CompletionState.CompletedThrice;
            }
        }

        // Update completion state and sprite in AdventureIsle
        advIsle.UpdateCompletionState(completionState);

        isles.Add(advIsle);

        // If scenario not done successfully, enable quest
        if (scenario.CurrentCompletions <= 0)
        {
            questManager.AddSideQuest("Quête n°" + (questManager.Quests.Count + 1), $"{"Aller à l'aventure"} <b>{scenario.Name}</b>", advIsle.transform.position);

            // New Scenario
            if (scenario.Failures <= 0)
            {
                advIsle.EnableQuestIndicator();
            }
            // Scenario already tried but failed
            else
            {
                advIsle.EnableFailedIndicator();
            }
        }
        else
        {
            // If scenario done successfully but reward not received, enable chest
            if (scenario.RewardReceived == 0)
            {
                advIsle.EnableChestIndicator();
            }
            // If scenario done successfully and reward received, enable success
            else
            {
                advIsle.EnableSuccessIndicator();
            }
        }
    }


    [SerializeField] private SceneController sceneController;
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
        PopUp dialog = Database.Instance.errorDialog.Instance("Retourne sur mon île", PopupStyle.YesNo); // instantiate the UI dialog box
        yield return null;
        while (dialog.result == dialog.NONE)
        {
            yield return null; // wait
        }

        if (dialog.result == dialog.YES)
        {
            LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.HomeScene, $"Retour au bercail"));
        }

        dialog.Destroy();
        popupOpen = false;
    }

    public void TriggerScenarioPanel(Scenario scenario)
    {
        scenarioPanel.gameObject.SetActive(!scenarioPanel.gameObject.activeInHierarchy);

        if (scenarioPanel.gameObject.activeInHierarchy)
        {
            if (!scenario.Locked)
            {
                scenarioPanel.SetScenario(scenario);
            }
            else
            {
                scenarioPanel.SetScenario(null);
            }
        }
    }

    public void EnableScenarioPanel(Scenario scenario)
    {
        scenarioPanel.gameObject.SetActive(true);
        if (!scenario.Locked)
        {
            scenarioPanel.SetScenario(scenario);
        }
        else
        {
            scenarioPanel.SetScenario(null);
        }
    }

    public void DisableScenarioPanel()
    {
        scenarioPanel.gameObject.SetActive(false);
    }

}
