using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PasswordWithItem : MonoBehaviour
{
    [SerializeField] private SelectProfile profilSelection;

    [SerializeField] private Toggle P1;
    [SerializeField] private Toggle P2;
    [SerializeField] private Toggle P3;
    [SerializeField] private Toggle P4;

    private bool pin1, pin2, pin3, pin4;
    private string codeEnter;

    [SerializeField] private UIAnimation errorAnimation;

    // Start is called before the first frame update
    void Start()
    {
        //ResetPasswordContent();
        //StartCoroutine(WritePsw());
    }

    //private void ResetPasswordContent()
    //{
    //    codeEnter = "";
    //    pin1 = false;
    //    P1.isOn = false;
    //    pin2 = false;
    //    P2.isOn = false;
    //    pin3 = false;
    //    P3.isOn = false;
    //    pin4 = false;
    //    P4.isOn = false;
    //}

    async void BackgroundWork()
    {
        PlayAudio.Instance.PlayOneShot(PlayAudio.Instance.bank.bipUnvalid);
        await errorAnimation.FlashColor();
    }

    //public void RemoveOnePin()
    //{
    //    if (codeEnter == "") return;

    //    StopAllCoroutines();
    //    if (pin4) { pin4 = false; P4.isOn = false; }
    //    else if (pin3) { pin3 = false; P3.isOn = false; }
    //    else if (pin2) { pin2 = false; P2.isOn = false; }
    //    else if (pin1) { pin1 = false; P1.isOn = false; }
    //    codeEnter = codeEnter.Remove(codeEnter.Length - 1, 1);
    //    //StartCoroutine(WritePsw());
    //}

    //public void ButtonPressed(PassPin pin)
    //{
    //    codeEnter += pin.Code;
    //    if (!pin1)
    //    {
    //        pin1 = true;
    //        P1.isOn = true;
    //        P1.graphic.GetComponent<Image>().sprite = pin.Sprite;
    //    }
    //    else if (!pin2)
    //    {
    //        pin2 = true;
    //        P2.isOn = true;
    //        P2.graphic.GetComponent<Image>().sprite = pin.Sprite;
    //    }
    //    else if (!pin3)
    //    {
    //        pin3 = true;
    //        P3.isOn = true;
    //        P3.graphic.GetComponent<Image>().sprite = pin.Sprite;
    //    }
    //    else if (!pin4)
    //    {
    //        pin4 = true;
    //        P4.isOn = true;
    //        P4.graphic.GetComponent<Image>().sprite = pin.Sprite;
    //        StartCoroutine(CheckPassword());
    //    }
    //}

    //public IEnumerator CheckPassword()
    //{
    //    string url = Database.Instance.API_ROOT + "/auth/verify-pin.min";
    //    WWWForm form = new();
    //    form.AddField("id", profilSelection.SelectedProfileID);
    //    form.AddField("pin", codeEnter);

    //    using (UnityWebRequest req = UnityWebRequest.Post(url, form))
    //    {
    //        req.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
    //        yield return req.SendWebRequest();
    //        if (req.result == UnityWebRequest.Result.ConnectionError)
    //        {
    //            Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
    //            req.Dispose();
    //            bool result = false;
    //            yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
    //            if (result)
    //            {
    //                yield return StartCoroutine(CheckPassword());
    //                yield break;
    //            }
    //        }
    //        else if (req.downloadHandler.text.Contains("true"))
    //        {
    //            Debug.Log("Logged in");
    //            PlayAudio.Instance.PlayOneShot(PlayAudio.Instance.bank.bipValid);
    //            PlayerPrefs.SetString("ID", UniqueKey.KeyGenerator.Cipher(profilSelection.SelectedProfileID.ToString()));
    //            PlayerPrefs.SetString("USERNAME", UniqueKey.KeyGenerator.Cipher(NetworkManager.Instance.profiles[profilSelection.SelectedProfileID].Item1));
    //            Database.Instance.userData.id = profilSelection.SelectedProfileID.ToString();
    //            Database.Instance.userData.username = NetworkManager.Instance.profiles[profilSelection.SelectedProfileID].Item1;

    //            LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(NetworkManager.Instance.sceneController.HomeScene, "Lancement du jeu en cours...",
    //            Database.Instance?.LoadGameDatas()));
    //        }
    //        else
    //        {
    //            BackgroundWork(); //Animation to notify that the code went wrong
    //            Debug.LogWarning(req.responseCode);
    //        }
    //    }
    //    ResetPasswordContent();
    //}
}
