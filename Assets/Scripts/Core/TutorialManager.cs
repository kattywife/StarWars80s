using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public RectTransform tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public float displayDuration = 4f;
    public float slideSpeed = 0.5f;

    [Header("Координаты (возьми из RectTransform)")]
    public Vector2 hiddenPos; // Позиция за экраном
    public Vector2 visiblePos; // Позиция на экране

    void Awake()
    {
        Instance = this;
        // При старте гарантированно прячем панель
        if(tutorialPanel != null) tutorialPanel.anchoredPosition = hiddenPos;
    }

    [ContextMenu("Test Show")] // Можно нажать правой кнопкой на компонент в инспекторе
    public void TestShow()
    {
        ShowTutorial("ТЕСТОВАЯ ПОДСКАЗКА: Жми Пробел!");
    }

    public void ShowTutorial(string message)
    {
        if (tutorialPanel == null) return;
        StopAllCoroutines();
        StartCoroutine(TutorialRoutine(message));
    }

    IEnumerator TutorialRoutine(string message)
    {
        tutorialText.text = message;

        // ВЫЕЗД
        float elapsed = 0;
        while (elapsed < slideSpeed)
        {
            tutorialPanel.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, elapsed / slideSpeed);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        tutorialPanel.anchoredPosition = visiblePos;

        // ОЖИДАНИЕ (пока игрок читает)
        yield return new WaitForSecondsRealtime(displayDuration);

        // УЕЗД
        elapsed = 0;
        while (elapsed < slideSpeed)
        {
            tutorialPanel.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, elapsed / slideSpeed);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        tutorialPanel.anchoredPosition = hiddenPos;
    }
}