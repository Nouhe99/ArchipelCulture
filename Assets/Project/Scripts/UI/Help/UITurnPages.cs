using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITurnPages : MonoBehaviour
{
    [SerializeField] private Button buttonLeft;
    [SerializeField] private Button buttonRight;
    [SerializeField] private List<GameObject> panelList = new();
    private int currentPageOpen = 0;
    [SerializeField] private TMPro.TMP_Text numberPageText;

    // Start is called before the first frame update
    void Start()
    {
        SetText();
        buttonLeft.onClick.AddListener(() => TurnPageLeft());
        buttonRight.onClick.AddListener(() => TurnPageRight());
    }
    private void TurnPageLeft()
    {
        panelList[currentPageOpen].SetActive(false);
        if (currentPageOpen == 0)
        {
            currentPageOpen = panelList.Count - 1;
        }
        else
        {
            currentPageOpen--;
        }
        panelList[currentPageOpen].SetActive(true);
        SetText();
        PlayAudio.Instance.PlayOneShot(PlayAudio.Instance.bank.closePage);
    }
    private void TurnPageRight()
    {
        panelList[currentPageOpen].SetActive(false);
        if (currentPageOpen == panelList.Count - 1)
        {
            currentPageOpen = 0;
        }
        else
        {
            currentPageOpen++;
        }
        panelList[currentPageOpen].SetActive(true);
        SetText();
        PlayAudio.Instance.PlayOneShot(PlayAudio.Instance.bank.openPage);
    }

    private void SetText()
    {
        numberPageText.text = (currentPageOpen + 1) + "/" + panelList.Count;
    }
}
