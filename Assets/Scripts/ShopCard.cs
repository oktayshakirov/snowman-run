using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCard : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public TMP_Text nameText;
    [SerializeField] public Image itemImage;
    [SerializeField] public TMP_Text priceText;
    [SerializeField] public Button actionButton;
    [SerializeField] public TMP_Text actionButtonText;
    [SerializeField] public Image coinIcon;

    private string itemName;
    private int itemPrice;
    private bool isPurchased = false;
    private bool isBooster = false;

    private int upgradeLevel = 0;

    public void SetupCard(string name, Sprite image, int price, bool isBoosterCard)
    {
        itemName = name;
        itemPrice = price;
        isBooster = isBoosterCard;
        nameText.text = name;
        itemImage.sprite = image;
        priceText.text = itemPrice.ToString();
        actionButtonText.text = isBooster ? "Upgrade" : "Buy";
        actionButton.onClick.AddListener(() => PerformAction());
        UpdateButtonState();
    }

    private void PerformAction()
    {
        if (isBooster)
        {
            UpgradeBooster();
        }
        else
        {
            if (!isPurchased)
            {
                BuyItem();
            }
            else
            {
                UseItem();
            }
        }
    }

    private void BuyItem()
    {
        if (WalletManager.SpendCoins(itemPrice))
        {
            isPurchased = true;
            Debug.Log($"{itemName} purchased!");
            UpdateButtonState();
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    private void UseItem()
    {
        Debug.Log($"{itemName} is now in use!");
    }

    private void UpgradeBooster()
    {
        if (WalletManager.SpendCoins(itemPrice))
        {
            upgradeLevel++;
            Debug.Log($"{itemName} upgraded to level {upgradeLevel}!");
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    private void UpdateButtonState()
    {
        if (isBooster)
        {
            actionButtonText.text = "Upgrade";
        }
        else
        {
            actionButtonText.text = isPurchased ? "Use" : "Buy";
            priceText.gameObject.SetActive(!isPurchased);
            coinIcon.gameObject.SetActive(!isPurchased);
        }
    }
}
