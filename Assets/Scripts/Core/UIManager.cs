using UnityEngine;
using UnityEngine.UI;
using TMPro; // Это пространство имен для TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // Изменили тип с Text на TextMeshProUGUI
    public TextMeshProUGUI levelText; 
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    // Добавь новые переменные в начало:
    public UnityEngine.UI.Image[] crystalIcons;
    public Sprite fullCrystal;
    public Sprite emptyCrystal;

    // Добавь новый метод в конец:
    public void UpdateCrystals(int currentCrystals)
    {
        for (int i = 0; i < crystalIcons.Length; i++)
        {
            if (crystalIcons[i] != null)
            {
                if (i < currentCrystals)
                    crystalIcons[i].sprite = fullCrystal;
                else
                    crystalIcons[i].sprite = emptyCrystal;
            }
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = "LEVEL: " + level;
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}