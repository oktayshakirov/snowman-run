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

    private void OnEnable()
    {
        WalletManager.OnCoinsChanged += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        WalletManager.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void HandleCoinsChanged(int totalCoins)
    {
        UpdateUI();
    }

    private void Start()
    {
        boostersTabButton.onClick.AddListener(() => ShowBoostersTab());
        itemsTabButton.onClick.AddListener(() => ShowItemsTab());

        ShowItemsTab();
        UpdateUI();
    }

    private void UpdateUI()
    {
        int totalCoins = WalletManager.GetTotalCoins();
        totalCoinsText.text = $"{totalCoins}";

        int currentLevel = PlayerPrefs.GetInt("UnlockedLevels", 1);
        currentLevelText.text = $"Level: {currentLevel}";
    }

    public void ShowBoostersTab()
    {
        SetTabActive(boostersTabButton, itemsTabButton, boostersContent, itemsContent);
        LoadContent(boostersParent, boosters, boosterCardPrefab, SetupBoosterCard);
    }

    public void ShowItemsTab()
    {
        SetTabActive(itemsTabButton, boostersTabButton, itemsContent, boostersContent);
        LoadContent(itemsParent, items, itemCardPrefab, SetupItemCard);
    }

    private void LoadContent<T>(Transform parent, T[] data, GameObject cardPrefab, System.Action<T, GameObject> setupCard)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
        if (data == null || data.Length == 0)
        {
            Debug.LogWarning("No data available to load content.");
            return;
        }
        foreach (var entry in data)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab is not assigned!");
                return;
            }

            var card = Instantiate(cardPrefab, parent);
            setupCard(entry, card);
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
    }
}
