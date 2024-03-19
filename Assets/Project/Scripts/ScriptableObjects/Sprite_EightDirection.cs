using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Sprite/8-Directional")]
public class Sprite_EightDirection : ScriptableObject
{
    [Tooltip("Starting from the north, rotating clockwise")]
    [SerializeField] private Sprite[] eightDirectionSprites = new Sprite[8];
    [SerializeField] private Sprite idleSprite;

    public void SetSpriteForRendererWithDirection(Vector2 direction, SpriteRenderer spriteRenderer, bool idleAtRest = true)
    {
        direction = direction.normalized;
        Sprite selectedSprite = spriteRenderer.sprite;
        // North
        if (direction.y > 0)
        {
            // North West
            if (direction.x > 0)
            {
                selectedSprite = eightDirectionSprites[1];
            }
            // North East
            else if (direction.x < 0)
            {
                selectedSprite = eightDirectionSprites[7];
            }
            // North
            else
            {
                selectedSprite = eightDirectionSprites[0];
            }
        }
        // South
        else if (direction.y < 0)
        {
            // South West
            if (direction.x > 0)
            {
                selectedSprite = eightDirectionSprites[3];
            }
            // South East
            else if (direction.x < 0)
            {
                selectedSprite = eightDirectionSprites[5];
            }
            // South
            else
            {
                selectedSprite = eightDirectionSprites[4];
            }
        }
        // Neutral
        else
        {
            // West
            if (direction.x > 0)
            {
                selectedSprite = eightDirectionSprites[2];
            }
            // East
            else if (direction.x < 0)
            {
                selectedSprite = eightDirectionSprites[6];
            }
            // Idle
            else
            {
                if (idleAtRest)
                {
                    selectedSprite = idleSprite;
                }
            }
        }
        spriteRenderer.sprite = selectedSprite;
    }
}
