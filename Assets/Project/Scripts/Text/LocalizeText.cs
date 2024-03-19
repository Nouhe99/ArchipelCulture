using System.Collections;
using TMPro;
using UnityEngine;

public class LocalizeText : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private string key;

    private TMP_Text textField;

    private void Awake()
    {
        textField = GetComponent<TMP_Text>();
    }

    private IEnumerator Start()
    {
        if ((id <= 0 && string.IsNullOrEmpty(key)) || textField == null) yield break;
        yield return new WaitUntil(() => ExternalTexts.IsLoaded());
        textField.SetText(ExternalTexts.GetContent(key));
    }
}
