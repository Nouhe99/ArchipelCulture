using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ScenarioPanel : MonoBehaviour
{
    private Scenario selectedScenario;
    [SerializeField] private SceneController sceneController;


    [Header("Scenario Panel")]
    [SerializeField] private TMP_Text scenarioName;
    [SerializeField] private Image scenarioImage;
    [SerializeField] private TMP_Text notion;
    [SerializeField] private GameObject ScenarioAvailable;
    
    public GameObject buttonStart;

    public void SetScenario(Scenario scenario)
    {
        if (scenario != null)
        {
            
            ScenarioAvailable.SetActive(true);
            selectedScenario = scenario;
            FillScenarioPanel(scenario);
        }
        else
        {
            
            ScenarioAvailable.SetActive(false);
        }
    }

    public void LaunchScenario()
    {
        StartCoroutine(LaunchScenarioCoroutine());
    }
    public IEnumerator LaunchScenarioCoroutine()
    {
        if (selectedScenario != null)
        {
            CrossSceneInformation.SelectedScenario = selectedScenario;
            sceneController.LoadScenarioScene();
            yield return StartCoroutine(Database.Instance?.GetQuizzes(selectedScenario));
           
        }
    }


    public void LaunchScenarioWithTransition()
    {
       // LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.ScenarioScene, "Chargement des quiz", Database.Instance.GetQuizzes(selectedScenario)));
        SceneManager.LoadScene(sceneController.ScenarioScene);
        //if (selectedScenario != null)
        //{
        //    CrossSceneInformation.SelectedScenario = selectedScenario;
        //    BoatController.Instance?.SaveMyPosition_ScenarioWorld();
        //    LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(sceneController.ScenarioScene, "Chargement des quiz", Database.Instance.GetQuizzes(selectedScenario)));
        //}
    }

    private void FillScenarioPanel(Scenario scenario)
    {
        // Fill Launch Panel
        if (scenario != null)
        {
            scenarioName.text = scenario.Name;
            if (scenario.PreviewSprite != null)
            {
                scenarioImage.sprite = scenario.PreviewSprite;
            }

            bool hasImage = false;
            if (scenario.Steps != null && scenario.Steps.Count > 0)
            {
                if (scenario.Steps[0].Quizzes != null && scenario.Steps[0].Quizzes.Count > 0)
                {
                    if (scenario.Steps[0].Quizzes[0].QuestionImage != null)
                    {
                        hasImage = true;
                        scenarioImage.sprite = scenario.Steps[0].Quizzes[0].QuestionImage;
                    }
                }
            }
            scenarioImage.enabled = hasImage;
            //notion.text = scenario.Notion.GetParent()?.Name + "\n" + scenario.Notion.Name;
            //difficulty.SetDifficulty(scenario.Difficulty);
        }
    }
}
