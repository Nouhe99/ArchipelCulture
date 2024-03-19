using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Rarity")]
public class Rarity : ScriptableObject
{
    public string Label;
    public Color Color;
    public Sprite Case;
    public Sprite Background;

    public string HtmlRGB
    {
        get
        {
            return ColorUtility.ToHtmlStringRGB(Color);
        }
    }

}
