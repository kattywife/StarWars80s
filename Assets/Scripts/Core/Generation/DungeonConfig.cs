using UnityEngine;

[CreateAssetMenu(fileName = "DungeonConfig", menuName = "Dungeon/Config", order = 1)]
public class DungeonConfig : ScriptableObject
{
    [Header("Настройки Геометрии подземелья")]
    public int minRoomSize = 6;                 // Минимальная ширина/высота комнаты
    public int maxRoomSize = 12;                // Максимальная ширина/высота комнаты
    [Range(1, 4)] public int corridorWidth = 2; // Рекомендуется 2 или 3 для маневрирования
    public int roomPadding = 2;                 // Минимальное расстояние между стенами комнат

    [Header("Вероятности и баланс")]
    [Range(0f, 1f)] public float crystalSpawnChance = 0.3f;
    [Range(0f, 100f)] public float wormSpawnChance = 30f;
    public float wormSpawnDelay = 8f;
    public int wormUnlockLevel = 5;

    [Header("Префабы Персонажей")]
    public GameObject playerPrefab;
    public GameObject stormtrooperPrefab;
    public GameObject eliteStormtrooperPrefab;
    public GameObject wormPrefab;

    [Header("Префабы Предметов")]
    public GameObject finishPrefab;
    public GameObject crystalPrefab;
    public GameObject diskettePrefab;
}
