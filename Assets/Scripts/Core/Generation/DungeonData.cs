using UnityEngine;
using System.Collections.Generic;

// Перечисление типов тайлов подземелья
public enum DungeonTile
{
    Empty = 0,
    Floor = 1,
    Corridor = 2,
    Wall = 3,
    LockedGate = 4
}

// Данные прямоугольной комнаты
public class RoomData
{
    public int id;
    public int x;
    public int y;
    public int width;
    public int height;

    public Vector2Int center => new Vector2Int(x + width / 2, y + height / 2);

    public RoomData(int id, int x, int y, int width, int height)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    // Проверка наложения комнат
    public bool Overlaps(RoomData other, int padding)
    {
        return (x - padding < other.x + other.width &&
                x + width + padding > other.x &&
                y - padding < other.y + other.height &&
                y + height + padding > other.y);
    }
}

// Ребро графа для связи комнат
public struct DungeonEdge
{
    public int roomA;
    public int roomB;
    public float distance;

    public DungeonEdge(int a, int b, float dist)
    {
        roomA = a;
        roomB = b;
        distance = dist;
    }
}

// Контекст состояния генерируемого уровня (передается по цепочке)
public class DungeonState
{
    public int mapWidth;
    public int mapHeight;
    public int currentLevel;
    public DungeonTile[,] grid;
    public int[,] dijkstraMap;
    public List<RoomData> rooms = new List<RoomData>();
    public List<GameObject> spawnedEntities = new List<GameObject>();
    public System.Random prng;

    public DungeonState(int width, int height, int level, System.Random random)
    {
        mapWidth = width;
        mapHeight = height;
        currentLevel = level;
        prng = random;
        grid = new DungeonTile[width, height];
        dijkstraMap = new int[width, height];
    }
}
