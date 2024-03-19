using UnityEngine;
using UnityEngine.EventSystems;
using static UIManager;

[RequireComponent(typeof(Collider2D))]
public class OpenWindow : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject window;
    [SerializeField] private Panel typeWindow;
    [SerializeField] private string customWindow;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (window != null)
            current.topBarPanel.TriggerPanel(window);
        else if (typeWindow != Panel.NULL)
            current.OpenClosePanel(typeWindow);
        else if (customWindow != null)
        {
            switch (customWindow)
            {
                

                
                default: break;
            }
        }

        PlayAudio.Instance.bank.PressButtonNormal();
    }
}
