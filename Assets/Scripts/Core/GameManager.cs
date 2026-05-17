using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Основные параметры")]
    public int currentLevel = 1;
    public int playerHealth = 3;
    public int crystals = 0;
    public LevelGenerator generator;

    [Header("Настройки Игры")]
    public int winLevel = 15; 
    public int totalEasterEggs = 5; 
    public int collectedEasterEggs = 0; 

    public delegate void UltimateAction();
    public static event UltimateAction OnUltimateUsed;

    [Header("Задержка смерти")]
    public float gameOverDelay = 2.0f;

    private string lastDeathReason = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (MenuController.Instance != null) 
        {
            MenuController.Instance.ShowMainMenu();
        }
    }

    public void StartNewGame()
    {
        currentLevel = 1;
        playerHealth = 3;
        crystals = 0;
        collectedEasterEggs = 0;
        StartLevel();
    }

    public void StartLevel()
    {
        Time.timeScale = 1; 
        playerHealth = 3; 
        
        if (MenuController.Instance != null) MenuController.Instance.HideAll();
        
        if (generator == null) generator = Object.FindAnyObjectByType<LevelGenerator>();
        if (generator != null) generator.Generate(currentLevel);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevel(currentLevel);
            UIManager.Instance.UpdateHearts(playerHealth);
            UIManager.Instance.UpdateCrystals(crystals);
        }
    }

    public void LevelCompleted()
    {
        Time.timeScale = 0; 
        SaveMyRecord();
        currentLevel++;

        if (currentLevel > winLevel)
        {
            // ЗВУК: Победа в игре
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.winSound);
            MenuController.Instance.ShowVictory(collectedEasterEggs, totalEasterEggs);
        }
        else
        {
            // Здесь можно добавить звук "прохода уровня", если он будет
            MenuController.Instance.ShowTransition(currentLevel);
        }
    }

    public void GameOver(string reason)
    {
        SaveMyRecord();
        // ЗВУК: Конец игры
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
        StartCoroutine(GameOverRoutine(reason));
    }

    private IEnumerator GameOverRoutine(string reason)
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);
        Time.timeScale = 0;
        if (MenuController.Instance != null) 
            MenuController.Instance.ShowGameOver(reason, currentLevel);
    }

    private void SaveMyRecord()
    {
        int lastRecord = PlayerPrefs.GetInt("Record", 0);
        if (currentLevel > lastRecord)
        {
            PlayerPrefs.SetInt("Record", currentLevel);
            PlayerPrefs.Save();
        }
    }

    // ==========================================
    // ЛОГИКА СБОРА
    // ==========================================

    public void AddCrystal()
    {
        if (crystals < 3)
        {
            crystals++;
            // ЗВУК: Кристалл поднят
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.crystalTakenSound);
            
            if (UIManager.Instance != null) UIManager.Instance.UpdateCrystals(crystals);
        }
    }

    public bool TryUseUltimate()
    {
        if (crystals >= 3)
        {
            crystals = 0;
            if (UIManager.Instance != null) UIManager.Instance.UpdateCrystals(crystals);
            if (OnUltimateUsed != null) OnUltimateUsed(); 
            return true;
        }
        return false;
    }

    public void AddEasterEgg()
    {
        collectedEasterEggs++;
    }
}