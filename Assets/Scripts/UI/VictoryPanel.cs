using UnityEngine;
using TMPro;

public class VictoryPanel : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    public void Setup(int collected, int total)
    {
        statsText.text = $"ПОБЕДА!\nСобрано пасхалок: {collected} из {total}";
    }

    public void RestartBtn()
    {
        GameManager.Instance.StartNewGame();
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
}