using UnityEngine;
using UnityEngine.UI;

public class GroundTypeSelection : MonoBehaviour
{
    [SerializeField] private int idGround;

    public void ChangeGround(Toggle tog)
    {
        /*
        if (!tog.group.AnyTogglesOn())
        {
            IslandBuilder.current.StateNumber = 0;
            return;
        }
        */

        if (!tog.isOn) return;

        IslandBuilder.current.StateNumber = idGround;
    }
}