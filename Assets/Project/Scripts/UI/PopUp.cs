using TMPro;
using UnityEngine;
public enum PopupStyle
{
    YesNo,
    Ok,
    ConnexionFailed
}
public class PopUp : MonoBehaviour
{
    [SerializeField] private TMP_Text infotext;
    [HideInInspector] public int NONE = 0, YES = 1, CANCEL = 2;
    [HideInInspector] public int result = 0;
    [SerializeField] private GameObject YesNoButtons;
    [SerializeField] private TMP_Text YesButton;
    [SerializeField] private TMP_Text NoButton;
    [SerializeField] private GameObject OkButton;
    [SerializeField] private TMP_Text OkText;
    [SerializeField] private UIAnimation anim;

    public void Oui()
    {
        result = YES;
    }

    public void Non()
    {
        result = CANCEL;
    }

    private void SetText(string text)
    {
        infotext.text = text;
    }

    private void SetPopUpStyle(PopupStyle style)
    {
        switch (style)
        {
            case PopupStyle.Ok:
                YesNoButtons.SetActive(false);
                OkButton.SetActive(true);
                OkText.text = "Ok";
                break;
            case PopupStyle.ConnexionFailed:
                YesNoButtons.SetActive(false);
                OkButton.SetActive(true);
                OkText.text = "Réessayer";
                break;
            case PopupStyle.YesNo:
            default:
                YesNoButtons.SetActive(true);
                YesButton.text = "Oui";
                NoButton.text = "Non";
                OkButton.SetActive(false);
                break;
        }
    }

    public PopUp Instance(string text, PopupStyle style)
    {
        SetText(text);
        SetPopUpStyle(style);
        return Instantiate(this, Database.Instance.errorCanva.transform);
    }

    private async void Start()
    {
        await anim.AnimateFromStartToEndAsync();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Non();
        }
    }

    private bool isDestroy = false;
    public async void Destroy()
    {
        await anim.AnimateFromEndToStartAsync();
        if (!isDestroy) Destroy(gameObject);
    }
    private async void OnDestroy()
    {
        isDestroy = true;
        await anim.CancelAnimation();
    }
}
