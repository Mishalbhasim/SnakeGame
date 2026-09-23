using System.Collections.Generic;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Populates LeaderboardPanel: spawns up to 20 ScoreRow prefab instances
/// into the ScrollView's Content, and shows/hides the pinned YourRankRow.
/// Self-contained on purpose - Leaderboard is an on-demand overlay opened
/// from Main Menu, not part of GameManager's GameState machine, so it
/// doesn't belong inside UIManager. Wire OpenPanel()/ClosePanel() to the
/// LeaderboardButton and BackButton's OnClick() in the Inspector.
/// Attach this script directly to the LeaderboardPanel GameObject.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject leaderboardPanel; // Usually this same GameObject.
    public GameObject mainMenuPanel;    // Hidden while leaderboard open, restored on close.

    [Header("Scroll List")]
    public Transform contentParent;     // The "Content" object inside ScrollView/Viewport.
    public GameObject scoreRowPrefab;   // The ScoreRow prefab from Assets/Prefabs.

    [Header("Your Rank Row")]
    public GameObject yourRankRow;
    public TextMeshProUGUI yourRankText;
    public TextMeshProUGUI yourNameText;
    public TextMeshProUGUI yourScoreText;

    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    /// <summary>Call from LeaderboardButton's OnClick().</summary>
    public async void OpenPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);

        ClearRows();
        if (yourRankRow != null) yourRankRow.SetActive(false);

        List<LeaderboardEntry> topScores = await LeaderboardManager.Instance.FetchTopScores();
        PopulateTopScores(topScores);

        LeaderboardEntry playerEntry = await LeaderboardManager.Instance.FetchPlayerScore();
        PopulateYourRank(playerEntry, topScores);
    }

    /// <summary>Call from BackButton's OnClick().</summary>
    public void ClosePanel()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void ClearRows()
    {
        foreach (GameObject row in spawnedRows)
        {
            Destroy(row);
        }
        spawnedRows.Clear();
    }

    private void PopulateTopScores(List<LeaderboardEntry> entries)
    {
        foreach (LeaderboardEntry entry in entries)
        {
            GameObject row = Instantiate(scoreRowPrefab, contentParent);
            row.SetActive(true);
            spawnedRows.Add(row);

            // Rank from the SDK is 0-indexed (0 = 1st place), so +1 for display.
            SetRowTexts(row, entry.Rank + 1, entry.PlayerName, (int)entry.Score);
        }
    }

    private void PopulateYourRank(LeaderboardEntry playerEntry, List<LeaderboardEntry> topScores)
    {
        if (playerEntry == null || yourRankRow == null) return;

        // If the player is already visible in the top list, don't show the
        // pinned row too - avoids a confusing duplicate.
        bool alreadyInTopList = topScores.Exists(e => e.PlayerId == playerEntry.PlayerId);
        if (alreadyInTopList) return;

        yourRankRow.SetActive(true);
        if (yourRankText != null) yourRankText.text = "#" + (playerEntry.Rank + 1);
        if (yourNameText != null) yourNameText.text = playerEntry.PlayerName;
        if (yourScoreText != null) yourScoreText.text = ((int)playerEntry.Score).ToString();
    }

    private void SetRowTexts(GameObject row, int rank, string playerName, int score)
    {
        TextMeshProUGUI rankText = row.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = row.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

        if (rankText != null) rankText.text = rank.ToString();
        if (nameText != null) nameText.text = playerName;
        if (scoreText != null) scoreText.text = score.ToString();
    }
}