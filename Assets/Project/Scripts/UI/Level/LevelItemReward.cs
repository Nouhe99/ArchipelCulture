using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelItemReward : MonoBehaviour, IPointerClickHandler
{
    public SceneController sceneController { get; set; }

    private LevelReward levelReward;
    public LevelReward LevelReward
    {
        get { return levelReward; }
        set
        {
            levelReward = value;
            quantity.text = $"<size=80%>x</size>{levelReward.Quantity}";
            requirement.text = $"{levelReward.Requirement}";
            image.sprite = levelReward.Item.Sprite;
            acquiredImage.SetActive(levelReward.Acquired);
            if (Database.Instance.userData.quizCompleted >= LevelReward.Requirement && !levelReward.Acquired)
            {
                notif.SetActive(true);
            }
        }
    }

    [SerializeField] private TMP_Text requirement;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text quantity;
    [SerializeField] private GameObject notif;
    [SerializeField] private GameObject acquiredImage;
    [SerializeField] private FloatingReward floatingRewardPrefab;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (LevelReward.Acquired)
        {
            UIManager.ShowInfoMessage("Tu as déjà récupéré cette récompense !");
            return;
        }
        if (Database.Instance.userData.quizCompleted < LevelReward.Requirement)
        {
            UIManager.ShowInfoMessage("Termine plus d'aventures pour débloquer cette récompense.");
            return;
        }
        if (openRewardCoroutine == null)
        {
            openRewardCoroutine = StartCoroutine(OpenReward_Coroutine());
        }
    }

    private Coroutine openRewardCoroutine = null;
    private IEnumerator OpenReward_Coroutine()
    {
        bool success = false;
        yield return StartCoroutine(Database.Instance.AddRewardToMyAccountCoroutine(LevelReward, value => success = value));
        if (success && sceneController != null)
        {
            //NOTE: items are adding to inventory and saved to db in RewardManager.
            UIManager.current.topBarPanel.dotNotification.SetNotificationCount(--UIManager.current.topBarPanel.nbOfRewardsWaiting);
            //sceneController.LoadRewardScene();
            Profil profil = UIManager.current.profil;
            if (profil != null)
            {
                var ftr = Instantiate(floatingRewardPrefab, profil.transform);
                switch (LevelReward.Item.Type)
                {
                    case ItemType.Resource:
                        ftr.PlayRewardAnimation(transform, LevelReward.Quantity, profil.CoinsImage, LevelReward.Item.Sprite);
                        break;
                    case ItemType.Rock:
                        ftr.PlayRewardAnimation(transform, LevelReward.Quantity, profil.RocksImage, LevelReward.Item.Sprite);
                        break;
                    case ItemType.Decoration:
                        ftr.PlayRewardAnimation(transform, LevelReward.Quantity, profil.DecorationsImage, LevelReward.Item.Sprite);
                        break;
                    case ItemType.Building:
                        ftr.PlayRewardAnimation(transform, LevelReward.Quantity, profil.BuildingsImage, LevelReward.Item.Sprite);
                        break;
                    default:
                        ftr.PlayRewardAnimation(transform, LevelReward.Quantity, profil.CoinsImage, LevelReward.Item.Sprite);
                        break;
                }
            }
            profil.UpdateResources();
            LevelReward.Acquired = true;
            acquiredImage.SetActive(levelReward.Acquired);
            notif.SetActive(false);
        }
        openRewardCoroutine = null;
    }
}
