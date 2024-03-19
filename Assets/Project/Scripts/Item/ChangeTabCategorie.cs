using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ChangeTabCategorie : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private Toggle change;

    private void Start()
    {
        change = GetComponent<Toggle>();
    }

    public void ToggleValueChanged()
    {
        if (change == null) return;

        if (change.isOn)
        {
            content.SetActive(true);
        }
        else
        {
            content.SetActive(false);
        }
    }
}
