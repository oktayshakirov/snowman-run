using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private Button styleTabButton;
    [SerializeField] private Button boostersTabButton;
    [SerializeField] private GameObject boostersContent;
    [SerializeField] private GameObject styleContent;

    [Header("Content Settings")]
    [SerializeField] private GameObject boosterCardPrefab;
    [SerializeField] private GameObject itemCardPrefab;
    [SerializeField] private Transform boostersParent;
    [SerializeField] private Transform styleParent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        boostersTabButton.onClick.AddListener(() => ShowBoostersTab());
        styleTabButton.onClick.AddListener(() => ShowStyleTab());

        ShowBoostersTab();
    }

    public void ShowBoostersTab()
    {
        boostersTabButton.interactable = false;
        styleTabButton.interactable = true;

        // Show boosters and hide Style
        boostersContent.SetActive(true);
        styleContent.SetActive(false);

        // Load boosters if not already loaded
        LoadBoosters();
    }

    public void ShowStyleTab()
    {
        // Highlight selected tab
        boostersTabButton.interactable = true;
        styleTabButton.interactable = false;

        // Show Style and hide boosters
        styleContent.SetActive(true);
        boostersContent.SetActive(false);

        // Load items if not already loaded
        LoadStyle();
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

    private void LoadStyle()
    {
        if (styleParent.childCount > 0) return; // Prevent duplicate loading

        // Example items data
        string[] items = { "Snowboard", "Hat", "Goggles" };
        foreach (var item in items)
        {
            var itemCard = Instantiate(itemCardPrefab, styleParent);
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
