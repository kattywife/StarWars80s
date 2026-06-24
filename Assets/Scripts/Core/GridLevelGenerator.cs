using UnityEngine;
using UnityEngine.Tilemaps;

public class GridLevelGenerator : MonoBehaviour
{
    [Header("Активная конфигурация")]
    public DungeonConfig activeConfig;         // ScriptableObject со всеми правилами и префабами

    [Header("Ссылки на сцену")]
    public GameObject playerReference;         // Ссылка на префаб джедая на сцене
    public Tilemap floorTilemap;               // Слой пола
    public Tilemap wallTilemap;                // Слой стен
    public TileBase wallTile;                  // Тайлик стены
    public TileBase floorTile;                  // Тайлик пола

    private DungeonState currentState;
    private System.Random prng;
    private int currentLevelLoaded = 1;

    // Отладочный метод для быстрой регенерации во время тестов на клавишу G
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log($"<color=yellow>[DEBUG]</color> Принудительная модульная генерация уровня {currentLevelLoaded}...");
            Generate(currentLevelLoaded);
        }
    }

    // Главная точка входа для ядра игры (вызывается из GameManager)
    public void Generate(int level)
    {
        currentLevelLoaded = level;

        if (activeConfig == null)
        {
            Debug.LogError("GridLevelGenerator: Не назначен ассет конфигурации activeConfig в инспекторе!");
            return;
        }

        // Инициализируем локальный PRNG для детерминизма на основе сида времени
        int seed = System.DateTime.Now.Millisecond + level * 100;
        prng = new System.Random(seed);

        // Масштабируем размеры карты на основе уровня
        int targetRoomCount = Mathf.Min(4 + level, 12);
        int mapWidth = Mathf.Min(40 + (level * 3), 85);
        int mapHeight = Mathf.Min(40 + (level * 3), 85);

        // Инициализируем чистый контекст состояния
        currentState = new DungeonState(mapWidth, mapHeight, level, prng);

        GenerateFullLevel(targetRoomCount);
    }

    [ContextMenu("Generate Full Level")]
    public void GenerateFullLevelFromInspector()
    {
        Generate(currentLevelLoaded);
    }

    private void GenerateFullLevel(int targetRoomCount)
    {
        ClearOldLevel();

        // Поочередно вызываем чистые расчетные модули (Конвейер Variant A)
        
        // 1. Расстановка комнат в сетке
        DungeonLayoutGenerator.PlaceRooms(currentState, activeConfig, targetRoomCount);

        // 2. Построение MST и нелинейных связей (коридоры)
        DungeonLayoutGenerator.ConnectRooms(currentState, activeConfig);

        // 3. Обводка стен
        DungeonLayoutGenerator.OutlineWalls(currentState);

        // 4. Построение волновой карты расстояний Дийкстры
        DijkstraPathfinder.GenerateDijkstraMap(currentState);

        // 5. Физическая отрисовка тайлов на сцене
        DungeonVisualizer.DrawTilemap(currentState, floorTilemap, wallTilemap, floorTile, wallTile);

        // 6. Наполнение уровня объектами, врагами и воротами по критическому пути
        DungeonSpawner.SpawnEntities(currentState, activeConfig, floorTilemap, playerReference);

        // 7. Планирование появления Космического Червя (Начиная с 5 уровня)
        if (currentLevelLoaded >= activeConfig.wormUnlockLevel && activeConfig.wormPrefab != null)
        {
            float roll = (float)(prng.NextDouble() * 100.0);
            if (roll <= activeConfig.wormSpawnChance)
            {
                Invoke("SpawnSpaceWorm", activeConfig.wormSpawnDelay);
                Debug.Log("<color=orange>ЧЕРВЬ ЗАПЛАНИРОВАН: появится через " + activeConfig.wormSpawnDelay + " сек.</color>");
            }
        }
    }

    // Метод вызова Космического Червя на таймере (MonoBehaviour)
    public void SpawnSpaceWorm()
    {
        if (activeConfig == null || activeConfig.wormPrefab == null || playerReference == null) return;

        float angle = (float)(prng.NextDouble() * 360.0) * Mathf.Deg2Rad;
        Vector3 playerPos = playerReference.transform.position;
        Vector3 spawnPos = playerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 25f;

        GameObject worm = Instantiate(activeConfig.wormPrefab, spawnPos, Quaternion.identity);
        
        if (currentState != null)
        {
            currentState.spawnedEntities.Add(worm); 
        }

        Debug.Log("<color=red>ВНИМАНИЕ: Пробудился Космический Червь!</color>");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sandwormRoarSound);
        }
    }

    private void ClearOldLevel()
    {
        CancelInvoke("SpawnSpaceWorm");

        // Очищаем сгенерированные сущности текущей сессии
        if (currentState != null)
        {
            foreach (GameObject obj in currentState.spawnedEntities)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            currentState.spawnedEntities.Clear();
        }

        // Страховочная очистка сцены по тегам от случайных дубликатов
        string[] tags = { "Enemy", "Finish", "Crystal", "Worm" };
        foreach (string tag in tags) 
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects) Destroy(obj);
        }
    }
}
