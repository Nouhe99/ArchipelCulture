public class ChestInteraction : Interactable
{
    protected override void Interact(bool centerOnObject = true)
    {
        base.Interact(false);
        ScenarioManager.Instance?.OpenReward();
    }
}
