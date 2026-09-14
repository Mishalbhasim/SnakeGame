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
    }

    private void OnDisable()
    {
        SnakeController.OnFoodEaten -= HandleFoodEaten;
        SnakeController.OnSnakeDied -= HandleSnakeDied;
        FoodSpawner.OnGridFull -= HandleGridFull;
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

        IsGameOver = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(CurrentScore, false);
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

        IsGameOver = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(CurrentScore, true);
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