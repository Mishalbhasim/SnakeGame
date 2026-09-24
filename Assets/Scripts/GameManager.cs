using UnityEngine;


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
        if (CurrentState != GameState.Playing) return; 

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


    private void OfferRevive()
    {
        CurrentState = GameState.ReviveOffer;
        OnReviveOfferShown?.Invoke();
    }


    public void OnWatchAdButtonPressed()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedAd();
        }
    }

 
    public void OnNoThanksButtonPressed()
    {
       
        FinalizeGameOver(false, showInterstitial: false);
    }


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

    
    private void HandleGridFull()
    {
        if (CurrentState != GameState.Playing) return;

        FinalizeGameOver(true);
    }


    private void FinalizeGameOver(bool isWin, bool showInterstitial = true)
    {
        CurrentState = GameState.GameOver;

        if (!isWin && showInterstitial && AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowInterstitialAd();
        }

        OnGameOver?.Invoke(CurrentScore, isWin);
    }


    public void RestartGame()
    {
        StartNewGame();
    }
}