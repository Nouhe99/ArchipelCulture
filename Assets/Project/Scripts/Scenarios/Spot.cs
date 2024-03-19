using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Spot : Interactable
{
    public int StepIndex { get; set; }
    [SerializeField] private GameObject todoAnimation;
    [SerializeField] private GameObject winAnimation;
    [SerializeField] private GameObject loseAnimation;
    [SerializeField] private GameObject neutralSpot;
    private Collider2D thiscollider;

    private void Awake()
    {
        StepIndex = -1;
        thiscollider = gameObject.GetComponent<Collider2D>();
    }


    protected override void Interact(bool centerOnObject = true)
    {
        base.Interact(false);
        ScenarioManager.Instance?.LaunchStage(ScenarioManager.Instance.ActiveScenario.GetQuizOfStep(StepIndex));
    }

    // Change animation state of the spot
    public void SetSpotFlag(QuizState state)
    {
        switch (state)
        {
            case QuizState.Pending:
                thiscollider.enabled = true;
                todoAnimation.SetActive(true);
                winAnimation.SetActive(false);
                loseAnimation.SetActive(false);
                break;
            case QuizState.Completed:
                thiscollider.enabled = true;
                neutralSpot.SetActive(false);
                todoAnimation.SetActive(false);
                winAnimation.SetActive(true);
                loseAnimation.SetActive(false);
                break;
            case QuizState.Failed:
                thiscollider.enabled = true;
                todoAnimation.SetActive(false);
                winAnimation.SetActive(false);
                loseAnimation.SetActive(true);
                break;
            case QuizState.None:
                thiscollider.enabled = false;
                todoAnimation.SetActive(false);
                winAnimation.SetActive(false);
                loseAnimation.SetActive(false);
                break;
            default:
                thiscollider.enabled = true;
                todoAnimation.SetActive(true);
                winAnimation.SetActive(false);
                loseAnimation.SetActive(false);
                break;
        }
    }

}
