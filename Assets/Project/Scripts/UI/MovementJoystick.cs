using UnityEngine;
using UnityEngine.EventSystems;

public class MovementJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public GameObject joystick;
    public GameObject joystickBackground;
    public Vector2 joystickVector;
    private Vector2 joystickTouchPos;
    private Vector2 joystickOriginalPos;
    private float joystickRadius;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragPos = eventData.position;
        joystickVector = (dragPos - joystickTouchPos).normalized;
        if (joystickVector.x > -0.5f && joystickVector.x < 0.5f) joystickVector.x = 0f;
        else if (joystickVector.x > 0f) joystickVector.x = 1;
        else if (joystickVector.x < 0f) joystickVector.x = -1;

        if (joystickVector.y > -0.5f && joystickVector.y < 0.5f) joystickVector.y = 0f;
        else if (joystickVector.y > 0f) joystickVector.y = 1;
        else if (joystickVector.y < 0f) joystickVector.y = -1;
        joystickVector = joystickVector.normalized; // Normalized again for the diagonal movement

        float joystickDist = Vector2.Distance(dragPos, joystickTouchPos);
        if (joystickDist < joystickRadius)
        {
            joystick.transform.position = joystickTouchPos + joystickVector * joystickDist;
        }
        else
        {
            //joystick follow finger touch
            if (joystickDist > joystickRadius * 4)
            {
                joystickTouchPos = (Vector2)joystickBackground.transform.position + 2 * joystickRadius * (dragPos - joystickTouchPos).normalized;
                joystickBackground.transform.position = joystickTouchPos;
            }
            joystick.transform.position = joystickTouchPos + joystickVector * joystickRadius;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        joystick.transform.position = eventData.position;
        joystickBackground.transform.position = eventData.position;
        joystickTouchPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystickVector = Vector2.zero;
        joystick.transform.position = joystickOriginalPos;
        joystickBackground.transform.position = joystickOriginalPos;
    }

    private void Start()
    {
        joystickOriginalPos = joystickBackground.transform.position;
        joystickRadius = joystickBackground.GetComponent<RectTransform>().sizeDelta.y / 4;
    }


}
