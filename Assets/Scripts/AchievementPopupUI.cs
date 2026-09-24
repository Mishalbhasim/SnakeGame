using System.Collections;
using TMPro;
using UnityEngine;


public class AchievementPopupUI : MonoBehaviour
{
    [Header("Popup")]
    public GameObject popupRoot; 
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