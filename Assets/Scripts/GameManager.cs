using UnityEngine;

/// <summary>
/// Central game state controller: tracks score, listens for the snake's
/// eat/die events, and tells the UI when to update or show Game Over.
/// Attach to an empty GameObject called "GameManager" in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Drag the Snake GameObject here")]
    public SnakeController snakeController;

    [Tooltip("Drag the FoodSpawner GameObject here")]
    public FoodSpawner foodSpawner;

    [Header("Scoring")]
    [Tooltip("Points awarded per food eaten")]
    public int pointsPerFood = 10;

    public int CurrentScore { get; private set; }
    public bool IsGameOver { get; private set; }

    // Whether the player has already used their one revive for this game.
    private bool hasUsedRevive;

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
        // Don't auto-start the game anymore - the Main Menu is shown first
        // (handled by UIManager.Start -> ShowMainMenu). The snake/food are
        // still reset once so they're in a valid state sitting behind the menu.
        IsGameOver = true;
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
        CurrentScore = 0;
        IsGameOver = false;
        hasUsedRevive = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(CurrentScore);
            UIManager.Instance.HideGameOver();
        }

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
        if (IsGameOver) return;

        CurrentScore += pointsPerFood;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(CurrentScore);
        }
    }

    private void HandleSnakeDied()
    {
        if (IsGameOver) return; // avoid double-trigger

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
    /// Game stays paused-but-not-over here: the player is asked whether they
    /// want to watch an ad to continue.
    ///
    /// TODO (next step): replace the Debug.Log below with
    /// UIManager.Instance.ShowReviveOffer() once that panel exists. Nothing
    /// else in this file needs to change when that happens.
    /// </summary>
    private void OfferRevive()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowReviveOffer();
        }
    }

    /// <summary>
    /// Wire this to the future "Watch Ad" button's OnClick().
    /// </summary>
    public void OnWatchAdButtonPressed()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedAd();
        }
    }

    /// <summary>
    /// Wire this to the future "No Thanks" button's OnClick().
    /// </summary>
    public void OnNoThanksButtonPressed()
    {
        FinalizeGameOver(false);
    }

    /// <summary>
    /// Fires when AdsManager confirms the player actually earned the reward
    /// (i.e. watched the rewarded ad to completion). Revives the snake and
    /// lets the game continue - does NOT reset score.
    ///
    /// TODO (next step): also call UIManager.Instance.HideReviveOffer() here
    /// once that panel exists.
    /// </summary>
    private void HandleRevivedGranted()
    {
        hasUsedRevive = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideReviveOffer();
        }

        if (snakeController != null)
        {
            snakeController.Revive();
        }
    }

    /// <summary>
    /// Called when the snake fills every cell on the grid - the win condition.
    /// Still ends the game and still shows/submits the final score, just with a
    /// different message than a death.
    /// </summary>
    private void HandleGridFull()
    {
        if (IsGameOver) return;

        FinalizeGameOver(true);
    }

    private void FinalizeGameOver(bool isWin)
    {
        IsGameOver = true;

        if (!isWin && AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowInterstitialAd();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(CurrentScore, isWin);
        }
    }

    /// <summary>
    /// Called by the Restart button via UIManager.
    /// </summary>
    public void RestartGame()
    {
        StartNewGame();
    }
}