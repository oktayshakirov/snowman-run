using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using System.Collections;

public class RewardedAdButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static RewardedAdButton Instance;

    [Header("Rewarded Ad IDs")]
    [SerializeField] private string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] private string _iOsAdUnitId = "Rewarded_iOS";
    private string _adUnitId;

    [Header("Rewarded Ad Buttons")]
    [SerializeField] private string _startScreenButtonName = "WatchAd";
    [SerializeField] private string _shopScreenButtonName = "WatchAd";

    [Header("Retry Settings")]
    [SerializeField] private float _retryDelay = 3f;
    [SerializeField] private int _maxRetries = 5;

    private bool _adLoaded = false;
    private bool _isLoading = false;
    private bool _isShowing = false;
    private int _retryCount = 0;

    private Button _cachedStartWatchButton;
    private Button _cachedShopWatchButton;
    private bool _shopWatchLocateIdle;
    private bool _pendingShopWatchResolve;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_IOS
        _adUnitId = _iOsAdUnitId;
#else
        _adUnitId = _androidAdUnitId;
#endif

        UpdateButtonVisibility();
    }

    void Start()
    {
        StartCoroutine(WaitForInitialization());
    }

    private IEnumerator WaitForInitialization()
    {
        while (AdsInitializer.Instance == null || !AdsInitializer.Instance.IsInitialized())
        {
            yield return null;
        }

        LoadAd();
    }

    public void LoadAd()
    {
        if (_isLoading || _adLoaded || IsAnyAdShowing())
        {
            return;
        }

        _isLoading = true;
        _retryCount = 0;
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        _adLoaded = true;
        _isLoading = false;
        _retryCount = 0;
        UpdateButtonVisibility();
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load Rewarded Ad {adUnitId}: {error} - {message}");
        _isLoading = false;
        UpdateButtonVisibility();

        if (_retryCount < _maxRetries)
        {
            _retryCount++;
            StartCoroutine(RetryLoadAd());
        }
        else
        {
            _retryCount = 0;
            StartCoroutine(RetryLoadAdAfterDelay(_retryDelay * 2));
        }
    }

    private IEnumerator RetryLoadAd()
    {
        yield return new WaitForSeconds(_retryDelay);
        LoadAd();
    }

    private IEnumerator RetryLoadAdAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void ShowAd()
    {
        if (!_adLoaded)
        {
            if (!_isLoading && !IsAnyAdShowing())
            {
                LoadAd();
            }
            return;
        }

        if (_isShowing || IsAnyAdShowing())
        {
            return;
        }

        _isShowing = true;
        _adLoaded = false;
        UpdateButtonVisibility();

        Advertisement.Show(_adUnitId, this);
        LoadAd();
    }

    public bool IsAdReady()
    {
        return _adLoaded;
    }

    public bool IsAdShowing()
    {
        return _isShowing;
    }

    private bool IsAnyAdShowing()
    {
        return InterstitialAd.Instance != null && InterstitialAd.Instance.IsAdShowing();
    }

    private void UpdateButtonVisibility()
    {
        UpdateAllButtons();
    }

    private void UpdateAllButtons()
    {
        ResolveRewardedWatchButtonsIfNeeded();

        ApplyVisibilityToButton(_cachedStartWatchButton);
        ApplyVisibilityToButton(_cachedShopWatchButton);
    }

    public void NotifyShopWatchAdUiMayExist()
    {
        _cachedShopWatchButton = null;
        _pendingShopWatchResolve = true;
        _shopWatchLocateIdle = false;
    }

    private void ApplyVisibilityToButton(Button button)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(_adLoaded);
        if (_adLoaded)
            button.interactable = true;
    }

    private void ResolveRewardedWatchButtonsIfNeeded()
    {
        if (_cachedStartWatchButton == null || !_cachedStartWatchButton)
        {
            _cachedStartWatchButton = null;
            _cachedShopWatchButton = null;
            _shopWatchLocateIdle = false;
            ScanWatchButtons(assignStart: true, assignShop: true);
            if (_cachedShopWatchButton == null)
                _shopWatchLocateIdle = true;
            _pendingShopWatchResolve = false;
            return;
        }

        bool shopGone = _cachedShopWatchButton == null || !_cachedShopWatchButton;
        if (_pendingShopWatchResolve || (shopGone && !_shopWatchLocateIdle))
        {
            if (shopGone)
                _cachedShopWatchButton = null;
            ScanWatchButtons(assignStart: false, assignShop: true);
            _shopWatchLocateIdle = _cachedShopWatchButton == null;
            _pendingShopWatchResolve = false;
        }
    }

    private void ScanWatchButtons(bool assignStart, bool assignShop)
    {
        string startBn = _startScreenButtonName.ToLower();
        string shopBn = _shopScreenButtonName.ToLower();
        const string startCv = "startscreen";
        const string shopCv = "shop";

        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (btn == null)
                continue;

            string n = btn.name.ToLower();
            if (assignStart && !n.Contains(startBn) && assignShop && !n.Contains(shopBn))
                continue;
            if (assignStart && !assignShop && !n.Contains(startBn))
                continue;
            if (!assignStart && assignShop && !n.Contains(shopBn))
                continue;

            bool underStart = false;
            bool underShop = false;
            for (Transform p = btn.transform.parent; p != null; p = p.parent)
            {
                string pn = p.name.ToLower();
                if (pn.Contains(startCv))
                    underStart = true;
                if (pn.Contains(shopCv))
                    underShop = true;
            }

            if (assignStart && n.Contains(startBn) && underStart && _cachedStartWatchButton == null)
            {
                _cachedStartWatchButton = btn;
                SetupButton(btn);
            }

            if (assignShop && n.Contains(shopBn) && underShop && _cachedShopWatchButton == null)
            {
                _cachedShopWatchButton = btn;
                SetupButton(btn);
            }
        }
    }

    private void SetupButton(Button button)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ShowAd);
        }
    }

    public void RefreshButtonVisibility()
    {
        UpdateButtonVisibility();

        if (!_adLoaded && !_isLoading && Advertisement.isInitialized && !IsAnyAdShowing())
        {
            LoadAd();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Rewarded Ad {adUnitId}: {error} - {message}");
        _isShowing = false;
        LoadAd();
    }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        _isShowing = false;

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            GrantReward();
        }

        LoadAd();
    }

    private void GrantReward()
    {
        WalletManager.AddCoins(300);
        AudioManager.Instance?.PlaySound(AudioManager.SoundType.Coin);
        NativeHaptics.TriggerSuccessNotification();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}