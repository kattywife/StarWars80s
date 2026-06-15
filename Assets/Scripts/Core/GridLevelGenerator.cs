using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// Helper class to store room dimensions and coordinates
public class RoomData
{
    public int x;      
    public int y;      
    public int width;
    public int height;

    public Vector2Int center => new Vector2Int(x + width / 2, y + height / 2);

    public RoomData(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    // Checks if this room overlaps with another room (with custom padding to prevent rooms squeezing together)
    public bool Overlaps(RoomData other, int padding = 2)
    {
        return (x - padding < other.x + other.width &&
                x + width + padding > other.x &&
                y - padding < other.y + other.height &&
                y + height + padding > other.y);
    }
}

public class GridLevelGenerator : MonoBehaviour
{
    [Header("Настройки Пространства (Spaciousness Settings)")]
    [Range(1, 4)] public int corridorWidth = 2; // Рекомендуется 2 или 3 для комфортного боя
    public int roomPadding = 3;                 // Минимальное расстояние между стенами комнат

    [Header("Настройки Карты (Map Dimensions)")]
    private int mapWidth = 40;
    private int mapHeight = 40;

    [Header("Настройки Комнат (Room Settings)")]
    private int targetRoomCount = 5;
    public int minRoomSize = 8;                 // Сделали комнаты просторнее
    public int maxRoomSize = 14;

    [Header("Префабы Объектов (Entity Prefabs)")]
    public GameObject playerReference;  
    public GameObject finishPrefab;
    public GameObject crystalPrefab;
    public GameObject diskettePrefab; 
    public GameObject stormtrooperPrefab;
    public GameObject eliteStormtrooperPrefab;

    [Header("Настройки Червя (Space Worm Settings)")]
    public GameObject wormPrefab;
    public int wormUnlockLevel = 5;
    [Range(0, 100)] public float wormSpawnChance = 30f;
    public float wormSpawnDelay = 8f;

    [Header("Настройки Спавна (Spawning Values)")]
    [Range(0f, 1f)] public float crystalSpawnChance = 0.3f; 

    [Header("Плитки (Tilemap Settings)")]
    public Tilemap floorTilemap; 
    public Tilemap wallTilemap;  
    public TileBase wallTile;
    public TileBase floorTile;

    // Grid constants
    public const int TILE_CORRIDOR = 0;
    public const int TILE_WALL = 1;
    public const int TILE_ROOM_FLOOR = 2;

    private int[,] grid;
    private List<RoomData> rooms = new List<RoomData>();
    private List<GameObject> spawnedEntities = new List<GameObject>();
    private int currentLevelLoaded = 1;

    public void Generate(int level)
    {
        currentLevelLoaded = level;

        // Делаем масштабирование более свободным
        // Уровень 1: 5 больших комнат, карта 42x42
        // Уровень 15: 12 огромных комнат, карта 85x85
        targetRoomCount = Mathf.Min(4 + level, 12);
        mapWidth = Mathf.Min(40 + (level * 3), 85);
        mapHeight = Mathf.Min(40 + (level * 3), 85);

        GenerateFullLevel();
    }

    [ContextMenu("Generate Full Level")]
    public void GenerateFullLevel()
    {
        ClearOldLevel();
        
        grid = new int[mapWidth, mapHeight];
        rooms.Clear();

        // 1. Fill map with walls
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                grid[x, y] = TILE_WALL;
            }
        }

        // 2. Carve non-overlapping rooms
        int attempts = 0;
        int maxAttempts = 1000; // Увеличили попытки для надежного размещения больших комнат

        while (rooms.Count < targetRoomCount && attempts < maxAttempts)
        {
            attempts++;

            int w = Random.Range(minRoomSize, maxRoomSize + 1);
            int h = Random.Range(minRoomSize, maxRoomSize + 1);

            int x = Random.Range(2, mapWidth - w - 2);
            int y = Random.Range(2, mapHeight - h - 2);

            RoomData newRoom = new RoomData(x, y, w, h);

            bool overlaps = false;
            foreach (RoomData existingRoom in rooms)
            {
                if (newRoom.Overlaps(existingRoom, roomPadding))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }

        // 3. Connect rooms sequentially with wide corridors
        ConnectRoomsSequential();

        // 4. Paint to Tilemaps
        DrawTilemap();

        // 5. Spawn Entities (Player, Exit, Crystals, Guards, Patrols)
        SpawnEntities();

        // 6. Планирование появления космического червя (Начиная с 5 уровня)
        if (currentLevelLoaded >= wormUnlockLevel && wormPrefab != null)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= wormSpawnChance)
            {
                Invoke("SpawnSpaceWorm", wormSpawnDelay);
                Debug.Log("<color=orange>ЧЕРВЬ ЗАПЛАНИРОВАН: появится через " + wormSpawnDelay + " сек.</color>");
            }
        }

        Debug.Log($"Level {currentLevelLoaded} Generation Complete! Solvable path secured across {rooms.Count} rooms.");
    }

    private void CarveRoom(RoomData room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                grid[x, y] = TILE_ROOM_FLOOR;
            }
        }
    }

    private void ConnectRoomsSequential()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int centerA = rooms[i].center;
            Vector2Int centerB = rooms[i + 1].center;
            CarveLCorridor(centerA, centerB);
        }
    }

    private void CarveLCorridor(Vector2Int start, Vector2Int end)
    {
        if (Random.value < 0.5f)
        {
            CarveHorizontalLine(start.x, end.x, start.y);
            CarveVerticalLine(start.y, end.y, end.x);
        }
        else
        {
            CarveVerticalLine(start.y, end.y, start.x);
            CarveHorizontalLine(start.x, end.x, end.y);
        }
    }

    private void CarveHorizontalLine(int xStart, int xEnd, int y)
    {
        int startX = Mathf.Min(xStart, xEnd);
        int endX = Mathf.Max(xStart, xEnd);

        // Рассчитываем симметричные границы по вертикали для расширения коридора
        int halfWidth = corridorWidth / 2;
        int startY = y - halfWidth;
        int endY = y + (corridorWidth - 1 - halfWidth);

        for (int x = startX; x <= endX; x++)
        {
            for (int cy = startY; cy <= endY; cy++)
            {
                // Защита от выхода за пределы карты (оставляем границы карты нетронутыми)
                if (x >= 1 && x < mapWidth - 1 && cy >= 1 && cy < mapHeight - 1)
                {
                    if (grid[x, cy] == TILE_WALL)
                    {
                        grid[x, cy] = TILE_CORRIDOR;
                    }
                }
            }
        }
    }

    private void CarveVerticalLine(int yStart, int yEnd, int x)
    {
        int startY = Mathf.Min(yStart, yEnd);
        int endY = Mathf.Max(yStart, yEnd);

        // Рассчитываем симметричные границы по горизонтали для расширения коридора
        int halfWidth = corridorWidth / 2;
        int startX = x - halfWidth;
        int endX = x + (corridorWidth - 1 - halfWidth);

        for (int y = startY; y <= endY; y++)
        {
            for (int cx = startX; cx <= endX; cx++)
            {
                if (cx >= 1 && cx < mapWidth - 1 && y >= 1 && y < mapHeight - 1)
                {
                    if (grid[cx, y] == TILE_WALL)
                    {
                        grid[cx, y] = TILE_CORRIDOR;
                    }
                }
            }
        }
    }

    private void DrawTilemap()
    {
        if (floorTilemap == null || wallTilemap == null || wallTile == null || floorTile == null) return;
        
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (grid[x, y] == TILE_WALL)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
                else
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }

    private void SpawnEntities()
    {
        if (rooms.Count < 2) return;

        // 1. Спавн игрока в первой комнате
        Vector3 playerPos = (floorTilemap != null) 
            ? floorTilemap.GetCellCenterWorld(new Vector3Int(rooms[0].center.x, rooms[0].center.y, 0))
            : new Vector3(rooms[0].center.x + 0.5f, rooms[0].center.y + 0.5f, 0f);

        if (playerReference != null)
        {
            playerReference.SetActive(true);
            playerReference.transform.position = playerPos;
            
            JediController jedi = playerReference.GetComponent<JediController>();
            if (jedi != null) jedi.health = 3; 
        }

        // 2. Спавн финиша в последней комнате
        if (finishPrefab != null)
        {
            Vector3 finishPos = (floorTilemap != null)
                ? floorTilemap.GetCellCenterWorld(new Vector3Int(rooms[rooms.Count - 1].center.x, rooms[rooms.Count - 1].center.y, 0))
                : new Vector3(rooms[rooms.Count - 1].center.x + 0.5f, rooms[rooms.Count - 1].center.y + 0.5f, 0f);

            GameObject finish = Instantiate(finishPrefab, finishPos, Quaternion.identity);
            spawnedEntities.Add(finish);

            if (currentLevelLoaded == 15 && GameManager.Instance != null && GameManager.Instance.IsTrueVictory())
            {
                SpriteRenderer sr = finish.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1, 0.84f, 0); 
                finish.transform.localScale *= 1.5f; 
            }
        }

        // 3. Заселение остальных комнат (Логика Кристаллов и Охраны)
        for (int i = 1; i < rooms.Count - 1; i++)
        {
            RoomData room = rooms[i];
            bool hasCrystal = Random.value < crystalSpawnChance;
            int guardCount = 0;

            if (hasCrystal && crystalPrefab != null)
            {
                Vector3 crystalPos = (floorTilemap != null)
                    ? floorTilemap.GetCellCenterWorld(new Vector3Int(room.center.x, room.center.y, 0))
                    : new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0f);

                SpawnObject(crystalPrefab, crystalPos);
                guardCount = Random.Range(4, 6);
            }
            else
            {
                guardCount = Random.Range(1, 3);
            }

            for (int j = 0; j < guardCount; j++)
            {
                Vector3 spawnPos = GetRandomFloorInRoom(room);
                GameObject trooperToSpawn = (Random.value < 0.3f && eliteStormtrooperPrefab != null) 
                    ? eliteStormtrooperPrefab 
                    : stormtrooperPrefab;

                if (trooperToSpawn != null)
                {
                    SpawnObject(trooperToSpawn, spawnPos);
                }
            }
        }

        // 4. Секретный Квест: Дискета Холлидея на 7 уровне
        if (currentLevelLoaded == 7 && GameManager.Instance != null && !GameManager.Instance.hasCrystalKey && diskettePrefab != null)
        {
            int disketteRoomIndex = Random.Range(1, rooms.Count - 1);
            Vector3 diskettePos = (floorTilemap != null)
                ? floorTilemap.GetCellCenterWorld(new Vector3Int(rooms[disketteRoomIndex].center.x, rooms[disketteRoomIndex].center.y, 0))
                : new Vector3(rooms[disketteRoomIndex].center.x + 0.5f, rooms[disketteRoomIndex].center.y + 0.5f, 0f);

            SpawnObject(diskettePrefab, diskettePos);
            Debug.Log("<color=cyan>СЕКРЕТ: Дискета Холлидея появилась в лабиринте на уровне 7!</color>");
        }

        // 5. Патрули в коридорах
        SpawnCorridorPatrols();
    }

    private void SpawnCorridorPatrols()
    {
        if (stormtrooperPrefab == null) return;

        List<Vector2Int> corridorTiles = new List<Vector2Int>();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (grid[x, y] == TILE_CORRIDOR)
                {
                    corridorTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        int patrolCount = Mathf.RoundToInt(corridorTiles.Count * 0.02f);
        for (int i = 0; i < patrolCount; i++)
        {
            if (corridorTiles.Count == 0) break;

            int randomIndex = Random.Range(0, corridorTiles.Count);
            Vector2Int tileCoords = corridorTiles[randomIndex];

            Vector3 spawnPos = (floorTilemap != null)
                ? floorTilemap.GetCellCenterWorld(new Vector3Int(tileCoords.x, tileCoords.y, 0))
                : new Vector3(tileCoords.x + 0.5f, tileCoords.y + 0.5f, 0f);
            
            SpawnObject(stormtrooperPrefab, spawnPos);
            corridorTiles.RemoveAt(randomIndex); 
        }
    }

    public void SpawnSpaceWorm()
    {
        if (wormPrefab == null || playerReference == null) return;

        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector3 playerPos = playerReference.transform.position;
        Vector3 spawnPos = playerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 25f;

        GameObject worm = Instantiate(wormPrefab, spawnPos, Quaternion.identity);
        spawnedEntities.Add(worm); 

        Debug.Log("<color=red>ВНИМАНИЕ: Пробудился Космический Червь!</color>");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sandwormRoarSound);
        }
    }

    private Vector3 GetRandomFloorInRoom(RoomData room)
    {
        int rx = Random.Range(room.x + 1, room.x + room.width - 1);
        int ry = Random.Range(room.y + 1, room.y + room.height - 1);

        return (floorTilemap != null)
            ? floorTilemap.GetCellCenterWorld(new Vector3Int(rx, ry, 0))
            : new Vector3(rx + 0.5f, ry + 0.5f, 0f);
    }

    private void SpawnObject(GameObject prefab, Vector3 position)
    {
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        spawnedEntities.Add(obj);
    }

    private void ClearOldLevel()
    {
        CancelInvoke("SpawnSpaceWorm");

        foreach (GameObject obj in spawnedEntities)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(obj);
                else
#endif
                    Destroy(obj);
            }
        }
        spawnedEntities.Clear();

        // УДАЛИЛИ "Wall" ИЗ ЭТОГО СПИСКА, ЧТОБЫ ЮНИТИ НИКОГДА НЕ УДАЛЯЛ ТВОЙ TILEMAP!
        string[] tags = { "Enemy", "Finish", "Crystal", "Worm" };
        foreach (string tag in tags) 
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects) Destroy(obj);
        }
    }
}