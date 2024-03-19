using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectProfile : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject passwordScreen;
    [SerializeField] private GameObject profileSelection;

    [Header("Selected Profile")]
    [SerializeField] private Image passwordProfilePicture;
    [SerializeField] private TMP_Text passwordUsernameText;
    [SerializeField] private TMP_Text passwordTitleText;
    public int SelectedProfileID
    {
        get
        {
            if (selectedObject != null) return selectedObject.ID;
            else return -1;
        }
    }
    [Header("Carousel")]
    public ClassProfileCard profileCardPrefab; // Prefab de l'objet
    private List<ClassProfileCard> profileCards = new(); // Liste des objets à afficher
    public float spacing = 50f; // Espacement horizontal entre les objets

    private ClassProfileCard selectedObject; // Objet actuellement sélectionné
    [SerializeField] private ScrollRect profilesScrollRect;
    private RectTransform contentTransform; // Transform de la zone de contenu de la ScrollView

    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private Vector3 selectedScale = Vector3.one * 1.2f;
    public List<Sprite> profilePictures;

    private void OnEnable()
    {
        ClosePassword();
        foreach (var cards in profileCards)
        {
            Destroy(cards.gameObject);
        }
        profileCards.Clear();
        // Récupérer le RectTransform de la zone de contenu
        contentTransform = profilesScrollRect.content;
        float sizeX = profileCardPrefab.GetComponent<RectTransform>().sizeDelta.x;
        // Instancier les objets dans la liste
        int nbProfiles = 0;
        foreach (var profile in NetworkManager.Instance.profiles)
        {
            ClassProfileCard obj = Instantiate(profileCardPrefab, contentTransform);
            obj.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, nbProfiles * (sizeX + spacing) + spacing, sizeX);
            obj.CardIndex = nbProfiles;
            obj.ID = profile.Key;
            obj.Username = profile.Value.Item1;

             // Set the picture based on the index
            if (nbProfiles < profilePictures.Count)
            {
                obj.Picture = profilePictures[nbProfiles];
            }
            else
            {
                obj.Picture = profilePictures[0]; 
            }
            obj.Title = profile.Value.Item3;

            // Ajouter un bouton à l'objet pour la sélection
            obj.GetComponent<Button>().onClick.AddListener(() => SelectObject(obj.CardIndex));

            profileCards.Add(obj);
            nbProfiles++;
        }

        foreach (var card in profileCards)
    {
        card.Alpha = 0.8f; 
    }

        contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (profileCardPrefab.GetComponent<RectTransform>().sizeDelta.x + spacing) * profileCards.Count);
        // Sélectionner le premier objet
        if (profileCards.Count > 0)
        {
            SelectObject(0);
        }
    }

    public void SelectObject(int index)
    {
        if (selectedObject != null)
        {
            // Réinitialiser l'objet précédemment sélectionné
            selectedObject.GetComponent<RectTransform>().localScale = baseScale;
            selectedObject.Alpha = 0.8f;
            
            
        }

        // Sélectionner le nouvel objet
        selectedObject = profileCards[index];
        selectedObject.GetComponent<RectTransform>().localScale = selectedScale;
        selectedObject.Alpha = 1f;

        // Effectuer des actions supplémentaires sur l'objet sélectionné
        // ...

        // Scroller la ScrollView pour afficher l'objet sélectionné
        float selectedObjectPosition = selectedObject.transform.localPosition.x;
        float contentWidth = contentTransform.rect.width;
        float scrollViewWidth = profilesScrollRect.GetComponent<RectTransform>().rect.width;
        float scrollAmount = Mathf.Clamp01((selectedObjectPosition - scrollViewWidth / 2 + contentWidth / 2) / (contentWidth - scrollViewWidth));
        profilesScrollRect.normalizedPosition = new Vector2(scrollAmount, 0f);
    }

    public void OpenPassword()
    {
        if (SelectedProfileID != -1)
        {
            passwordScreen.SetActive(true);
            profileSelection.SetActive(false);

            passwordProfilePicture.sprite = selectedObject.Picture;
            passwordUsernameText.text = selectedObject.Username;
            passwordTitleText.text = selectedObject.Title;
        }
    }

    public void ClosePassword()
    {
        passwordScreen.SetActive(false);
        profileSelection.SetActive(true);
    }
}
