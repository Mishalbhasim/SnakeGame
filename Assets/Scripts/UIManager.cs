using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Button playButton;
    public TextMeshProUGUI highScoreText;

    [Header("Score")]
    public TextMeshProUGUI scoreText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverHeadlineText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI gameOverHighScoreText;

    [Header("Revive Offer")]
    public GameObject reviveOfferPanel;
    public Button watchAdButton;
    public Button noThanksButton;

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
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

    private void OnEnable()
    {
        GameManager.OnScoreChanged += HandleScoreChanged;
        GameManager.OnGameOver += HandleGameOver;
        GameManager.OnGameOverHidden += HandleGameOverHidden;
        GameManager.OnReviveOfferShown += HandleReviveOfferShown;
        GameManager.OnReviveOfferHidden += HandleReviveOfferHidden;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= HandleScoreChanged;
        GameManager.OnGameOver -= HandleGameOver;
        GameManager.OnGameOverHidden -= HandleGameOverHidden;
        GameManager.OnReviveOfferShown -= HandleReviveOfferShown;
        GameManager.OnReviveOfferHidden -= HandleReviveOfferHidden;
    }

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(HandleRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        if (playButton != null) playButton.onClick.AddListener(HandlePlayClicked);
        if (quitButton != null) quitButton.onClick.AddListener(HandleQuitClicked);
        if (watchAdButton != null) watchAdButton.onClick.AddListener(HandleWatchAdClicked);
        if (noThanksButton != null) noThanksButton.onClick.AddListener(HandleNoThanksClicked);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (reviveOfferPanel != null) reviveOfferPanel.SetActive(false);

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (reviveOfferPanel != null) reviveOfferPanel.SetActive(false);

        if (highScoreText != null)
        {
            int best = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + best;
        }

        Time.timeScale = 0f;
    }

    public void HideMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void HandlePlayClicked()
    {
        HideMainMenu();
        if (GameManager.Instance != null) GameManager.Instance.StartNewGame();
    }

    private void HandleQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HandleMainMenuClicked()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        ShowMainMenu();
    }



    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }

    private void HandleGameOver(int finalScore, bool isWin)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

   
        if (reviveOfferPanel != null) reviveOfferPanel.SetActive(false);

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

    private void HandleGameOverHidden()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void HandleReviveOfferShown()
    {
        if (reviveOfferPanel != null) reviveOfferPanel.SetActive(true);
    }

    private void HandleReviveOfferHidden()
    {
        if (reviveOfferPanel != null) reviveOfferPanel.SetActive(false);
    }



    private void HandleRestartClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    private void HandleWatchAdClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnWatchAdButtonPressed();
    }

    private void HandleNoThanksClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnNoThanksButtonPressed();
    }
}