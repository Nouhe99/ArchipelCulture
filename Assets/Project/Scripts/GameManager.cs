using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;


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
        LoadDatas();
        SceneManager.sceneLoaded += OnSceneLoaded;


    }

 

    private void LoadDatas()
    {
        LoadUserData();
        LoadUserConfig();
        SaveDataInventory.Instance.LoadInventoryFromLocal();
    }
    public void LoadUserData()
    {
        if (File.Exists(userDataPath))
        {
            // Load the existing UserData
            string json = File.ReadAllText(userDataPath);
            JsonUtility.FromJsonOverwrite(json,this);
        }
        else
        {
            userData = new UserData();
            userData.ResetUserData();
            SaveUserData();
        }
    }

    public void SaveUserData()
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
            // Perform first-time initialization tasks here

            // Database.Instance.LoadUserDataFromLocal();
            userData.ResetUserData();
            Debug.Log("Handling first connection tasks.");
            userConfig.isFirstConnection = false;
            SaveUserConfig();
        }
        else
        {
            //Database.Instance.LoadUserDataFromLocal();
            LoadUserData();



        }
    }


    public  void DeleteAllDatas()
    {
        SaveDataInventory.Instance.ResetInventory();
        userData.ResetUserData();
        string paths = Application.persistentDataPath;
        try
        {
            DirectoryInfo directory = new DirectoryInfo(paths);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while trying to delete all game data: " + e.Message);
        }
           // SceneManager.LoadScene(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            DeleteAllDatas(); 
            Debug.Log("All JSON files deleted. Restarting game...");
            // reload the scene or restart the game logic as needed
        }

    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Home")
        {
            LoadDatas();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the sceneLoaded event.
    }
}

