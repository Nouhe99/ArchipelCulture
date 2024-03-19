using TMPro;
using UnityEngine;

public class DotNotification : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationCount;

    public void SetNotificationCount(int value)
    {
        if (value <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
        notificationCount.text = value.ToString();
        gameObject.SetActive(true);
    }
}
