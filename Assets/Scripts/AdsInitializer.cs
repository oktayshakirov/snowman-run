using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    public static AdsInitializer Instance;

#if UNITY_ANDROID
    [SerializeField] private string _androidGameId = "5755677";
#elif UNITY_IOS
    [SerializeField] private string _iOSGameId = "5755676";
#endif
    [SerializeField] private bool _testMode = false;

    [Header("Secondary Splash Screen")]
    [SerializeField] private GameObject splashScreenCanvas;

    public UnityEvent OnInitializationCompleteEvent;

    private string _gameId;
    private bool _isInitialized = false;

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
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(true);
        }
        StartCoroutine(InitializeAdsCoroutine());
    }

    private IEnumerator InitializeAdsCoroutine()
    {
#if UNITY_IOS
        _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
        _gameId = "testGameId";
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        while (!Advertisement.isInitialized)
        {
            yield return null;
        }
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(false);
        }

        _isInitialized = true;
        OnInitializationComplete();
    }

    public bool IsInitialized()
    {
        return _isInitialized;
    }

    public void OnInitializationComplete()
    {
        _isInitialized = true;
        OnInitializationCompleteEvent?.Invoke();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(false);
        }
    }
}