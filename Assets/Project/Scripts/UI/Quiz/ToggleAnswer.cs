using TMPro;
using UnityEngine;

public class ToggleAnswer : MonoBehaviour
{
    [SerializeField] private TMP_Text numberTxt;
    [SerializeField] private TMP_Text answerTxt;
    public Answer answer { get; set; }

    public void SetAnswer(string answerText, int i = 0)
    {
        answerTxt.text = answerText;
        numberTxt.text = ((NumToLetter)i).ToString();
    }

    private enum NumToLetter
    {
        A, B, C, D, E, F, G, H, I, J
    }

    public void ShowCorrect()
    {
        if (TryGetComponent(out InvertedToggleEvent togEve))
        {
            GetComponent<UnityEngine.UI.Toggle>().SetIsOnWithoutNotify(true);
            togEve.SetCorrectColor();
        }
    }
}

