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
        boostersTabButton.interactable = false;
        itemsTabButton.interactable = true;
        boostersContent.SetActive(true);
        itemsContent.SetActive(false);
        LoadBoosters();
    }

    public void ShowItemsTab()
    {
        boostersTabButton.interactable = true;
        itemsTabButton.interactable = false;
        itemsContent.SetActive(true);
        boostersContent.SetActive(false);
        LoadItems();
    }

    private void LoadBoosters()
    {
        foreach (Transform child in boostersParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var boosterData in boosters)
        {
            var boosterCard = Instantiate(boosterCardPrefab, boostersParent);
            boosterCard.GetComponent<ShopCard>().SetupBoosterCard(boosterData, boostersController);
        }
    }

    private void LoadItems()
    {
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < items.Length; i++)
        {
            var itemCard = Instantiate(itemCardPrefab, itemsParent);
            bool isHat = itemTypes[i] == ItemType.Hat;
            bool isGoggles = itemTypes[i] == ItemType.Goggles;
            bool isRide = itemTypes[i] == ItemType.Ride;
            bool isScarf = itemTypes[i] == ItemType.Scarf;

            itemCard.GetComponent<ShopCard>().SetupCard(
                items[i],
                itemImages[i],
                itemPrices[i],
                itemPrefabs[i],
                playerCustomization,
                isHat,
                isGoggles,
                isRide,
                isScarf,
                false
            );
        }
    }

    public void OpenShop()
    {
        shopCanvas.SetActive(true);
    }
}
