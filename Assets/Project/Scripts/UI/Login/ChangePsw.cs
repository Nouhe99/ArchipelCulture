using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChangePsw : MonoBehaviour
{
    [SerializeField] private Button validate;

    [SerializeField] private Toggle P1;
    [SerializeField] private Toggle P2;
    [SerializeField] private Toggle P3;
    [SerializeField] private Toggle P4;

    private bool pin1 = false;
    private bool pin2 = false;
    private bool pin3 = false;
    private bool pin4 = false;
    public string codeEnter = "";

    private void OnEnable()
    {
        StartCoroutine(WritePsw());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        codeEnter = "";
        pin1 = false;
        P1.isOn = false;
        pin2 = false;
        P2.isOn = false;
        pin3 = false;
        P3.isOn = false;
        pin4 = false;
        P4.isOn = false;
    }

    private IEnumerator WritePsw()
    {
        validate.interactable = false;
        yield return new WaitUntil(() => pin1 == true);
        P1.isOn = true;
        yield return new WaitUntil(() => pin2 == true);
        P2.isOn = true;
        yield return new WaitUntil(() => pin3 == true);
        P3.isOn = true;
        yield return new WaitUntil(() => pin4 == true);
        P4.isOn = true;
        yield return null;
        validate.interactable = true;
    }

    public void RemoveOnePin()
    {
        if (codeEnter == "") return;

        StopAllCoroutines();
        if (pin4) { pin4 = !pin4; P4.isOn = false; }
        else if (pin3) { pin3 = !pin3; P3.isOn = false; }
        else if (pin2) { pin2 = !pin2; P2.isOn = false; }
        else if (pin1) { pin1 = !pin1; P1.isOn = false; }
        codeEnter = codeEnter.Remove(codeEnter.Length - 1, 1);
        StartCoroutine(WritePsw());
    }


    public void ButtonPressed(PassPin pin)
    {
        if (codeEnter.Length < 4) codeEnter += pin.Code;
        if (!pin1)
        {
            pin1 = !pin1;
            P1.graphic.GetComponent<Image>().sprite = pin.Sprite;
        }
        else if (!pin2)
        {
            pin2 = !pin2;
            P2.graphic.GetComponent<Image>().sprite = pin.Sprite;
        }
        else if (!pin3)
        {
            pin3 = !pin3;
            P3.graphic.GetComponent<Image>().sprite = pin.Sprite;
        }
        else if (!pin4)
        {
            pin4 = !pin4;
            P4.graphic.GetComponent<Image>().sprite = pin.Sprite;
        }
    }

    public void CheckPassword()
    {
        UIManager.current.profil.pswHasChanged = codeEnter;
        UIManager.current.profil.CloseActualTab();
    }
}
