using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;

/// <summary>
/// Tracks and unlocks achievements, persisting them via Unity Cloud Save
/// (tied to an anonymous authenticated player, so progress survives a
/// reinstall or switching devices - unlike PlayerPrefs).
///
/// Fully decoupled from GameManager/SnakeController: listens to their
/// existing events (SnakeController.OnFoodEaten, GameManager.OnScoreChanged,
/// GameManager.OnGameOver) rather than being called into directly.
///
/// Sign-in is handled by AuthManager (shared with LeaderboardManager) to
/// avoid a race condition where two scripts try to sign in at once.
///
/// Attach to an empty GameObject called "AchievementManager" in the scene.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private const string CloudSaveKey = "unlocked_achievements";

    // The 5 achievement IDs used throughout.
    public const string FirstBite = "first_bite";
    public const string Score50 = "score_50";
    public const string Score100 = "score_100";
    public const string Length20 = "length_20";
    public const string GridFilled = "grid_filled";


    [System.Serializable]
    public struct AchievementDefinition
    {
        public string Id;
        public string Title;
        public string Description;
    }

    public static readonly AchievementDefinition[] AllAchievements = new AchievementDefinition[]
    {
        new AchievementDefinition { Id = FirstBite, Title = "First Bite", Description = "Eat your first food" },
        new AchievementDefinition { Id = Score50, Title = "Getting Started", Description = "Reach a score of 50" },
        new AchievementDefinition { Id = Score100, Title = "Century", Description = "Reach a score of 100" },
        new AchievementDefinition { Id = Length20, Title = "Growing Strong", Description = "Reach a snake length of 20" },
        new AchievementDefinition { Id = GridFilled, Title = "Perfectionist", Description = "Fill the entire grid" },
    };

    private HashSet<string> unlockedAchievements = new HashSet<string>();
    private bool foodEatenThisGame = false;

    // Fired whenever a NEW achievement is unlocked - a future UI popup
    // script can subscribe to this to show a toast/popup.
    public delegate void AchievementUnlocked(string achievementId);
    public static event AchievementUnlocked OnAchievementUnlocked;

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

    private void OnEnable()
    {
        SnakeController.OnFoodEaten += HandleFoodEaten;
        GameManager.OnScoreChanged += HandleScoreChanged;
        GameManager.OnGameOver += HandleGameOver;
        GameManager.OnGameStarted += HandleGameStarted;
    }

    private void OnDisable()
    {
        SnakeController.OnFoodEaten -= HandleFoodEaten;
        GameManager.OnScoreChanged -= HandleScoreChanged;
        GameManager.OnGameOver -= HandleGameOver;
        GameManager.OnGameStarted -= HandleGameStarted;
    }

    private async void Start()
    {
        await AuthManager.Instance.EnsureSignedIn();
        await LoadUnlockedAchievements();
    }

    private async System.Threading.Tasks.Task LoadUnlockedAchievements()
    {
        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { CloudSaveKey });

        if (result.TryGetValue(CloudSaveKey, out var item))
        {
            List<string> saved = item.Value.GetAs<List<string>>();
            unlockedAchievements = new HashSet<string>(saved);
            Debug.Log("AchievementManager: loaded " + unlockedAchievements.Count + " unlocked achievement(s).");
        }
        else
        {
            Debug.Log("AchievementManager: no saved achievements found yet (new player).");
        }
    }

    private async void SaveUnlockedAchievements()
    {
        var data = new Dictionary<string, object>
        {
            { CloudSaveKey, new List<string>(unlockedAchievements) }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    private void Unlock(string achievementId)
    {
        if (unlockedAchievements.Contains(achievementId)) return; // already unlocked

        unlockedAchievements.Add(achievementId);
        SaveUnlockedAchievements();

        Debug.Log("Achievement unlocked: " + achievementId);
        OnAchievementUnlocked?.Invoke(achievementId);
    }

    public bool IsUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    // Event handlers

    private void HandleGameStarted()
    {
        foodEatenThisGame = false;
    }

    private void HandleFoodEaten()
    {
        if (!foodEatenThisGame)
        {
            foodEatenThisGame = true;
            Unlock(FirstBite);
        }

        if (SnakeController.Instance != null && SnakeController.Instance.CurrentLength >= 20)
        {
            Unlock(Length20);
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (newScore >= 50) Unlock(Score50);
        if (newScore >= 100) Unlock(Score100);
    }

    private void HandleGameOver(int finalScore, bool isWin)
    {
        if (isWin) Unlock(GridFilled);
    }
}