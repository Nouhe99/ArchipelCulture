using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private Image bar;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TMP_Text rewardQty;
    [SerializeField] private TMP_Text level;

    private int startingLevel;
    private int rewardLevel;

    //public void InitializeExpBar(Experience exp, LevelReward lvlReward, int startLevel)
    //{
    //    rewardLevel = lvlReward.level;
    //    experience = exp;
    //    startingLevel = startLevel;

    //    UpdateExpBar();
    //    rewardImage.sprite = lvlReward.reward.Item.Sprite;
    //    rewardQty.text = $"+{lvlReward.reward.Quantity}";
    //    level.text = $"Niv.{lvlReward.level}";
    //}

    //public void UpdateExpBar()
    //{
    //    if(experience != null)
    //    {
    //        float fillAmount = 0.0f;
    //        for(int i = startingLevel; i <= rewardLevel; i++)
    //        {
    //            fillAmount += experience.GetLevelPercentage(i) / (rewardLevel - startingLevel);
    //        }
    //        if (gameObject.activeInHierarchy && fillAmount != 0.0f)
    //        {
    //            StartCoroutine(FillXPBarToAmountInSeconds(fillAmount, 1.0f));
    //        }
    //        else
    //        {
    //            bar.fillAmount = fillAmount;
    //        }
    //    }
    //}

    //private IEnumerator FillXPBarToAmountInSeconds(float targetAmount, float seconds)
    //{
    //    float timeElapsed = 0.0f;
    //    float startAmount = bar.fillAmount;
    //    while (timeElapsed < seconds)
    //    {
    //        bar.fillAmount = Mathf.Lerp(startAmount, targetAmount, timeElapsed / seconds);
    //        timeElapsed += Time.deltaTime;
    //        yield return null;
    //    }
    //}
}
