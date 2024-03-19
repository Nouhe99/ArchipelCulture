using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "ScriptableObject/Scene Controller")]
public class SceneController : ScriptableObject
{
#if UNITY_EDITOR
    [Header("Scene References")]
    [SerializeField] private SceneAsset homeScene;
    [SerializeField] private SceneAsset loginScene;
    [SerializeField] private SceneAsset registerScene;
    [SerializeField] private SceneAsset loadingScene;
    [SerializeField] private SceneAsset scenarioScene;
    [SerializeField] private SceneAsset rewardScene;
    [SerializeField] private SceneAsset missionScene;
    [SerializeField] private SceneAsset postalScene;

    private void OnValidate()
    {
        if (homeScene != null) HomeScene = homeScene.name;
        if (loginScene != null) LoginScene = loginScene.name;
        if (scenarioScene != null) ScenarioScene = scenarioScene.name;
        if (rewardScene != null) RewardScene = rewardScene.name;
        if (missionScene != null) MissionScene = missionScene.name;
      
    }
#endif

    [Header("Scene Names")]
    public string HomeScene;
    public string LoginScene;
    public string ScenarioScene;
    public string RewardScene;
    public string MissionScene;
    //public string PostalScene;
   

    [HideInInspector] public string PreviousScene = "";
    [HideInInspector] public string ActiveScene = "";
    [HideInInspector] public string CurrentlyLoadingScene = "";

    public void UpdateSceneReferences(string newActiveScene)
    {
        if (ActiveScene != null)
        {
            PreviousScene = ActiveScene;
        }
        ActiveScene = newActiveScene;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (CurrentlyLoadingScene == scene.name)
        {
            CurrentlyLoadingScene = "";
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadPreviousScene()
    {
        // Check whether the same scene is already loading
        if (CurrentlyLoadingScene == PreviousScene) return;
        string sceneToLoad = PreviousScene;
        SceneManager.sceneLoaded += OnSceneLoaded;

        CurrentlyLoadingScene = sceneToLoad;
        UpdateSceneReferences(PreviousScene);
        if (PreviousScene != null && PreviousScene != ActiveScene)
        {
            SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            LoadHomeScene();
        }
    }

    public void LoadScene(string sceneToLoad, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Check whether the same scene is already loading
        if (CurrentlyLoadingScene == sceneToLoad) return;
        CurrentlyLoadingScene = sceneToLoad;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // TODO : Move this elsewhere
        if (BoatController.Instance != null)
        {
            if (ActiveScene == MissionScene)
            {
                BoatController.Instance?.SaveMyPosition_ScenarioWorld();
            }
        }
        // END TODO
        if (mode == LoadSceneMode.Single)
        {
            UpdateSceneReferences(sceneToLoad);
        }

        SceneManager.LoadSceneAsync(sceneToLoad, mode);
    }

    public AsyncOperation LoadSceneAsync(string sceneToLoad, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // Check whether the same scene is already loading
        if (CurrentlyLoadingScene == sceneToLoad) return null;
        CurrentlyLoadingScene = sceneToLoad;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (mode == LoadSceneMode.Single)
        {
            UpdateSceneReferences(sceneToLoad);
        }
        return SceneManager.LoadSceneAsync(sceneToLoad, mode);
    }

    public void LoadHomeScene()
    {
        LoadScene(HomeScene);
    }

    public void LoadLoginScene()
    {
        LoadScene(LoginScene);
    }

    public void LoadMissionScene()
    {
        LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(MissionScene, ""));
    }

    public void LoadScenarioScene()
    {
        LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(ScenarioScene, "En route vers l'aventure !"));
    }

    public AsyncOperation LoadRewardScene()
    {
        return LoadSceneAsync(RewardScene, LoadSceneMode.Additive);
    }

    public void LoadPostalScene()
    {
        //LogoTransition.Instance?.StartCoroutine(LogoTransition.Instance.LoadSceneAfterCoroutines(PostalScene, "Vers le centre de tri postal..."));
    }

    public void UnloadRewardScene()
    {
        if (SceneManager.GetSceneByName(RewardScene).isLoaded)
        {
            SceneManager.UnloadSceneAsync(RewardScene);
        }
    }
}
