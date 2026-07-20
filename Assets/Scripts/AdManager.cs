using System.Collections;
using UnityEngine;
using Unity.Services.LevelPlay;

// LevelPlay 8.x still types its public events with the deprecated
// com.unity3d.mediation aliases, so referencing them is unavoidable here.
#pragma warning disable 0618

public class InterstitialAd : MonoBehaviour
{
    public static InterstitialAd Instance;

    [Header("LevelPlay Interstitial Ad Unit IDs")]
    [SerializeField] private string _androidInterstitialAdUnitId = "lffdi21azjq03dw2";
    [SerializeField] private string _iOsInterstitialAdUnitId = "ooht1ea0q2gypi49";

    [Header("Ad Settings")]
    [SerializeField] private bool _autoShowOnLoad = true;

    private LevelPlayInterstitialAd _interstitialAd;
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
            return;
        }
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

        CreateInterstitialAd();
    }

    private void CreateInterstitialAd()
    {
        if (_interstitialAd != null)
            return;

#if UNITY_IOS
        string adUnitId = _iOsInterstitialAdUnitId;
#else
        string adUnitId = _androidInterstitialAdUnitId;
#endif

        _interstitialAd = new LevelPlayInterstitialAd(adUnitId);
        _interstitialAd.OnAdLoaded += OnAdLoaded;
        _interstitialAd.OnAdLoadFailed += OnAdLoadFailed;
        _interstitialAd.OnAdDisplayFailed += OnAdDisplayFailed;
        _interstitialAd.OnAdClosed += OnAdClosed;
    }

    public void LoadAd()
    {
        if (_interstitialAd == null || _isLoading || _adLoaded || _isShowing || IsAnyAdShowing())
        {
            return;
        }

        _isLoading = true;
        _interstitialAd.LoadAd();
    }

    private void OnAdLoaded(com.unity3d.mediation.LevelPlayAdInfo adInfo)
    {
        _adLoaded = true;
        _isLoading = false;

        if (_autoShowOnLoad && !IsAnyAdShowing())
        {
            ShowAd();
        }
    }

    private void OnAdLoadFailed(com.unity3d.mediation.LevelPlayAdError error)
    {
        _isLoading = false;
    }

    public void ShowAd()
    {
        if (_interstitialAd == null)
        {
            return;
        }

        if (!_adLoaded || !_interstitialAd.IsAdReady())
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
        _interstitialAd.ShowAd();
    }

    private void OnAdDisplayFailed(com.unity3d.mediation.LevelPlayAdDisplayInfoError error)
    {
        _isShowing = false;
        NotifyRewardedAdToReload();
    }

    private void OnAdClosed(com.unity3d.mediation.LevelPlayAdInfo adInfo)
    {
        _isShowing = false;
        NotifyRewardedAdToReload();
    }

    private void NotifyRewardedAdToReload()
    {
        if (RewardedAdButton.Instance != null)
        {
            RewardedAdButton.Instance.LoadAd();
        }
    }

    public bool IsAdShowing()
    {
        return _isShowing;
    }

    private bool IsAnyAdShowing()
    {
        return RewardedAdButton.Instance != null && RewardedAdButton.Instance.IsAdShowing();
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        if (Instance != this)
            return;

        if (_interstitialAd != null)
        {
            _interstitialAd.DestroyAd();
            _interstitialAd = null;
        }
    }
}
