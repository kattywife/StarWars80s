using UnityEngine;
using TMPro;

public class TransitionPanel : MonoBehaviour
{
    public TextMeshProUGUI jokeText;
    public JokeManager jokeManager;
    
    // Ссылка на компонент печати
    private TypewriterEffect typewriter;

    void Awake()
    {
        typewriter = jokeText.GetComponent<TypewriterEffect>();
        // Если забыли повесить скрипт на текст - добавим его сами
        if (typewriter == null) typewriter = jokeText.gameObject.AddComponent<TypewriterEffect>();
    }

    public void Setup(int nextLevel)
    {
        if (jokeManager == null) return;

        string finalString = "";

        if (nextLevel == 2)
            finalString = "ПОДСКАЗКА: Отражай красные пули мечом, чтобы они стали зелеными и смертельными для врагов!";
        else if (nextLevel == 5)
            finalString = "ВНИМАНИЕ: На этом уровне пробуждается Космический Червь. Он проходит сквозь стены!";
        else if (nextLevel == 10)
            finalString = "СОВЕТ: Твоя Ульта (Пробел) не только убивает врагов, но и отпугивает Червя.";
        else
            finalString = jokeManager.GetAnything();

        // Запускаем печать вместо обычной вставки текста
        typewriter.StartTyping(finalString);
    }

    public void NextLevelBtn()
    {
        if (GameManager.Instance != null)
        {
            // Звук клика (у нас уже есть в MenuController, но можно и тут)
            if (MenuController.Instance != null) MenuController.Instance.PlayButtonSound();
            
            GameManager.Instance.StartLevel();
        }
    }
}