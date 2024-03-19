using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text buttonText;

    private Vector2 location;
    private void Start()
    {
        buttonText.text = "Voyage rapide";
    }

    public void StartQuest(string title, string description, Vector2 location)
    {
        ModifyTitle(title);
        ModifyDescription(description);
        this.location = location;
    }

    public void ModifyTitle(string title)
    {
        this.title.text = title;
    }

    public void ModifyDescription(string description)
    {
        this.description.text = description;
    }

    public void CompleteQuest()
    {
        Destroy(gameObject);
    }

    public void Interact()
    {
        // Make the player go to this quest location
        Debug.Log("Interacted with" + name);
        BoatController.Instance.GoTo(location);
    }
}
