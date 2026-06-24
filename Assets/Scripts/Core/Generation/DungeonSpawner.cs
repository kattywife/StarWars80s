using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class DungeonSpawner
{
    // Главный метод наполнения сцены объектами (только штурмовики, кристаллы, финиш и дискета)
    public static void SpawnEntities(DungeonState state, DungeonConfig config, Tilemap floorTilemap, GameObject playerReference)
    {
        if (state.rooms.Count == 0) return;

        // 1. Позиционирование джедая в центре Комнаты 0
        Vector3 playerPos = floorTilemap.GetCellCenterWorld(new Vector3Int(state.rooms[0].center.x, state.rooms[0].center.y, 0));
        if (playerReference != null)
        {
            playerReference.SetActive(true);
            playerReference.transform.position = playerPos;

            JediController jedi = playerReference.GetComponent<JediController>();
            if (jedi != null) jedi.health = 3;
        }

        // 2. Нахождение дальней комнаты для Портала финиша
        RoomData exitRoom = DijkstraPathfinder.FindExitRoom(state);
        if (exitRoom == null) return;

        Vector3 finishPos = floorTilemap.GetCellCenterWorld(new Vector3Int(exitRoom.center.x, exitRoom.center.y, 0));
        if (config.finishPrefab != null)
        {
            GameObject finish = SpawnObject(state, config.finishPrefab, finishPos);

            // Если это финальный 15-й уровень и пройдена истинная победа (все 3 Ключа Холлидея собраны)
            if (state.currentLevel == 15 && GameManager.Instance != null && GameManager.Instance.IsTrueVictory())
            {
                SpriteRenderer sr = finish.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1, 0.84f, 0); // Золотой флаг
                finish.transform.localScale *= 1.5f;
            }
        }

        // 3. Заселение промежуточных комнат стражей и кристаллами
        for (int i = 1; i < state.rooms.Count; i++)
        {
            RoomData room = state.rooms[i];
            bool isExitRoom = (room.id == exitRoom.id);
            bool hasCrystal = state.prng.NextDouble() < config.crystalSpawnChance;
            int guardCount = 0;

            if (hasCrystal && config.crystalPrefab != null && !isExitRoom)
            {
                Vector3 crystalPos = floorTilemap.GetCellCenterWorld(new Vector3Int(room.center.x, room.center.y, 0));
                SpawnObject(state, config.crystalPrefab, crystalPos);
                guardCount = state.prng.Next(4, 6); // Укрепленная стража кристалла
            }
            else
            {
                guardCount = state.prng.Next(1, 3); // Обычная комнатная стража
            }

            // Спавним штурмовиков в комнате
            for (int j = 0; j < guardCount; j++)
            {
                Vector3 spawnPos = GetRandomFloorInRoom(state, room, floorTilemap);
                GameObject trooperToSpawn = (state.prng.NextDouble() < 0.3f && config.eliteStormtrooperPrefab != null) 
                    ? config.eliteStormtrooperPrefab 
                    : config.stormtrooperPrefab;

                if (trooperToSpawn != null && spawnPos != Vector3.zero)
                {
                    SpawnObject(state, trooperToSpawn, spawnPos);
                    
                    // Инкрементируем счетчик живых врагов в GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.activeEnemyCount++;
                    }
                }
            }
        }

        // 4. Секретный Квест: Дискета Холлидея на 7 уровне
        if (state.currentLevel == 7 && GameManager.Instance != null && !GameManager.Instance.hasCrystalKey && config.diskettePrefab != null)
        {
            int disketteRoomIndex = state.prng.Next(1, state.rooms.Count);
            Vector3 diskettePos = floorTilemap.GetCellCenterWorld(new Vector3Int(state.rooms[disketteRoomIndex].center.x, state.rooms[disketteRoomIndex].center.y, 0));
            SpawnObject(state, config.diskettePrefab, diskettePos);
            Debug.Log("<color=cyan>СЕКРЕТ: Дискета Холлидея появилась в лабиринте на уровне 7!</color>");
        }

        // 5. Спавн патрулей в коридорах
        SpawnCorridorPatrols(state, config, floorTilemap);
    }

    private static void SpawnCorridorPatrols(DungeonState state, DungeonConfig config, Tilemap floorTilemap)
    {
        if (config.stormtrooperPrefab == null) return;

        List<Vector2Int> corridorTiles = new List<Vector2Int>();
        for (int x = 0; x < state.mapWidth; x++)
        {
            for (int y = 0; y < state.mapHeight; y++)
            {
                if (state.grid[x, y] == DungeonTile.Corridor && state.dijkstraMap[x, y] > 0)
                {
                    corridorTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        int patrolCount = Mathf.RoundToInt(corridorTiles.Count * 0.02f);
        for (int i = 0; i < patrolCount; i++)
        {
            if (corridorTiles.Count == 0) break;

            int randomIndex = state.prng.Next(corridorTiles.Count);
            Vector2Int tileCoords = corridorTiles[randomIndex];

            Vector3 spawnPos = floorTilemap.GetCellCenterWorld(new Vector3Int(tileCoords.x, tileCoords.y, 0));
            SpawnObject(state, config.stormtrooperPrefab, spawnPos);
            
            // Инкрементируем счетчик живых патрульных в GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.activeEnemyCount++;
            }
            corridorTiles.RemoveAt(randomIndex); 
        }
    }

    private static Vector3 GetRandomFloorInRoom(DungeonState state, RoomData room, Tilemap floorTilemap)
    {
        int rx = state.prng.Next(room.x + 1, room.x + room.width - 1);
        int ry = state.prng.Next(room.y + 1, room.y + room.height - 1);

        rx = Mathf.Clamp(rx, 2, state.mapWidth - 2);
        ry = Mathf.Clamp(ry, 2, state.mapHeight - 2);

        return floorTilemap.GetCellCenterWorld(new Vector3Int(rx, ry, 0));
    }

    private static GameObject SpawnObject(DungeonState state, GameObject prefab, Vector3 position)
    {
        GameObject obj = Object.Instantiate(prefab, position, Quaternion.identity);
        state.spawnedEntities.Add(obj);
        return obj;
    }
}
