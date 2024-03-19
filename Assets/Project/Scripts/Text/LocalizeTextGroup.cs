using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizeTextGroup : MonoBehaviour
{
    [System.Serializable]
    public struct LocalizedText
    {
        public TMP_Text TextField;
        [Header("Identifier")]
        public int LocalizedID;
        public string LocalizedKey;
    }

    [SerializeField] private List<LocalizedText> localizedTexts = new();
    public IEnumerator Start()
    {
        yield return new WaitUntil(() => ExternalTexts.IsLoaded());
        foreach (LocalizedText text in localizedTexts)
        {
            if (text.LocalizedID > 0)
            {
                text.TextField?.SetText(ExternalTexts.GetContent(text.LocalizedID));
            }
            else if (string.IsNullOrEmpty(text.LocalizedKey) == false)
            {
                text.TextField?.SetText(ExternalTexts.GetContent(text.LocalizedKey));
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var localizedText in GetComponentsInChildren<LocalizeText>(true))
        {
            Debug.LogWarning($"Possible duplicate from <b>{localizedText.gameObject.name}</b>", localizedText.gameObject);
        }
    }
#endif
}
