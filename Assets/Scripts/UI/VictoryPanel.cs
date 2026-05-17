using UnityEngine;
using TMPro;

public class VictoryPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText; // "ПОБЕДА" или "ПАСХАЛКА НАЙДЕНА"
    public TextMeshProUGUI statsText;
    public GameObject goldenEggIcon;   // Картинка золотого яйца (включим если победа истинная)

    public void Setup(int collected, int total)
    {
        if (collected == total) // Если собраны все 3 ключа
        {
            titleText.text = "<color=yellow>ПАСХАЛКА ХОЛЛИДЕЯ НАЙДЕНА!</color>";
            statsText.text = "Вы прошли протокол OASIS и нашли Золотое Яйцо.\nТеперь вы владелец системы!";
            if (goldenEggIcon != null) goldenEggIcon.SetActive(true);
        }
        else
        {
            titleText.text = "ПРОТОКОЛ ЗАВЕРШЕН";
            statsText.text = $"Вы прошли все уровни, но не нашли ключи Холлидея.\nСобрано ключей: {collected} из {total}";
            if (goldenEggIcon != null) goldenEggIcon.SetActive(false);
        }
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