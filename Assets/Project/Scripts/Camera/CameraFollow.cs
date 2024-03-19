using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private Vector2 maxPosition;

    private Camera mainCamera;
    private float cameraHalfWidth;
    private float cameraHalfHeight;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        cameraHalfHeight = mainCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = target.position;
            targetPosition.z = currentPosition.z;

            // Calculate the new camera position
            Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, speed * Time.deltaTime);
            newPosition.x = Mathf.Clamp(newPosition.x, minPosition.x + cameraHalfWidth, maxPosition.x - cameraHalfWidth);
            newPosition.y = Mathf.Clamp(newPosition.y, minPosition.y + cameraHalfHeight, maxPosition.y - cameraHalfHeight);

            // Update the camera position
            transform.position = newPosition;
        }
    }
}
