using UnityEngine;

public class StagePanel : MonoBehaviour
{
    public QuestionPanel questionPanel;
    public AnswerPanel answerPanel;
    public InformationPanel informationPanel;
    [SerializeField] private VideoPanel videoPanel;
    public VideoPanel VideoPanel
    {
        get
        {
            return videoPanel;
        }
    }

    public void FillPanel(Quiz quiz)
    {
        //questionPanel.gameObject.SetActive(false);
        answerPanel.gameObject.SetActive(false);
        informationPanel.gameObject.SetActive(false);
        videoPanel.gameObject.SetActive(false);

        questionPanel.gameObject.SetActive(true);
        questionPanel.FillPanel(quiz, answerPanel);
    }
}
