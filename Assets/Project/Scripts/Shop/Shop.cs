using System;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Shop : MonoBehaviour
{
    [SerializeField] private ItemSlot selector;
    [SerializeField] private GameObject hideSelector;
    [SerializeField] private TMPro.TMP_Text nameText;
    [SerializeField] private TMPro.TMP_Text rarityText;
    [SerializeField] private TMPro.TMP_Text costText;
    [SerializeField] private GameObject fxCoin;
    [SerializeField] private Image sizeImg;
    [SerializeField] private Button PrevButton;
    [SerializeField] private Button NextButton;

    private ItemSlot shopSlot;

    [Header("Content")]
    [SerializeField] private ContentByType[] contentShop;
    [Serializable]
    public struct ContentByType
    {
        public ItemType itemType;
        public Transform content;
    }
    public GameObject categoryObject;
    public GameObject caseShop;

    private int currentPrice = 0;
    private Item currentItem;

    private void Start()
    {
        shopSlot = gameObject.GetComponentInChildren<ItemSlot>();
    }

    private void NextButtonClick()
    {
        currentItem.CycleSprites(1);

    }

    private void PrevButtonClick()
    {
        currentItem.CycleSprites(-1);

    }

    public Transform GetContentByType(ItemType it)
    {
        foreach (var item in contentShop)
        {
            if (item.itemType == it) return item.content;
        }
        Debug.Log("Cannot find content holder for this type (" + it + ") in Shop.");
        return null;
    }

    public void PlaceItem()
    {

        hideSelector.SetActive(true);
        if (selector.selected.GetComponent<Item>())
        {

            currentItem = selector.selected.GetComponent<Item>();
            currentPrice = currentItem.price;
            nameText.text = currentItem.nameItem;
            rarityText.text = currentItem.rarity.Label;
            costText.text = "x " + currentPrice;
            string tempCoins = "Pièces";
            if (currentPrice <= 1) tempCoins = "Pièce";
            costText.text = "<font-weight=500>" + "Coût" + " <size=150%><font-weight=700><color=yellow>" + currentPrice + "</color></font-weight></size><size=80%> " + tempCoins + "</size></font-weight>";
            sizeImg.sprite = currentItem.size.size_Sprite;
            NextButton.onClick.AddListener(NextButtonClick);
            PrevButton.onClick.AddListener(PrevButtonClick);

        }

    }


    public void BuyItem()
    {
        if (Database.Instance.userData.gold < currentPrice) return; //not enough coins

        PlayAudio.Instance.bank.BuySound();
        GameObject CoinsVFX = Instantiate(fxCoin, gameObject.transform);
        if (selector != null)
        {
            CoinsVFX.transform.position = selector.transform.position;
        }
        ParticleSystem ps;
        if ((ps = CoinsVFX.GetComponent<ParticleSystem>()) != null)
        {
            Destroy(CoinsVFX, ps.main.duration);
        }

        //update gold after buying
        Database.Instance.userData.gold -= currentPrice;
        Database.Instance.userData.totalGoldSpent += currentPrice;
        SaveDataInventory.Instance.AddBuyObj(currentItem);//store item id for update
        UIManager.current.inventoryUI.AddNewSlot(currentItem);
        SaveDataInventory.Instance.UpdateItemBuyDatabaseLocal(currentItem);
        shopSlot.ResetSlot();
    }

}
