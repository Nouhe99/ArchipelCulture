using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerPanel : MonoBehaviour
{
    private const string ANSWERSEPARATOR = ", ";
    [Header("Image")]
    [SerializeField] private Image image;
    [Header("Explanation")]
    [SerializeField] private TMP_Text explanationTitle;
    [SerializeField] private TMP_Text explanationText;
    [Header("Links")]
    [SerializeField] private GameObject linkPanel;
    [SerializeField] private Transform linkParent;
    [SerializeField] private Button linkButtonPrefab;
    private List<Button> linkButtons = new List<Button>();
    [Header("Texts")]
    [SerializeField] private TMP_Text nextButtonText;

    private Quiz activeQuiz;
    private QuestionPanel questionPanel;
    private void Start()
    {
        nextButtonText.text = "Suivant";
    }

    public void FillPanel(Quiz quiz, QuestionPanel questionPanel, bool success = false)
    {
        activeQuiz = quiz;
        this.questionPanel = questionPanel;
        linkParent.gameObject.SetActive(true);
        image.sprite = quiz.CorrectionImage;
        if (success)
        {
            explanationTitle.text = "<font-weight=700>Super !</font-weight><br><font-weight=500><size=75%>C'est la bonne réponse !</size></font-weight>";
            explanationTitle.transform.parent.GetComponent<Image>().color = new Color(0.09411766f, 0.6470588f, 0);
        }
        else
        {
            explanationTitle.text = "<font-weight=700>Bien essayé !</font-weight><br><font-weight=500><size=75%>Mais ce n'est pas la bonne réponse.</size></font-weight>";
            explanationTitle.transform.parent.GetComponent<Image>().color = new Color(0.7764707f, 0.2156863f, 0.003921569f);
        }
        explanationText.text = quiz.Correction;
        InitializeLinks(quiz);
    }

    public string GetAnswerSentence(QuizStage quiz)
    {
        string sentence = "";
        if (quiz.State == StageState.CompletedSuccessfully)
        {
            sentence += "Correct !\n\n";
        }
        else
        {
            sentence += "Incorrect !\n\n";
        }
        string correctAnswers = "";
        foreach (Answer answer in quiz.Answers)
        {
            if (answer.Correct)
            {
                if (correctAnswers != "")
                {
                    correctAnswers += ANSWERSEPARATOR;
                }
                correctAnswers += answer.Label;
            }
        }
        if (quiz.MultipleChoice)
        {
            sentence += $"{"Les bonnes réponses sont :"} <b>{correctAnswers}</b>\n";
        }
        else
        {
            sentence += $"{"La bonne réponse est :"} <b>{correctAnswers}</b>\n";
        }
        return sentence;
    }

    private void InitializeLinks(Quiz quiz)
    {
        // Clear already present videos if needed
        for (int i = linkButtons.Count; i-- > 0;)
        {
            if (linkButtons[i] != null)
            {
                Destroy(linkButtons[i].gameObject);
                linkButtons.RemoveAt(i);
            }
        }
        // Instantiate Link buttons
        if (!string.IsNullOrEmpty(quiz.VideoUrl))
        {
            Button link = Instantiate(linkButtonPrefab, linkParent);
            linkButtons.Add(link);
            string url = quiz.VideoUrl;
            link.onClick.AddListener(delegate { OpenQuizVideo(); });
        }
        linkPanel.SetActive(!string.IsNullOrEmpty(quiz.VideoUrl));
    }

    public void OpenQuizVideo()
    {
        ScenarioManager.Instance?.StagePanel?.VideoPanel?.gameObject.SetActive(true);
        ScenarioManager.Instance?.StagePanel?.VideoPanel?.FillPanel(new VideoStage(ScenarioStageType.Quiz, activeQuiz.Title, activeQuiz.VideoUrl, activeQuiz.VideoUrl, int.Parse(activeQuiz.ID), 0), true);
    }

    public void Continue()
    {
        ScenarioManager.Instance?.CloseStage();
    }

    public void ShowQuestion()
    {
        questionPanel.gameObject.SetActive(true);
        questionPanel.LockSubmit();
        gameObject.SetActive(false);
    }

    /// <returns>String with all the infos for the TTS.</returns>
    public string GetInfos()
    {
        return explanationText.text;
    }
}
