using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnRule
{
    public string enemyName;
    public GameObject enemyPrefab;
    public int unlockLevel = 1;
}

public class LevelGenerator : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public GameObject playerReference; 
    
    [Header("Префабы")]
    public GameObject[] wallPrefabs; 
    public GameObject finishPrefab;  
    public GameObject crystalPrefab; 
    public GameObject diskettePrefab;

    [Header("Настройки врагов")]
    public EnemySpawnRule[] enemyTypes; 

    [Header("Настройки позиций")]
    public Vector2 startPoint = new Vector2(-12, -12); 
    public Vector2 finishPoint = new Vector2(12, 12);  

    [Header("Параметры уровня")]
    public int wallCount = 10;
    public float areaSize = 15f;
    public float safetyRadius = 3f; 
    
    [Tooltip("Зазор для прохода (1.5 = полторы ширины джедая)")]
    public float passageMargin = 1.5f; 

    [Header("Настройки Космического Червя")]
    public GameObject wormPrefab;
    public int wormUnlockLevel = 5;
    [Range(0, 100)] public float wormSpawnChance = 30f; 
    public float wormSpawnDelay = 5f;

    private float playerSize = 1f;
    private bool wormCalledThisLevel = false; // ВОТ ЭТОЙ СТРОЧКИ НЕ ХВАТАЛО

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     SpawnSpaceWorm();
        //     Debug.Log("DEBUG: Червь вызван вручную!");
        // }
    }

   public void Generate(int level)
    {
        CalculatePlayerSize();
        
        // Вместо поиска по тегу, используем прямую ссылку playerReference
        if (playerReference != null)
        {
            playerReference.SetActive(true); // ВКЛЮЧАЕМ игрока, если он был выключен
            playerReference.transform.position = startPoint;
            
            // Сбрасываем жизни джедая до максимума (3) при каждом новом уровне или рестарте
            JediController jedi = playerReference.GetComponent<JediController>();
            if (jedi != null) jedi.health = 3; 
        }

        ClearOldLevel();

        // 1. Спавним стены
        for (int i = 0; i < wallCount; i++)
        {
            GameObject wallPrefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
            float rotation = (Random.value > 0.5f) ? 0f : 90f;
            
            Vector2 checkSize = GetWallCheckSize(wallPrefab);
            Vector2 pos = GetValidWallPos(checkSize, rotation);

            if (pos != Vector2.zero)
            {
                Instantiate(wallPrefab, pos, Quaternion.Euler(0, 0, rotation));
            }
        }

        SpawnEnemies(level);
        SpawnCrystals();

        if (level == 7 && GameManager.Instance != null && !GameManager.Instance.hasCrystalKey && diskettePrefab != null)
        {
            Vector2 pos = GetValidPointPos(playerSize * 0.5f);
            if (pos != Vector2.zero)
            {
                Instantiate(diskettePrefab, pos, Quaternion.identity);
                Debug.Log("<color=cyan>СЕКРЕТ: Дискета Холлидея появилась на уровне!</color>");
            }
        }

        if (finishPrefab != null) 
        {
            GameObject finish = Instantiate(finishPrefab, finishPoint, Quaternion.identity);
            
            // Если это последний уровень и все ключи собраны
            if (level == 15 && GameManager.Instance.IsTrueVictory())
            {
                // Можно изменить цвет финиша на золотой
                SpriteRenderer sr = finish.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1, 0.84f, 0); // Золотой цвет
                finish.transform.localScale *= 1.5f; // Сделаем его больше
            }
        }

        if (!wormCalledThisLevel && level >= wormUnlockLevel)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= wormSpawnChance)
            {
                wormCalledThisLevel = true; 
                // Вызываем червя через несколько секунд после начала уровня
                Invoke("SpawnSpaceWorm", wormSpawnDelay);
                Debug.Log("<color=orange>ЧЕРВЬ ЗАПЛАНИРОВАН: появится через " + wormSpawnDelay + " сек.</color>");
            }
        }
    }

    public void SpawnSpaceWorm()
    {
        if (wormPrefab == null) return;

        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector2 spawnPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 25f;
        
        Instantiate(wormPrefab, spawnPos, Quaternion.identity);
        Debug.Log("<color=red>ВНИМАНИЕ: Пробудился Космический Червь!</color>");

        // ЗВУК: Рык червя при появлении
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sandwormRoarSound);
        }
    }

    private void CalculatePlayerSize()
    {
        if (playerReference != null)
        {
            Collider2D col = playerReference.GetComponent<Collider2D>();
            if (col != null) playerSize = Mathf.Max(col.bounds.size.x, col.bounds.size.y);
            else playerSize = 1f;
        }
    }

    private Vector2 GetWallCheckSize(GameObject wallPrefab)
    {
        // Ищем ВСЕ коллайдеры как на самом объекте, так и на всех его детях-квадратиках
        Collider2D[] colliders = wallPrefab.GetComponentsInChildren<Collider2D>();

        if (colliders.Length > 0)
        {
            // Берем границы первого квадратика
            Bounds bounds = colliders[0].bounds;
            
            // Добавляем к этим границам все остальные квадратики фигуры
            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            // bounds.size теперь содержит реальные ширину и высоту всей фигуры целиком!
            
            // Добавляем зазор для прохода
            float padding = playerSize * passageMargin;
            return new Vector2(bounds.size.x + padding, bounds.size.y + padding);
        }

        return new Vector2(2f, 2f); // На случай, если префаб вообще без коллайдеров
    }


    Vector2 GetValidWallPos(Vector2 checkSize, float rotation)
    {
        Vector2 pos = Vector2.zero;
        int attempts = 0;
        while (attempts < 150) 
        {
            attempts++;
            pos = new Vector2(Random.Range(-areaSize, areaSize), Random.Range(-areaSize, areaSize));
            if (Vector2.Distance(pos, startPoint) < safetyRadius || Vector2.Distance(pos, finishPoint) < safetyRadius)
                continue;

            Collider2D hit = Physics2D.OverlapBox(pos, checkSize, rotation);
            if (hit == null) return pos; 
        }
        return Vector2.zero;
    }

    Vector2 GetValidPointPos(float checkRadius)
    {
        Vector2 pos = Vector2.zero;
        int attempts = 0;
        while (attempts < 50)
        {
            attempts++;
            pos = new Vector2(Random.Range(-areaSize, areaSize), Random.Range(-areaSize, areaSize));
            if (Vector2.Distance(pos, startPoint) < safetyRadius || Vector2.Distance(pos, finishPoint) < safetyRadius) continue;
            Collider2D hit = Physics2D.OverlapCircle(pos, checkRadius);
            if (hit == null) return pos;
        }
        return Vector2.zero;
    }

    void SpawnEnemies(int level)
    {
        List<GameObject> availableEnemies = new List<GameObject>();
        foreach (var rule in enemyTypes)
        {
            if (level >= rule.unlockLevel && rule.enemyPrefab != null) 
                availableEnemies.Add(rule.enemyPrefab);
        }

        int enemiesToSpawn = 1 + level;
        if (availableEnemies.Count > 0)
        {
            foreach (GameObject type in availableEnemies) 
            {
                if (enemiesToSpawn > 0) 
                {
                    Vector2 pos = GetValidPointPos(playerSize * 0.8f);
                    if (pos != Vector2.zero) { Instantiate(type, pos, Quaternion.identity); enemiesToSpawn--; }
                }
            }
            for (int i = 0; i < enemiesToSpawn; i++) 
            {
                Vector2 pos = GetValidPointPos(playerSize * 0.8f);
                if (pos != Vector2.zero) Instantiate(availableEnemies[Random.Range(0, availableEnemies.Count)], pos, Quaternion.identity);
            }
        }
    }

    void SpawnCrystals()
    {
        if (crystalPrefab == null) return;
        int count = Random.Range(0, 3);
        for (int i = 0; i < count; i++) 
        {
            Vector2 pos = GetValidPointPos(playerSize * 0.5f);
            if (pos != Vector2.zero) Instantiate(crystalPrefab, pos, Quaternion.identity);
        }
    }

    void ClearOldLevel()
    {
        CancelInvoke("SpawnSpaceWorm");
        wormCalledThisLevel = false; 

        string[] tags = { "Wall", "Enemy", "Finish", "Crystal", "Worm" };
        foreach (string tag in tags) 
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects) Destroy(obj);
        }
    }
}