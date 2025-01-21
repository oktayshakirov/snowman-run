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
    [SerializeField] private Animator splashScreenAnimator;

    public UnityEvent OnInitializationCompleteEvent;

    private string _gameId;
    private bool _isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        Debug.Log("Starting Ads initialization...");
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

        Debug.Log("Ads initialization complete!");
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

        if (splashScreenAnimator != null && splashScreenCanvas != null)
        {
            splashScreenAnimator.SetTrigger("Hide");
        }
        else if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(false);
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {message}");
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(false);
        }
    }

    public void DisableCanvas()
    {
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(false);
        }
    }

}
