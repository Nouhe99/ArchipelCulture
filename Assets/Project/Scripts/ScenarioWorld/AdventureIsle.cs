using UnityEngine;
using UnityEngine.EventSystems;

public class AdventureIsle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject questIndicator;
    [SerializeField] private GameObject chestIndicator;
    [SerializeField] private GameObject successIndicator;
    [SerializeField] private GameObject failedIndicator;
    [SerializeField] private GameObject StarIndicator;
    [SerializeField] private Sprite imgIsle;

    private SpriteRenderer starIndicatorSpriteRenderer;
    private CompletionState completionState = CompletionState.NotCompleted;

    [SerializeField] private Sprite completedOnceSprite;
    [SerializeField] private Sprite completedTwiceSprite;
    [SerializeField] private Sprite completedThriceSprite;

    private Scenario scenario;

    public Scenario Scenario
    {
        get { return scenario; }
        set { scenario = value; }
    }

    public enum CompletionState
    {
        NotCompleted,
        CompletedOnce,
        CompletedTwice,
        CompletedThrice
    }

    public void Awake()
    {
        starIndicatorSpriteRenderer = StarIndicator.GetComponent<SpriteRenderer>();

    }

    public void Start()
    {
        DisableStarIndicator();
    }

    public void EnableQuestIndicator()
    {
        questIndicator.SetActive(true);
        DisableChestIndicator();
        DisableSuccessIndicator();
        DisableFailedIndicator();
    }

    public void DisableQuestIndicator()
    {
        questIndicator.SetActive(false);
    }

    public void EnableChestIndicator()
    {
        chestIndicator.SetActive(true);
        DisableQuestIndicator();
        DisableSuccessIndicator();
        DisableFailedIndicator();
    }

    public void DisableChestIndicator()
    {
        chestIndicator.SetActive(false);
    }

    public void EnableSuccessIndicator()
    {
        successIndicator.SetActive(true);
        DisableQuestIndicator();
        DisableChestIndicator();
        DisableFailedIndicator();
    }
    public void DisableSuccessIndicator()
    {
        successIndicator.SetActive(false);
    }

    public void EnableFailedIndicator()
    {
        failedIndicator.SetActive(true);
        DisableChestIndicator();
        DisableQuestIndicator();
        DisableSuccessIndicator();
    }

    public void DisableFailedIndicator()
    {
        failedIndicator.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BoatController>() != null)
        {
            WorldManager.Instance?.EnableScenarioPanel(scenario);
            EnableStarIndicator();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<BoatController>() != null)
        {
            WorldManager.Instance?.DisableScenarioPanel();
            DisableStarIndicator();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BoatController.Instance != null)
        {
            BoatController.Instance.GoTo(Camera.main.ScreenToWorldPoint(eventData.pressPosition));

        }
    }

    public void EnableStarIndicator()
    {

        StarIndicator.SetActive(true);

        if (completionState > CompletionState.CompletedThrice)
        {
            completionState = CompletionState.CompletedThrice;
        }

        // Change the StarIndicator sprite based on the completion state
        switch (completionState)
        {
            case CompletionState.CompletedOnce:
                starIndicatorSpriteRenderer.sprite = completedOnceSprite;
                break;
            case CompletionState.CompletedTwice:
                starIndicatorSpriteRenderer.sprite = completedTwiceSprite;
                break;
            case CompletionState.CompletedThrice:
                starIndicatorSpriteRenderer.sprite = completedThriceSprite;
                break;
            default:
                break;
        }
    }


    public void DisableStarIndicator()
    {
        StarIndicator.SetActive(false);
    }

    public void UpdateCompletionState(CompletionState newState)
    {
        completionState = newState;
        EnableStarIndicator();
    }

    // // // SPRITE // // //
    public Sprite GetSprite()
    {
        if (imgIsle != null) return imgIsle;
        return null;
    }
}
