using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.LevelPlay;
using GoogleMobileAds.Ump.Api;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

// LevelPlay 8.x still types its public events with the deprecated
// com.unity3d.mediation aliases, so referencing them is unavoidable here.
#pragma warning disable 0618

public class AdsInitializer : MonoBehaviour
{
    public static AdsInitializer Instance;

    [Header("LevelPlay App Keys")]
    [SerializeField] private string _androidAppKey = "273ef13bd";
    [SerializeField] private string _iOSAppKey = "273eeda35";

    [Header("Init Retry Settings")]
    [SerializeField] private float _retryDelay = 5f;
    [SerializeField] private int _maxRetries = 3;

    [Header("Consent")]
    [Tooltip("Max seconds to wait for the UMP consent info update before initializing ads anyway.")]
    [SerializeField] private float _consentUpdateTimeout = 15f;

    [Header("Debug")]
    [Tooltip("Opens the LevelPlay mediation test suite after init (development builds only). Has no effect in release builds.")]
    [SerializeField] private bool _launchTestSuiteOnInit = false;

    [Header("Secondary Splash Screen")]
    [SerializeField] private GameObject splashScreenCanvas;
    [SerializeField] private Animator splashScreenAnimator;

    public UnityEvent OnInitializationCompleteEvent;

    private string _appKey;
    private bool _isInitialized = false;
    private bool _splashHidden = false;
    private int _retryCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_IOS
        _appKey = _iOSAppKey;
#else
        _appKey = _androidAppKey;
#endif
    }

    private void Start()
    {
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(true);
        }

        LevelPlay.OnInitSuccess += OnInitializationComplete;
        LevelPlay.OnInitFailed += OnInitializationFailed;
        StartCoroutine(ConsentFlowThenInit());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        LevelPlay.OnInitSuccess -= OnInitializationComplete;
        LevelPlay.OnInitFailed -= OnInitializationFailed;
    }

    // App start order: ATT prompt (iOS) -> UMP consent form (GDPR regions) ->
    // LevelPlay init. The networks read the resulting TCF consent string and
    // ATT status themselves, so no popup of their own is shown afterwards.
    private IEnumerator ConsentFlowThenInit()
    {
#if !UNITY_EDITOR
#if UNITY_IOS
        yield return RequestTrackingAuthorization();
#endif
        yield return GatherUmpConsent();
#endif
        InitializeLevelPlay();
        yield break;
    }

#if UNITY_IOS && !UNITY_EDITOR
    private IEnumerator RequestTrackingAuthorization()
    {
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() !=
            ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            yield break;
        }

        ATTrackingStatusBinding.RequestAuthorizationTracking();

        while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
               ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
#endif

    private IEnumerator GatherUmpConsent()
    {
        bool updateDone = false;
        FormError updateError = null;

        ConsentInformation.Update(new ConsentRequestParameters(), error =>
        {
            updateError = error;
            updateDone = true;
        });

        // Ads must never be blocked by a hanging consent request (e.g. offline).
        float deadline = Time.realtimeSinceStartup + _consentUpdateTimeout;
        while (!updateDone && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!updateDone || updateError != null)
        {
            yield break;
        }

        bool formDone = false;
        ConsentForm.LoadAndShowConsentFormIfRequired(error => { formDone = true; });

        // The form is modal; the callback also fires immediately when no form
        // is required for this region or the form fails to load.
        while (!formDone)
        {
            yield return null;
        }
    }

    // Wire this to a settings button so EEA users can change their consent
    // choice later, as required by GDPR. Only show the button when
    // IsPrivacyOptionsRequired is true.
    public static bool IsPrivacyOptionsRequired =>
        ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

    public void ShowPrivacyOptionsForm()
    {
        ConsentForm.ShowPrivacyOptionsForm(error => { });
    }

    private void InitializeLevelPlay()
    {
        if (_launchTestSuiteOnInit && Debug.isDebugBuild)
        {
            IronSource.Agent.setMetaData("is_test_suite", "enable");
        }

        LevelPlay.Init(_appKey, null, new[]
        {
            com.unity3d.mediation.LevelPlayAdFormat.REWARDED,
            com.unity3d.mediation.LevelPlayAdFormat.INTERSTITIAL
        });
    }

    public bool IsInitialized()
    {
        return _isInitialized;
    }

    private void OnInitializationComplete(com.unity3d.mediation.LevelPlayConfiguration configuration)
    {
        _isInitialized = true;
        OnInitializationCompleteEvent?.Invoke();
        HideSplash(animated: true);

        if (_launchTestSuiteOnInit && Debug.isDebugBuild)
        {
            LevelPlay.LaunchTestSuite();
        }
    }

    private void OnInitializationFailed(com.unity3d.mediation.LevelPlayInitError error)
    {
        // Never keep the player stuck on the splash because of ads.
        HideSplash(animated: false);

        if (_retryCount < _maxRetries)
        {
            _retryCount++;
            StartCoroutine(RetryInitialize());
        }
    }

    private IEnumerator RetryInitialize()
    {
        // Realtime: menus run with Time.timeScale = 0.
        yield return new WaitForSecondsRealtime(_retryDelay);
        InitializeLevelPlay();
    }

    private void HideSplash(bool animated)
    {
        if (_splashHidden)
            return;
        _splashHidden = true;

        if (animated && splashScreenAnimator != null && splashScreenCanvas != null)
        {
            splashScreenAnimator.SetTrigger("Hide");
        }
        else if (splashScreenCanvas != null)
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
