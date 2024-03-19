using UnityEngine;
using UnityEngine.UI;

public class ChangeTitle : MonoBehaviour
{

    [HideInInspector] public string id;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text subtitleText;
    public Button button;

    public void Change()
    {
        if (Database.Instance.userData.title != titleText.text)
        {
            Database.Instance.userData.title = titleText.text;
            UIManager.current.profil.titleText.text = titleText.text;
            UIManager.current.profil.titleHasChanged = true;
        }
        UIManager.current.profil.CloseActualTab();
    }

    public void SetTitleCase(UnlockTitle.Title title)
    {
        id = title.id;
        titleText.text = title.name;
        subtitleText.text = title.method;
        button.interactable = title.unlock;
    }

    public void UnlockTitle()
    {
        button.interactable = true;
    }
}
