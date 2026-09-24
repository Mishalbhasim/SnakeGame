using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;


public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private const string LeaderboardId = "snake_highscore";
    private const int TopScoresCount = 20;

    private const string PendingScoreKey = "PendingLeaderboardScore";

    private bool isFlushing = false;

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
        // send previous unsynced score
        await FlushPendingScore();
    }


    private async void HandleGameOver(int finalScore, bool isWin)
    {
        SavePendingScore(finalScore);
        await FlushPendingScore();
    }

    private void SavePendingScore(int score)
    {
        int pending = PlayerPrefs.HasKey(PendingScoreKey)
            ? PlayerPrefs.GetInt(PendingScoreKey)
            : -1;

        if (score > pending)
        {
            PlayerPrefs.SetInt(PendingScoreKey, score);
            PlayerPrefs.Save();
        }
    }


    private async Task FlushPendingScore()
    {
        if (isFlushing) return;
        isFlushing = true;

        try
        {
            while (PlayerPrefs.HasKey(PendingScoreKey))
            {
                int toSend = PlayerPrefs.GetInt(PendingScoreKey);

                try
                {
                    await AuthManager.Instance.EnsureSignedIn();
                    await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, toSend);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("LeaderboardManager: could not send score, will retry later - " + e.Message);
                    return; 
                }

                
                if (PlayerPrefs.GetInt(PendingScoreKey) <= toSend)
                {
                    PlayerPrefs.DeleteKey(PendingScoreKey);
                    PlayerPrefs.Save();
                    Debug.Log("LeaderboardManager: score " + toSend + " submitted.");
                }

            }
        }
        finally
        {
            isFlushing = false;
        }
    }


    public async Task<List<LeaderboardEntry>> FetchTopScores()
    {
        try
        {
            await AuthManager.Instance.EnsureSignedIn();

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

    public async Task<LeaderboardEntry> FetchPlayerScore()
    {
        try
        {
            await AuthManager.Instance.EnsureSignedIn();
            return await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("LeaderboardManager: failed to fetch player score - " + e.Message);
            return null;
        }
    }
}