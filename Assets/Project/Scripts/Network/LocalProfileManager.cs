using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LocalProfileManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField username;
    [SerializeField] private SceneController sceneController;
    [SerializeField] private GameObject AddProfileButton;
    [SerializeField] private LocalProfile[] Profiles;
    [SerializeField] private GameObject AddProfilePanel;

    private const int MAXPROFILES = 3;

    private void OnEnable()
    {
        UpdateProfilesDisplay();
        removeMenuOpen = false;
        foreach (LocalProfile profil in Profiles)
        {
            profil.ShowDeleteButton(removeMenuOpen);
        }
    }

    private void UpdateProfilesDisplay()
    {
        //int numberOfCreatedProfiles = GetNumberOfCreatedProfiles();
        var profiles = NetworkManager.Instance.profiles;
        int nbOfProfiles = 0;
        foreach (var profile in profiles)
        {
            Profiles[nbOfProfiles].gameObject.SetActive(true);
            Profiles[nbOfProfiles].Identifier = profile.Key;
            Profiles[nbOfProfiles].Username.text = profile.Value.Item1;
            Profiles[nbOfProfiles].Picture.sprite = Database.Instance.profilPictures.GetPicture(profile.Value.Item2);
            nbOfProfiles++;
        }
        for (int i = nbOfProfiles; i < MAXPROFILES; i++)
        {
            Profiles[i].gameObject.SetActive(false);
        }

        AddProfileButton.SetActive(profiles.Count < MAXPROFILES);
    }

    public void AddLocalProfile()
    {
        if (NetworkManager.Instance.profiles.Count >= MAXPROFILES) return;
        // Create new profile 
        if (string.IsNullOrWhiteSpace(username.text))
        {
            createProfileCoroutine = Database.Instance.StartCoroutine(CreateProfile("Aventurier"));
        }
        else
        {
            createProfileCoroutine = Database.Instance.StartCoroutine(CreateProfile(username.text));
        }
    }

    public IEnumerator RemoveLocalProfile(LocalProfile profile)
    {
        PopUp dialog = Database.Instance.errorDialog.Instance($"Es-tu sûr de vouloir <b>supprimer</b> le profil de {profile.Username.text} ?", PopupStyle.YesNo);
        yield return null;
        while (dialog.result == dialog.NONE)
        {
            yield return null; // wait
        }

        if (dialog.result == dialog.YES)
        {
            // Remove Datas
            yield return StartCoroutine(RemoveProfile(profile.Identifier.ToString()));
            // Update display
            UpdateProfilesDisplay();
            OpenRemoveMenu();
        }
        dialog.Destroy();
    }

    public void ConnectToProfile(LocalProfile profile)
    {
        Database.Instance.userData.id = profile.Identifier.ToString();
        Database.Instance.userData.username = NetworkManager.Instance.profiles[profile.Identifier].Item1;

        // Launch game
        LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(NetworkManager.Instance.sceneController.HomeScene, "Lancement du jeu en cours...",
        Database.Instance?.LoadGameDatas()));
        //  Database.Instance?.LoadGifts()));
    }

    private bool removeMenuOpen = false;
    public void OpenRemoveMenu()
    {
        removeMenuOpen = !removeMenuOpen;

        foreach (LocalProfile profil in Profiles)
        {
            profil.ShowDeleteButton(removeMenuOpen);
        }

    }

    private Coroutine createProfileCoroutine = null;
    public IEnumerator CreateProfile(string username)
    {
        if (createProfileCoroutine != null) yield break;
        string url =  Database.Instance.API_ROOT + "/auth/create-profile.min";
        Debug.Log("Path : " + url);
        WWWForm form = new();
        form.AddField("username", username);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string[] rawProfile = webRequest.downloadHandler.text.Split(Database.Instance.rowSeparator).SkipLast(1).ToArray();
                string[] profile = rawProfile[0].Split(Database.Instance.columnSeparator); // 0 = Student ID | 1 = Username | 2 = Profile Picture | 3 = Title
                if (NetworkManager.Instance.profiles.TryAdd(int.Parse(profile[0]), new Tuple<string, string, string>(profile[1], profile[2], profile[3])))
                {
                    // Connect to this profile
                    Database.Instance.userData.id = profile[0];
                    Database.Instance.userData.username = profile[1];

                    LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(NetworkManager.Instance.sceneController.HomeScene, "Lancement du jeu en cours...",
                    Database.Instance?.LoadGameDatas()));
                }
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                bool result = false;
                yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
                if (result)
                {
                    yield return StartCoroutine(CreateProfile(username));
                    yield break;
                }
            }
            else
            {
                Debug.LogError(webRequest.error);
            }
        }
        createProfileCoroutine = null;
    }

    public IEnumerator RemoveProfile(string id)
    {
        string url = Database.Instance.API_ROOT + "/auth/remove-profile";
        WWWForm form = new();
        form.AddField("id", id);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                string[] rawProfiles = response.Split(Database.Instance.rowSeparator).SkipLast(1).ToArray();
                NetworkManager.Instance.profiles.Clear();
                foreach (string rawProfile in rawProfiles)
                {
                    string[] profile = rawProfile.Split(Database.Instance.columnSeparator); // 0 = Student ID | 1 = Username | 2 = Profile Picture | 3 = Title
                    NetworkManager.Instance.profiles.Add(int.Parse(profile[0]), new Tuple<string, string, string>(profile[1], profile[2], profile[3]));
                }
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                bool result = false;
                yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
                if (result)
                {
                    yield return StartCoroutine(RemoveProfile(id));
                    yield break;
                }
            }
            else
            {
                Debug.LogError(webRequest.error);
            }
        }
    }
}
