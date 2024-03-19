using UnityEngine;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour, IPointerDownHandler
{
    protected virtual void Interact(bool centerOnObject = true)
    {
        if (CameraBehavior.Instance != null && centerOnObject)
        {
            CameraBehavior.Instance.CenterCameraOnGameObject(gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        Interact();
    }
}
