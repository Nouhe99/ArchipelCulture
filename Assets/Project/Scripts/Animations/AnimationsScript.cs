using System.Collections;
using UnityEngine;

public static class AnimationsScript
{
    public static IEnumerator Disapear(Transform tr, float speed = 8f)
    {
        while ((tr.localScale.x + tr.localScale.y) > 0.001f)
        {
            tr.localScale -= speed * Time.deltaTime * Vector3.one;
            yield return null;
        }
    }

    public static IEnumerator Extend(RectTransform transfRect, float time = 0.4f)
    {
        Vector3 initialScale = transfRect.localScale;
        transfRect.localScale = Vector3.zero;
        float timeElapsed = 0f;
        while (timeElapsed < time / 2)
        {
            try
            {
                timeElapsed += Time.deltaTime;
                transfRect.localScale = Vector3.Lerp(Vector3.zero, initialScale + (Vector3.one * 1.5f), timeElapsed / (time / 2));
            }
            catch (MissingReferenceException e)
            {
                Debug.LogError("This slot has been destroy, cannot be animated. (" + e.Message + ")");
                yield break;
            }
            yield return null;
        }
        timeElapsed = 0f;
        while (timeElapsed < time / 2)
        {
            timeElapsed += Time.deltaTime;
            transfRect.localScale = Vector3.Lerp(initialScale + (Vector3.one * 1.5f), initialScale, timeElapsed / (time / 2));
            yield return null;
        }
    }

    private static bool isPlayingNotif = false;
    public static IEnumerator Notif(RectTransform transfRect, float size = 5f, float time = 0.9f)
    {
        if (isPlayingNotif) { yield break; }
        PlayAudio.Instance?.PlayOneShot(PlayAudio.Instance.bank.bipUnvalid);

        isPlayingNotif = true;
        Vector3 initialScale = transfRect.localScale;
        Vector3 initialRot = transfRect.rotation.eulerAngles;

        float timeElapsed = 0f;
        while (timeElapsed < time / 4)
        {
            try
            {
                timeElapsed += Time.deltaTime;
                transfRect.localScale = Vector3.Lerp(initialScale, initialScale + (Vector3.one * size), timeElapsed / (time / 4));
            }
            catch (MissingReferenceException e)
            {
                Debug.LogError("This gameobject has been destroy, cannot be animated. (" + e.Message + ")");
                yield break;
            }
            yield return null;
        }
        timeElapsed = 0f;
        while (timeElapsed < time / 2)
        {
            timeElapsed += Time.deltaTime;
            transfRect.Rotate(2f * Mathf.Cos(40 * timeElapsed) * Vector3.forward); //z
            yield return null;
        }
        transfRect.rotation = Quaternion.Euler(initialRot);
        timeElapsed = 0f;
        while (timeElapsed < time / 4)
        {
            timeElapsed += Time.deltaTime;
            transfRect.localScale = Vector3.Lerp(initialScale + (Vector3.one * size), initialScale, timeElapsed / (time / 4));
            yield return null;
        }
        transfRect.localScale = initialScale;
        isPlayingNotif = false;
    }

}
