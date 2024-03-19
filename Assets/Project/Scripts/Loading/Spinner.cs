using UnityEngine;
public class Spinner : MonoBehaviour
{
    public RectTransform Circle;
    public float Speed = 200f;

    private void Update()
    {
        Circle.Rotate(0f, 0f, -Speed * Time.deltaTime);

    }
}
