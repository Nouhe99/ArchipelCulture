using System.Collections;
using UnityEngine;

public class Pop : MonoBehaviour
{
    public float time = 0.01f;
    public Transform specificObject = null;
    // Start is called before the first frame update
    void Start()
    {
        if (specificObject == null)
        {
            specificObject = gameObject.transform;
        }
    }

    public void startPop()
    {
        StartCoroutine(playAnimation());
    }

    IEnumerator playAnimation()
    {
        Vector3 finalSize = specificObject.localScale;
        specificObject.localScale = Vector3.zero;
        for (int i = 0; i < 70; i++)
        {
            specificObject.localScale += finalSize / 40;
            yield return new WaitForSeconds(time / 100);
        }
        for (int i = 0; i < 30; i++)
        {
            specificObject.localScale -= finalSize / 40;
            yield return new WaitForSeconds(time / 100);
        }
        specificObject.localScale = finalSize;
    }
}
