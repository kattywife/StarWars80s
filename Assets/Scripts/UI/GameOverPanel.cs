using UnityEngine;
using TMPro;

public class GameOverPanel : MonoBehaviour
{
    public TextMeshProUGUI reasonText;
    public TextMeshProUGUI scoreText;

    public void Setup(string reason, int level)
    {
        reasonText.text = "опхвхмю: " + reason;
        scoreText.text = "бш днькх дн " + level + " спнбмъ";
    }

    public void RestartBtn()
    {
        GameManager.Instance.StartNewGame();
    }

    public void ToMainMenuBtn()
    {
        MenuController.Instance.ShowMainMenu();
    }
}