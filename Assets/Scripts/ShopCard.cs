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

    [Header("Customization Type")]
    [SerializeField] private bool isHat;
    [SerializeField] private bool isGoggles;
    [SerializeField] private bool isRide;

    private GameObject itemPrefab;
    private string itemName;
    private int itemPrice;
    private bool isPurchased;

    private PlayerCustomization playerCustomization;

    private void OnEnable()
    {
        PlayerCustomization.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void OnDisable()
    {
        PlayerCustomization.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    public void SetupCard(string name, Sprite image, int price, GameObject prefab, PlayerCustomization customization, bool isHatCard, bool isGogglesCard, bool isRideCard, bool isBoosterCard)
    {
        itemName = name;
        itemPrice = price;
        itemPrefab = prefab;
        isHat = isHatCard;
        isGoggles = isGogglesCard;
        isRide = isRideCard;
        playerCustomization = customization;

        isPurchased = PlayerPrefs.GetInt($"Purchased_{itemName}", itemPrice == 0 ? 1 : 0) == 1;

        nameText.text = name;
        itemImage.sprite = image;
        priceText.text = itemPrice.ToString();
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => PerformAction());
        UpdateButtonState(playerCustomization.CurrentHatName, playerCustomization.CurrentGogglesName, playerCustomization.CurrentRideName);
    }


    private void PerformAction()
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

    private void BuyItem()
    {
        if (WalletManager.SpendCoins(itemPrice))
        {
            isPurchased = true;
            PlayerPrefs.SetInt($"Purchased_{itemName}", 1);
            PlayerPrefs.Save();
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
        if (isHat)
        {
            playerCustomization.EquipHat(itemPrefab);
        }
        else if (isGoggles)
        {
            playerCustomization.EquipGoggles(itemPrefab);
        }
        else if (isRide)
        {
            playerCustomization.EquipRide(itemPrefab);
        }

        Debug.Log($"{itemName} is now in use!");
    }

    private void HandleEquipmentChanged(string currentHat, string currentGoggles, string currentRide)
    {
        UpdateButtonState(currentHat, currentGoggles, currentRide);
    }

    private void UpdateButtonState(string currentHat = null, string currentGoggles = null, string currentRide = null)
    {
        bool isInUse = (isHat && currentHat == itemName) ||
                       (isGoggles && currentGoggles == itemName) ||
                       (isRide && currentRide == itemName);

        if (isInUse)
        {
            actionButton.interactable = false;
            actionButtonText.text = "In Use";
            priceText.gameObject.SetActive(false);
            coinIcon.gameObject.SetActive(false);
        }
        else if (isPurchased)
        {
            actionButton.interactable = true;
            actionButtonText.text = "Use";
            priceText.gameObject.SetActive(false);
            coinIcon.gameObject.SetActive(false);
        }
        else
        {
            actionButton.interactable = true;
            actionButtonText.text = "Buy";
            priceText.gameObject.SetActive(true);
            coinIcon.gameObject.SetActive(true);
        }
    }
}
