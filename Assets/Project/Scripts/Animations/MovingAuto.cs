using UnityEngine;

public class MovingAuto : GoingTo
{
    [SerializeField] private Bounds zone;
    [Tooltip("Some fish goes up, some goes down, for example a shark cannot go up ! y value give the direction.\n" +
    "Fish move following diagonal isometric lines, so here it must be x = 2y")]
    [SerializeField] private Vector3 distance;

    [SerializeField] private SpriteRenderer spriteRenderer;
    //[SerializeField] private float spriteDisappearDistance = 1f;
    private void Start()
    {
        from = (Vector3.right * Random.Range(zone.min.x, zone.max.x)) + (Vector3.up * Random.Range(zone.min.y, zone.max.y));
        gameObject.transform.position = from;
        ChangeDirectionInBounds();
        startTime = Time.time;
    }

    private float distanceToDestination;
    void Update()
    {
        distanceToDestination = Vector3.Distance(gameObject.transform.position, to);
        if (distanceToDestination > 0.001f)
        {
            float frac = (Time.time - startTime) / time;
            gameObject.transform.position = Vector3.Lerp(from, to, frac);
        }
        else
        {
            ChangeDirectionInBounds();
        }
        if (floating)
        {
            float fracFloating = (Time.time - startTime) / speedFloating;
            transform.position += Mathf.Sin(fracFloating) * heightFloating * Vector3.up;
        }

        ////Disappear
        //if (distanceToDestination < spriteDisappearDistance)
        //{
        //    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, distanceToDestination);
        //}
        //else
        //{
        //    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Clamp01(Time.time - startTime));
        //}
    }

    private void ChangeDirectionInBounds()
    {
        if (!zone.Contains(to)) //must replace object
        {
            from = zone.ClosestPoint(to);
            if (to.y > zone.max.y || to.y < zone.min.y)
            {
                if (distance.y < 0)
                {
                    from = Vector3.right * Random.Range(zone.min.x, zone.max.x) + Vector3.up * zone.extents.y;
                }
                else
                {
                    from = Vector3.right * Random.Range(zone.min.x, zone.max.x) + Vector3.down * zone.extents.y;
                }
            }
            gameObject.transform.position = from;
        }
        Vector3 newDirection = gameObject.transform.position + new Vector3(distance.x * (Random.Range(0, 2) == 1 ? 1 : -1), distance.y, distance.z);
        if (spriteRenderer != null)
        {
            if (newDirection.x <= gameObject.transform.position.x) spriteRenderer.flipX = distance.y >= 0;
            else spriteRenderer.flipX = distance.y < 0;
        }
        ChangeDirection(newDirection);
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
