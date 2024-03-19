using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum AuthenticationType
{
    None,
    Credentials,
    Code,
    Token
}
public enum AccountType
{
    None = 0,
    Public = 1,
    Class = 2
}

public class NetworkManager : MonoBehaviour
{
    #region Singleton
    public static NetworkManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion
    #region Variables
    [Header("Global")]
    public SceneController sceneController;
    [SerializeField] private PlaySound playSound;
    private bool authenticationInProgress;
    private bool authenticated = false;
    /// <summary>
    /// <list type="bullet">
    /// <item><see cref="Tuple{T1,T2,T3}.Item1"/>: Username</item>
    /// <item><see cref="Tuple{T1,T2,T3}.Item2"/>: Profile picture ID</item>
    /// <item><see cref="Tuple{T1,T2,T3}.Item3"/>: User Title</item>
    /// </list>
    /// </summary>
    [HideInInspector] public Dictionary<int, Tuple<string, string, string>> profiles = new(); //id ; username, profil picture id, title
    private const string ACC_CLASS = "Class";
    private const string ACC_PUBLIC = "Public";

    [Header("Home")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private Button localConnectButton;
    [SerializeField] private Button classConnectButton;
    [SerializeField] private Button logoutButton;

    [Header("Public Connection")]
    [SerializeField] private GameObject publicConnectionPanel;
    [SerializeField] private GameObject publicProfilesPanel;
    [SerializeField] private TMP_Text publicConnectionError;

    [Header("Public Registration")]
    [SerializeField] private GameObject publicRegistrationPanel;
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField verifyPasswordField;
    [SerializeField] private TMP_Text registerCredentialError;

    [Header("Public Forgot Password")]
    [SerializeField] private GameObject publicForgotPwdPanel;
    [SerializeField] private TMP_InputField forgotEmailField;
    [SerializeField] private TMP_Text forgotPwdInfo;

    [Header("Class Connection")]
    [SerializeField] private GameObject classCodePanel;
    [SerializeField] private GameObject classProfilesPanel;
    [SerializeField] private TMP_Text classConnectionError;
    #endregion

    private void Start()
    {
        // Default state
        authenticationInProgress = false;
        // Version text
        if (versionText != null)
        {
            versionText.SetText($"V.{Application.version}");
        }

        // Default display = Home
        DisplayHome();
        // Play menu music 
        PlayAudio.Instance.PlayMusic(PlayAudio.Instance.bank.musicLogin, "music");
        // Auto connection if token found
        if (PlayerPrefs.HasKey("TOKEN"))
        {
            TryAuthenticate(AuthenticationType.Token, PlayerPrefs.GetString("TOKEN"));
        }
        // Delete persistent data from previous connection
        CrossSceneInformation.Reset();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Database.QuittingGame();
        }
    }

    #region Display management
    public void DisplayHome()
    {
        publicConnectionPanel.SetActive(false);
        publicRegistrationPanel.SetActive(false);
        classCodePanel.SetActive(false);
        publicProfilesPanel.SetActive(false);
        classProfilesPanel.SetActive(false);
        publicForgotPwdPanel.SetActive(false);
        homePanel.SetActive(true);
    }

    public void DisplayPublicConnection()
    {
        if (PlayerPrefs.GetInt("ACCTYPE", (int)AccountType.None) == (int)AccountType.Public && PlayerPrefs.HasKey("TOKEN") && authenticated)
        {
            // Already connected
            publicConnectionPanel.SetActive(false);
            publicRegistrationPanel.SetActive(false);
            classCodePanel.SetActive(false);
            classProfilesPanel.SetActive(false);
            homePanel.SetActive(false);
            publicForgotPwdPanel.SetActive(false);

            publicProfilesPanel.SetActive(true);
        }
        else
        {
            // Not connected
            publicRegistrationPanel.SetActive(false);
            classCodePanel.SetActive(false);
            publicProfilesPanel.SetActive(false);
            classProfilesPanel.SetActive(false);
            homePanel.SetActive(false);
            publicForgotPwdPanel.SetActive(false);

            publicConnectionPanel.SetActive(true);
        }
    }

    public void DisplayPublicRegistration()
    {
        publicConnectionPanel.SetActive(false);
        classCodePanel.SetActive(false);
        classProfilesPanel.SetActive(false);
        homePanel.SetActive(false);
        publicProfilesPanel.SetActive(false);
        publicForgotPwdPanel.SetActive(false);

        publicRegistrationPanel.SetActive(true);
    }
    public void DisplayPublicForgotPassword()
    {
        publicConnectionPanel.SetActive(false);
        classCodePanel.SetActive(false);
        classProfilesPanel.SetActive(false);
        homePanel.SetActive(false);
        publicProfilesPanel.SetActive(false);
        publicRegistrationPanel.SetActive(false);

        publicForgotPwdPanel.SetActive(true);
    }

    public void DisplayClassConnection()
    {
        if (PlayerPrefs.GetInt("ACCTYPE", (int)AccountType.None) == (int)AccountType.Class && PlayerPrefs.HasKey("TOKEN") && authenticated)
        {
            // Already connected
            publicConnectionPanel.SetActive(false);
            publicRegistrationPanel.SetActive(false);
            classCodePanel.SetActive(false);
            publicProfilesPanel.SetActive(false);
            homePanel.SetActive(false);

            classProfilesPanel.SetActive(true);
        }
        else
        {
            // Not connected
            publicConnectionPanel.SetActive(false);
            publicRegistrationPanel.SetActive(false);
            publicProfilesPanel.SetActive(false);
            classProfilesPanel.SetActive(false);
            homePanel.SetActive(false);

            classCodePanel.SetActive(true);
        }
    }

    private void DisplayClassProfiles()
    {
        classProfilesPanel.SetActive(true);
    }

    private void DisplayPublicProfiles()
    {
        publicProfilesPanel.SetActive(true);
    }
    #endregion

    #region Authentication
    public void TryAuthenticate(AuthenticationType authType, string token, string code = null, string mail = null, string password = null)
    {
        if (authenticationInProgress) return;
        try
        {
            StartCoroutine(Authentication_Coroutine(authType, token, code, mail, password));
        }
        catch (Exception e)
        {
            Debug.LogError("Authentication error : " + e.Message);
        }
        return;
    }

    private IEnumerator Authentication_Coroutine(AuthenticationType authType, string token, string code = null, string identifier = null, string password = null)
    {
        authenticationInProgress = true;
        authenticated = false;
        //classCodeCross.SetActive(false);
        string url = Database.Instance.API_ROOT + "/auth/authenticate.min";
        WWWForm form = new();
        switch (authType)
        {
            case AuthenticationType.Token:
                form.AddField("token", token);
                break;
            case AuthenticationType.Code:
                form.AddField("code", code);
                classConnectionError.text = "";
                break;
            case AuthenticationType.Credentials:
                form.AddField("mail", identifier);
                form.AddField("password", password);
                publicConnectionError.text = "";
                break;
            default: break;
        }

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                switch (authType)
                {
                    case AuthenticationType.Code:
                        classConnectionError.text = "Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.";
                        break;
                    case AuthenticationType.Credentials:
                        publicConnectionError.text = "Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.";
                        break;
                    default: break;
                }
                Debug.Log("Network error has occured: " + request.GetResponseHeader(""));
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                string[] response = request.downloadHandler.text.Split(Database.Instance.requestSeparator);
                string[] user = response[0].Split(Database.Instance.columnSeparator); // 0 = Token | 1 = AccountType (Public or Class)
                if (string.IsNullOrEmpty(user[0]))
                {
                    Debug.LogError("No token found");
                    yield break;
                }

                PlayerPrefs.SetString("TOKEN", user[0]);
                string[] rawProfiles = response[1].Split(Database.Instance.rowSeparator).SkipLast(1).ToArray();
                profiles.Clear();
                foreach (string rawProfile in rawProfiles)
                {
                    string[] profile = rawProfile.Split(Database.Instance.columnSeparator); // 0 = Student ID | 1 = Username | 2 = Profile Picture | 3 = Title
                    profiles.Add(int.Parse(profile[0]), new Tuple<string, string, string>(profile[1], profile[2], profile[3]));
                }


                switch (user[1])
                {
                    case ACC_CLASS:
                        // Authenticated in class account
                        PlayerPrefs.SetInt("ACCTYPE", (int)AccountType.Class);
                        Database.Instance.userData.accountType = AccountType.Class;
                        DisplayClassProfiles();
                        break;
                    case ACC_PUBLIC:
                        // Authenticated in public account
                        PlayerPrefs.SetInt("ACCTYPE", (int)AccountType.Public);
                        Database.Instance.userData.accountType = AccountType.Public;
                        DisplayPublicProfiles();
                        break;
                    default:
                        PlayerPrefs.SetInt("ACCTYPE", (int)AccountType.None);
                        Database.Instance.userData.accountType = AccountType.None;
                        DisplayHome();
                        break;
                }
                authenticated = true;
            }
            else
            {
                switch (authType)
                {
                    case AuthenticationType.Code:
                        classConnectionError.text = "Vos informations d'authentification sont incorrectes";
                        break;
                    case AuthenticationType.Credentials:
                        publicConnectionError.text = "Vos informations d'authentification sont incorrectes";
                        break;
                    default: break;
                }
                Debug.LogError("Authentication error " + request.error);
            }
        }
        authenticationInProgress = false;
    }

    #endregion

    #region Registration

    public void TryRegister()
    {
        if (passwordField.text.Equals(verifyPasswordField.text))
            try
            {
                StartCoroutine(Register_Coroutine(passwordField.text, emailField.text));
            }
            catch (Exception e) { Debug.Log("Registration error : " + e.Message); }
    }

    private IEnumerator Register_Coroutine(string password, string email)
    {
        string url = Database.Instance.API_ROOT + "/auth/register.min";
        WWWForm form = new();
        form.AddField("mail", email);
        form.AddField("password", password);

        UnityWebRequest req = UnityWebRequest.Post(url, form);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            registerCredentialError.text = req.error;
            Debug.Log("Network error has occured: " + req.GetResponseHeader("")); req.Dispose();
            bool result = false;
            yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
            if (result)
            {
                yield return StartCoroutine(Register_Coroutine(password, email));
                yield break;
            }
        }
        else if (req.result == UnityWebRequest.Result.Success)
        {
            if (req.downloadHandler.text.Contains(Database.Instance.columnSeparator))
            {
                registerCredentialError.text = "Compte créé avec succès";
                sceneController.LoadLoginScene();
            }
            else
            {
                registerCredentialError.text = req.downloadHandler.text;
            }
        }
        else
        {
            registerCredentialError.text = req.error;
        }
        req.Dispose();
    }
    #endregion

    #region Forgot Password
    public void TryResetPassword()
    {
        if (!string.IsNullOrEmpty(forgotEmailField.text))
        {
            try
            {
                StartCoroutine(ResetPassword_Coroutine(forgotEmailField.text));
            }
            catch (Exception e)
            {
                Debug.Log("Forgot password error : " + e.Message);
            }
        }

    }

    private IEnumerator ResetPassword_Coroutine(string email)
    {
        string url = Database.Instance.API_ROOT + "/auth/forgot-password.min";
        WWWForm form = new();
        form.AddField("mail", email);

        UnityWebRequest req = UnityWebRequest.Post(url, form);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            registerCredentialError.text = req.error;
            Debug.Log("Network error has occured: " + req.GetResponseHeader("")); req.Dispose();
            bool result = false;
            yield return StartCoroutine(Database.ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
            if (result)
            {
                yield return StartCoroutine(ResetPassword_Coroutine(email));
                yield break;
            }
        }
        else if (req.result == UnityWebRequest.Result.Success)
        {
            forgotPwdInfo.text = req.downloadHandler.text;
        }
        else
        {
            registerCredentialError.text = req.error;
        }
        req.Dispose();
    }
    #endregion

    #region Logout
    public void Logout()
    {
        PlayerPrefs.DeleteKey("TOKEN");
        PlayerPrefs.DeleteKey("ACCTYPE");
    }
    #endregion
}

namespace UniqueKey
{
    public class KeyGenerator
    {
        private static readonly string loginIV = "It8Hio4dcwT1wI10m4LtjGOuiEFhGkPc"; //NOTE: this is a random generated key, can be modify whenever you want :)
                                                                                     //Debug.Log(UniqueKey.KeyGenerator.GetUniqueKey(32)); 
        internal static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        internal static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        internal static string Decipher(string message)
        {
            if (PlayerPrefs.HasKey("LOGINKEY"))
            {
                return SecurityTools.DecryptAes(message, PlayerPrefs.GetString("LOGINKEY"), loginIV);
            }
            else
            {
                Init();
                throw new AccessViolationException();
            }
        }
        internal static string Cipher(string message)
        {
            if (!PlayerPrefs.HasKey("LOGINKEY"))
            {
                Init();
            }
            return SecurityTools.EncryptAes(message, PlayerPrefs.GetString("LOGINKEY"), loginIV);
        }

        private static void Init()
        {
            PlayerPrefs.SetString("LOGINKEY", GetUniqueKey(18));
        }
    }
}