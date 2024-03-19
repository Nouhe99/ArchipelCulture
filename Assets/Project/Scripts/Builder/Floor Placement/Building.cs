using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public static Building current;

    #region Initialisation

    private void Awake()
    {
        current = this;
    }
    #endregion

    public void ShowConstructionBlocks(bool show)
    {
        foreach (ItemFloor item in UIManager.current.floorContener.GetComponentsInChildren<ItemFloor>())
        {
            if (item.selectionImage != null) item.selectionImage.SetActive(show);
        }
    }



    #region Animation

    [Header("Animation")]
    public AnimationCurve animationCurveUp;
    public AnimationCurve animationCurveDown;
    public float speedAnimation = 8f;

}

public enum AnimDirection
{
    UP,
    DOWN,
    NONE
}
#endregion