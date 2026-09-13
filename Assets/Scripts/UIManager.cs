using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles all UI updates: live score display, Game Over panel with final score,
/// and the Restart button.
/// Attach to the Canvas GameObject (or a dedicated "UIManager" empty GameObject under it).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score")]
    [Tooltip("TextMeshPro text showing the live score during play")]
    public TextMeshProUGUI scoreText;

    [Header("Game Over")]
    [Tooltip("Panel shown when the snake dies")]
    public GameObject gameOverPanel;

    [Tooltip("TextMeshPro text showing the final score on the Game Over panel")]
    public TextMeshProUGUI finalScoreText;

    [Header("Buttons")]
    [Tooltip("Restart button on the Game Over panel")]
    public Button restartButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(HandleRestartClicked);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void HandleRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}