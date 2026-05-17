using UnityEngine;
using TMPro;

public class GameOverPanel : MonoBehaviour
{
    public TextMeshProUGUI reasonText;
    public TextMeshProUGUI levelReachedText;
    
    private TypewriterEffect typewriter;

    public void Setup(string reason, int level)
    {
        // Проверка: привязан ли текст уровня
        if (levelReachedText != null)
        {
            levelReachedText.text = "ВЫ ДОСТИГЛИ УРОВНЯ: " + level;
        }
        else 
        {
            Debug.LogError("GameOverPanel: Не привязана ссылка на LevelReachedText!");
        }

        // Проверка: привязан ли текст причины
        if (reasonText != null)
        {
            if (typewriter == null) 
                typewriter = reasonText.GetComponent<TypewriterEffect>();

            if (typewriter != null)
            {
                typewriter.StartTyping("ПРИЧИНА: " + reason);
            }
            else
            {
                reasonText.text = "ПРИЧИНА: " + reason;
            }
        }
        else
        {
            Debug.LogError("GameOverPanel: Не привязана ссылка на ReasonText!");
        }
    }

    public void RestartBtn()
    {
        if (MenuController.Instance != null) MenuController.Instance.PlayButtonSound();
        GameManager.Instance.StartNewGame();
    }

    public void MainMenuBtn()
    {
        if (MenuController.Instance != null) MenuController.Instance.PlayButtonSound();
        MenuController.Instance.ShowMainMenu();
    }
}