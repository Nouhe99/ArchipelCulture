using UnityEngine;

public class Bounce : MonoBehaviour
{
    public float height = 0.5f;
    public float speed = 1f;
    public bool rotateOnY = false;
    public float speedY = 1f;


    private Vector3 startingPoint;
    private Vector3 targetPosition;
    private bool retour = false;

    // Start is called before the first frame update
    void Start()
    {
        startingPoint = gameObject.transform.localPosition;
        targetPosition = startingPoint + new Vector3(0, height, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (retour == false)
        {
            if (Vector3.Distance(targetPosition, gameObject.transform.localPosition) <= 0.01f)
            {
                retour = true;
            }
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, speed * Time.deltaTime);
            if (rotateOnY) transform.Rotate(speedY * Time.deltaTime * Vector3.up);
        }
        if (retour)
        {
            if (Vector3.Distance(startingPoint, gameObject.transform.localPosition) <= 0.01f)
            {
                retour = false;
            }
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startingPoint, speed * Time.deltaTime);
            if (rotateOnY) transform.Rotate(speedY * Time.deltaTime * Vector3.down);
        }
    }
}
