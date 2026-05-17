using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    public GameObject mainMenuPanel;
    public GameObject transitionPanel;
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public GameObject introPanel; // Добавь ссылку в инспекторе


    public bool showJokes = true;

    void Awake() { Instance = this; }

    public void StartGameBtn()
    {
        PlayButtonSound();
        
        // 1. Сбрасываем статы (уровень 1, жизни 3)
        if (GameManager.Instance != null) 
            GameManager.Instance.PrepareNewGame();

        // 2. Скрываем меню
        HideAll();

        // 3. Показываем интро
        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }
        else
        {
            // Если забыли назначить панель в инспекторе - просто стартуем
            GameManager.Instance.StartLevel();
        }
    }

    // НОВЫЙ МЕТОД: для вызова из кнопок
    public void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    public void ShowMainMenu()
    {
        HideAll();
        mainMenuPanel.SetActive(true);
    }

    public void ShowGameOver(string reason, int level)
    {
        HideAll();
        gameOverPanel.SetActive(true);
        gameOverPanel.GetComponent<GameOverPanel>().Setup(reason, level);
    }

    public void ShowTransition(int nextLevel)
    {
        HideAll();
        transitionPanel.SetActive(true);
        transitionPanel.GetComponent<TransitionPanel>().Setup(nextLevel);
    }

    public void ShowVictory(int collected, int total)
    {
        HideAll();
        victoryPanel.SetActive(true);
        victoryPanel.GetComponent<VictoryPanel>().Setup(collected, total);
    }

    public void HideAll()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (transitionPanel != null) transitionPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        // ДОБАВЬ ЭТУ СТРОЧКУ:
        if (introPanel != null) introPanel.SetActive(false); 

    }

    public void ToggleJokes(bool isOn)
    {
        PlayButtonSound(); // Добавляем звук при переключении
        showJokes = isOn;
    }
}