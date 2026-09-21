using UnityEngine;
using GoogleMobileAds.Api;

/// <summary>
/// Handles AdMob rewarded (revive) and interstitial (game over) ads.
/// Currently using Google's official TEST ad unit IDs - these always return
/// real test ads regardless of AdMob account review status. Swap in the real
/// ad unit IDs (see comments below) once this is confirmed working end-to-end.
/// </summary>
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Ad Unit IDs (currently TEST IDs)")]
    [Tooltip("Official Google test ID - always serves a test rewarded ad")]
    public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    [Tooltip("Official Google test ID - always serves a test interstitial ad")]
    public string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";

    // Real IDs, for later once test ads are confirmed working:
    // rewardedAdUnitId     = "ca-app-pub-5547868366005717/7708097998"
    // interstitialAdUnitId = "ca-app-pub-5547868366005717/6823672999"

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    // GameManager listens for this to know when a revive was actually earned
    public delegate void RevivedGranted();
    public static event RevivedGranted OnRevivedGranted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("AdsManager: about to call MobileAds.Initialize()");

        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdsManager: MobileAds SDK initialized.");
            LoadRewardedAd();
            LoadInterstitialAd();
        });
    }

    // Rewarded Ad

    private void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning("Rewarded ad failed to load: " + error);
                return;
            }

            Debug.Log("Rewarded ad loaded.");
            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                // Preload the next one regardless of outcome.
                LoadRewardedAd();
            };

            rewardedAd.OnAdFullScreenContentFailed += (AdError err) =>
            {
                Debug.LogWarning("Rewarded ad failed to show: " + err);
                LoadRewardedAd();
            };
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                // Player watched the ad to completion - grant the revive.
                OnRevivedGranted?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready yet.");
        }
    }

    public bool IsRewardedAdReady()
    {
        return rewardedAd != null && rewardedAd.CanShowAd();
    }

    // Interstitial Ad

    private void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning("Interstitial ad failed to load: " + error);
                return;
            }

            Debug.Log("Interstitial ad loaded.");
            interstitialAd = ad;

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                LoadInterstitialAd(); // preload the next one
            };

            interstitialAd.OnAdFullScreenContentFailed += (AdError err) =>
            {
                Debug.LogWarning("Interstitial ad failed to show: " + err);
                LoadInterstitialAd();
            };
        });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("Interstitial ad not ready yet.");
        }
    }
}