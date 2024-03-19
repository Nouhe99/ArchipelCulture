using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Component for moving a gameobject from a point to an other in a given time.
/// It can be floating.
/// </summary>
public class GoingTo : MonoBehaviour
{
    public Vector3 from;
    public Vector3 to;
    public float time = 60;
    public bool floating = false;
    public float heightFloating = 0.1f;
    public float speedFloating = .5f;
    public UnityEvent arrivedEvent;
    public bool destroyWhenArrived = false;

    protected float startTime;

    private void Start()
    {
        startTime = Time.time;
    }
    void Update()
    {
        if (Vector3.Distance(gameObject.transform.position, to) > 0.001f)
        {
            float frac = (Time.time - startTime) / time;
            gameObject.transform.position = Vector3.Lerp(from, to, frac);
        }
        else
        {
            if (arrivedEvent != null)
            {
                arrivedEvent.Invoke();
            }
            if (destroyWhenArrived)
            {
                Destroy(gameObject);
            }
        }
        if (floating)
        {
            float fracFloating = (Time.time - startTime) / speedFloating;
            transform.position += Mathf.Sin(fracFloating) * heightFloating * Vector3.up;
        }
    }

    public void ChangeDirection(Vector3 newDirection)
    {
        from = gameObject.transform.position;
        to = newDirection;
        startTime = Time.time;
    }
}
