﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
//using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;


public class Database : MonoBehaviour
{
    public static Database Instance;

    public enum Environment
    {
        Development,
        Production
    }
    [SerializeField] private Environment environment;
    private const string API_PROD = "https://application.archipel.education/api";
    private const string API_DEV = "https://application.archipel.education/api_dev";
    public string API_ROOT
    {
        get
        {
            switch (environment)
            {
                case Environment.Development:
                    return API_DEV;
                case Environment.Production:
                    return API_PROD;
                default:
                    return API_DEV;
            }
        }
    }

    [HideInInspector] public readonly char columnSeparator = '○';
    [HideInInspector] public readonly char rowSeparator = '•';
    [HideInInspector] public readonly char requestSeparator = '█';

    [HideInInspector] public List<LevelReward> LevelRewards;
    [HideInInspector] public Dictionary<string, Notion> Notions = new();
    [HideInInspector] public List<Scenario> Scenarios;

    [Header("Required datas")]
    public SceneController sceneController;
    public UserData userData;
    public DatabaseItems itemsList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            itemsList.UpdateDictionnary(); //load shop items
        }
        else
        {
            Destroy(gameObject);
            return;
        }
       // DontDestroyOnLoad(gameObject);
        Debug.LogWarning($"Current environment : <b><color=yellow>{environment}</color></b>");
    }

  


    //public void LoadUserDataFromLocal()
    //{
    //    string filePath = Application.persistentDataPath + "/userData.json";
    //    if (File.Exists(filePath))
    //    {
    //        string jsonData = File.ReadAllText(filePath);
    //        JsonUtility.FromJsonOverwrite(jsonData, this);
    //    }
    //    else
    //    {
    //        Debug.Log("No existing UserData found. Initializing new player data.");
    //        userData.ResetUserData();
    //    }
    //    userData.LoadGold();
        
    //}
    

    #region NewSavingSystem
    //public void SavePlayerDataToLocal()
    //{
    //    string jsonData = JsonUtility.ToJson(userData, true);
    //    File.WriteAllText(Application.persistentDataPath + "/playerData.json", jsonData);
    //}

    
    #endregion

    #region Load Game Datas
    //    public IEnumerator LoadGameDatas()
    //    {
    //        WWWForm form = new();
    //        form.AddField("id", userData.id);
    //        string url = API_PROD + "/datas/load-player-datas";
    //        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
    //        {
    //            webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
    //#if UNITY_EDITOR
    //            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    //            stopwatch.Start();
    //#endif
    //            yield return webRequest.SendWebRequest();

    //            // Process datas
    //            if (webRequest.result == UnityWebRequest.Result.Success)
    //            {
    //                // Format request : 0 = Account rewards, 1 = Notions, 2 = Scenario Rewards, 3 = Scenarios, 4 = Player infos
    //                // 5 = Owned items, 6 = Titles, 7 = Friends
    //                string[] requests = webRequest.downloadHandler.text.Split(requestSeparator);

    //                if (requests.Length < 8)
    //                {
    //                    Debug.LogError("Error in the request");
    //                }
    //                // ########## Request[0] = Account Rewards ###########
    //                LoadAccountRewards(requests[0].Split(rowSeparator).SkipLast(1).ToArray());
    //               // LoadAccountRewards(Notions);

    //                // ########## Request[1] = Notions ##########
    //                LoadNotions(requests[1].Split(rowSeparator).SkipLast(1).ToArray());

    //                // ########## Request[2/3] = Scenarios ##########
    //                yield return StartCoroutine(LoadScenarios(requests[2].Split(rowSeparator).SkipLast(1).ToArray(), requests[3].Split(rowSeparator).SkipLast(1).ToArray()));

    //                // ########## Request[4] = Player infos ##########
    //                LoadPlayerInfos(requests[4].Split(rowSeparator));

    //                // ########## Request[5] = Owned items ##########
    //                LoadPlayerItems(requests[5].Split(rowSeparator).SkipLast(1).ToArray());





    //#if UNITY_EDITOR
    //                stopwatch.Stop();
    //                Debug.Log($"Loaded required datas in <color=yellow>{stopwatch.Elapsed}</color>");
    //#endif
    //          /*  }
    //            else
    //            {
    //                Debug.LogError($"An error occured [Scenarios]: {webRequest.error}");
    //                webRequest.Dispose();
    //                bool result = false;
    //                yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
    //                if (result)
    //                {
    //                    yield return StartCoroutine(LoadGameDatas());
    //                    yield break;
    //                }*/
    //            }
    //        }
    //    }
    #endregion
    /// <summary>
    /// Load account rewards from string
    /// </summary>
    /// <param name="rewards">0: Reward ID (int), 1: Item ID (int), 2: Quantity (int), 3: Requirement (int), 4: Acquired (bool)</param>
    private void LoadAccountRewards(string[] rewards)
    {
        LevelRewards = new List<LevelReward>();

        for (int i = 0; i < rewards.Length; i++)
        {
            // Separate each column
            // 0: Reward ID (int), 1: Item ID (int), 2: Quantity (int), 3: Requirement (int), 4: Acquired (bool)
            string[] subs = rewards[i].Split(columnSeparator);

            Item temp = itemsList.GetItemData(subs[1]);
            if (temp != null)
            {
                LevelReward rwd = new(subs[0], temp.Data(), int.Parse(subs[2]), int.Parse(subs[3]), subs[4] != "0");
                LevelRewards.Add(rwd);
            }
            else
            {
                Debug.LogError("This item (id: " + subs[1] + ") doesn't exist in item's list, must create it !");
                //TODO: create a new item from database because don't exist here (example for titles, profil pictures...)
            }
        }
    } //tomodify

    /// <summary>
    /// Load notions from string
    /// </summary>
    /// <param name="notions">0: Notion ID (int), 1: Notion Name (string), 2: Parent Notion ID (int)</param>
    private void LoadNotions(string[] notions) //tomodify
    {
        for (int i = 0; i < notions.Length; i++)
        {
            // Separate row's columns with its separator
            string[] columns = notions[i].Split(columnSeparator);

            // 0: Notion ID (int), 1: Notion Name (string), 2: Parent Notion ID (int)
            Notions.TryAdd(columns[0], new Notion(columns[0], columns[1], columns[2]));
        }
    }

    /// <summary>
    /// Load scenarios from string
    /// </summary>
    /// <param name="scenarioRewards">0: Scenario ID (string), 1: Item ID (string), 2: Quantity (int)</param>
    /// <param name="scenarios">0: Scenario ID (int), 1: Scenario Name (string), 2: Isle Name (string), 3: Maximum Isle Stages (byte), 
    /// 4: Isle Prefab Address (string), 5: Successes (int), 6: Failures (int), 7: Success at first try (bool), 8: Reward Received (bool), 
    /// 9: Quiz One Tries (int), 10: Quiz Two Tries (int), 11: Quiz Three Tries (int), 12: Quiz Four Tries (int), 13: Quiz Five Tries (int), 
    /// 14: Locked (bool) // 5+ May be NULL</param>
    private IEnumerator LoadScenarios(string[] scenarioRewards, string[] scenarios) //tomodify
    {
        // Scenario Rewards
        List<ScenarioReward> rewards = new();
        for (int i = 0; i < scenarioRewards.Length; i++)
        {
            // Separate row's columns with its separator
            string[] columns = scenarioRewards[i].Split(columnSeparator);

            ItemData tempData = new ItemData();
            Item temp = itemsList.GetItemData(columns[1]);
            if (temp == null)
            {
                //TODO: if item doesn't exist, create it from database (for rocks, titles, profil picture, etc...)
                tempData.ID = columns[1];
            }
            else
            {
                Item.CopyItem(temp, tempData);
            }
            rewards.Add(new ScenarioReward(columns[0], tempData, int.Parse(columns[2])));
        }

        // Scenarios
        Scenarios = new();
        for (int i = 0; i < scenarios.Length; i++)
        {
            // Separate row's columns with its separator
            string[] columns = scenarios[i].Split(columnSeparator);

            Isle isle = new(columns[2], byte.Parse(columns[3]), columns[4]);
            GameObject islePrefab = null;
            yield return StartCoroutine(GetPrefabByAddressAsync(isle.Address, value => islePrefab = value));
            if (islePrefab != null)
            {
                ScenarioIsle si = islePrefab.GetComponent<ScenarioIsle>();
                if (si != null)
                {
                    isle.PreviewIsle = si.PreviewIsle;
                }
            }
            int success = 0, fail = 0, reward = 0, restarted = 0;
            int.TryParse(columns[5], out success);
            int.TryParse(columns[6], out fail);
            int.TryParse(columns[8], out reward);
            int.TryParse(columns[15], out restarted);
            Scenario scenario = new Scenario(columns[0], columns[1], isle, rewards.FindAll(scenRew => scenRew.ScenarioID == columns[0]), success, fail, reward, restarted, columns[14] == "1");
            for (int q = 9; q < 14; q++)
            {
                int tries = 0;
                int.TryParse(columns[q], out tries);
                scenario.Steps.Add(new ScenarioStep(new List<Quiz>(), tries));
            }
            Scenarios.Add(scenario);
        }
    }

    /// <summary>
    /// Load player datas from string
    /// </summary>
    /// <param name="infos">0: Experience, 1:island tiles (json), 2:id-picture profil, 3:code, 4:resources, 5:building position, 6:title_profil, 7:rocks, 8:last_login, 9:kraken notion already done, 10:kraken choice, 11:gold spent, 12: tutorial step</param>
    private void LoadPlayerInfos(string[] infos) //tomodify
    {
        string[] subs = infos[0].Split(columnSeparator); //0:experience ; 1:island tiles (json) ; 2:id-picture profil ; 3:code; 4:resources; 5:building position; 6:title_profil; 7:rocks, 8:last_login, 9:kraken notion already done, 10:kraken choice, 11:gold spent, 12: tutorial step

        try
        {
            //subs[0] (xp) is not used
            
            userData.gold = int.Parse(subs[4]);

            userData.buildingPlacement = Vector3Int.zero;
            //subs[5] (building position) is not used
            /*
            string[] buildingPos = subs[5].Split(";");
            try
            {
                if (buildingPos.Length > 2) userData.buildingPlacement = new Vector3Int(int.Parse(buildingPos[0]), int.Parse(buildingPos[1]), int.Parse(buildingPos[2]));
                else userData.buildingPlacement = Vector3Int.zero;
            }catch (FormatException)
            {
                userData.buildingPlacement = Vector3Int.zero;
            }
            */

            userData.title = subs[6];
            userData.rocksRemaining = int.Parse(subs[7]);
            //subs[8] (last login) is not used
       
            userData.totalGoldSpent = int.Parse(subs[11]);
            userData.tutorialStep = byte.Parse(subs[12]);
            // Base island
            if (userData.tutorialStep == 0 && (!string.IsNullOrWhiteSpace(userData.rockPlacementJson) || userData.rockPlacementJson == "[]"))
            {
                userData.rockPlacementJson = "[{\"x_pos\": -4,\"y_pos\": -4,\"style_tile\": 1}," +
                    "{\"x_pos\": -5,\"y_pos\": -4,\"style_tile\": 1}," +
                    "{\"x_pos\": -5,\"y_pos\": -5,\"style_tile\": 1}," +
                    "{\"x_pos\": -4,\"y_pos\": -5,\"style_tile\": 1}]";
                if (SaveDataInventory.Instance != null)
                {
                    //SaveDataInventory.Instance.LoadPlacedObjects();

                }
                else
                {
                    Debug.LogError("Save object is NULL");
                }
            }
        }
        catch
        {
            //PlayerPrefs.DeleteKey("ID");
            //PlayerPrefs.DeleteKey("USERNAME");

            LogoTransition.Instance?.CancelTransition();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneController.LoginScene);
        }
    }

    /// <summary>
    /// Load player inventory (items) from string
    /// </summary>
    /// <param name="items">0: id, 1: placed, 2: x position, 3: y position, 4: inventory id, 5: variante, 6: z position</param>
    private void LoadPlayerItems(string[] items) //to change
    {
        userData.inventory.Clear();
        foreach (var item in items)
        {
            string[] subs2 = item.Split(columnSeparator);
            if (subs2[1] == "0")
                userData.inventory.Add(new UserData.ItemPlacement(subs2[0], false, 0, 0, 0, subs2[4], 0));
            else
            {
                int xPos = 0, yPos = 0, zPos = 0;
                int.TryParse(subs2[2], out xPos);
                int.TryParse(subs2[3], out yPos);
                int.TryParse(subs2[6], out zPos);
                userData.inventory.Add(new UserData.ItemPlacement(subs2[0], true, xPos, yPos, zPos, subs2[4], int.Parse(subs2[5]))); //NOTE : parse float using dots not commas ; NOTE : use this for Parse float : float.Parse(subs2[3], System.Globalization.CultureInfo.InvariantCulture)
            }
        }
    }



    #region Adventures (Quiz & Scenarios) //tomodify
    /// <summary>
    /// Complete the Scenario information by downloading the quiz datas from database
    /// </summary>
    /// <param name="scenario">Scenario from which the quizzes comes from</param>
    public IEnumerator GetQuizzes(Scenario scenario)
    {
        if (scenario.Steps.Count > 0 && scenario.Steps[0].Quizzes.Count > 0)
        {
            Debug.Log("Already have the quizzes for this scenario");
            yield break;
        }

        //WWWForm form = new WWWForm();
        //form.AddField("scenario_id", scenario.ID);
        //form.AddField("id", userData.id);
        //string url = API_PROD + "/datas/load-new-quiz";
        //using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        //{
        //    webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
        //    yield return webRequest.SendWebRequest();
        //    // Process datas
        //    if (webRequest.result == UnityWebRequest.Result.Success)
        //    {
        //        // Separate each request with its separator
        //        string[] requests = webRequest.downloadHandler.text.Split(requestSeparator);
        //        string[] rows;

        //        // ########## Request[0] = Quizzes ##########
        //        // Separate request's rows with its separator
        //        rows = requests[0].Split(rowSeparator).SkipLast(1).ToArray();
        //        for (int i = 0; i < rows.Length; i++)
        //        {
        //            // Separate row's columns with its separator
        //            string[] columns = rows[i].Split(columnSeparator);

        //            // 0: Sorting number (int), 1: Quiz ID (int), 2: Quiz Title (string), 3: Level (string), 4: Notion ID (int)
        //            // 5: Question (string), 6: Question Image (string, url), 7: Correction (string), 8: Correction Image (string, url)
        //            // 9: Multiple Choice (bool), 10: Video (string, url), 11: Parent ID (int), 12: Answered (NULL = not answered, 0 = Wrong answer, 1 = Correct answer)
        //            // Ordered by Parent ID [ASC]

        //            Notion notion;
        //            if (Notions.TryGetValue(columns[4], out notion) == false)
        //            {
        //                Debug.LogError("Notion do not exist");
        //            }
        //            Sprite questionImage = null;
        //            yield return StartCoroutine(CreateSpriteFromURLAsync(columns[6], value => questionImage = value));
        //            Sprite correctionImage = null;
        //            yield return StartCoroutine(CreateSpriteFromURLAsync(columns[8], value => correctionImage = value));

        //            int sort = int.Parse(columns[0]);
        //            if (scenario.Steps.Count > sort)
        //            {
        //                scenario.Steps[sort].Quizzes.Add(new Quiz(columns[1], columns[2], columns[3], notion, columns[5], questionImage, columns[7], correctionImage, new List<Answer>(), columns[9] == "1", columns[10], columns[11]));
        //                if (scenario.Steps[sort].State == QuizState.Pending)
        //                {
        //                    if (columns[12] == "1")
        //                    {
        //                        scenario.Steps[sort].State = QuizState.Completed;
        //                    }
        //                    else if (columns[12] == "0")
        //                    {
        //                        scenario.Steps[sort].State = QuizState.Failed;
        //                    }
        //                }
        //                else if (scenario.Steps[sort].State == QuizState.Failed && columns[12] == "1")
        //                {
        //                    scenario.Steps[sort].State = QuizState.Completed;
        //                }
        //            }
        //        }

        //        // ########## Request[1] = Answers ##########
        //        // Separate request's rows with its separator
        //        rows = requests[1].Split(rowSeparator).SkipLast(1).ToArray();
        //        for (int i = 0; i < rows.Length; i++)
        //        {
        //            // Separate row's columns with its separator
        //            string[] columns = rows[i].Split(columnSeparator);

        //            // 0: Answer ID (int), 1: Quiz ID (int), 2: Label (string), 3: Correct (bool)
        //            Quiz quiz = scenario.FindQuizById(columns[1]);
        //            if (quiz != null)
        //            {
        //                quiz.Answers.Add(new Answer(columns[0], columns[1], columns[2], columns[3] == "1"));
        //            }
        //            else
        //            {
        //                Debug.LogError("Could not find corresponding quiz");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError($"An error occured [Quiz]: {webRequest.error}");
        //        webRequest.Dispose();
        //        bool result = false;
        //        yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
        //        if (result)
        //        {
        //            yield return StartCoroutine(GetQuizzes(scenario));
        //            yield break;
        //        }
        //    }
        //}
    }

    public IEnumerator GetQuizById(string quizId)  //tomodify
    {
        yield return null;
        //// Fetch the quiz data based on the quiz ID
        //WWWForm form = new WWWForm();
        //form.AddField("quiz_id", quizId);
        //form.AddField("id", userData.id);
        //string url = API_PROD + "/datas/load-quiz-ID";
        //using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        //{
        //    webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
        //    yield return webRequest.SendWebRequest();

        //    if (webRequest.result == UnityWebRequest.Result.Success)
        //    {
        //        // Process the quiz data and create the quiz
        //        string[] rows = webRequest.downloadHandler.text.Split(rowSeparator).SkipLast(1).ToArray();
        //        for (int i = 0; i < rows.Length; i++)
        //        {
        //            string[] columns = rows[i].Split(columnSeparator);
        //            // Parse and create the quiz here...
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError($"An error occurred [Quiz]: {webRequest.error}");
        //        webRequest.Dispose();
        //        bool result = false;
        //       // yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
        //        if (result)
        //        {
        //            yield break;
        //        }
        //    }
        //}

    }





    /// <summary>
    /// Update the scenario for the connected student in the database
    /// </summary>
    /// <param name="scenario">Scenario to update for the connected user</param>
    public IEnumerator UpdateStudentScenario(Scenario scenario)
    {
        yield return null;
        //  WWWForm form = new();
        //form.AddField("failures", scenario.Failures);
        //form.AddField("successes", scenario.Successes);
        //form.AddField("reward_received", scenario.RewardReceived);
        //form.AddField("restarted", scenario.Restarted);
        //for (int i = 0; i < scenario.Steps.Count; i++)
        //{
        //    form.AddField("quiz_tries[" + i + "]", scenario.Steps[i].Tries);
        //}
        //form.AddField("id", userData.id);
        //form.AddField("scenario_id", scenario.ID);
        //string url = API_PROD + "/datas/new-update-scenario.php";
        //using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        //{
        //    webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
        //    yield return webRequest.SendWebRequest();
        //    if (webRequest.result == UnityWebRequest.Result.Success)
        //    {
        //        if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
        //        {
        //            Debug.LogError(webRequest.downloadHandler.text);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError(webRequest.error);
        //        webRequest.Dispose();
        //        bool result = false;
        //       // yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
        //        if (result)
        //        {
        //            yield return StartCoroutine(UpdateStudentScenario(scenario));
        //            yield break;
        //        }
        //    }
        //}
    }

    public IEnumerator UpdateStudentAnswersCoroutine(List<Answer> answers)
    {
        yield return null;
        //WWWForm form = new WWWForm();
        //form.AddField("id", userData.id);
        //if (answers.Count > 0)
        //{
        //    form.AddField("quiz_id", answers[0].QuizId);
        //    for (int i = 0; i < answers.Count; i++)
        //    {
        //        form.AddField("answers_id[" + i + "]", answers[i].ID);
        //    }
        //}

        //string url = API_PROD + "/datas/update-student-answers";
        //using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        //{
        //    webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
        //    yield return webRequest.SendWebRequest();

        //    // Process datas
        //    if (webRequest.result != UnityWebRequest.Result.Success)
        //    {
        //        Debug.LogError(webRequest.error);
        //        webRequest.Dispose();
        //        bool result = false;
        //     //   yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
        //        if (result)
        //        {
        //            yield return StartCoroutine(UpdateStudentAnswersCoroutine(answers));
        //            yield break;
        //        }
        //    }
        //}
    }


    public IEnumerator UpdateNumberOfCompletedQuiz()
    {
        yield return null;
    //      WWWForm form = new WWWForm();
    //    form.AddField("id", userData.id);
    //    string url = API_PROD + "/datas/get-completed-quiz.php";
    //    using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
    //    {
    //        yield return webRequest.SendWebRequest();
    //        // Process datas
    //        if (webRequest.result == UnityWebRequest.Result.Success)
    //        {
    //            userData.quizCompleted = int.Parse(webRequest.downloadHandler.text);
    //        }
    //        else
    //        {
    //            Debug.LogError(webRequest.error);
    //            userData.quizCompleted = 0;
    //            bool result = false;
    //         //   yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
    //            if (result)
    //            {
    //                yield return StartCoroutine(UpdateNumberOfCompletedQuiz());
    //                yield break;
    //            }
    //        }
    //    }
    //}

    //public IEnumerator UpdateQuizProgress(List<Answer> answers, int scenarioId)
    //{

    //    WWWForm form = new WWWForm();
    //    form.AddField("scenario_id", scenarioId);
    //    form.AddField("student_id", userData.id);

    //    if (answers.Count > 0)
    //    {
    //        form.AddField("quiz_id", answers[0].QuizId);
    //        for (int i = 0; i < answers.Count; i++)
    //        {
    //            form.AddField("answer_ids[" + i + "]", answers[i].ID);
    //        }
    //    }



    //    string url = API_PROD + "/datas/update-quiz-progress";

    //    using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
    //    {
    //        webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));

    //        yield return webRequest.SendWebRequest();

    //        if (webRequest.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError(webRequest.error);
    //            webRequest.Dispose();

    //            bool result = false;
    //           // yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));

    //            if (result)
    //            {
    //                yield return StartCoroutine(UpdateQuizProgress(answers, scenarioId));
    //                yield break;
    //            }
    //        }
    //    }
    }

    public IEnumerator RemoveQuizProgress(int scenarioId)
{
        yield return null;
    //      WWWForm form = new WWWForm();
    //form.AddField("id", userData.id);
    //form.AddField("scenario_id", scenarioId);

    //string url = API_PROD + "/datas/remove-quiz-progress";

    //using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
    //{
    //    webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));

    //    yield return webRequest.SendWebRequest();

    //    if (webRequest.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError(webRequest.error);
    //    }
    //}
}




    public int GetScenarSuccesses()
    {
        int islesSucceed = 0;
        foreach (Scenario sc in Scenarios)
        {
            if (sc.Successes > 0)
            {
                islesSucceed++;
            }
        }
        return islesSucceed;
    }

    #endregion

   

    public void UpdateDictionnaryItems() //tomodify
    {
        itemsList.UpdateDictionnary();
    }

    #region Account Reward //tomodify
    public IEnumerator AddRewardToMyAccountCoroutine(LevelReward reward, Action<bool> success) //tomodify
    {
        yield return null;
        //WWWForm form = new();
        //form.AddField("id", userData.id);
        //form.AddField("reward_id", reward.ID);
        //string url = API_PROD + "/datas/get-account-reward.php";
        //using UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
        //webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
        //yield return webRequest.SendWebRequest();
        //// Success -> Put Item in user Inventory
        //if (webRequest.result == UnityWebRequest.Result.Success)
        //{
        //    success(true);
        //    if (reward.Item.ID == "1") // Gold
        //    {
        //        userData.gold += reward.Quantity;
        //    }
        //    else if (reward.Item.ID == "2") // Rocks
        //    {
        //        userData.rocksRemaining += reward.Quantity;
        //        IslandBuilder.current?.UpdateRocks();
        //    }
        //    else
        //    {
        //        string[] items = webRequest.downloadHandler.text.Split(rowSeparator);
        //        items = items.SkipLast(1).ToArray();
        //        foreach (var item in items)
        //        {
        //            string[] subs = item.Split(columnSeparator); //0:item id, 1:inventory id

        //            //Adding to inventory
        //            Item refitem = UIManager.current.itemsList.GetItemData(subs[0]);
        //            refitem.id_inventory = subs[1];
        //            userData.inventory.Add(new UserData.ItemPlacement(refitem.id, false, 0, 0, 0, refitem.id_inventory, 0));
        //            UIManager.current?.inventoryUI.AddNewSlot(refitem);
        //        }
        //    }
        //    reward.Acquired = true;
        //}
        //else
        //{
        //    Debug.LogError(webRequest.error);
        //    webRequest.Dispose();
        //    bool result = false;
        //  //  yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
        //    if (result)
        //    {
        //        yield return StartCoroutine(AddRewardToMyAccountCoroutine(reward, success));
        //        yield break;
        //    }
        //    else
        //    {
        //        success(false);
        //    }
        //}
    }
    #endregion

    // Asset Management
    private static readonly Dictionary<string, AsyncOperationHandle<GameObject>> refObj = new();
    public IEnumerator GetPrefabByAddressAsync(string address, Action<GameObject> result)
    {
        GameObject prefab = null;
        if (refObj.ContainsKey(address)) //already been loaded
            prefab = refObj[address].Task.Result;
        else
        {
            AsyncOperationHandle<GameObject> loadOp = Addressables.LoadAssetAsync<GameObject>(address);
            yield return loadOp;
            if (loadOp.Status == AsyncOperationStatus.Succeeded)
            {
                try
                {
                    refObj.Add(address, loadOp);
                }
                catch (ArgumentException e)
                {
                    Debug.LogWarning("Load asset in the same time (" + e.Message + ")");
                }
                prefab = loadOp.Result;
            }
        }
        result(prefab);
    }

    private static readonly Dictionary<string, Sprite> refSprites = new Dictionary<string, Sprite>();

    public IEnumerator CreateSpriteFromURLAsync(string url, Action<Sprite> result)
    {
        //    Sprite resultSprite = null;

        //    if (refSprites.ContainsKey(url))
        //    {
        //        resultSprite = refSprites[url];
        //    }
        //    else
        //    {
        //        //UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
        //        //yield return webRequest.SendWebRequest();
        yield return null;
    //        //if (webRequest.result == UnityWebRequest.Result.Success)
    //        //{
    //        //    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

    //        //    if (texture != null)
    //        //    {
    //        //        // CompressTexture(texture);
    //        //        resultSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 256f, 0, SpriteMeshType.FullRect);
    //        //        refSprites.Add(url, resultSprite);
    //        //    }
    //        //}
    //        //else
    //        //{
    //        //    Debug.LogError($"An error has occurred: {webRequest.error} for URL '{url}'");
    //        //}

    //        //webRequest.Dispose();
    //    }

    //    if (resultSprite == null)
    //    {
    //        // If there's an issue loading the sprite, create a default one
    //        resultSprite = Sprite.Create(new Texture2D(100, 100), new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f), 256f, 0, SpriteMeshType.FullRect);
    //    }

    //    result(resultSprite);
    }

    void CompressTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        texture.LoadImage(bytes);

        texture.Compress(true);
    }

    #region updateTuto //remove

    //public IEnumerator UpdateTutorialProgression() // remove
    //{
    //    string url = API_PROD + "/datas/update-tutorial-progression";
    //    WWWForm form = new();
    //    form.AddField("id", userData.id);
    //    form.AddField("tutorial_step", userData.tutorialStep);

    //    UnityWebRequest req = UnityWebRequest.Post(url, form);
    //    req.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("TOKEN"));
    //    yield return req.SendWebRequest();
    //    if (req.result == UnityWebRequest.Result.ConnectionError)
    //    {
    //        Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
    //        req.Dispose();
    //        bool result = false;
    //        yield return StartCoroutine(ShowErrorDialog("Une erreur de connexion est apparue, connectez-vous à internet puis réessayez.", value => result = value));
    //        if (result)
    //        {
    //            yield return StartCoroutine(UpdateTutorialProgression());
    //            yield break;
    //        }
    //    }
    //    req.Dispose();
    //}
    #endregion

    #region Pop Up   //remove

    [Header("Pop Up")]
    public PopUp errorDialog;
    public Transform errorCanva;
    /// <summary>
    /// Show a Pop Up window, and get the user choice as result <br/>
    /// Use it as follow :<br/><code>  bool result;<br/>  StartCoroutine(Database.ShowErrorDialog("test", value => result = value)); </code>
    /// </summary>
    public static IEnumerator ShowErrorDialog(string text, Action<bool> choice)
    {
        PopUp dialog = Instance.errorDialog.Instance(text, PopupStyle.ConnexionFailed);
        // Wait for a choice
        while (dialog.result == dialog.NONE)
        {
            yield return null;
        }

        if (dialog.result == dialog.YES)
        {
            choice(true);
        }
        else if (dialog.result == dialog.CANCEL)
        {
            choice(false);
        }

        dialog.Destroy();
    }
    #endregion

    #region Quit Game
    public static void QuittingGame()
    {
        if (!popupOpen)
            Instance.StartCoroutine(QuitGame());
    }
    private static bool popupOpen = false;
    private static IEnumerator QuitGame()
    {
        popupOpen = true;
        PopUp dialog = Instance.errorDialog.Instance("Quitter le jeu", PopupStyle.YesNo);
        yield return null;
        while (dialog.result == dialog.NONE)
        {
            yield return null; // wait
        }

        if (dialog.result == dialog.YES)
        {
            Application.Quit();
        }

        dialog.Destroy();
        popupOpen = false;
    }
    #endregion
}
