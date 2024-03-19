using System.Collections.Generic;
using UnityEngine;

public static class CrossSceneInformation
{
    public static Scenario SelectedScenario;
    public static Vector3 BoatPosition_ScenarioWorld;
    public static List<Reward> Rewards;
    public static Reward AccountReward;
    public static int CompletedQuizSinceLastTime = 0;


    public static void Reset()
    {
        SelectedScenario = null;
        BoatPosition_ScenarioWorld = Vector3.zero;
        Rewards = null;
        AccountReward = null;
        CompletedQuizSinceLastTime = 0;
    }
}
