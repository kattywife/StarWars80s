using UnityEngine;
using TMPro;

public class MainMenuPanel : MonoBehaviour
{
    public TextMeshProUGUI recordText;

    void OnEnable() // Срабатывает каждый раз, когда панель включается
    {
        int record = PlayerPrefs.GetInt("Record", 0);
        recordText.text = "РЕКОРД: " + record + " УРОВНЕЙ";
    }

    public void StartGameBtn()
    {
        GameManager.Instance.StartNewGame(); 
    }
}