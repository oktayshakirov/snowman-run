using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas; // Shop Canvas
    [SerializeField] private Button boostersTabButton; // Tab button for Boosters
    [SerializeField] private Button itemsTabButton; // Tab button for Items
    [SerializeField] private GameObject boostersContent; // Content container for Boosters
    [SerializeField] private GameObject itemsContent; // Content container for Items

    [Header("Content Settings")]
    [SerializeField] private GameObject boosterCardPrefab; // Prefab for a booster card
    [SerializeField] private GameObject itemCardPrefab; // Prefab for an item card
    [SerializeField] private Transform boostersParent; // Parent object for booster cards
    [SerializeField] private Transform itemsParent; // Parent object for item cards

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        boostersTabButton.onClick.AddListener(() => ShowBoostersTab());
        itemsTabButton.onClick.AddListener(() => ShowItemsTab());

        ShowBoostersTab(); // Default tab
    }

    public void ShowBoostersTab()
    {
        // Highlight selected tab
        boostersTabButton.interactable = false;
        itemsTabButton.interactable = true;

        // Show boosters and hide items
        boostersContent.SetActive(true);
        itemsContent.SetActive(false);

        // Load boosters if not already loaded
        LoadBoosters();
    }

    public void ShowItemsTab()
    {
        // Highlight selected tab
        boostersTabButton.interactable = true;
        itemsTabButton.interactable = false;

        // Show items and hide boosters
        itemsContent.SetActive(true);
        boostersContent.SetActive(false);

        // Load items if not already loaded
        LoadItems();
    }

    private void LoadBoosters()
    {
        if (boostersParent.childCount > 0) return; // Prevent duplicate loading

        // Example boosters data
        string[] boosters = { "Speed Boost", "Double Coins", "Shield" };
        foreach (var booster in boosters)
        {
            var boosterCard = Instantiate(boosterCardPrefab, boostersParent);
            boosterCard.GetComponent<ShopCard>().SetupCard(booster, "Upgrade");
        }
    }

    private void LoadItems()
    {
        if (itemsParent.childCount > 0) return; // Prevent duplicate loading

        // Example items data
        string[] items = { "Snowboard", "Hat", "Goggles" };
        foreach (var item in items)
        {
            var itemCard = Instantiate(itemCardPrefab, itemsParent);
            itemCard.GetComponent<ShopCard>().SetupCard(item, "Buy");
        }
    }

    public void OpenShop()
    {
        shopCanvas.SetActive(true);
    }

    public void CloseShop()
    {
        shopCanvas.SetActive(false);
    }
}
