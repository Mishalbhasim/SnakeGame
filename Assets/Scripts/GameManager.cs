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
    }

    private void OnDisable()
    {
        SnakeController.OnFoodEaten -= HandleFoodEaten;
        SnakeController.OnSnakeDied -= HandleSnakeDied;
    }

    private void Start()
    {
        StartNewGame();
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
            UIManager.Instance.ShowGameOver(CurrentScore);
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