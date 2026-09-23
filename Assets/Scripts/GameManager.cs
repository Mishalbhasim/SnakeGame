using UnityEngine;

/// <summary>
/// Central game state controller: tracks score, listens for the snake's
/// eat/die events, and tells the UI when to update or show Game Over.
/// Attach to an empty GameObject called "GameManager" in the scene.
///
/// Uses an explicit state machine (GameState enum) for its own state, and
/// broadcasts events (OnScoreChanged, OnGameOver, etc.) rather than calling
/// UIManager directly - UIManager subscribes to these, the same way
/// GameManager itself subscribes to SnakeController/FoodSpawner/AdsManager
/// events. GameManager never needs to know UIManager exists.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Playing,
        ReviveOffer,
        GameOver
    }

    [Header("References")]
    [Tooltip("Drag the Snake GameObject here")]
    public SnakeController snakeController;

    [Tooltip("Drag the FoodSpawner GameObject here")]
    public FoodSpawner foodSpawner;

    [Header("Scoring")]
    [Tooltip("Points awarded per food eaten")]
    public int pointsPerFood = 10;

    public int CurrentScore { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public bool IsGameOver => CurrentState == GameState.GameOver;

    private bool hasUsedRevive;

    // Events UIManager (or anything else) can subscribe to, instead of
    // GameManager calling into UIManager directly.
    public delegate void ScoreChanged(int newScore);
    public static event ScoreChanged OnScoreChanged;

    public delegate void GameOverEvent(int finalScore, bool isWin);
    public static event GameOverEvent OnGameOver;

    public delegate void SimpleEvent();
    public static event SimpleEvent OnGameStarted;
    public static event SimpleEvent OnGameOverHidden;
    public static event SimpleEvent OnReviveOfferShown;
    public static event SimpleEvent OnReviveOfferHidden;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SnakeController.OnFoodEaten += HandleFoodEaten;
        SnakeController.OnSnakeDied += HandleSnakeDied;
        FoodSpawner.OnGridFull += HandleGridFull;
        AdsManager.OnRevivedGranted += HandleRevivedGranted;
    }

    private void OnDisable()
    {
        SnakeController.OnFoodEaten -= HandleFoodEaten;
        SnakeController.OnSnakeDied -= HandleSnakeDied;
        FoodSpawner.OnGridFull -= HandleGridFull;
        AdsManager.OnRevivedGranted -= HandleRevivedGranted;
    }

    private void Start()
    {
        CurrentState = GameState.MainMenu;
        CurrentScore = 0;

        if (snakeController != null)
        {
            snakeController.ResetSnake();
        }

        if (foodSpawner != null)
        {
            foodSpawner.ResetSpawner();
        }
    }

    /// <summary>
    /// Resets score/state and restarts the snake. Called at launch and on Restart button press.
    /// </summary>
    public void StartNewGame()
    {
        OnGameStarted?.Invoke();

        CurrentScore = 0;
        CurrentState = GameState.Playing;
        hasUsedRevive = false;

        OnScoreChanged?.Invoke(CurrentScore);
        OnGameOverHidden?.Invoke();

        if (snakeController != null)
        {
            snakeController.ResetSnake();
        }

        if (foodSpawner != null)
        {
            foodSpawner.ResetSpawner();
        }

        Time.timeScale = 1f;
    }

    private void HandleFoodEaten()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentScore += pointsPerFood;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    private void HandleSnakeDied()
    {
        if (CurrentState != GameState.Playing) return; // avoid double-trigger

        bool reviveAvailable = !hasUsedRevive
            && AdsManager.Instance != null
            && AdsManager.Instance.IsRewardedAdReady();

        if (reviveAvailable)
        {
            OfferRevive();
        }
        else
        {
            FinalizeGameOver(false);
        }
    }

    /// <summary>
    /// Called when the snake dies and a rewarded "revive" ad is ready to show.
    /// Enters the ReviveOffer state and broadcasts OnReviveOfferShown.
    /// </summary>
    private void OfferRevive()
    {
        CurrentState = GameState.ReviveOffer;
        OnReviveOfferShown?.Invoke();
    }

    /// <summary>
    /// Wire this to the "Watch Ad" button's OnClick().
    /// </summary>
    public void OnWatchAdButtonPressed()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedAd();
        }
    }

    /// <summary>
    /// Wire this to the "No Thanks" button's OnClick().
    /// </summary>
    public void OnNoThanksButtonPressed()
    {
        // Player already saw revive offer, declined. No interstitial here -
        // stacking interstitial right after a declined rewarded offer feels
        // punishing, defeats point of offering choice at all.
        FinalizeGameOver(false, showInterstitial: false);
    }

    /// <summary>
    /// Fires when AdsManager confirms the player actually earned the reward.
    /// Revives the snake, returns to Playing, and broadcasts OnReviveOfferHidden.
    /// </summary>
    private void HandleRevivedGranted()
    {
        hasUsedRevive = true;
        CurrentState = GameState.Playing;

        OnReviveOfferHidden?.Invoke();

        if (snakeController != null)
        {
            snakeController.Revive();
        }
    }

    /// <summary>
    /// Called when the snake fills every cell on the grid - the win condition.
    /// </summary>
    private void HandleGridFull()
    {
        if (CurrentState != GameState.Playing) return;

        FinalizeGameOver(true);
    }

    /// <summary>
    /// Single place where a game actually ends: enters GameOver state,
    /// optionally shows an interstitial ad (loss only, and only when player
    /// never got a revive offer - see showInterstitial param), broadcasts
    /// OnGameOver.
    /// </summary>
    private void FinalizeGameOver(bool isWin, bool showInterstitial = true)
    {
        CurrentState = GameState.GameOver;

        if (!isWin && showInterstitial && AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowInterstitialAd();
        }

        OnGameOver?.Invoke(CurrentScore, isWin);
    }

    /// <summary>
    /// Called by the Restart button via UIManager.
    /// </summary>
    public void RestartGame()
    {
        StartNewGame();
    }
}