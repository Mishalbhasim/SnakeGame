using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Small toast-style popup that appears whenever an achievement unlocks,
/// then auto-hides after a few seconds. Listens to
/// AchievementManager.OnAchievementUnlocked directly - works no matter
/// where in the game the achievement unlocks (mid-gameplay, menu, etc.),
/// independent of whether the Achievements list screen is open.
/// Attach this script to a small popup GameObject living directly under
/// the Canvas (sibling of MainMenuPanel/GameOverPanel, not inside them),
/// so it can appear over anything.
/// </summary>
public class AchievementPopupUI : MonoBehaviour
{
    [Header("Popup")]
    public GameObject popupRoot; // Usually this same GameObject.
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Timing")]
    public float displaySeconds = 3f;

    private Coroutine hideRoutine;

    private void OnEnable()
    {
        AchievementManager.OnAchievementUnlocked += HandleAchievementUnlocked;
    }

    private void OnDisable()
    {
        AchievementManager.OnAchievementUnlocked -= HandleAchievementUnlocked;
    }

    private void Start()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
    }

    private void HandleAchievementUnlocked(string achievementId, string title, string description)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        if (popupRoot != null) popupRoot.SetActive(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displaySeconds);
        if (popupRoot != null) popupRoot.SetActive(false);
        hideRoutine = null;
    }


}