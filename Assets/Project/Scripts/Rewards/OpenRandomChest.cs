using System.Collections.Generic;
using UnityEngine;

public class OpenRandomChest : MonoBehaviour
{
    public ItemData chest;
    public void Open()
    {
        List<Reward> giftReward = new()
        {
            new Reward(chest, 1)
        };
        CrossSceneInformation.Rewards = giftReward;
        UIManager.current.sceneController.LoadRewardScene();
    }
}
