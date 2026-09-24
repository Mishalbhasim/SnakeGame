using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;


public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private const string CloudSaveKey = "unlocked_achievements";

    public const string FirstBite = "first_bite";
    public const string GridFilled = "grid_filled";

    private const int ScoreTierStep = 100;
    private const int ScoreFirstMilestone = 50; 
    private const int LengthTierStep = 10;

    private HashSet<string> unlockedAchievements = new HashSet<string>();
    private bool foodEatenThisGame = false;

    
    public delegate void AchievementUnlocked(string achievementId, string title, string description);
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
        try
        {
            await AuthManager.Instance.EnsureSignedIn();
            await LoadUnlockedAchievements();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("AchievementManager: could not load achievements (offline?) - " + e.Message);
        }
    }

    private async System.Threading.Tasks.Task LoadUnlockedAchievements()
    {
        var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { CloudSaveKey });

        if (result.TryGetValue(CloudSaveKey, out var item))
        {
            List<string> saved = item.Value.GetAs<List<string>>();

            
            unlockedAchievements.UnionWith(saved);
            Debug.Log("AchievementManager: loaded " + unlockedAchievements.Count + " unlocked achievement(s).");
        }
        else
        {
            Debug.Log("AchievementManager: no saved achievements found yet (new player).");
        }
    }

    private async void SaveUnlockedAchievements()
    {
        try
        {
            await AuthManager.Instance.EnsureSignedIn();

            var data = new Dictionary<string, object>
            {
                { CloudSaveKey, new List<string>(unlockedAchievements) }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("AchievementManager: could not save achievements (offline?) - " + e.Message);
        }
    }

    private void Unlock(string achievementId)
    {
        if (unlockedAchievements.Contains(achievementId)) return; // already unlocked

        unlockedAchievements.Add(achievementId);
        SaveUnlockedAchievements();

        (string title, string description) = GetDisplayInfo(achievementId);

        Debug.Log("Achievement unlocked: " + achievementId + " (" + title + ")");
        OnAchievementUnlocked?.Invoke(achievementId, title, description);
    }

    public bool IsUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    public static readonly string[] CoreAchievementIds =
    {
        FirstBite,
        "score_50", "score_100", "score_200", "score_300",
        "length_10", "length_20", "length_30",
        GridFilled
    };


    public IEnumerable<string> GetUnlockedIds()
    {
        return unlockedAchievements;
    }

    public static (string title, string description) GetDisplayInfo(string achievementId)
    {
        switch (achievementId)
        {
            case FirstBite:
                return ("First Bite", "Eat your first food");
            case GridFilled:
                return ("Perfectionist", "Fill the entire grid");
        }

        if (achievementId.StartsWith("score_"))
        {
            string tier = achievementId.Substring("score_".Length);
            return ("Score " + tier, "Reach a score of " + tier);
        }

        if (achievementId.StartsWith("length_"))
        {
            string tier = achievementId.Substring("length_".Length);
            return ("Length " + tier, "Reach a snake length of " + tier);
        }

        return (achievementId, ""); 
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

        if (SnakeController.Instance != null)
        {
            int length = SnakeController.Instance.CurrentLength;

                for (int tier = LengthTierStep; tier <= length; tier += LengthTierStep)
            {
                Unlock("length_" + tier);
            }
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (newScore >= ScoreFirstMilestone)
        {
            Unlock("score_" + ScoreFirstMilestone);
        }

        for (int tier = ScoreTierStep; tier <= newScore; tier += ScoreTierStep)
        {
            Unlock("score_" + tier);
        }
    }

    private void HandleGameOver(int finalScore, bool isWin)
    {
        if (isWin) Unlock(GridFilled);
    }

#if UNITY_EDITOR
    [ContextMenu("DEV: Reset All Achievements (Cloud + Local)")]
    private async void ResetAllAchievements()
    {
        await AuthManager.Instance.EnsureSignedIn();

        unlockedAchievements.Clear();

        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(CloudSaveKey);
            Debug.Log("AchievementManager: all achievements reset (cloud + local).");
        }
        catch (Unity.Services.CloudSave.CloudSaveException e)
        {
            
            Debug.Log("AchievementManager: nothing to delete (already empty). " + e.Message);
        }
    }
#endif
}