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

    [Header("Optional UI")]
    [SerializeField] private Button _showAdButton;

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
        Debug.Log($"Loading Rewarded Ad: {_adUnitId}");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Rewarded Ad Loaded: {adUnitId}");
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
            Debug.Log($"Retrying to load ad (attempt {_retryCount}/{_maxRetries})...");
            StartCoroutine(RetryLoadAd());
        }
        else
        {
            Debug.LogWarning("Max retries reached. Will retry later.");
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
            Debug.Log("Rewarded ad not ready yet. Loading now...");
            if (!_isLoading && !IsAnyAdShowing())
            {
                LoadAd();
            }
            return;
        }

        if (_isShowing || IsAnyAdShowing())
        {
            Debug.Log("Another ad is currently showing. Please wait.");
            return;
        }

        Debug.Log($"Showing Rewarded Ad: {_adUnitId}");
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
        if (InterstitialAd.Instance != null && InterstitialAd.Instance.IsAdShowing())
        {
            return true;
        }
        return false;
    }

    private void UpdateButtonVisibility()
    {
        if (_showAdButton != null)
        {
            _showAdButton.gameObject.SetActive(_adLoaded);
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Rewarded Ad {adUnitId}: {error} - {message}");
        _isShowing = false;

        LoadAd();
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("Rewarded ad started...");
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log("Rewarded ad clicked.");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        _isShowing = false;

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("✅ User watched full ad — grant reward!");
            GrantReward();
        }
        else
        {
            Debug.Log("Ad not completed — no reward.");
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