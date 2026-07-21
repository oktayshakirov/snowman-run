using System;
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
    [Tooltip("Max seconds to wait for the consent form (it is modal, so this allows reading time).")]
    [SerializeField] private float _consentFormTimeout = 60f;
    [Tooltip("Max seconds to wait for the iOS ATT prompt response.")]
    [SerializeField] private float _attTimeout = 30f;

    [Header("Debug")]
    [Tooltip("Opens the LevelPlay mediation test suite after init. MUST be OFF for store builds.")]
    [SerializeField] private bool _launchTestSuiteOnInit = false;
    [Tooltip("Logs the consent/init sequence. Works in release builds - leave OFF for store builds.")]
    [SerializeField] private bool _verboseAdLogging = false;

    [Header("Secondary Splash Screen")]
    [SerializeField] private GameObject splashScreenCanvas;
    [SerializeField] private Animator splashScreenAnimator;

    public UnityEvent OnInitializationCompleteEvent;

    private string _appKey;
    private bool _isInitialized = false;
    private bool _initRequested = false;
    private bool _splashHidden = false;
    private int _retryCount = 0;

    // The splash animation ends with an AnimationEvent calling DisableCanvas(),
    // which deactivates this very GameObject and would kill any coroutine
    // running on it. The consent flow outlives the splash, so it runs on a
    // persistent host instead.
    private sealed class AdsCoroutineRunner : MonoBehaviour { }

    private static AdsCoroutineRunner _runner;

    private static AdsCoroutineRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                GameObject host = new GameObject("AdsCoroutineRunner");
                DontDestroyOnLoad(host);
                _runner = host.AddComponent<AdsCoroutineRunner>();
            }
            return _runner;
        }
    }

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
        Runner.StartCoroutine(ConsentFlowThenInit());
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
    //
    // Every wait below is bounded: a stalled consent step must never leave the
    // game without ads, so the flow always ends in InitializeLevelPlay().
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
            AdLog($"ATT already resolved: {ATTrackingStatusBinding.GetAuthorizationTrackingStatus()}");
            yield break;
        }

        AdLog("Requesting ATT authorization...");
        ATTrackingStatusBinding.RequestAuthorizationTracking();

        float attDeadline = Time.realtimeSinceStartup + Mathf.Max(1f, _attTimeout);
        while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
               ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED &&
               Time.realtimeSinceStartup < attDeadline)
        {
            yield return new WaitForSecondsRealtime(0.2f);
        }

        AdLog($"ATT status: {ATTrackingStatusBinding.GetAuthorizationTrackingStatus()}");
    }
#endif

    private IEnumerator GatherUmpConsent()
    {
        bool updateDone = false;
        FormError updateError = null;
        bool umpUnavailable = false;

        AdLog("Requesting UMP consent info update...");

        // The consent SDK is a hard dependency of neither the game nor the ad
        // SDK. If it is missing or stripped it throws here, and ads must still
        // initialize rather than the flow dying with the coroutine.
        try
        {
            ConsentInformation.Update(new ConsentRequestParameters(), error =>
            {
                updateError = error;
                updateDone = true;
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"UMP consent unavailable, continuing without it: {e.Message}");
            umpUnavailable = true;
        }

        if (umpUnavailable)
            yield break;

        // Ads must never be blocked by a hanging consent request (e.g. offline).
        float deadline = Time.realtimeSinceStartup + Mathf.Max(1f, _consentUpdateTimeout);
        while (!updateDone && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!updateDone)
        {
            AdLog("UMP update timed out; continuing to ads init.");
            yield break;
        }

        if (updateError != null)
        {
            AdLog($"UMP update error: {updateError.Message}; continuing to ads init.");
            yield break;
        }

        bool formDone = false;
        AdLog("Loading consent form if required...");
        try
        {
            ConsentForm.LoadAndShowConsentFormIfRequired(error =>
            {
                if (error != null)
                    AdLog($"Consent form error: {error.Message}");
                formDone = true;
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Consent form unavailable, continuing without it: {e.Message}");
            umpUnavailable = true;
        }

        if (umpUnavailable)
            yield break;

        // The callback normally fires at once when no form is required for this
        // region. The generous cap covers the modal case (user reading it) while
        // still guaranteeing ads initialize if the callback never arrives.
        float formDeadline = Time.realtimeSinceStartup + Mathf.Max(1f, _consentFormTimeout);
        while (!formDone && Time.realtimeSinceStartup < formDeadline)
        {
            yield return null;
        }

        AdLog(formDone
            ? $"Consent flow done. CanRequestAds={ConsentInformation.CanRequestAds()}"
            : "Consent form timed out; continuing to ads init.");
    }

    private void AdLog(string message)
    {
        if (_verboseAdLogging)
            Debug.Log($"[Ads] {message}");
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
        // Guarded so a retry or a late consent callback cannot double-init.
        if (_initRequested)
            return;
        _initRequested = true;

        if (_launchTestSuiteOnInit)
        {
            IronSource.Agent.setMetaData("is_test_suite", "enable");
        }

        AdLog($"Initializing LevelPlay (appKey {_appKey})...");
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
        AdLog("LevelPlay init complete.");
        _isInitialized = true;
        OnInitializationCompleteEvent?.Invoke();
        HideSplash(animated: true);

        if (_launchTestSuiteOnInit)
        {
            LevelPlay.LaunchTestSuite();
        }
    }

    private void OnInitializationFailed(com.unity3d.mediation.LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay init failed: {error.ErrorCode} - {error.ErrorMessage}");

        // Never keep the player stuck on the splash because of ads.
        HideSplash(animated: false);

        if (_retryCount < _maxRetries)
        {
            _retryCount++;
            Runner.StartCoroutine(RetryInitialize());
        }
    }

    private IEnumerator RetryInitialize()
    {
        // Realtime: menus run with Time.timeScale = 0.
        yield return new WaitForSecondsRealtime(_retryDelay);
        _initRequested = false;
        InitializeLevelPlay();
    }

    private void HideSplash(bool animated)
    {
        if (_splashHidden)
            return;
        _splashHidden = true;

        if (splashScreenCanvas == null)
            return;

        // The splash may already be gone: its animation ends with an
        // AnimationEvent calling DisableCanvas(). Triggering an animator on a
        // deactivated object does nothing, so fall through to deactivating it.
        if (animated && splashScreenAnimator != null && splashScreenCanvas.activeInHierarchy)
        {
            splashScreenAnimator.SetTrigger("Hide");
            return;
        }

        splashScreenCanvas.SetActive(false);
    }

    // Called from an AnimationEvent at the end of the splash animation.
    public void DisableCanvas()
    {
        _splashHidden = true;
        if (splashScreenCanvas != null)
        {
            splashScreenCanvas.SetActive(false);
        }
    }
}
