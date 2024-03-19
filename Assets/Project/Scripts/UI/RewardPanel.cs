using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPanel : MonoBehaviour
{
    [SerializeField] private Transform rewardsParent;
    [SerializeField] private GameObject rewardPrefab;

    public void FillPanel(Scenario scenario)
    {
        foreach (Transform child in rewardsParent)
        {
            if (child != rewardsParent)
            {
                Destroy(child.gameObject);
            }
        }
        //FillPanelWithRewards(new List<ScenarioReward>(scenario.Rewards));
    }

    public void FillPanelWithRewards(List<ScenarioReward> rewards)
    {
        foreach (ScenarioReward reward in rewards)
        {
            GameObject rwd = Instantiate(rewardPrefab, rewardsParent);
            Image rewardImg = rwd.GetComponentInChildren<Image>();
            TMP_Text rewardQty = rwd.GetComponentInChildren<TMP_Text>();
            if (rewardImg != null && rewardQty != null)
            {
                rewardImg.sprite = reward.Item.Sprite;
                rewardQty.text = $"<b>{reward.Quantity}</b><size=50%>x</size>";
            }
        }
    }

    public void CloseReward()
    {
        gameObject.SetActive(false);
    }
}
