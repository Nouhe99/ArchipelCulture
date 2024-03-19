using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TextEventHandler : MonoBehaviour
#if UNITY_STANDALONE || UNITY_EDITOR
    , IPointerMoveHandler
#else
    , IPointerDownHandler, IPointerUpHandler
#endif
{
    [Serializable]
    public class LinkSelectionEvent : UnityEvent<string, string, int> { }

    [Serializable]
    public class LinkDeselectionEvent : UnityEvent { }

    /// <summary>
    /// Event delegate triggered when pointer is over a link.
    /// </summary>
    public LinkSelectionEvent OnLinkSelection
    {
        get { return onLinkSelection; }
        set { onLinkSelection = value; }
    }
    [SerializeField]
    private LinkSelectionEvent onLinkSelection = new LinkSelectionEvent();

    /// <summary>
    /// Event delegate triggered when pointer quit a link.
    /// </summary>
    public LinkDeselectionEvent OnLinkDeselection
    {
        get { return onLinkDeselection; }
        set { onLinkDeselection = value; }
    }
    [SerializeField]
    private LinkDeselectionEvent onLinkDeselection = new LinkDeselectionEvent();

    private TMP_Text textComponent;
    private Camera mainCamera;
    private Canvas canvas;

    private int selectedLink = -1;

    void Awake()
    {
        // Get a reference to the text component.
        textComponent = gameObject.GetComponent<TMP_Text>();

        // Get a reference to the camera rendering the text taking into consideration the text component type.
        if (textComponent.GetType() == typeof(TextMeshProUGUI))
        {
            canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    mainCamera = null;
                else
                    mainCamera = canvas.worldCamera;
            }
        }
        else
        {
            mainCamera = Camera.main;
        }
    }

    private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)
    {
        if (onLinkSelection != null)
            onLinkSelection.Invoke(linkID, linkText, linkIndex);
    }

    private void SendOnLinkDeselection()
    {
        if (onLinkDeselection != null)
        {
            onLinkDeselection.Invoke();
        }
    }
#if UNITY_STANDALONE || UNITY_EDITOR
    public void OnPointerMove(PointerEventData eventData)
    {
        if (TMP_TextUtilities.IsIntersectingRectTransform(textComponent.rectTransform, eventData.position, mainCamera))
        {
            // Check if pointer intersects with any links.
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, mainCamera);
            // Handle new Link selection.
            if (linkIndex != -1 && linkIndex != selectedLink)
            {
                selectedLink = linkIndex;

                // Get information about the link.
                TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];

                // Send the event to any listeners.
                SendOnLinkSelection(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkIndex);
            }
            else if (linkIndex == -1 && selectedLink != -1)
            {
                selectedLink = -1;
                SendOnLinkDeselection();
            }
        }
    }
#else

    public void OnPointerDown(PointerEventData eventData)
    {
        if (TMP_TextUtilities.IsIntersectingRectTransform(textComponent.rectTransform, eventData.position, mainCamera))
        {
            // Check if pointer intersects with any links.
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, mainCamera);
            // Handle new Link selection.
            if (linkIndex != -1 && linkIndex != selectedLink)
            {
                selectedLink = linkIndex;

                // Get information about the link.
                TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];

                // Send the event to any listeners.
                SendOnLinkSelection(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkIndex);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (TMP_TextUtilities.IsIntersectingRectTransform(textComponent.rectTransform, eventData.position, mainCamera))
        {
            //// Check if pointer intersects with any links.
            //int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, mainCamera);

            if (/*linkIndex == -1 && */selectedLink != -1)
            {
                selectedLink = -1;
                SendOnLinkDeselection();
            }
        }
    }
#endif
}
