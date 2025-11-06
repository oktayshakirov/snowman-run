using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
        Button[] buttons = FindAllRewardedAdButtons();
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
                button.interactable = _adLoaded;
            }
        }
    }

    private Button[] FindAllRewardedAdButtons()
    {
        List<Button> buttons = new List<Button>();

        Button startButton = FindButtonInCanvas(_startScreenButtonName, "StartScreen");
        if (startButton != null)
        {
            SetupButton(startButton);
            buttons.Add(startButton);
        }

        Button shopButton = FindButtonInCanvas(_shopScreenButtonName, "Shop");
        if (shopButton != null)
        {
            SetupButton(shopButton);
            buttons.Add(shopButton);
        }

        return buttons.ToArray();
    }

    private Button FindButtonInCanvas(string buttonName, string canvasName)
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        string buttonNameLower = buttonName.ToLower();
        string canvasNameLower = canvasName.ToLower();

        foreach (Button btn in allButtons)
        {
            if (btn == null) continue;

            if (btn.name.ToLower().Contains(buttonNameLower))
            {
                Transform parent = btn.transform.parent;
                while (parent != null)
                {
                    if (parent.name.ToLower().Contains(canvasNameLower))
                    {
                        return btn;
                    }
                    parent = parent.parent;
                }
            }
        }

        return null;
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