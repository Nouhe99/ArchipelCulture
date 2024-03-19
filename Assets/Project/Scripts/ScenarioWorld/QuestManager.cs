using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    [Header("Side Quests")]
    [SerializeField] private QuestUI questUiPrefab;
    [SerializeField] private ScrollRect questScrollView;
    public List<QuestUI> Quests { get; private set; }

    private void Awake()
    {
        Quests = new List<QuestUI>();
    }

    public void AddSideQuest(string title, string description, Vector2 location)
    {
        QuestUI questUi = Instantiate(questUiPrefab, questScrollView.content);
        Quests.Add(questUi);
        questUi.StartQuest(title, description, location);
    }

    private bool opened = false;
    public async void ToggleSideQuests()
    {
        PlayAudio.Instance?.bank.Book(!opened);
        opened = !opened;
        if (!opened)
            await questScrollView.gameObject.GetComponent<UIAnimation>().AnimateFromStartToEndAsync();
        else
            await questScrollView.gameObject.GetComponent<UIAnimation>().AnimateFromEndToStartAsync();

    }

}
