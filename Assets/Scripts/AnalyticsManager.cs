using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;


public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

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


    private int highScoreAtRunStart;
    private float runStartTime;


    private bool reviveOfferPending;

    private void OnEnable()
    {
        GameManager.OnGameStarted += HandleGameStarted;
        GameManager.OnGameOver += HandleGameOver;
        GameManager.OnReviveOfferShown += HandleReviveOfferShown;
        GameManager.OnReviveOfferHidden += HandleReviveAccepted;
        SnakeController.OnFoodEaten += HandleFoodEaten;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= HandleGameStarted;
        GameManager.OnGameOver -= HandleGameOver;
        GameManager.OnReviveOfferShown -= HandleReviveOfferShown;
        GameManager.OnReviveOfferHidden -= HandleReviveAccepted;
        SnakeController.OnFoodEaten -= HandleFoodEaten;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("AnalyticsManager: Unity Services initialized.");
    }

    private void HandleGameStarted()
    {
        highScoreAtRunStart = PlayerPrefs.GetInt("HighScore", 0);
        runStartTime = Time.time;
        reviveOfferPending = false;

        AnalyticsService.Instance.RecordEvent("game_start");
    }

    private void HandleFoodEaten()
    {
        AnalyticsService.Instance.RecordEvent("food_eaten");
    }

    private void HandleReviveOfferShown()
    {
        reviveOfferPending = true;
        AnalyticsService.Instance.RecordEvent("revive_offer_shown");
    }

    private void HandleReviveAccepted()
    {
        reviveOfferPending = false;
        AnalyticsService.Instance.RecordEvent("revive_ad_accepted");
    }

    private void HandleGameOver(int finalScore, bool isWin)
    {
        
        if (reviveOfferPending)
        {
            reviveOfferPending = false;
            AnalyticsService.Instance.RecordEvent("revive_ad_declined");
        }

        float durationSeconds = Time.time - runStartTime;

        CustomEvent gameOverEvent = new CustomEvent("game_over")
        {
            { "final_score", finalScore },
            { "is_win", isWin },
            { "game_duration_seconds", durationSeconds }
        };
        AnalyticsService.Instance.RecordEvent(gameOverEvent);

        if (finalScore > highScoreAtRunStart)
        {
            CustomEvent highScoreEvent = new CustomEvent("new_high_score")
            {
                { "final_score", finalScore }
            };
            AnalyticsService.Instance.RecordEvent(highScoreEvent);
        }
    }
}