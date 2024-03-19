using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image image;
    [Header("Description")]
    [SerializeField] private TMP_Text descriptionTitle;
    [SerializeField] private TMP_Text descriptionText;
    [Header("Answers")]
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private GridLayoutGroup gridGroup;
    [SerializeField] private RectTransform togglesPanel;
    [SerializeField] private ToggleAnswer toggleAnswerPrefab;
    private List<ToggleAnswer> toggleAnswers = new List<ToggleAnswer>();
    [Header("Button")]
    [SerializeField] private Button submitButton;
    [SerializeField] private GameObject submitButtonSpinner;
    [SerializeField] private Button showAnswerButton;

    private Quiz activeQuiz;
    private AnswerPanel answerPanel;
    private void Start()
    {
        submitButton.GetComponentInChildren<TMP_Text>(true).text = "Valider la réponse";
    }

    public void FillPanel(Quiz quiz, AnswerPanel answerPanel)
    {
        activeQuiz = quiz;
        this.answerPanel = answerPanel;

        submitButtonSpinner.SetActive(false);
        image.sprite = quiz.QuestionImage;
        descriptionTitle.text = quiz.Title;
        descriptionText.text = quiz.Question;
        InitializeAnswers(quiz);

        if (ScenarioManager.Instance.ActiveScenario.Steps[ScenarioManager.Instance.ActiveScenario.GetStepIndexOfQuiz(quiz)].State != QuizState.Completed)
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = false;
            showAnswerButton.gameObject.SetActive(false);
            showAnswerButton.interactable = true;
        }
        else
        {
            LockSubmit();
        }
    }

    private int scenarioId;

    public int GetActiveScenarioId()
    {
        scenarioId = ScenarioManager.Instance.GetActiveScenarioId();
        return scenarioId;
    }


    public void LockSubmit()
    {
        // Lock Buttons
        submitButton.gameObject.SetActive(false);
        submitButton.interactable = false;
        showAnswerButton.gameObject.SetActive(true);
        showAnswerButton.interactable = true;

        // Lock Answers
        foreach (ToggleAnswer toggleAnswer in toggleAnswers)
        {
            toggleAnswer.GetComponent<Toggle>().interactable = false;
            if (toggleAnswer.answer.Correct) toggleAnswer.ShowCorrect();
        }
    }

    private void InitializeAnswers(Quiz quiz)
    {
        for (int i = toggleAnswers.Count; i-- > 0;)
        {
            if (toggleAnswers[i] != null)
            {
                Destroy(toggleAnswers[i].gameObject);
                toggleAnswers.RemoveAt(i);
            }
        }
        // Set gridlayout a responsive cell size
        if (togglesPanel != null)
        {
            Canvas.ForceUpdateCanvases();
            float width = togglesPanel.rect.width;
            float height = togglesPanel.rect.height;
            //Padding 5%
            width -= width * 0.05f;
            height -= height * 0.05f;
            if (gridGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                if (gridGroup.constraintCount != 0)
                {
                    gridGroup.cellSize = new Vector2(width / gridGroup.constraintCount, height / ((float)quiz.Answers.Count / gridGroup.constraintCount));
                }
            }
            else if (gridGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                if (gridGroup.constraintCount != 0)
                {
                    gridGroup.cellSize = new Vector2(width / ((float)quiz.Answers.Count / gridGroup.constraintCount), height / gridGroup.constraintCount);
                }
            }
        }

        // Instantiate Answers
        for (int i = 0; i < quiz.Answers.Count; i++)
        {
            ToggleAnswer toggleAnswer = Instantiate(toggleAnswerPrefab, toggleGroup.transform);
            toggleAnswer.SetAnswer(quiz.Answers[i].Label, i);
            toggleAnswer.answer = quiz.Answers[i];
            toggleAnswers.Add(toggleAnswer);
        }
        // Multiple Correct Answers
        if (quiz.HasMultipleChoice)
        {
            toggleGroup.enabled = false;
            foreach (ToggleAnswer answer in toggleAnswers)
            {
                Toggle toggle = answer.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.group = null;
                    toggle.isOn = false;
                    toggle.onValueChanged.AddListener((on) => { submitButton.interactable = AnyToggleOn(); });
                }
            }
        }
        // Single Correct Answer
        else
        {
            toggleGroup.enabled = true;
            foreach (ToggleAnswer answer in toggleAnswers)
            {
                Toggle toggle = answer.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.group = toggleGroup;
                    toggle.onValueChanged.AddListener((on) => { submitButton.interactable = on; });
                }
            }
            toggleGroup.SetAllTogglesOff();
        }
    }

    public bool AnyToggleOn()
    {
        bool result = false;
        foreach (ToggleAnswer answer in toggleAnswers)
        {
            Toggle toggle = answer.GetComponent<Toggle>();
            if (toggle != null)
            {
                if (toggle.isOn)
                {
                    result = true;
                }
            }
        }
        return result;
    }

    private bool IsAnswerCorrect()
    {
        foreach (ToggleAnswer toggleAnswer in toggleAnswers)
        {
            Toggle toggle = toggleAnswer.GetComponent<Toggle>();
            if (toggle != null)
            {
                if (toggleAnswer.answer.Correct != toggle.isOn)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private IEnumerator SendAnswers()
    {
        submitButtonSpinner.SetActive(true);
        bool correctAnswer = IsAnswerCorrect();

        List<Answer> sentAnswers = new List<Answer>();
        foreach (ToggleAnswer toggleAnswer in toggleAnswers)
        {
            Toggle toggle = toggleAnswer.GetComponent<Toggle>();
            if (toggle != null)
            {
                if (toggle.isOn)
                {
                    sentAnswers.Add(toggleAnswer.answer);
                }
            }
        }

        yield return Database.Instance.StartCoroutine(Database.Instance.UpdateStudentAnswersCoroutine(sentAnswers));
        Database.Instance.StartCoroutine(Database.Instance.UpdateQuizProgress(sentAnswers, GetActiveScenarioId()));
        if (correctAnswer)
        {
            PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.goodAnswer);
            if (ScenarioManager.Instance.ActiveScenario.Steps[ScenarioManager.Instance.ActiveScenario.GetStepIndexOfQuiz(activeQuiz)].State != QuizState.Completed)
            {
                CrossSceneInformation.CompletedQuizSinceLastTime++;
            }
            ScenarioManager.Instance.ActiveScenario.Steps[ScenarioManager.Instance.ActiveScenario.GetStepIndexOfQuiz(activeQuiz)].State = QuizState.Completed;
        }
        else
        {
            PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.badAnswer);
            ScenarioManager.Instance.ActiveScenario.Steps[ScenarioManager.Instance.ActiveScenario.GetStepIndexOfQuiz(activeQuiz)].State = QuizState.Failed;
        }
        int index = ScenarioManager.Instance.ActiveScenario.GetStepIndexOfQuiz(activeQuiz);
        if (index != -1)
        {
            ScenarioManager.Instance.ActiveScenario.Steps[index].Tries++;
        }
        submitButtonSpinner.SetActive(false);
        ShowAnswer();
    }

    public void SubmitAnswer()
    {

        Database.Instance?.StartCoroutine(SendAnswers());

    }

    public void ShowAnswer()
    {
        answerPanel.gameObject.SetActive(true);
        answerPanel.FillPanel(activeQuiz, this,
            ScenarioManager.Instance.ActiveScenario.Steps[ScenarioManager.Instance.ActiveScenario.GetStepIndexOfQuiz(activeQuiz)].State == QuizState.Completed);
        gameObject.SetActive(false);
    }


    /// <returns>String with all the infos for the TTS.</returns>
    public string GetInfos()
    {
        string stringInfos = "";
        //stringInfos += descriptionTitle.text + ". "; 
        stringInfos += descriptionText.text + " ";
        foreach (ToggleAnswer answer in toggleAnswers)
        {
            stringInfos += answer.answer.Label + "? ";
        }
        return stringInfos;
    }
}
