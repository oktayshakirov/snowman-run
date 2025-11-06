using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static InterstitialAd Instance;
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] string _iOsAdUnitId = "Interstitial_iOS";

    [Header("Ad Settings")]
    [SerializeField] private bool _autoShowOnLoad = true;

    string _adUnitId;
    private bool _adLoaded = false;
    private bool _isLoading = false;
    private bool _isShowing = false;

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
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;
    }

    public void LoadAd()
    {
        if (_isLoading || _adLoaded || IsAnyAdShowing())
        {
            return;
        }

        _isLoading = true;
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        _adLoaded = true;
        _isLoading = false;

        if (_autoShowOnLoad && !IsAnyAdShowing())
        {
            ShowAd();
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load Interstitial Ad {adUnitId}: {error} - {message}");
        _isLoading = false;
    }

    public void ShowAd()
    {
        if (!_adLoaded)
        {
            LoadAd();
            return;
        }

        if (_isShowing || IsAnyAdShowing())
        {
            return;
        }

        _isShowing = true;
        _adLoaded = false;
        NotifyRewardedAdToReload();
        Advertisement.Show(_adUnitId, this);
    }

    private void NotifyRewardedAdToReload()
    {
        if (RewardedAdButton.Instance != null)
        {
            RewardedAdButton.Instance.LoadAd();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Interstitial Ad {adUnitId}: {error} - {message}");
        _isShowing = false;
        NotifyRewardedAdToReload();
    }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        _isShowing = false;
        NotifyRewardedAdToReload();
    }

    public bool IsAdShowing()
    {
        return _isShowing;
    }

    private bool IsAnyAdShowing()
    {
        return RewardedAdButton.Instance != null && RewardedAdButton.Instance.IsAdShowing();
    }
}