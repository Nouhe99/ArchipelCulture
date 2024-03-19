using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Profil : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] private Transform profilTab;
    [SerializeField] private Transform titleTab;
    [SerializeField] private Transform pictureTab;
    [SerializeField] private Transform pswTab;

    [Header("Profil")]
    public TMP_Text usernameText;
    [SerializeField] private TMP_Text usernameTextNoEdit;
    [SerializeField] private GameObject fieldNewName;
    [SerializeField] private AccountLevelBar levelBar;
    [SerializeField] private GameObject onlinePanel;
    [SerializeField] private GameObject localPanel;

    [Header("Titles")]
    public TMP_Text titleText;
    [SerializeField] private ChangeTitle titleCase;
    public Transform titleContent;

    [Header("Profil Picture")]
    public Image pictureProfil;
    [SerializeField] private GameObject pictureCase;
    [SerializeField] private Transform pictureContent;

    [Header("Resources")]
    [SerializeField] private TMP_Text rocksText;
    [SerializeField] private TMP_Text decorationsText;
    [SerializeField] private TMP_Text buildingsText;
    public Transform CoinsImage;
    public Transform RocksImage;
    public Transform DecorationsImage;
    public Transform BuildingsImage;

   
    [SerializeField] private TMP_Text idText;

    #region Initialize Profil
    private void Start()
    {
        if (Database.Instance.IsPublicAccount())
        {
            onlinePanel.SetActive(false);
            localPanel.SetActive(true);
        }
        StartCoroutine(LoadProfil());
    }

    private bool profilLoaded = false;
    private IEnumerator LoadProfil()
    {
        profilLoaded = false;
        usernameText.text = Database.Instance.userData.username;
        usernameTextNoEdit.text = Database.Instance.userData.username;
        titleText.text = Database.Instance.userData.title;
        idText.text = Database.Instance.userData.code;
        pictureProfil.sprite = Database.Instance.profilPictures.GetPicture(Database.Instance.userData.profilPicture);
        UpdateResources();
        yield return StartCoroutine(levelBar.UpdateFillBarCoroutine());

        
        profilLoaded = true;
    }
    private void OnEnable()
    {
        if (profilLoaded) StartCoroutine(levelBar.UpdateFillBarCoroutine());
        UpdateResources();
    }
    #endregion

    private void OnApplicationPause(bool pauseStatus)
    {
        bool paused = pauseStatus;
        if (paused)
        {
            //backup datas
            //TODO: save on local version too
            StartCoroutine(Database.Instance.UpdateProfilInfos());
        }
    }

    [HideInInspector] public bool nameHasChanged = false;
    [HideInInspector] public bool titleHasChanged = false;
    [HideInInspector] public bool pictureHasChanged = false;
    [HideInInspector] public string pswHasChanged = null;
    public void UpdateDB()
    {
        StartCoroutine(Database.Instance.UpdateProfilInfos());
    }

    public void MoveScrollRectUp(ScrollRect scr)
    {
        scr.velocity = Vector2.down * 1000f;
    }

    public void MoveScrollRectDown(ScrollRect scr)
    {
        scr.velocity = Vector2.up * 1000f;
    }
   

    #region Open Close Tab
    public void CloseActualTab()
    {
        if (titleTab.gameObject.activeInHierarchy)
        {
            titleTab.gameObject.SetActive(false);
            profilTab.gameObject.SetActive(true);
        }
      
        else if (pictureTab.gameObject.activeInHierarchy)
        {
            pictureTab.gameObject.SetActive(false);
            profilTab.gameObject.SetActive(true);
        }
        else if (pswTab.gameObject.activeInHierarchy)
        {
            pswTab.gameObject.SetActive(false);
            profilTab.gameObject.SetActive(true);
        }
        else if (profilTab.gameObject.activeInHierarchy)
        {
            StartCoroutine(Database.Instance.UpdateProfilInfos());
            gameObject.SetActive(false);
        }
    }
    public void CloseProfilWindow()
    {
        UIManager.current.CloseCurrentPanel();
    }
    public void ReorganizeTabs()
    {
        if (!profilTab.gameObject.activeInHierarchy)
            profilTab.gameObject.SetActive(true);
        if (titleTab.gameObject.activeInHierarchy)
            titleTab.gameObject.SetActive(false);
       
        else if (pictureTab.gameObject.activeInHierarchy)
            pictureTab.gameObject.SetActive(false);
        else if (pswTab.gameObject.activeInHierarchy)
            pswTab.gameObject.SetActive(false);
    }

   
    public void OpenTitleList()
    {
        if (profilTab.gameObject.activeInHierarchy)
            profilTab.gameObject.SetActive(false);
        if (!titleTab.gameObject.activeInHierarchy)
            titleTab.gameObject.SetActive(true);
    }
    public void OpenPictureList()
    {
        if (profilTab.gameObject.activeInHierarchy)
            profilTab.gameObject.SetActive(false);
        if (!pictureTab.gameObject.activeInHierarchy)
            pictureTab.gameObject.SetActive(true);
    }

    public void OpenPsw()
    {
        if (profilTab.gameObject.activeInHierarchy)
            profilTab.gameObject.SetActive(false);
        if (!pswTab.gameObject.activeInHierarchy)
            pswTab.gameObject.SetActive(true);
    }

    #endregion

    #region Resources
    public void UpdateResources()
    {
        if (Database.Instance != null && IslandBuilder.current != null)
        {
            rocksText.text = (IslandBuilder.current.islandTiles.Count - 4 + Database.Instance.userData.rocksRemaining).ToString();
            decorationsText.text = Database.Instance.userData.GetTotalDecoration().ToString();
            buildingsText.text = Database.Instance.userData.GetTotalBuildings().ToString();
        }
    }
    #endregion

}
