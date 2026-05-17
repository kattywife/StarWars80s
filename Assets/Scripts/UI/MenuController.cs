using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    public GameObject mainMenuPanel;
    public GameObject transitionPanel;
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    
    public bool showJokes = true;

    void Awake() { Instance = this; }

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
        mainMenuPanel.SetActive(false);
        transitionPanel.SetActive(false);
        victoryPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ToggleJokes(bool isOn)
    {
        PlayButtonSound(); // Добавляем звук при переключении
        showJokes = isOn;
    }
}