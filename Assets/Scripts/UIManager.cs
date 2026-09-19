using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles all UI panels: Main Menu, live score, Game Over/Win, Revive Offer, and their buttons.
/// Attach to the Canvas GameObject (or a dedicated "UIManager" empty GameObject under it).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu")]
    [Tooltip("Panel shown before the game starts")]
    public GameObject mainMenuPanel;

    [Tooltip("Play button on the Main Menu panel")]
    public Button playButton;

    [Tooltip("Text showing best score ever, on the Main Menu panel")]
    public TextMeshProUGUI highScoreText;

    [Header("Score")]
    [Tooltip("TextMeshPro text showing the live score during play")]
    public TextMeshProUGUI scoreText;

    [Header("Game Over")]
    [Tooltip("Panel shown when the snake dies or wins")]
    public GameObject gameOverPanel;

    [Tooltip("TextMeshPro text showing 'Game Over' or 'You Win!' on the Game Over panel")]
    public TextMeshProUGUI gameOverHeadlineText;

    [Tooltip("TextMeshPro text showing the final score on the Game Over panel")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("TextMeshPro text showing the best-ever score on the Game Over panel")]
    public TextMeshProUGUI gameOverHighScoreText;

    [Header("Revive Offer")]
    [Tooltip("Panel shown right after death, offering a rewarded-ad revive, before Game Over is finalized")]
    public GameObject reviveOfferPanel;

    [Tooltip("Button on the Revive Offer panel - watches a rewarded ad to revive")]
    public Button watchAdButton;

    [Tooltip("Button on the Revive Offer panel - declines the offer and goes to normal Game Over")]
    public Button noThanksButton;

    [Header("Buttons")]
    [Tooltip("Restart button on the Game Over panel")]
    public Button restartButton;

    [Tooltip("Optional: button on the Game Over panel that returns to the Main Menu")]
    public Button mainMenuButton;

    [Tooltip("Quit button on the Main Menu - closes the app")]
    public Button quitButton;

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

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(HandlePlayClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(HandleQuitClicked);
        }

        if (watchAdButton != null)
        {
            watchAdButton.onClick.AddListener(HandleWatchAdClicked);
        }

        if (noThanksButton != null)
        {
            noThanksButton.onClick.AddListener(HandleNoThanksClicked);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (reviveOfferPanel != null)
        {
            reviveOfferPanel.SetActive(false);
        }

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (reviveOfferPanel != null)
        {
            reviveOfferPanel.SetActive(false);
        }

        if (highScoreText != null)
        {
            int best = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + best;
        }

        // Pause gameplay while on the menu
        Time.timeScale = 0f;
    }

    public void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void HandlePlayClicked()
    {
        HideMainMenu();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
    }

    private void HandleQuitClicked()
    {
        // On Android this properly closes the app. In the Editor it stops Play mode.
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HandleMainMenuClicked()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        ShowMainMenu();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    /// <summary>
    /// Shows the end-of-game panel. isWin=true means the snake filled the entire grid
    /// (the win condition); isWin=false means it died to a wall or itself.
    /// Reuses the same panel with a different headline for either case.
    /// Also updates the locally-tracked high score used on the Main Menu.
    /// </summary>
    public void ShowGameOver(int finalScore, bool isWin = false)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        int best = PlayerPrefs.GetInt("HighScore", 0);
        if (finalScore > best)
        {
            best = finalScore;
            PlayerPrefs.SetInt("HighScore", best);
            PlayerPrefs.Save();
        }

        if (gameOverHeadlineText != null)
        {
            gameOverHeadlineText.text = isWin ? "You Win! Grid Filled!" : "Game Over";
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = "High Score: " + best;
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the "Watch Ad to Revive?" offer panel. Called right after death,
    /// instead of Game Over, when a rewarded ad is ready.
    /// </summary>
    public void ShowReviveOffer()
    {
        if (reviveOfferPanel != null)
        {
            reviveOfferPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the Revive Offer panel - called once the player picks an option
    /// (or once a revive is actually granted).
    /// </summary>
    public void HideReviveOffer()
    {
        if (reviveOfferPanel != null)
        {
            reviveOfferPanel.SetActive(false);
        }
    }

    private void HandleRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void HandleWatchAdClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWatchAdButtonPressed();
        }
    }

    private void HandleNoThanksClicked()
    {
        HideReviveOffer();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNoThanksButtonPressed();
        }
    }
}