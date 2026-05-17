using UnityEngine;
using TMPro;

public class TransitionPanel : MonoBehaviour
{
    public TextMeshProUGUI jokeText;
    public JokeManager jokeManager;

    public void Setup(int nextLevel)
    {
        if (nextLevel == 5) jokeText.text = "ПОДСКАЗКА: Чтобы убить штурмовика, отразите пулю мечом.";
        else if (nextLevel == 10) jokeText.text = "ПОДСКАЗКА: Сила испугает Космического Червя!";
        else if (MenuController.Instance.showJokes && jokeManager != null) jokeText.text = jokeManager.GetRandomJoke();
        else jokeText.text = "Готовы к следующему уровню?";
    }

    public void NextLevelBtn()
    {
        GameManager.Instance.StartLevel(); // Продолжаем игру
    }
}