using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;

// LevelPlay 8.x still types its public events with the deprecated
// com.unity3d.mediation aliases, so referencing them is unavoidable here.
#pragma warning disable 0618

public class RewardedAdButton : MonoBehaviour
{
    public static RewardedAdButton Instance;

    [Header("LevelPlay Rewarded Ad Unit IDs")]
    [SerializeField] private string _androidRewardedAdUnitId = "nvjpzi3lio9ymvc1";
    [SerializeField] private string _iOsRewardedAdUnitId = "9wyg2x5zcbjyva2f";

    [Header("Reward Settings")]
    [Tooltip("Used if the dashboard does not supply a reward amount.")]
    [SerializeField] private int _fallbackRewardAmount = 300;

    [Header("Rewarded Ad Buttons")]
    [SerializeField] private Button _startScreenWatchButton;
    [SerializeField] private Button _shopScreenWatchButton;

    [Header("Retry Settings")]
    [SerializeField] private float _retryDelay = 3f;
    [SerializeField] private int _maxRetries = 5;

    private LevelPlayRewardedAd _rewardedAd;
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
            return;
        }

        SetupButton(_startScreenWatchButton);
        SetupButton(_shopScreenWatchButton);
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

        CreateRewardedAd();
        LoadAd();
    }

    private void CreateRewardedAd()
    {
        if (_rewardedAd != null)
            return;

#if UNITY_IOS
        string adUnitId = _iOsRewardedAdUnitId;
#else
        string adUnitId = _androidRewardedAdUnitId;
#endif

        _rewardedAd = new LevelPlayRewardedAd(adUnitId);
        _rewardedAd.OnAdLoaded += OnAdLoaded;
        _rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        _rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        _rewardedAd.OnAdRewarded += OnAdRewarded;
        _rewardedAd.OnAdClosed += OnAdClosed;
    }

    public void LoadAd()
    {
        if (_rewardedAd == null || _isLoading || _adLoaded || _isShowing || IsAnyAdShowing())
        {
            return;
        }

        _isLoading = true;
        _rewardedAd.LoadAd();
    }

    private void OnAdLoaded(com.unity3d.mediation.LevelPlayAdInfo adInfo)
    {
        _adLoaded = true;
        _isLoading = false;
        _retryCount = 0;
        UpdateButtonVisibility();
    }

    private void OnAdLoadFailed(com.unity3d.mediation.LevelPlayAdError error)
    {
        _isLoading = false;
        UpdateButtonVisibility();

        if (_retryCount < _maxRetries)
        {
            _retryCount++;
            StartCoroutine(RetryLoadAd(_retryDelay));
        }
        else
        {
            _retryCount = 0;
            StartCoroutine(RetryLoadAd(_retryDelay * 2));
        }
    }

    private IEnumerator RetryLoadAd(float delay)
    {
        // Realtime: menus run with Time.timeScale = 0.
        yield return new WaitForSecondsRealtime(delay);
        LoadAd();
    }

    public void ShowAd()
    {
        if (_rewardedAd == null)
        {
            return;
        }

        if (_isShowing || IsAnyAdShowing())
        {
            return;
        }

        if (!_adLoaded || !_rewardedAd.IsAdReady())
        {
            if (!_isLoading)
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

        _rewardedAd.ShowAd();
    }

    private void OnAdDisplayFailed(com.unity3d.mediation.LevelPlayAdDisplayInfoError error)
    {
        _isShowing = false;
        LoadAd();
    }

    private void OnAdRewarded(com.unity3d.mediation.LevelPlayAdInfo adInfo, com.unity3d.mediation.LevelPlayReward reward)
    {
        GrantReward(reward != null && reward.Amount > 0 ? reward.Amount : _fallbackRewardAmount);
    }

    private void OnAdClosed(com.unity3d.mediation.LevelPlayAdInfo adInfo)
    {
        _isShowing = false;
        _adLoaded = false;
        UpdateButtonVisibility();
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
        ApplyVisibilityToButton(_startScreenWatchButton);
        ApplyVisibilityToButton(_shopScreenWatchButton);
    }

    private void ApplyVisibilityToButton(Button button)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(_adLoaded);
        if (_adLoaded)
            button.interactable = true;
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

        if (_rewardedAd != null && !_adLoaded && !_isLoading && !IsAnyAdShowing())
        {
            LoadAd();
        }
    }

    private void GrantReward(int amount)
    {
        WalletManager.AddCoins(amount);
        AudioManager.Instance?.PlaySound(AudioManager.SoundType.Coin);
        NativeHaptics.TriggerSuccessNotification();
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        if (Instance != this)
            return;

        if (_rewardedAd != null)
        {
            _rewardedAd.DestroyAd();
            _rewardedAd = null;
        }
    }
}
