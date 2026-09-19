using UnityEngine;
using Unity.Services.LevelPlay;


public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("LevelPlay IDs")]
    [Tooltip("From the Unity Dashboard > Monetization > Apps page.")]
    public string appKey = "800373542";

    [Tooltip("Placement ID for the rewarded 'revive' ad.")]
    public string rewardedPlacementId = "BP_Rewarded_Android";

    [Tooltip("Placement ID for the interstitial ad shown on game over.")]
    public string interstitialPlacementId = "gameover_interstitial";

    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;

    private bool isSdkInitialized = false;

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
        InitializeAds();
    }

    private void InitializeAds()
    {
        Debug.Log("AdsManager: about to call LevelPlay.Init()");

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        LevelPlay.Init(appKey);

        Debug.Log("AdsManager: LevelPlay.Init() call completed (this doesn't mean init succeeded, just that the call was made)");
    }

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay SDK initialized successfully.");
        isSdkInitialized = true;

        SetupRewardedAd();
        SetupInterstitialAd();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogWarning("LevelPlay SDK failed to initialize: " + error);
        isSdkInitialized = false;
    }

    //Rewarded Ad

    private void SetupRewardedAd()
    {
        rewardedAd = new LevelPlayRewardedAd(rewardedPlacementId);

        rewardedAd.OnAdLoaded += (LevelPlayAdInfo info) => Debug.Log("Rewarded ad loaded.");
        rewardedAd.OnAdLoadFailed += (LevelPlayAdError error) => Debug.LogWarning("Rewarded ad failed to load: " + error);
        rewardedAd.OnAdRewarded += (LevelPlayAdInfo info, LevelPlayReward reward) => OnRevivedGranted?.Invoke();
        rewardedAd.OnAdDisplayFailed += (LevelPlayAdInfo info, LevelPlayAdError error) => Debug.LogWarning("Rewarded ad failed to show: " + error);

        rewardedAd.LoadAd();
    }

    
    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.IsAdReady())
        {
            rewardedAd.ShowAd();
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready yet.");
        }
    }

    public bool IsRewardedAdReady()
    {
        return rewardedAd != null && rewardedAd.IsAdReady();
    }

    //Interstitial Ad

    private void SetupInterstitialAd()
    {
        interstitialAd = new LevelPlayInterstitialAd(interstitialPlacementId);

        interstitialAd.OnAdLoaded += (LevelPlayAdInfo info) => Debug.Log("Interstitial ad loaded.");
        interstitialAd.OnAdLoadFailed += (LevelPlayAdError error) => Debug.LogWarning("Interstitial ad failed to load: " + error);
        interstitialAd.OnAdClosed += (LevelPlayAdInfo info) => interstitialAd.LoadAd(); // preload the next one

        interstitialAd.LoadAd();
    }


    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.IsAdReady())
        {
            interstitialAd.ShowAd();
        }
    }
}