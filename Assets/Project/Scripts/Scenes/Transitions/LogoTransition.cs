using System.Collections;
using TMPro;
using UnityEngine;

public class LogoTransition : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private TMP_Text transitionDescription;

    [Header("Animation")]
    [SerializeField] private Animator transition;
    [SerializeField] private float minTransitionDuration;
    [SerializeField] private string startTransitionTrigger;
    [SerializeField] private string endTransitionTrigger;

    public static LogoTransition Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        sceneController.CurrentlyLoadingScene = "";
    }

    public void CancelTransition()
    {
        StopAllCoroutines();
        EndTransition();
    }

    private bool running = false;
    public IEnumerator LoadSceneAfterCoroutines(string scene, string loadDescription, params IEnumerator[] coroutines)
    {
        if (running) yield break;
        running = true;
        float timer = 0f;
        transitionDescription.text = loadDescription;
        StartTransition();
        foreach (IEnumerator coroutine in coroutines)
        {
            timer += Time.deltaTime;
            yield return StartCoroutine(coroutine);
        }

        AsyncOperation asyncOps = sceneController.LoadSceneAsync(scene);
        if (asyncOps == null)
        {
            running = false;
            yield break;
        }
        asyncOps.allowSceneActivation = false;
        while (!asyncOps.isDone && timer < minTransitionDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        asyncOps.allowSceneActivation = true;
        EndTransition();
        running = false;
    }

    public void StartTransition()
    {
        transition.SetTrigger(startTransitionTrigger);
    }

    public void EndTransition()
    {
        transition.SetTrigger(endTransitionTrigger);
        if (Database.Instance.errorCanva.childCount > 0)
        {
            for (int i = 0; i < Database.Instance.errorCanva.childCount; i++)
            {
                Destroy(Database.Instance.errorCanva.GetChild(i).gameObject);
            }
        }
    }
}
