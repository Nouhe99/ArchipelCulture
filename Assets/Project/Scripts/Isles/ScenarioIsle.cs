using System.Collections.Generic;
using UnityEngine;

public class ScenarioIsle : MonoBehaviour
{
    [SerializeField] private GameObject chestFX;
    public void SetActiveChestVFX(bool enable)
    {
        chestFX.SetActive(enable);
    }

    public AdventureIsle PreviewIsle;

    [SerializeField] private List<Spot> spots;
    public List<Spot> Spots
    {
        get { return spots; }
    }
    public void InitializeIsle(Scenario scenario, bool reset = false)
    {
        InitializeSpots(scenario, reset);
    }

    private void InitializeSpots(Scenario scenario, bool reset)
    {
        // Check if there is atleast one spot
        if (spots == null)
        {
            return;
        }

        // Assign a spot to each quiz
        int i;
        for (i = 0; i < scenario.Steps.Count; i++)
        {
            // Check if the number of quiz exceeds the number of spot
            if (i < spots.Count)
            {
                spots[i].StepIndex = i;
                switch (scenario.Steps[i].State)
                {
                    // Reset Quiz state if not completed successfully => user can do it again
                    case QuizState.Failed:
                        scenario.Steps[i].State = QuizState.Pending;
                        break;
                    case QuizState.Completed:
                        if (reset)
                        {
                            scenario.Steps[i].State = QuizState.Pending;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // Hide exceeding number of spots
        for (int y = i; y < Spots.Count; y++)
        {
            Spots[y].gameObject.SetActive(false);
        }

        UpdateSpots();
    }

    public void UpdateSpots()
    {
        bool firstUnansweredQuizReached = false;
        foreach (Spot spot in Spots)
        {
            if (spot.StepIndex != -1)
            {
                switch (ScenarioManager.Instance.ActiveScenario.Steps[spot.StepIndex].State)
                {
                    case QuizState.Pending:
                        if (firstUnansweredQuizReached)
                        {
                            spot.SetSpotFlag(QuizState.None);
                            //spot.gameObject.SetActive(false);
                            //continue;
                        }
                        else
                        {
                            firstUnansweredQuizReached = true;
                            spot.SetSpotFlag(QuizState.Pending);
                        }
                        break;
                    case QuizState.Failed:
                        spot.SetSpotFlag(QuizState.Failed);
                        break;
                    case QuizState.Completed:
                        spot.SetSpotFlag(QuizState.Completed);
                        break;
                    default:
                        spot.SetSpotFlag(QuizState.Pending);
                        break;
                }
                spot.gameObject.SetActive(true);
            }
        }
    }
}
