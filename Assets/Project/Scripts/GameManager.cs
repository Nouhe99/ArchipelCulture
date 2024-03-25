using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class UserConfig
{
    public bool isFirstConnection = true;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UserData userData;
    private UserConfig userConfig;
    private string userDataPath;
    private string userConfigPath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        userConfigPath = Path.Combine(Application.persistentDataPath, "userConfig.json");
        userDataPath = Path.Combine(Application.persistentDataPath, "userData.json");

        
        LoadUserData();
        LoadUserConfig();
    }

    private void LoadUserData()
    {
        if (File.Exists(userDataPath))
        {
            // Load the existing UserData
            string json = File.ReadAllText(userDataPath);
            JsonUtility.FromJsonOverwrite(json,this);
        }
        else
        {
            // Create a new UserData object since it does not exist
            userData = ScriptableObject.CreateInstance<UserData>();
            // Here you may want to set default values or perform additional setup on userData
            // Then you can save it to JSON if necessary
            SaveUserData();
        }
    }

    private void SaveUserData()
    {
        string json = JsonUtility.ToJson(userData);
        File.WriteAllText(userDataPath, json);
    }

    void LoadUserConfig()
    {
        if (File.Exists(userConfigPath))
        {
            string json = File.ReadAllText(userConfigPath);
            userConfig = JsonUtility.FromJson<UserConfig>(json);
        }
        else
        {
            userConfig = new UserConfig();
        }

        CheckFirstConnection();
    }

    void SaveUserConfig()
    {
        string json = JsonUtility.ToJson(userConfig);
        File.WriteAllText(userConfigPath, json);
    }

    public void RequestSaveUserData()
    {
        SaveUserData();
    }
    void CheckFirstConnection()
    {
        if (userConfig.isFirstConnection)
        {
            Debug.Log("Handling first connection tasks.");
            // Perform first-time initialization tasks here

            // Set the flag to false so this block won't run again
            userConfig.isFirstConnection = false;
            SaveUserConfig();
        }
    }


    public static void DeleteAllJsonFiles()
    {
        DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] jsonFiles = directory.GetFiles("*.json");

        foreach (FileInfo file in jsonFiles)
        {
            file.Delete();
            Debug.Log($"Deleted file: {file.FullName}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            DeleteAllJsonFiles(); // or DeleteSpecificJsonFile("userConfig.json");
            Debug.Log("All JSON files deleted. Restarting game...");
            // Optionally, reload the scene or restart the game logic as needed
        }
    }
    // Rest of your GameManager code...
}

