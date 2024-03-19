using UnityEngine;

public class ChangeName : MonoBehaviour
{
 /*   private Profil profil;
    [SerializeField] private GameObject nameSlot;
    private TMPro.TMP_InputField inputField;

    private void Start()
    {
        profil = UIManager.current.profil;
        inputField = gameObject.GetComponent<TMPro.TMP_InputField>();
        inputField.Select();
    }

    private void Update()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#else
        if (Input.GetMouseButtonDown(0))
#endif 
        {
            Vector2 clickpos = UIManager.GetMousePos();
            Collider2D[] hits = Physics2D.OverlapPointAll(clickpos);
            foreach (var h in hits)
            {
                if (h.gameObject == nameSlot)
                {
                    //if click on name input, do nothing
                    return;
                }
            }

            //else, save pseudo
            if (inputField.text != "")
            {
                Database.Instance.userData.username = inputField.text;
                profil.usernameText.text = inputField.text;
                profil.nameHasChanged = true;
                inputField.text = "";
            }
            gameObject.SetActive(false);

        }
    }*/
}
