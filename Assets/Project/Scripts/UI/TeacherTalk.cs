using TMPro;
using UnityEngine;

public class TeacherTalk : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TMP_Text textBox;

    //public void say(string text)
    //{
    //    dialogBox.SetActive(true);
    //    if (Database.Instance && Database.Instance.userData != null)
    //    {
    //        textBox.SetText(text.Replace("#playerName#", Database.Instance.userData.username));
    //    }
    //    else
    //    {
    //        textBox.SetText(text.Replace(" #playerName#", ""));
    //    }
    //}

    ///// Dictionnary of custom tag :
    ///// #playerName# : user.namePlayer

    //public void stop()
    //{
    //    textBox.text = "";
    //    dialogBox.SetActive(false);
    //}
}