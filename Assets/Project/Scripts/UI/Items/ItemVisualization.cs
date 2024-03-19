using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemVisualization : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject itemVisualPanel;
    [Header("Panel Content")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemDescription;

    private void Start()
    {
        itemVisualPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemVisualPanel.SetActive(true);
        FillPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemVisualPanel.SetActive(false);
    }
    public void OnSelect(BaseEventData eventData)
    {
        itemVisualPanel.SetActive(true);
        FillPanel();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        itemVisualPanel.SetActive(false);
    }

    private void FillPanel()
    {
        // TODO : Fill With Item properties (class item do not exist)
        Image img = GetComponent<Image>();
        if (img != null && itemImage != null)
        {
            itemImage.sprite = img.sprite;
        }
        if (itemName != null)
            itemName.text = "Nom de l'item";
        if (itemDescription != null)
            itemDescription.text = "Description de l'item";
    }


}
