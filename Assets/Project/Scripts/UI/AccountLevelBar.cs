using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelReward : Reward
{
    public string ID;
    public int Requirement;
    public bool Acquired;

    public LevelReward(string id, ItemData item, int qty, int req, bool acquired) : base(item, qty)
    {
        ID = id;
        Requirement = req;
        Acquired = acquired;
    }

    public bool RewardReached(int requiredNumber)
    {
        return requiredNumber >= Requirement ? true : false;
    }
}

[RequireComponent(typeof(ScrollRect))]
public class AccountLevelBar : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    private ScrollRect scrollRect;
    [SerializeField] private Image fillBackground;
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text quizCompleted;
    private List<LevelReward> levelRewards;
    [SerializeField][Min(0)] private int spacing;
    [SerializeField] private LevelItemReward levelRewardPrefab;
    [SerializeField] private RectTransform currentPosition;
    private int studentQuizCompleted;
    private bool Initialized = false;

    private IEnumerator InitializeAsync()
    {
        yield return StartCoroutine(Database.Instance.UpdateNumberOfCompletedQuiz());
        studentQuizCompleted = Database.Instance.userData.quizCompleted;
        scrollRect = GetComponent<ScrollRect>();
        levelRewards = Database.Instance?.LevelRewards;
        levelRewards.Sort((x, y) => x.Requirement.CompareTo(y.Requirement));
        for (int i = 0; i < levelRewards.Count; i++)
        {
            LevelItemReward obj = Instantiate(levelRewardPrefab, fillBackground.transform);
            obj.sceneController = sceneController;
            obj.LevelReward = levelRewards[i];
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2((i + 1) * spacing, levelRewardPrefab.GetComponent<RectTransform>().anchoredPosition.y);
            yield return null;

        }
        fillBackground.rectTransform.sizeDelta = new Vector2((levelRewards.Count + 1) * spacing, fillBackground.rectTransform.rect.height);
        currentPosition.SetAsLastSibling();
        LayoutRebuilder.MarkLayoutForRebuild(scrollRect.content);
        yield return new WaitForEndOfFrame();
        Initialized = true;
    }


    private Coroutine initCoroutine = null;
    public IEnumerator UpdateFillBarCoroutine(bool lerp = false)
    {
        if (!Initialized)
        {
            if (initCoroutine == null)
            {
                initCoroutine = StartCoroutine(InitializeAsync());
                yield return initCoroutine;
                initCoroutine = null;
            }
            else
            {
                yield break;
            }
        }
        if (levelRewards == null || fill == null)
        {
            yield break;
        }
        int nbOfRewardReached = 0;
        foreach (LevelReward lvlRew in levelRewards)
        {
            if (lvlRew.RewardReached(studentQuizCompleted))
            {
                nbOfRewardReached++;
            }
        }
        quizCompleted.text = $"{studentQuizCompleted} <sprite name=isle>";

        float rewardCountPlusOne = levelRewards.Count + 1;
        int lastLevelIndex = levelRewards.FindLastIndex(reward => reward.RewardReached(studentQuizCompleted) == true);
        float lastLevelRequirement = lastLevelIndex == -1 ? 0 : levelRewards[lastLevelIndex].Requirement;
        LevelReward nextLevelReward = levelRewards.Find(reward => reward.RewardReached(studentQuizCompleted) == false);
        float nextLevelRequirement = 0;
        if (nextLevelReward != null)
        {
            nextLevelRequirement = nextLevelReward.Requirement;
        }

        if (nextLevelRequirement == 0)
        {
            fill.fillAmount = 1;
            RectOffset padding = scrollRect.content.GetComponent<HorizontalLayoutGroup>().padding;
            float currentXPosition = fill.fillAmount * (scrollRect.content.sizeDelta.x - padding.left - padding.right);
            currentPosition.anchoredPosition3D = new Vector3(currentXPosition, currentPosition.anchoredPosition3D.y, currentPosition.anchoredPosition3D.z);
        }
        else
        {
            scrollRect.content.anchoredPosition3D = new Vector3(-lastLevelIndex * spacing, scrollRect.content.anchoredPosition3D.y, scrollRect.content.anchoredPosition3D.z);
            float newFillAmount = (float)(nbOfRewardReached / rewardCountPlusOne) + ((studentQuizCompleted - lastLevelRequirement) / (nextLevelRequirement - lastLevelRequirement)) * (1f / rewardCountPlusOne);
            RectOffset padding = scrollRect.content.GetComponent<HorizontalLayoutGroup>().padding;
            float currentXPosition = newFillAmount * (scrollRect.content.sizeDelta.x - padding.left - padding.right);
            currentPosition.anchoredPosition3D = new Vector3(currentXPosition, currentPosition.anchoredPosition3D.y, currentPosition.anchoredPosition3D.z);
            if (lerp)
            {
                StartCoroutine(SetFillAmountWithLerpCoroutine(newFillAmount, 2f));
            }
            else
            {
                fill.fillAmount = newFillAmount;
            }
        }
    }

    private IEnumerator SetFillAmountWithLerpCoroutine(float newFillAmount, float seconds)
    {
        float timeElapsed = 0.0f;
        float startAmount = fill.fillAmount;
        while (timeElapsed < seconds)
        {
            fill.fillAmount = Mathf.Lerp(startAmount, newFillAmount, timeElapsed / seconds);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
