using UnityEngine;
using System.Collections.Generic;

public static class DijkstraPathfinder
{
    // Запуск волнового алгоритма BFS от стартовой комнаты
    public static void GenerateDijkstraMap(DungeonState state)
    {
        for (int x = 0; x < state.mapWidth; x++)
        {
            for (int y = 0; y < state.mapHeight; y++)
            {
                state.dijkstraMap[x, y] = -1; // -1 означает "не посещено"
            }
        }

        if (state.rooms.Count == 0) return;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int startPoint = state.rooms[0].center;

        state.dijkstraMap[startPoint.x, startPoint.y] = 0;
        queue.Enqueue(startPoint);

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            int currentDist = state.dijkstraMap[curr.x, curr.y];

            Vector2Int[] neighbors = {
                new Vector2Int(curr.x + 1, curr.y),
                new Vector2Int(curr.x - 1, curr.y),
                new Vector2Int(curr.x, curr.y + 1),
                new Vector2Int(curr.x, curr.y - 1)
            };

            foreach (var n in neighbors)
            {
                if (n.x >= 0 && n.x < state.mapWidth && n.y >= 0 && n.y < state.mapHeight)
                {
                    // Прокладываем путь только по полу и коридорам
                    if (state.dijkstraMap[n.x, n.y] == -1 && 
                        (state.grid[n.x, n.y] == DungeonTile.Floor || state.grid[n.x, n.y] == DungeonTile.Corridor))
                    {
                        state.dijkstraMap[n.x, n.y] = currentDist + 1;
                        queue.Enqueue(n);
                    }
                }
            }
        }
    }

    // Поиск комнаты с максимальным топологическим удалением от старта
    public static RoomData FindExitRoom(DungeonState state)
    {
        if (state.rooms.Count == 0) return null;

        int maxDistValue = 0;
        RoomData exitRoom = state.rooms[0];

        foreach (var room in state.rooms)
        {
            int dist = state.dijkstraMap[room.center.x, room.center.y];
            if (dist > maxDistValue)
            {
                maxDistValue = dist;
                exitRoom = room;
            }
        }

        return exitRoom;
    }

    // Трассировка критического пути назад и выбор оптимальной точки для Силовых ворот
    public static bool FindGateCoordinate(DungeonState state, RoomData exitRoom, out Vector2Int gateCoord, out int gateDistance)
    {
        gateCoord = Vector2Int.zero;
        gateDistance = -1;

        if (exitRoom == null || state.rooms.Count == 0) return false;

        int maxDistValue = state.dijkstraMap[exitRoom.center.x, exitRoom.center.y];
        if (maxDistValue <= 10) return false; // Слишком короткий путь для безопасного барьера

        Vector2Int currentTrace = exitRoom.center;
        List<Vector2Int> criticalPath = new List<Vector2Int>();

        // Воспроизводим критический путь по шагам в обратном направлении
        while (currentTrace != state.rooms[0].center)
        {
            criticalPath.Add(currentTrace);
            int currentDist = state.dijkstraMap[currentTrace.x, currentTrace.y];

            Vector2Int[] neighbors = {
                new Vector2Int(currentTrace.x + 1, currentTrace.y),
                new Vector2Int(currentTrace.x - 1, currentTrace.y),
                new Vector2Int(currentTrace.x, currentTrace.y + 1),
                new Vector2Int(currentTrace.x, currentTrace.y - 1)
            };

            Vector2Int nextStep = currentTrace;
            foreach (var n in neighbors)
            {
                if (n.x >= 0 && n.x < state.mapWidth && n.y >= 0 && n.y < state.mapHeight)
                {
                    if (state.dijkstraMap[n.x, n.y] == currentDist - 1)
                    {
                        nextStep = n;
                        break;
                    }
                }
            }

            if (nextStep == currentTrace) break; // Защита от бесконечного цикла
            currentTrace = nextStep;
        }

        // Ищем оптимальное горлышко в коридоре (TILE_CORRIDOR), ближе к середине пути
        int midIndex = criticalPath.Count / 2;
        for (int i = midIndex; i < criticalPath.Count - 2; i++)
        {
            Vector2Int pathPoint = criticalPath[i];
            if (state.grid[pathPoint.x, pathPoint.y] == DungeonTile.Corridor)
            {
                gateCoord = pathPoint;
                gateDistance = state.dijkstraMap[pathPoint.x, pathPoint.y];
                return true;
            }
        }

        return false;
    }
}