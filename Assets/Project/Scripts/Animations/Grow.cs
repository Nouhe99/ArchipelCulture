using System.Collections;
using UnityEngine;

public class Grow : MonoBehaviour
{
    public int size = 50;
    public float speed = 0.01f;
    public Vector3 direction = new(0.3f, 0.3f, 0);
    public bool stop = false;
    private Coroutine routine;
    Vector3 initialSize;


    private void Awake()
    {
        initialSize = transform.localScale;
    }

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        transform.localScale = initialSize;
    }

    private void Update()
    {
        if (stop && routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        else if (!stop && routine == null)
            Play();
    }

    private void OnDestroy()
    {
        if (routine != null) StopCoroutine(routine);
    }

    private void Play()
    {
        routine = StartCoroutine(Play(direction));
    }

    private IEnumerator Play(Vector3 direction)
    {
        while (!stop)
        {
            for (int i = 0; i < 100; i++)
            {
                gameObject.transform.localScale += direction / 100;
                yield return new WaitForSeconds(speed);
            }
            for (int i = 0; i < 100; i++)
            {
                gameObject.transform.localScale -= direction / 100;
                yield return new WaitForSeconds(speed);
            }
            gameObject.transform.localScale = initialSize;
        }

    }
}
