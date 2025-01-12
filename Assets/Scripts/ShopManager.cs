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

    [Header("Data")]
    [SerializeField] private Sprite[] itemImages;
    [SerializeField] private int[] boosterPrices = { };
    [SerializeField] private string[] items = { };
    [SerializeField] private Sprite[] boosterImages;
    [SerializeField] private int[] itemPrices = { };
    [SerializeField] private string[] boosters = { };


    private void Awake()
    {
        Instance = this;
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
        if (boostersParent.childCount > 0) return;
        for (int i = 0; i < boosters.Length; i++)
        {
            var boosterCard = Instantiate(boosterCardPrefab, boostersParent);
            boosterCard.GetComponent<ShopCard>().SetupCard(
                boosters[i],
                boosterImages[i],
                boosterPrices[i],
                true
            );
        }
    }

    private void LoadItems()
    {
        if (itemsParent.childCount > 0) return;
        for (int i = 0; i < items.Length; i++)
        {
            var itemCard = Instantiate(itemCardPrefab, itemsParent);
            itemCard.GetComponent<ShopCard>().SetupCard(
                items[i],
                itemImages[i],
                itemPrices[i],
                false
            );
        }
    }

    public void OpenShop()
    {
        shopCanvas.SetActive(true);
    }
}
