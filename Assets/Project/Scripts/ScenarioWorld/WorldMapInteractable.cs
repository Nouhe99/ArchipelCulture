using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WorldMapInteractable : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent triggerEnterEvent;
    public UnityEvent triggerExitEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BoatController.Instance != null)
        {
            BoatController.Instance.GoTo(Camera.main.ScreenToWorldPoint(eventData.pressPosition));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BoatController>() != null)
        {
            triggerEnterEvent.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<BoatController>() != null)
        {
            triggerExitEvent.Invoke();
        }
    }
}
