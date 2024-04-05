using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    
    #region Random boxes
    // -- public -- //
    [Serializable]
    public struct RarityPercentage
    {
        public Rarity rarity;
        public double percentage;
    }
    [Serializable]
    public struct Box
    {
        public int maxValue;
        public int nbrItems;
        [Header("Percentages")]
        public RarityPercentage nothing;
        public RarityPercentage common;
        public RarityPercentage rare;
        public RarityPercentage epic;
        public RarityPercentage legendary;
        public double floor;
    }
    
    [Header("Random Boxes")]
    public Box normalBox;
    public Box bigBox;
    public Box megaBox;

    // -- private -- //
    private Dictionary<string, Box> boxID; //itemID in database : 4:normal, 6:big, 7:mega.

    private void Awake()
    {
        boxID = new()
        {
            { "4", normalBox },
            { "6", bigBox },
            { "7", megaBox },
        };

        itemList = Database.Instance.itemsList;

        if (PlayerPrefs.HasKey("CurrentNotion"))
        {
            int savedNotion = PlayerPrefs.GetInt("CurrentNotion");
        }
    }
    private DatabaseItems itemList;
    #endregion

    private readonly RandomNumberGenerator rng = new RNGCryptoServiceProvider();
    /// <summary>
    /// Random double between 0 and 1
    /// </summary>
    private double RandomNumber()
    {
        var data = new byte[sizeof(uint)];
        rng.GetBytes(data);
        return (BitConverter.ToUInt32(data, 0) / (uint.MaxValue + 1.0));
    }

    #region Scene Informations
    [Header("Scene informations")]
    [SerializeField] private SceneController sceneController;
    private List<Reward> rewards;
    #endregion
    #region Chest
    [Header("Chest")]
    [SerializeField] private Image chestPanel;
    [SerializeField] private Button chestButton;
    [SerializeField] private Animator chestAnimation;

    private void ActiveChestPanel(bool active)
    {
        if (active) chestAnimation.Play("Chest-closed");
        chestButton.interactable = active;
        chestPanel.gameObject.SetActive(active);
    }
    #endregion
    #region Wooden Gift Box
    [Header("Gift")]
    [SerializeField] private Image giftPanel;
    [SerializeField] private Button giftButton;

    private void ActiveGiftPanel(bool active)
    {
        giftButton.interactable = active;
        giftPanel.gameObject.SetActive(active);
    }
    #endregion

    #region Item
    [Header("Item")]
    [SerializeField] private Image itemPanel;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemLabel;
    [SerializeField] private Button nextItemButton;
    [SerializeField] private Button backToIsleButton;
    private int currentItemIndex;
    [SerializeField] private Canvas canvas;
    [SerializeField] private AudioClip confettiClip;

    private void ActiveItemPanel(bool active)
    {
        nextItemButton.gameObject.SetActive(active);
        itemPanel.gameObject.SetActive(active);
        if (active)
        {
            PlayAudio.Instance?.PlayOneShot(confettiClip);
        }
    }

    private void FillItemPanelWithCurrentItem()
    {
        //if reward is 4, 6 or 7 : it is a random box, must add random items to reward list.
        string rewardID = rewards[currentItemIndex].Item.ID;
        if (boxID.ContainsKey(rewardID))
        {
            int quantity = rewards[currentItemIndex].Quantity;
            rewards.RemoveAt(currentItemIndex);
            for (int i = 0; i < quantity; i++)
            {
                RandomRewards(boxID[rewardID]);
            }
        }
        //TODO: sort rewards by type and rarity : box > common > rare > epic > unique > gold 

        rewardID = rewards[currentItemIndex].Item.ID;
        if (currentItemIndex < rewards.Count)
        {
            Debug.Log("reward");
            Item item = itemList.GetItemData(rewards[currentItemIndex].Item.ID);

            itemPanel.sprite = item.rarity.Background;
            itemLabel.text = $"<size=80%>{rewards[currentItemIndex].Quantity}x</size> " +
            $"{item.nameItem} " +
            $"<color=#{item.rarity.HtmlRGB}><size=80%>({item.rarity.Label})";
            itemImage.sprite = rewards[currentItemIndex].Item.Sprite;
            // Size
            RectTransform imageRectTransform = itemImage.GetComponent<RectTransform>();
            float maxHeight = rewards[currentItemIndex].Item.Sprite.texture.height;
            float maxWidth = rewards[currentItemIndex].Item.Sprite.texture.width;
            imageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(128f + 128f * rewards[currentItemIndex].Item.zSize, Mathf.Min(128, maxHeight), maxHeight));
            imageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(128f + 128f * Mathf.Max(rewards[currentItemIndex].Item.xSize, rewards[currentItemIndex].Item.ySize), Mathf.Min(128, maxHeight), maxWidth));

            // End size
            if (rewards[currentItemIndex].Item.Type == ItemType.Decoration || rewards[currentItemIndex].Item.Type == ItemType.Building) //this is an item
            {
                Item temp = itemList.GetItemData(rewardID);
                SaveDataInventory.Instance.AddBuyObj(temp); //store item id for update in database
            }
            else if (rewards[currentItemIndex].Item.ID == "1") //this is gold
            {
                Database.Instance.userData.gold += rewards[currentItemIndex].Quantity;
            }
            else if (rewards[currentItemIndex].Item.Type == ItemType.Rock)//if is rock
            {
                Database.Instance.userData.rocksRemaining += rewards[currentItemIndex].Quantity;
                if (FindObjectOfType<IslandBuilder>()) IslandBuilder.current.UpdateRocks();
               // StartCoroutine(SaveDataInventory.Instance.UpdateItemPlacedDatabaseLocal());
            }
            else
            {
                //TODO: add for other type of item (Boat, ground, title, profilpicture)
            }
            //StartCoroutine(SaveDataInventory.Instance.UpdateItemBuyDatabase());

            // SaveDataInventory.Instance.UpdateItemBuyDatabaseLocal(); need to be fixed

            ActiveItemButton();
        }
    }


    #region Random Reward
   

    private void RandomRewards(Box typeBox)
    {

        int totalGold = typeBox.maxValue;

        ///examples :
        ///if it's a box with 1 item, it will be an item with chances of normal box (1).
        ///if it's a box with 3 items, the first will be an item with chances of normal box (1), second with chances of big box (2) and last with chances of mega box (3).
        ///if it's a box with 5 items, the first, second and third will be an item with chances of normal box (1), penultimate with chances of big box (2) and last with chances of mega box (3).
        ///if it's a kraken reward, then it will be a kraken box with no items and only gold, depending on if the user has completed the kraken notion.

        for (int i = 0; i < typeBox.nbrItems - 2; i++) //3 items and more
        {
            if (totalGold > 0)
                totalGold = RandomObject(megaBox, totalGold);
        }

        if (typeBox.nbrItems > 1) //2 items and more
        {
            if (totalGold > 0)
                totalGold = RandomObject(bigBox, totalGold);
        }

        if (typeBox.nbrItems > 0 && totalGold > 0) //at least one item
            totalGold = RandomObject(normalBox, totalGold);

        
        
        
            
        

        //StartCoroutine(SaveDataInventory.current.UpdateItemBuyDatabase());
    }
    /// <returns>Chest gold value remaining</returns>
    /// 
    
    private int RandomObject(Box typeBox, int goldValue)
    {
        if (itemList == null)
        {
            Debug.LogError("No Database script found, so no item found neither");
            return goldValue;
        }

        Rarity rarityItemWin;
        double rand = RandomNumber() * 100;
        //Debug.Log(rand);
        if (rand <= typeBox.nothing.percentage)
        {
            //you win no item, nothing, sorry dude
            return goldValue;
        }
        else if (rand <= (typeBox.nothing.percentage + typeBox.common.percentage))
        {
            //you win common item
            rarityItemWin = typeBox.common.rarity;
        }
        else if (rand <= (typeBox.nothing.percentage + typeBox.common.percentage + typeBox.rare.percentage))
        {
            //you win rare item
            rarityItemWin = typeBox.rare.rarity;
        }
        else if (rand <= (typeBox.nothing.percentage + typeBox.common.percentage + typeBox.rare.percentage + typeBox.epic.percentage))
        {
            //you win epic item
            rarityItemWin = typeBox.epic.rarity;
        }
        else if (rand <= (typeBox.nothing.percentage + typeBox.common.percentage + typeBox.rare.percentage + typeBox.epic.percentage + typeBox.legendary.percentage))
        {
            //WHAT YOU JUST WON A UNIQUE LEGENDARY ITEM ? Lucky you.
            rarityItemWin = typeBox.legendary.rarity;
        }
        else
        {
            Debug.LogError("Random value " + rand + " must not be more than 1. Retreive nothing sorry '3' ");
            return goldValue;
        }

        rand = RandomNumber() * 100;
        ItemType typeItem;
        if (rand <= typeBox.floor) typeItem = ItemType.Building;
        else typeItem = ItemType.Decoration;

        Item temp = itemList.GetRandomObjectOfRarityAndType(rarityItemWin, typeItem);
        bool found = false;

        foreach (var rew in rewards)
        {
            if (rew.Item.ID == temp.id)
            {
                rew.Quantity++;
                found = true;
                break;
            }
        }
        if (!found)
        {
            ItemData item = new(temp.id, temp.name, temp.GetSprite(), temp.type, temp.rarity, temp.style, temp.price, temp.area.size.x, temp.area.size.y, temp.area.size.z);
            rewards.Insert(currentItemIndex, new Reward(item, 1));
        }

        return goldValue - temp.price;

    }

    private void AddGoldToReward(int goldToAdd)
    {
        ItemData Gold = itemList.GetItemData("1").Data(); //Gold

        foreach (var rew in rewards)
        {
            if (rew.Item.ID == Gold.ID)
            {
                rew.Quantity += goldToAdd;
                return;
            }
        }
        //if there was no gold reward yet, add it
        rewards.Add(new Reward(Gold, goldToAdd));
    }
    #endregion

    private void ActiveItemButton()
    {
        // More rewards ? -> Show next one
        if (currentItemIndex + 1 < rewards.Count)
        {
            nextItemButton.gameObject.SetActive(true);
            backToIsleButton.gameObject.SetActive(false);
            currentItemIndex++;
        }
        else
        {
            nextItemButton.gameObject.SetActive(false);
            backToIsleButton.gameObject.SetActive(true);
        }
    }
    #endregion

   

    private void Start()
    {

        if (Camera.main != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 999;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        // Get Rewards To Show
        if (CrossSceneInformation.Rewards != null)
        {
            rewards = CrossSceneInformation.Rewards;
            if (rewards.Count <= 0)
            {
                sceneController.UnloadRewardScene();
                return;
            }
            currentItemIndex = 0;
            FillItemPanelWithCurrentItem();
            // Initialize UI
         /*   if (!CrossSceneInformation.gift) ActiveChestPanel(true);
            else ActiveGiftPanel(true);
            ActiveItemPanel(false); */

            // Set Buttons Actions
            chestButton.onClick.AddListener(() =>
            {
                StartCoroutine(ChestAnimationTransition());
            });
            giftButton.onClick.AddListener(() =>
            {
                ActiveItemPanel(true);
                ActiveGiftPanel(false);
            });

            nextItemButton.onClick.AddListener(FillItemPanelWithCurrentItem);
            backToIsleButton.onClick.AddListener(() =>
            {
                CrossSceneInformation.Rewards = null;
               // CrossSceneInformation.gift = false;
                sceneController.UnloadRewardScene();
            });
        }
        else if (CrossSceneInformation.AccountReward != null)
        {
            rewards = new List<Reward>() { CrossSceneInformation.AccountReward };
            if (rewards.Count <= 0)
            {
                sceneController.UnloadRewardScene();
                return;
            }
            currentItemIndex = 0;
            FillItemPanelWithCurrentItem();

            ActiveChestPanel(false);
            ActiveGiftPanel(false);
            ActiveItemPanel(true);

            nextItemButton.onClick.AddListener(FillItemPanelWithCurrentItem);
            backToIsleButton.onClick.AddListener(() =>
            {
                CrossSceneInformation.AccountReward = null;
                sceneController.UnloadRewardScene();
            });
        }
        else
        {
            sceneController.UnloadRewardScene();
        }


    }

    private IEnumerator ChestAnimationTransition()
    {
        chestAnimation.Play("chest-opening");
        yield return new WaitForSeconds(.8f);
        ActiveItemPanel(true);
        ActiveChestPanel(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (chestButton.gameObject.activeInHierarchy)
            {
                chestButton.onClick.Invoke();
            }
            else if (giftButton.gameObject.activeInHierarchy)
            {
                giftButton.onClick.Invoke();
            }
            else if (nextItemButton.gameObject.activeInHierarchy)
            {
                nextItemButton.onClick.Invoke();
            }
            else if (backToIsleButton.gameObject.activeInHierarchy)
            {
                backToIsleButton.onClick.Invoke();
            }
        }
    }
}
