using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;

/// <summary>
/// Tracks and unlocks achievements, persisting them via Unity Cloud Save
/// (tied to an anonymous authenticated player, so progress survives a
/// reinstall or switching devices - unlike PlayerPrefs).
///
/// Score and Length achievements are open-ended tiers (score_100, score_200,
/// ... and length_10, length_20, ...), generated and unlocked dynamically
/// rather than a fixed hardcoded list - so higher tiers unlock naturally as
/// the player improves, with no upper limit.
///
/// Fully decoupled from GameManager/SnakeController: listens to their
/// existing events rather than being called into directly.
///
/// Attach to an empty GameObject called "AchievementManager" in the scene.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private const string CloudSaveKey = "unlocked_achievements";

    // Fixed, one-off achievement IDs (not tiered).
    public const string FirstBite = "first_bite";
    public const string GridFilled = "grid_filled";

    private const int ScoreTierStep = 100;
    private const int ScoreFirstMilestone = 50; // special one-off before the regular 100-tiers start
    private const int LengthTierStep = 10;

    private HashSet<string> unlockedAchievements = new HashSet<string>();
    private bool foodEatenThisGame = false;

    // Fired whenever a NEW achievement is unlocked - carries display text
    // directly, so listeners (popup, list screen) don't need a separate lookup.
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

        (string title, string description) = GetDisplayInfo(achievementId);

        Debug.Log("Achievement unlocked: " + achievementId + " (" + title + ")");
        OnAchievementUnlocked?.Invoke(achievementId, title, description);
    }

    public bool IsUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    /// <summary>
    /// Returns a saved achievement's title/description purely by parsing its
    /// ID - works for any tier (past, current, or future) without needing a
    /// hardcoded list, since IDs follow a consistent "score_N" / "length_N"
    /// pattern. Used by the popup and (later) the achievements list screen.
    /// </summary>
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

        return (achievementId, ""); // fallback, shouldn't normally happen
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

            // Unlocks every crossed 10-length tier in one pass, in case
            // growth ever jumps by more than 1 (it doesn't currently, but
            // this stays correct either way).
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
            // 404 just means there was nothing saved yet for this player - not a real error.
            Debug.Log("AchievementManager: nothing to delete (already empty). " + e.Message);
        }
    }
#endif
}