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
    [Header("Пасхалки (Ключи Холлидея)")]
    public bool hasCopperKey = false;   // Ключ за зачистку 10+ уровня
    public bool hasJadeKey = false;     // Ключ за победу над Червем
    public bool hasCrystalKey = false;  // Ключ за секретную дискету

    // Метод получения ключа
    public void CollectKey(int keyIndex, string keyName)
    {
        if (keyIndex == 1 && !hasCopperKey) hasCopperKey = true;
        else if (keyIndex == 2 && !hasJadeKey) hasJadeKey = true;
        else if (keyIndex == 3 && !hasCrystalKey) hasCrystalKey = true;
        else return;

        // ОБНОВЛЯЕМ UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateKeys(hasCopperKey, hasJadeKey, hasCrystalKey);
        }

        // Звуки и туториалы (как были раньше)
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ShowTutorial("<color=yellow>КЛЮЧ ПОЛУЧЕН!</color>\n" + keyName);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.winSound);
    }
        // Проверка на уничтожение всех врагов (для Медного ключа)
    public void CheckEnemyCount()
    {
        // Условие: Уровень 10 или выше и ключа еще нет
        if (currentLevel >= 10 && !hasCopperKey)
        {
            // Считаем всех врагов на сцене
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            // Если остался всего один (тот, который сейчас умирает), значит уровень зачищен
            if (enemies.Length <= 1)
            {
                CollectKey(1, "Медный ключ (Зачистка 10+ уровня)");
            }
        }
    }

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
    
    public void PrepareNewGame()
    {
        currentLevel = 1;
        playerHealth = 3;
        crystals = 0;
        collectedEasterEggs = 0;
        
        // СБРОС КЛЮЧЕЙ
        hasCopperKey = false;
        hasJadeKey = false;
        hasCrystalKey = false;
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
        
        if (currentLevel == 1)
        {
            Invoke("ShowFirstTutorial", 2f);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevel(currentLevel);
            UIManager.Instance.UpdateHearts(playerHealth);
            UIManager.Instance.UpdateCrystals(crystals);
            UIManager.Instance.UpdateKeys(hasCopperKey, hasJadeKey, hasCrystalKey);

        }
    }

    void ShowFirstTutorial()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ShowTutorial("МЫШЬ/стрелки: Вращение\nWASD: Движение");
        
        Invoke("ShowSecondTutorial", 5f);
    }

    void ShowSecondTutorial()
    {
        TutorialManager.Instance.ShowTutorial("ОТРАЖАЙ пули мечом или нажми Е для удара");
    }

        // В GameManager.cs добавь этот метод для проверки всех ключей
    public bool IsTrueVictory()
    {
        return hasCopperKey && hasJadeKey && hasCrystalKey;
    }

    // Измени метод LevelCompleted, чтобы он учитывал ключи
    public void LevelCompleted()
    {
        Time.timeScale = 0; 
        SaveMyRecord();

        if (currentLevel >= winLevel)
        {
            // ЗВУК: Эпичная победа
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.winSound);
            
            // ПРОВЕРКА: Если собраны все ключи, вызываем "Истинную победу"
            if (IsTrueVictory())
            {
                MenuController.Instance.ShowVictory(3, 3); // Передаем 3 из 3 пасхалок
            }
            else
            {
                MenuController.Instance.ShowVictory(0, 3); // Обычная победа без пасхалок
            }
        }
        else
        {
            currentLevel++;
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