using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using RevenueCatUI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(Purchases))]
[RequireComponent(typeof(SnowmanPurchasesListener))]
public class RevenueCatManager : MonoBehaviour
{
    public static RevenueCatManager Instance { get; private set; }

    [Tooltip("Optional RevenueCat offering id. Empty = dashboard \"current\" offering.")]
    [SerializeField] private string offeringIdentifier;

    [Tooltip("Wait after opening the shop before showing the paywall (realtime seconds; works when Time.timeScale is 0).")]
    [SerializeField] private float shopPaywallDelaySeconds = 1f;

    private Purchases _purchases;
    private bool _configured;
    private Coroutine _shopPaywallDelayRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _purchases = GetComponent<Purchases>();
        SnowmanPurchasesListener listener = GetComponent<SnowmanPurchasesListener>();
        _purchases.useRuntimeSetup = true;
        _purchases.listener = listener;
        _purchases.productIdentifiers = CoinIapCatalog.ProductIds;
    }

    private IEnumerator Start()
    {
        yield return null;

        RevenueCatKeysData keys;
#if UNITY_ANDROID && !UNITY_EDITOR
        RevenueCatKeysData loaded = null;
        yield return RevenueCatKeysLoader.LoadFromStreamingAssets(k => loaded = k);
        keys = loaded;
#else
        keys = RevenueCatKeysLoader.LoadFromStreamingAssetsSync();
#endif

        string apiKey = keys != null ? keys.ApiKeyForCurrentPlatform() : null;
        if (string.IsNullOrEmpty(apiKey))
        {
            string jsonPath = Path.Combine(Application.streamingAssetsPath, "revenuecat_keys.json");
            bool fileOk = File.Exists(jsonPath);
            string hint = fileOk
                ? "File exists but no key matched this context. In the Editor, set File → Build Settings → iOS or Android (or ensure at least one key is set in .env and sync again)."
                : "Missing Assets/StreamingAssets/revenuecat_keys.json — run Snowman Run → Revenue Cat → Sync keys from .env.";
#if UNITY_EDITOR
            hint += $" Active build target: {EditorUserBuildSettings.activeBuildTarget}.";
#endif
            if (keys != null)
                hint += $" (ios empty={string.IsNullOrEmpty(keys.ios)}, android empty={string.IsNullOrEmpty(keys.android)})";
            Debug.LogError($"[RevenueCat] Missing API key for this platform. {hint}");
            yield break;
        }

        try
        {
            Purchases.PurchasesConfiguration config = Purchases.PurchasesConfiguration.Builder.Init(apiKey).Build();
            _purchases.Configure(config);
            _configured = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            yield break;
        }

        _purchases.GetProducts(CoinIapCatalog.ProductIds, (_, __) => { }, "inapp");
        _purchases.GetCustomerInfo((info, error) =>
        {
            if (error != null)
                Debug.LogWarning($"[RevenueCat] GetCustomerInfo: {error.Message}");
            else
                CoinPurchaseGrant.ProcessCustomerInfo(info);
        });
    }

    public bool IsConfigured => _configured;

    /// <summary>Called when the shop opens; shows the paywall after <see cref="shopPaywallDelaySeconds"/>.</summary>
    public void PresentCoinPaywallAfterShopOpened()
    {
        if (_shopPaywallDelayRoutine != null)
            StopCoroutine(_shopPaywallDelayRoutine);
        _shopPaywallDelayRoutine = StartCoroutine(ShopPaywallDelayRoutine());
    }

    private IEnumerator ShopPaywallDelayRoutine()
    {
        float delay = Mathf.Max(0f, shopPaywallDelaySeconds);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);
        _shopPaywallDelayRoutine = null;
        PresentCoinPaywall();
    }

    /// <summary>Wire this to a UI Button for the coin store paywall (device builds only).</summary>
    public async void PresentCoinPaywall()
    {
#if UNITY_EDITOR
        Debug.LogWarning("[RevenueCat] Paywall UI is not supported in the Unity Editor. Build to iOS or Android.");
        return;
#endif
        if (!_configured)
        {
            Debug.LogWarning("[RevenueCat] SDK not configured yet.");
            return;
        }

        try
        {
            Purchases.Offering offering = await ResolveOfferingAsync();
            PaywallOptions options = offering != null
                ? new PaywallOptions(offering)
                : new PaywallOptions();

            PaywallResult result = await PaywallsPresenter.Present(options);
            if (result.Result == PaywallResultType.Purchased)
            {
                _purchases.GetCustomerInfo((info, error) =>
                {
                    if (error != null)
                        Debug.LogWarning($"[RevenueCat] GetCustomerInfo after purchase: {error.Message}");
                    else
                        CoinPurchaseGrant.ProcessCustomerInfo(info);
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private Task<Purchases.Offering> ResolveOfferingAsync()
    {
        var tcs = new TaskCompletionSource<Purchases.Offering>();
        _purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                Debug.LogWarning($"[RevenueCat] GetOfferings failed: {error.Message}");
                tcs.SetResult(null);
                return;
            }

            if (offerings == null)
            {
                tcs.SetResult(null);
                return;
            }

            if (!string.IsNullOrEmpty(offeringIdentifier))
            {
                if (offerings.All != null && offerings.All.TryGetValue(offeringIdentifier, out Purchases.Offering o))
                {
                    tcs.SetResult(o);
                    return;
                }

                Debug.LogWarning($"[RevenueCat] Offering '{offeringIdentifier}' not found. Using current offering.");
            }

            tcs.SetResult(offerings.Current);
        });
        return tcs.Task;
    }
}
