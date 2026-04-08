using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private Button itemsTabButton;
    [SerializeField] private Button boostersTabButton;
    [SerializeField] private GameObject boostersContent;
    [SerializeField] private GameObject itemsContent;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text currentLevelText;

    [Header("Content Settings")]
    [SerializeField] private GameObject boosterCardPrefab;
    [SerializeField] private GameObject itemCardPrefab;
    [SerializeField] private Transform boostersParent;
    [SerializeField] private Transform itemsParent;

    [Header("Item Data")]
    [SerializeField] private string[] items;
    [SerializeField] private Sprite[] itemImages;
    [SerializeField] private int[] itemPrices;
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private ItemType[] itemTypes;

    [Header("Booster Data")]
    [SerializeField] private BoosterData[] boosters;
    [SerializeField] private Boosters boostersController;

    [Header("Player Customization")]
    [SerializeField] private PlayerCustomization playerCustomization;

    private bool _itemsContentBuilt;
    private bool _boostersContentBuilt;

    public enum ItemType
    {
        Hat = 0,
        Goggles = 1,
        Ride = 2,
        Scarf = 3
    }

    private void Awake()
    {
        Instance = this;
    }

    private void HandleCoinsChanged(int totalCoins)
    {
        UpdateUI();
    }

    private void Start()
    {
        WalletManager.OnCoinsChanged += HandleCoinsChanged;
        boostersTabButton.onClick.AddListener(() => ShowBoostersTab());
        itemsTabButton.onClick.AddListener(() => ShowItemsTab());

        ShowItemsTab();
        UpdateUI();
    }

    private void OnDestroy()
    {
        WalletManager.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void UpdateUI()
    {
        int totalCoins = WalletManager.GetTotalCoins();
        totalCoinsText.text = $"{totalCoins}";

        int currentLevel = PlayerPrefs.GetInt("UnlockedLevels", 1);
        currentLevelText.text = $"Level {currentLevel}";
    }

    public void ShowBoostersTab()
    {
        SetTabActive(boostersTabButton, itemsTabButton, boostersContent, itemsContent);
        EnsureBoostersBuilt();
    }

    public void ShowItemsTab()
    {
        SetTabActive(itemsTabButton, boostersTabButton, itemsContent, boostersContent);
        EnsureItemsBuilt();
    }

    private void EnsureItemsBuilt()
    {
        if (_itemsContentBuilt || items == null || items.Length == 0)
            return;
        if (itemCardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned!");
            return;
        }

        foreach (string entry in items)
        {
            GameObject card = Instantiate(itemCardPrefab, itemsParent);
            SetupItemCard(entry, card);
        }

        _itemsContentBuilt = true;
        RewardedAdButton.Instance?.NotifyShopWatchAdUiMayExist();
    }

    private void EnsureBoostersBuilt()
    {
        if (_boostersContentBuilt || boosters == null || boosters.Length == 0)
            return;
        if (boosterCardPrefab == null)
        {
            Debug.LogError("Booster card prefab is not assigned!");
            return;
        }

        foreach (BoosterData bd in boosters)
        {
            GameObject card = Instantiate(boosterCardPrefab, boostersParent);
            SetupBoosterCard(bd, card);
        }

        _boostersContentBuilt = true;
        RewardedAdButton.Instance?.NotifyShopWatchAdUiMayExist();
    }

    private void RefreshAllShopCardsFromPrefs()
    {
        RefreshChildrenShopCards(itemsParent);
        RefreshChildrenShopCards(boostersParent);
    }

    private void RefreshChildrenShopCards(Transform parent)
    {
        if (parent == null)
            return;
        for (int i = 0; i < parent.childCount; i++)
        {
            ShopCard card = parent.GetChild(i).GetComponent<ShopCard>();
            if (card != null)
                card.RefreshFromPrefs(playerCustomization);
        }
    }

    private void SetupBoosterCard(BoosterData boosterData, GameObject card)
    {
        var shopCard = card.GetComponent<ShopCard>();
        if (shopCard != null)
        {
            shopCard.SetupBoosterCard(boosterData, boostersController);
        }
        else
        {
            Debug.LogError("ShopCard component is missing on the booster card prefab!");
        }
    }

    private void SetupItemCard(string itemName, GameObject card)
    {
        int index = System.Array.IndexOf(items, itemName);
        if (index < 0 || index >= items.Length)
        {
            Debug.LogError($"Item {itemName} not found in the items array!");
            return;
        }

        var shopCard = card.GetComponent<ShopCard>();
        if (shopCard != null)
        {
            shopCard.SetupCard(
                items[index],
                itemImages[index],
                itemPrices[index],
                itemPrefabs[index],
                playerCustomization,
                itemTypes[index] == ItemType.Hat,
                itemTypes[index] == ItemType.Goggles,
                itemTypes[index] == ItemType.Ride,
                itemTypes[index] == ItemType.Scarf,
                false
            );
        }
        else
        {
            Debug.LogError("ShopCard component is missing on the item card prefab!");
        }
    }

    private void SetTabActive(Button activeTab, Button inactiveTab, GameObject activeContent, GameObject inactiveContent)
    {
        activeTab.interactable = false;
        inactiveTab.interactable = true;

        activeContent.SetActive(true);
        inactiveContent.SetActive(false);
    }

    public void OpenShop()
    {
        shopCanvas.SetActive(true);
        UpdateUI();
        RefreshAllShopCardsFromPrefs();
        RewardedAdButton.Instance?.NotifyShopWatchAdUiMayExist();

        if (RevenueCatManager.Instance != null)
            RevenueCatManager.Instance.PresentCoinPaywallAfterShopOpened();
    }
}
