using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance;

    private string _gameId;
    private string _adUnitId;
    private bool _isAdReady = false;

    [SerializeField] private bool _testMode = true;

    private void Awake()
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
    }

    private void Start()
    {
        InitializeAds();
    }

    private void InitializeAds()
    {
        bool isPlatformSupported = false;

#if UNITY_IOS
        _gameId = _iOSGameId;
        _adUnitId = _iOSAdUnitId;
        isPlatformSupported = true;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
        _adUnitId = _androidAdUnitId;
        isPlatformSupported = true;
#endif

        if (isPlatformSupported && !string.IsNullOrEmpty(_gameId))
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public bool IsAdReady()
    {
        return _isAdReady;
    }

    public void ShowAd(System.Action onAdComplete)
    {
        if (_isAdReady)
        {
            Advertisement.Show(_adUnitId, new UnityAdsShowListener(onAdComplete));
        }
        else
        {
            onAdComplete?.Invoke();
        }
    }

    public void LoadAd()
    {
        if (!string.IsNullOrEmpty(_adUnitId))
        {
            Advertisement.Load(_adUnitId, this);
        }
    }

    public void OnInitializationComplete()
    {
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message) { }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == _adUnitId)
        {
            _isAdReady = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        _isAdReady = false;
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == _adUnitId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            LoadAd();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) { }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }

    private class UnityAdsShowListener : IUnityAdsShowListener
    {
        private readonly System.Action _onAdComplete;

        public UnityAdsShowListener(System.Action onAdComplete)
        {
            _onAdComplete = onAdComplete;
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                _onAdComplete?.Invoke();
            }
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            _onAdComplete?.Invoke();
        }

        public void OnUnityAdsShowStart(string placementId) { }

        public void OnUnityAdsShowClick(string placementId) { }
    }
}
