using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker), typeof(Rigidbody2D))]
public class CustomAiPath : MonoBehaviour
{
    private Seeker seeker;
    private Rigidbody2D rigidbody2d;

    public Path path;
    public float speed = 2;
    public float nextWaypointDistance = 3;
    public int currentWaypoint = 0;
    public bool reachedEndOfPath;

    [SerializeField] public GameObject destinationObjectPrefab;
    public GameObject destinationObject;
    [SerializeField] public LineRenderer pathLineRenderer;


    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    public void SetDestination(Vector3 destination)
    {
        seeker.StartPath(transform.position, destination, OnPathComplete);
    }

    public void StopPath()
    {
        path = null;
        if (destinationObject != null)
        {
            destinationObject.SetActive(false);
        }
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
        }
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
            if (path.vectorPath.Count > 0)
            {
                SpawnDestinationPointer(path.vectorPath[path.vectorPath.Count - 1]);
            }
            UpdatePathLine();
        }
    }

    private void SpawnDestinationPointer(Vector2 position)
    {
        if (destinationObject != null)
        {
            destinationObject.transform.position = position;
            destinationObject.SetActive(true);
        }
        else
        {
            destinationObject = Instantiate(destinationObjectPrefab, position, destinationObjectPrefab.transform.rotation);
        }
    }

    private void UpdatePathLine()
    {
        if (path != null && path.vectorPath.Count > 0 && currentWaypoint < path.vectorPath.Count)
        {
            pathLineRenderer.positionCount = path.vectorPath.Count - currentWaypoint;
            pathLineRenderer.SetPositions(path.vectorPath.GetRange(currentWaypoint, path.vectorPath.Count - currentWaypoint).ToArray());
        }
        else
        {
            pathLineRenderer.positionCount = 0;
        }
    }

    public void Update()
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }
        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        reachedEndOfPath = false;
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    reachedEndOfPath = true;
                    StopPath();
                    rigidbody2d.velocity = Vector2.zero;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (reachedEndOfPath == false && path != null)
        {
            UpdatePathLine();
            // Slow down smoothly upon approaching the end of the path
            // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
            var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            // Direction to the next waypoint
            // Normalize it so that it has a length of 1 world unit
            Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position);
            if (dir.x > -0.1f && dir.x < 0.1f) dir.x = 0f;
            else if (dir.x > 0f) dir.x = 1;
            else if (dir.x < 0f) dir.x = -1;

            if (dir.y > -0.1f && dir.y < 0.1f) dir.y = 0f;
            else if (dir.y > 0f) dir.y = 1;
            else if (dir.y < 0f) dir.y = -1;

            Vector3 normalizedDir = dir.normalized;

            // Apply the movement
            rigidbody2d.velocity = new Vector2(normalizedDir.x * speed * speedFactor, normalizedDir.y * speed * speedFactor);
        }
    }
}
