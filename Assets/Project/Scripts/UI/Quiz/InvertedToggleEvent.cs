using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InvertedToggleEvent : MonoBehaviour
{
    public UnityEvent<bool> onValueChangedInverse;

    private void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener((on) => { onValueChangedInverse.Invoke(!on); });
    }

    private Color lightblue = new(0.9568628f, 0.9607844f, 1);
    private Color mediumblue = new(0.0509804f, 0.4745098f, 0.9490197f);
    private Color darkblue = new(0.03921569f, 0.07058824f, 0.4509804f);
    private Color green = new Color(0.09411766f, 0.6470588f, 0);
    [SerializeField] private Image caseLine;
    [SerializeField] private Image caseInside;
    [SerializeField] private TMPro.TMP_Text caseText;
    [SerializeField] private TMPro.TMP_Text textAnswer;
    [SerializeField] private Image backgroundAnswer;
    public void ChangeValueUI(Toggle tog)
    {
        if (tog.isOn)
        {
            caseLine.color = Color.white;
            caseInside.color = mediumblue;
            caseText.color = Color.white;
            textAnswer.color = Color.white;
        }
        else
        {
            caseLine.color = darkblue;
            caseInside.color = lightblue;
            caseText.color = darkblue;
            textAnswer.color = darkblue;
        }
    }

    public void SetCorrectColor()
    {
        caseLine.color = Color.white;
        caseInside.color = green;
        caseText.color = Color.white;
        textAnswer.color = Color.white;
        backgroundAnswer.color = green;
    }
}
