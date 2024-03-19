using TMPro;
using UnityEngine;

public class PanelConnexion : MonoBehaviour
{
    [Header("Connection")]
    public TMP_InputField identifier;
    public TMP_InputField password;
    [SerializeField] private AuthenticationType authenticationType;
    public void TryConnect()
    {

        switch (authenticationType)
        {
            case AuthenticationType.Code:
                if (!string.IsNullOrEmpty(identifier.text))
                {
                    NetworkManager.Instance.TryAuthenticate(AuthenticationType.Code, null, identifier.text);
                }
                break;
            case AuthenticationType.Credentials:
                if (!string.IsNullOrEmpty(identifier.text) && !string.IsNullOrEmpty(password.text))
                {
                    NetworkManager.Instance.TryAuthenticate(AuthenticationType.Credentials, null, null, identifier.text, password.text);
                }
                break;
            default:
                return;
        }

        //if (identifier != null) identifier.text = "";
        if (password != null) password.text = "";

        // Delete persistent data from previous connection
        CrossSceneInformation.Reset();
    }

    //public void ConnectLocally()
    //{
    //    PlayerPrefs.DeleteKey("CLASSROOM");
    //    PlayerPrefs.SetString("ID", "local"); //is local account
    //    if(identifier.text == "") PlayerPrefs.SetString("USERNAME", "Aventurier");
    //    else PlayerPrefs.SetString("USERNAME", identifier.text);

    //    // Reset previous user datas
    //    Database.Instance.userData.InitialiseDatas();
    //    // Reset previous scenario datas
    //    Database.Instance?.Scenarios.Clear();
    //    Database.Instance?.SpecialScenarios.Clear();
    //    SaveDataScenario.current.WriteScenarioFile();

    //    // Delete persistent data from previous connection
    //    CrossSceneInformation.Reset();

    //    // Load Home Scene
    //    LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(NetworkManager.Instance.sceneController.HomeScene, "Lancement du jeu en cours...",
    //        Database.Instance?.PullAccountRewardsCoroutine(),
    //        Database.Instance?.LoadGameDatas(),
    //        Database.Instance?.LoadData()
    //        //Database.Instance?.ClassmatesData(),
    //        //Database.Instance?.ReceiveGifts()
    //        )); 
    //}
}
