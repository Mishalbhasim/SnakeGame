using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

/// <summary>
/// Handles auto-submitting the player's score on GameOver, and fetching
/// data for the Leaderboard UI. Auto-submit is event-driven (listens to
/// GameManager.OnGameOver), matching the rest of the project's pattern.
/// Fetch methods are request-response, called directly by the Leaderboard
/// UI script when the panel opens - not events, since nothing else needs
/// to react to a fetch happening.
///
/// Sign-in is handled by AuthManager (shared with AchievementManager) to
/// avoid a race condition where two scripts try to sign in at once.
///
/// Attach to an empty GameObject called "LeaderboardManager" in the scene.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    // Must match the Leaderboard ID created on the UGS Dashboard exactly.
    private const string LeaderboardId = "snake_highscore";
    private const int TopScoresCount = 20;

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
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    private async void Start()
    {
        await AuthManager.Instance.EnsureSignedIn();
    }

    /// <summary>
    /// Fires on every GameOver (win or loss) and submits the run's score.
    /// AddPlayerScoreAsync only actually updates the stored score if it
    /// beats the player's previous best - the leaderboard's "Best score"
    /// update strategy handles that server-side, no client check needed.
    /// </summary>
    private async void HandleGameOver(int finalScore, bool isWin)
    {
        await AuthManager.Instance.EnsureSignedIn();

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, finalScore);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("LeaderboardManager: failed to submit score - " + e.Message);
        }
    }

    /// <summary>
    /// Fetches the top 20 entries. Called directly by the Leaderboard UI
    /// script when the panel opens. Returns an empty list on failure so the
    /// UI can handle it gracefully instead of crashing.
    /// </summary>
    public async Task<List<LeaderboardEntry>> FetchTopScores()
    {
        await AuthManager.Instance.EnsureSignedIn();

        try
        {
            var options = new GetScoresOptions { Offset = 0, Limit = TopScoresCount };
            var response = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId, options);
            return response.Results;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("LeaderboardManager: failed to fetch top scores - " + e.Message);
            return new List<LeaderboardEntry>();
        }
    }

    /// <summary>
    /// Fetches the current player's own entry (score + rank), regardless of
    /// where they sit. Returns null if the player has no score yet (never
    /// finished a game) or the request fails - the UI should handle null by
    /// hiding the "Your Rank" row.
    /// </summary>
    public async Task<LeaderboardEntry> FetchPlayerScore()
    {
        await AuthManager.Instance.EnsureSignedIn();

        try
        {
            return await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("LeaderboardManager: failed to fetch player score - " + e.Message);
            return null;
        }
    }
}