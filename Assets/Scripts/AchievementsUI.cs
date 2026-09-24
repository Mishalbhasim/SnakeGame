using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class AchievementsUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject achievementsPanel; 
    public GameObject mainMenuPanel;   

    [Header("List")]
    public Transform contentParent;         
    public GameObject achievementRowPrefab;  

    [Header("Counter")]
    public TextMeshProUGUI counterText;

    [Header("Look")]
    public Color unlockedStatusColor = new Color(0.4f, 0.9f, 0.4f);
    public Color lockedStatusColor = new Color(0.7f, 0.7f, 0.7f);
    [Range(0f, 1f)] public float lockedAlpha = 0.4f;

    public void OpenPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (achievementsPanel != null) achievementsPanel.SetActive(true);

        Populate();
    }

    public void ClosePanel()
    {
        if (achievementsPanel != null) achievementsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void Populate()
    {
        if (AchievementManager.Instance == null || contentParent == null || achievementRowPrefab == null)
        {
            Debug.LogWarning("AchievementsUI: missing reference, cannot populate.");
            return;
        }

        
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        
        List<string> ids = new List<string>(AchievementManager.CoreAchievementIds);

        IEnumerable<string> extras = AchievementManager.Instance.GetUnlockedIds()
            .Where(id => !ids.Contains(id))
            .OrderBy(id => id.StartsWith("score_") ? 0 : 1)
            .ThenBy(id => TierNumber(id));

        ids.AddRange(extras);

        int unlockedCount = 0;

        foreach (string id in ids)
        {
            bool unlocked = AchievementManager.Instance.IsUnlocked(id);
            if (unlocked) unlockedCount++;

            (string title, string description) = AchievementManager.GetDisplayInfo(id);
            CreateRow(title, description, unlocked);
        }

        if (counterText != null)
        {
            counterText.text = "Unlocked: " + unlockedCount + " / " + ids.Count;
        }
    }

    private void CreateRow(string title, string description, bool unlocked)
    {
        GameObject row = Instantiate(achievementRowPrefab, contentParent);

        TextMeshProUGUI titleText = row.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = row.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusText = row.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        if (statusText != null)
        {
            statusText.text = unlocked ? "Unlocked" : "Locked";
            statusText.color = unlocked ? unlockedStatusColor : lockedStatusColor;
        }

 
        CanvasGroup group = row.GetComponent<CanvasGroup>();
        if (group == null) group = row.AddComponent<CanvasGroup>();
        group.alpha = unlocked ? 1f : lockedAlpha;
    }

    
    private static int TierNumber(string id)
    {
        int underscore = id.LastIndexOf('_');
        if (underscore >= 0 && int.TryParse(id.Substring(underscore + 1), out int n)) return n;
        return 0;
    }
}