using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassProfileCard : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text cardNumberTMP;
    [SerializeField] private TMP_Text usernameTMP;

    public int CardIndex;
    public int ID;
    public string Title;
    private CanvasGroup canvasGroup;

     public void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public float Alpha
    {
        get { return canvasGroup.alpha; }
        set { canvasGroup.alpha = value; }
    }

    public Sprite Picture
    {
        get
        {
            return image.sprite;
        }
        set
        {
            image.sprite = value;
        }
    }

    public string CardNumber
    {
        get
        {
            return cardNumberTMP.text;
        }
        set
        {
            cardNumberTMP.text = value;
        }
    }

    public string Username
    {
        get { return usernameTMP.text; }
        set { usernameTMP.text = value; }
    }
}
