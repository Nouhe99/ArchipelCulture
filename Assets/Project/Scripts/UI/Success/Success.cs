using System.Collections.Generic;
using UnityEngine;

public class Success : MonoBehaviour
{
    public enum NotifType { UnlockTitle }
    private Dictionary<NotifType, string> titles;

    [SerializeField] private TMPro.TMP_Text bigTitle;
    [SerializeField] private TMPro.TMP_Text subtitle;
    [SerializeField] private TMPro.TMP_Text description;
    private UIAnimation anim;

    private void Awake()
    {
        titles = new()
        {
            {NotifType.UnlockTitle, "Nouveau titre !" }
        };
    }
    private void Start()
    {
        anim = gameObject.GetComponent<UIAnimation>();
    }

    private readonly Queue<(NotifType, string, string)> newNotif = new();
    private bool isShowing = false;
    public void Open(string notifText, string notifDesc, NotifType typeSuccess = NotifType.UnlockTitle)
    {
        if (!isShowing)
        {
            isShowing = true;
            OpenAsync(notifText, notifDesc, typeSuccess);
        }
        else
        {
            newNotif.Enqueue((typeSuccess, notifText, notifDesc));
        }
    }
    private async void OpenAsync(string notifText, string notifDescr, NotifType typeSuccess = NotifType.UnlockTitle)
    {
        subtitle.text = titles[typeSuccess];
        bigTitle.text = notifText;
        description.text = notifDescr;

        await anim.AnimateFromStartToEndAsync();
        await System.Threading.Tasks.Task.Delay(5000);
        await anim.AnimateFromEndToStartAsync();

        isShowing = false;

        if (newNotif.Count > 0)
        {
            (NotifType, string, string) dequeueTitle = newNotif.Dequeue();
            OpenAsync(dequeueTitle.Item2, dequeueTitle.Item3, dequeueTitle.Item1);
        }
    }
}
