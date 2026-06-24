using UnityEngine;
using System.Collections.Generic;

public static class DungeonLayoutGenerator
{
    // Расстановка прямоугольных комнат на карте без пересечений
    public static void PlaceRooms(DungeonState state, DungeonConfig config, int targetRoomCount)
    {
        int attempts = 0;
        int maxAttempts = 2000;
        int roomId = 0;

        while (state.rooms.Count < targetRoomCount && attempts < maxAttempts)
        {
            attempts++;

            int w = state.prng.Next(config.minRoomSize, config.maxRoomSize + 1);
            int h = state.prng.Next(config.minRoomSize, config.maxRoomSize + 1);

            // Оставляем рамку в 3 тайла у краев карты для стен
            int x = state.prng.Next(3, state.mapWidth - w - 3);
            int y = state.prng.Next(3, state.mapHeight - h - 3);

            RoomData newRoom = new RoomData(roomId, x, y, w, h);

            bool overlaps = false;
            foreach (var room in state.rooms)
            {
                if (newRoom.Overlaps(room, config.roomPadding))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                state.rooms.Add(newRoom);
                CarveRoom(state, newRoom);
                roomId++;
            }
        }
    }

    private static void CarveRoom(DungeonState state, RoomData room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                state.grid[x, y] = DungeonTile.Floor;
            }
        }
    }

    // Соединение комнат коридорами по алгоритму Прима (MST) с добавлением петель
    public static void ConnectRooms(DungeonState state, DungeonConfig config)
    {
        if (state.rooms.Count < 2) return;

        // Находим все потенциальные связи между комнатами
        List<DungeonEdge> allPossibleEdges = new List<DungeonEdge>();
        for (int i = 0; i < state.rooms.Count; i++)
        {
            for (int j = i + 1; j < state.rooms.Count; j++)
            {
                float dist = Vector2.Distance(state.rooms[i].center, state.rooms[j].center);
                allPossibleEdges.Add(new DungeonEdge(i, j, dist));
            }
        }

        List<DungeonEdge> activeEdges = new List<DungeonEdge>();
        HashSet<int> reached = new HashSet<int>() { 0 };
        HashSet<int> unreached = new HashSet<int>();
        for (int i = 1; i < state.rooms.Count; i++) unreached.Add(i);

        // Алгоритм Прима для гарантированного связывания без разрывов
        while (unreached.Count > 0)
        {
            DungeonEdge bestEdge = default;
            float minDist = float.MaxValue;
            bool found = false;

            foreach (var edge in allPossibleEdges)
            {
                bool hasA = reached.Contains(edge.roomA);
                bool hasB = reached.Contains(edge.roomB);

                if ((hasA && !hasB) || (!hasA && hasB))
                {
                    if (edge.distance < minDist)
                    {
                        minDist = edge.distance;
                        bestEdge = edge;
                        found = true;
                    }
                }
            }

            if (found)
            {
                activeEdges.Add(bestEdge);
                reached.Add(bestEdge.roomA);
                reached.Add(bestEdge.roomB);
                unreached.Remove(bestEdge.roomA);
                unreached.Remove(bestEdge.roomB);
            }
            else
            {
                break;
            }
        }

        // Выделяем неиспользованные в остове ребра
        List<DungeonEdge> unusedEdges = new List<DungeonEdge>();
        foreach (var edge in allPossibleEdges)
        {
            if (!activeEdges.Contains(edge))
            {
                unusedEdges.Add(edge);
            }
        }

        // Перемешиваем неиспользованные связи PRNG
        for (int i = unusedEdges.Count - 1; i > 0; i--)
        {
            int k = state.prng.Next(i + 1);
            var temp = unusedEdges[i];
            unusedEdges[i] = unusedEdges[k];
            unusedEdges[k] = temp;
        }

        // Добавляем обратно 12% неиспользованных ребер для создания нелинейных петель
        int loopsToAdd = Mathf.Clamp(Mathf.RoundToInt(unusedEdges.Count * 0.12f), 1, unusedEdges.Count);
        for (int i = 0; i < Mathf.Min(loopsToAdd, unusedEdges.Count); i++)
        {
            activeEdges.Add(unusedEdges[i]);
        }

        // Высекаем L-коридоры вдоль выбранных граней графа
        foreach (var edge in activeEdges)
        {
            CarveLCorridor(state, state.rooms[edge.roomA].center, state.rooms[edge.roomB].center, config.corridorWidth);
        }
    }

    private static void CarveLCorridor(DungeonState state, Vector2Int start, Vector2Int end, int corridorWidth)
    {
        if (state.prng.NextDouble() < 0.5f)
        {
            CarveHorizontalCorridor(state, start.x, end.x, start.y, corridorWidth);
            CarveVerticalCorridor(state, start.y, end.y, end.x, corridorWidth);
        }
        else
        {
            CarveVerticalCorridor(state, start.y, end.y, start.x, corridorWidth);
            CarveHorizontalCorridor(state, start.x, end.x, end.y, corridorWidth);
        }
    }

    private static void CarveHorizontalCorridor(DungeonState state, int xStart, int xEnd, int y, int corridorWidth)
    {
        int startX = Mathf.Min(xStart, xEnd);
        int endX = Mathf.Max(xStart, xEnd);

        int halfWidth = corridorWidth / 2;
        int startY = y - halfWidth;
        int endY = y + (corridorWidth - 1 - halfWidth);

        for (int x = startX; x <= endX; x++)
        {
            for (int cy = startY; cy <= endY; cy++)
            {
                if (x >= 2 && x < state.mapWidth - 2 && cy >= 2 && cy < state.mapHeight - 2)
                {
                    if (state.grid[x, cy] == DungeonTile.Empty)
                    {
                        state.grid[x, cy] = DungeonTile.Corridor;
                    }
                }
            }
        }
    }

    private static void CarveVerticalCorridor(DungeonState state, int yStart, int yEnd, int x, int corridorWidth)
    {
        int startY = Mathf.Min(yStart, yEnd);
        int endY = Mathf.Max(yStart, yEnd);

        int halfWidth = corridorWidth / 2;
        int startX = x - halfWidth;
        int endX = x + (corridorWidth - 1 - halfWidth);

        for (int y = startY; y <= endY; y++)
        {
            for (int cx = startX; cx <= endX; cx++)
            {
                if (cx >= 2 && cx < state.mapWidth - 2 && y >= 2 && y < state.mapHeight - 2)
                {
                    if (state.grid[cx, y] == DungeonTile.Empty)
                    {
                        state.grid[cx, y] = DungeonTile.Corridor;
                    }
                }
            }
        }
    }

    // ИСПРАВЛЕННЫЙ МЕТОД: Заполняет абсолютно всю пустоту (Empty) сплошными блоками стен (Wall)
    public static void OutlineWalls(DungeonState state)
    {
        for (int x = 0; x < state.mapWidth; x++)
        {
            for (int y = 0; y < state.mapHeight; y++)
            {
                if (state.grid[x, y] == DungeonTile.Empty)
                {
                    state.grid[x, y] = DungeonTile.Wall;
                }
            }
        }
    }
}
