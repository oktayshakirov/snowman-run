using System.Collections;
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

    [Header("Performance")]
    [SerializeField] private int cardsPerFrame = 6;

    private Animator _openAnimator;
    private float _openAnimationLength = 1.5f;

    private int _itemCardsBuilt;
    private int _boosterCardsBuilt;
    private bool _itemsBuildRunning;
    private bool _boostersBuildRunning;

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

        _openAnimator = shopCanvas.GetComponent<Animator>();
        if (_openAnimator != null && _openAnimator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in _openAnimator.runtimeAnimatorController.animationClips)
                _openAnimationLength = Mathf.Max(_openAnimationLength, clip.length);
        }

        IsolateShopCanvas();
        OptimizeScrollMasks();
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
    }

    private void OnEnable()
    {
        UpdateUI();
        RefreshAllShopCardsFromPrefs();
        RewardedAdButton.Instance?.NotifyShopWatchAdUiMayExist();

        if (_openAnimator != null)
        {
            _openAnimator.enabled = true;
            StartCoroutine(DisableAnimatorWhenDone());
        }

        if (_itemCardsBuilt > 0 && items != null && _itemCardsBuilt < items.Length)
            EnsureItemsBuilt();
        if (_boosterCardsBuilt > 0 && boosters != null && _boosterCardsBuilt < boosters.Length)
            EnsureBoostersBuilt();
    }

    private void OnDisable()
    {
        _itemsBuildRunning = false;
        _boostersBuildRunning = false;
    }

    private void OnDestroy()
    {
        WalletManager.OnCoinsChanged -= HandleCoinsChanged;
    }

    // The whole game UI shares one root Canvas; without its own nested Canvas,
    // any UI change anywhere forces Unity to rebatch all shop cards (and vice versa).
    private void IsolateShopCanvas()
    {
        if (shopCanvas.GetComponent<Canvas>() != null)
            return;

        shopCanvas.AddComponent<Canvas>();
        shopCanvas.AddComponent<GraphicRaycaster>();
    }

    // Stencil Mask renders every card each frame, even offscreen ones;
    // RectMask2D culls cards outside the viewport entirely.
    private void OptimizeScrollMasks()
    {
        foreach (Mask mask in shopCanvas.GetComponentsInChildren<Mask>(true))
        {
            GameObject maskObject = mask.gameObject;
            Image maskImage = maskObject.GetComponent<Image>();
            DestroyImmediate(mask);
            if (maskImage != null)
                maskImage.enabled = false;
            if (maskObject.GetComponent<RectMask2D>() == null)
                maskObject.AddComponent<RectMask2D>();
        }
    }

    // Once the open animation finishes, a still-enabled Animator keeps writing the
    // same values every frame, dirtying the canvas and forcing constant rebuilds.
    private IEnumerator DisableAnimatorWhenDone()
    {
        yield return new WaitForSecondsRealtime(_openAnimationLength + 0.1f);
        if (_openAnimator != null)
            _openAnimator.enabled = false;
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
        if (items == null || items.Length == 0 || _itemsBuildRunning || _itemCardsBuilt >= items.Length)
            return;
        if (itemCardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned!");
            return;
        }

        StartCoroutine(BuildItemCards());
    }

    private void EnsureBoostersBuilt()
    {
        if (boosters == null || boosters.Length == 0 || _boostersBuildRunning || _boosterCardsBuilt >= boosters.Length)
            return;
        if (boosterCardPrefab == null)
        {
            Debug.LogError("Booster card prefab is not assigned!");
            return;
        }

        StartCoroutine(BuildBoosterCards());
    }

    // Instantiating all cards in a single frame causes a long hitch on first open,
    // so the build is spread across frames.
    private IEnumerator BuildItemCards()
    {
        _itemsBuildRunning = true;
        while (_itemCardsBuilt < items.Length)
        {
            int end = Mathf.Min(_itemCardsBuilt + Mathf.Max(1, cardsPerFrame), items.Length);
            for (; _itemCardsBuilt < end; _itemCardsBuilt++)
            {
                GameObject card = Instantiate(itemCardPrefab, itemsParent);
                SetupItemCard(_itemCardsBuilt, card);
            }

            if (_itemCardsBuilt < items.Length)
                yield return null;
        }

        _itemsBuildRunning = false;
        RewardedAdButton.Instance?.NotifyShopWatchAdUiMayExist();
    }

    private IEnumerator BuildBoosterCards()
    {
        _boostersBuildRunning = true;
        while (_boosterCardsBuilt < boosters.Length)
        {
            int end = Mathf.Min(_boosterCardsBuilt + Mathf.Max(1, cardsPerFrame), boosters.Length);
            for (; _boosterCardsBuilt < end; _boosterCardsBuilt++)
            {
                GameObject card = Instantiate(boosterCardPrefab, boostersParent);
                SetupBoosterCard(boosters[_boosterCardsBuilt], card);
            }

            if (_boosterCardsBuilt < boosters.Length)
                yield return null;
        }

        _boostersBuildRunning = false;
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

    private void SetupItemCard(int index, GameObject card)
    {
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

        if (RevenueCatManager.Instance != null)
            RevenueCatManager.Instance.PresentCoinPaywallAfterShopOpened();
    }
}
