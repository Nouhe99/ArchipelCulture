using UnityEngine;

public class MovingAutoAlongLine : GoingTo
{
    private float delay;
    [SerializeField] private float minDelay, maxDelay;
    private float timer;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        transform.position = to;
        startTime = Time.time;
        delay = Random.Range(minDelay, maxDelay);
        timer = 0f;
    }

    void Update()
    {
        if (Vector3.Distance(gameObject.transform.position, to) > 0.001f)
        {
            float frac = (Time.time - startTime) / time;
            transform.position = Vector3.Lerp(from, to, frac);
        }
        else
        {
            WaitDelayAndRestart();
        }
        if (floating)
        {
            float fracFloating = (Time.time - startTime) / speedFloating;
            transform.position += Mathf.Sin(fracFloating) * heightFloating * Vector3.up;
        }
    }

    private void WaitDelayAndRestart()
    {
        if (timer < delay)
        {
            if (spriteRenderer.enabled) spriteRenderer.enabled = false;
            timer += Time.deltaTime;
        }
        else
        {
            if (!spriteRenderer.enabled) spriteRenderer.enabled = true;
            timer = 0f;
            startTime = Time.time;
            delay = Random.Range(minDelay, maxDelay);
            transform.position = from;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(from, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(to, 1);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(from, to);
    }
}
