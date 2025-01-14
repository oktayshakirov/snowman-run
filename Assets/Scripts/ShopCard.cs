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
    [SerializeField] public TMP_Text descriptionText;
    [SerializeField] public TMP_Text boosterStateText;

    [Header("Customization Type")]
    [SerializeField] private bool isHat;
    [SerializeField] private bool isGoggles;
    [SerializeField] private bool isRide;
    [SerializeField] private bool isScarf;

    private GameObject itemPrefab;
    private string itemName;
    private int itemPrice;

    private bool isPurchased;
    private PlayerCustomization playerCustomization;
    private BoosterData boosterData;
    private Boosters boostersController;
    private int currentUpgradeLevel;

    private void OnEnable()
    {
        PlayerCustomization.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void OnDisable()
    {
        PlayerCustomization.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    public void SetupCard(string name, Sprite image, int price, GameObject prefab, PlayerCustomization customization, bool isHatCard, bool isGogglesCard, bool isRideCard, bool isScarfCard, bool isBoosterCard)
    {
        itemName = name;
        itemPrice = price;
        itemPrefab = prefab;
        isHat = isHatCard;
        isGoggles = isGogglesCard;
        isRide = isRideCard;
        isScarf = isScarfCard;
        playerCustomization = customization;

        isPurchased = PlayerPrefs.GetInt($"Purchased_{itemName}", itemPrice == 0 ? 1 : 0) == 1;

        nameText.text = name;
        itemImage.sprite = image;
        priceText.text = itemPrice.ToString();
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => PerformAction());
        UpdateButtonState(playerCustomization.CurrentHatName, playerCustomization.CurrentGogglesName, playerCustomization.CurrentRideName, playerCustomization.CurrentScarfName); // Include Scarf
    }

    public void SetupBoosterCard(BoosterData data, Boosters controller)
    {
        boosterData = data;
        boostersController = controller;
        currentUpgradeLevel = PlayerPrefs.GetInt($"{boosterData.boosterName}_UpgradeLevel", 0);

        nameText.text = boosterData.boosterName;
        itemImage.sprite = boosterData.boosterImage;

        // Display the description
        descriptionText.text = boosterData.description;

        // Update the booster UI
        UpdateBoosterUI();

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(UpgradeBooster);
    }


    private void UpgradeBooster()
    {
        while (currentUpgradeLevel < boosterData.maxUpgrades)
        {
            int upgradeCost = boosterData.basePrice * (int)Mathf.Pow(2, currentUpgradeLevel);
            if (WalletManager.SpendCoins(upgradeCost))
            {
                currentUpgradeLevel++;
                PlayerPrefs.SetInt($"{boosterData.boosterName}_UpgradeLevel", currentUpgradeLevel);
                PlayerPrefs.Save();
                boostersController.ApplyBoosterUpgrade(boosterData);
                NativeHaptics.TriggerHeavyHaptic();
                AudioManager.Instance.PlaySound(AudioManager.SoundType.Buy);
                UpdateBoosterUI();

                Debug.Log($"{boosterData.boosterName} upgraded to level {currentUpgradeLevel}!");
            }
            else
            {
                Debug.Log("Not enough coins for further upgrades!");
                break;
            }
        }
    }


    private void UpdateBoosterUI()
    {
        string actualState = boosterData.GetBoosterState(currentUpgradeLevel);
        string nextState = currentUpgradeLevel < boosterData.maxUpgrades
            ? boosterData.GetBoosterState(currentUpgradeLevel, true)
            : null;
        boosterStateText.text = $"Actual: {actualState}" +
                                (nextState != null ? $"\nNext: {nextState}" : "");

        if (currentUpgradeLevel < boosterData.maxUpgrades)
        {
            int upgradeCost = boosterData.basePrice * (int)Mathf.Pow(2, currentUpgradeLevel);
            priceText.text = $"{upgradeCost}";
            actionButtonText.text = "Upgrade";
            coinIcon.gameObject.SetActive(true);
            priceText.gameObject.SetActive(true);
        }
        else
        {
            actionButtonText.text = "Maxed Out";
            actionButton.interactable = false;
            coinIcon.gameObject.SetActive(false);
            priceText.gameObject.SetActive(false);
        }
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
            NativeHaptics.TriggerMediumHaptic();
            AudioManager.Instance.PlaySound(AudioManager.SoundType.Buy);
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
        else if (isScarf)
        {
            playerCustomization.EquipScarf(itemPrefab);
        }
        NativeHaptics.TriggerHeavyHaptic();
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Equip);
        Debug.Log($"{itemName} is now in use!");
    }

    private void HandleEquipmentChanged(string currentHat, string currentGoggles, string currentRide, string currentScarf)
    {
        UpdateButtonState(currentHat, currentGoggles, currentRide, currentScarf);
    }

    private void UpdateButtonState(string currentHat = null, string currentGoggles = null, string currentRide = null, string currentScarf = null)
    {
        bool isInUse = (isHat && currentHat == itemName) ||
                       (isGoggles && currentGoggles == itemName) ||
                       (isRide && currentRide == itemName) ||
                       (isScarf && currentScarf == itemName);

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
