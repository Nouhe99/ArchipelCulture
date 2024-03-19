using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTreasure : MonoBehaviour
{
    public Image contener;
    public TMPro.TMP_Text nbGold;
    [SerializeField] private TMPro.TMP_Text coinName;

    [SerializeField] private List<Sprite> imgGold = new();
    [SerializeField] private List<int> nbrLevelGold = new(); //30, 10, 3, 2, 1, 0 //note: no sorting, must be already in order in editor.

    private void OnEnable()
    {
        FillTreasure();
        nbGold.text = Database.Instance.userData.gold.ToString();
        if (coinName != null)
        {
            if (Database.Instance.userData.gold <= 1) coinName.text = "Pièce";
            else coinName.text = "Pièces";
        }
        Database.Instance.userData.OnVariableChange += Reload;

    }

    private void OnDisable()
    {
        Database.Instance.userData.OnVariableChange -= Reload;
    }

    public void Reload(int goldDiff)
    {
        FillTreasure();
        nbGold.text = Database.Instance.userData.gold.ToString();
        if (coinName != null)
        {
            if (Database.Instance.userData.gold <= 1) coinName.text = "Pièce";
            else coinName.text = "Pièces";
        }
    }
    private void FillTreasure()
    {
        int nbrgold = Database.Instance.userData.gold;
        for (int i = 0; i < nbrLevelGold.Count; i++)
        {
            if (nbrgold > nbrLevelGold[i])
            {
                contener.sprite = imgGold[i];
                return;
            }
        }
        contener.sprite = imgGold[^1];
    }

}
