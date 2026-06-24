using UnityEngine;
using UnityEngine.Tilemaps;

public static class DungeonVisualizer
{
    // Отрисовка сгенерированной сетки DungeonTile на компонентах Tilemap Unity
    public static void DrawTilemap(DungeonState state, Tilemap floorTilemap, Tilemap wallTilemap, TileBase floorTile, TileBase wallTile)
    {
        if (floorTilemap == null || wallTilemap == null || floorTile == null || wallTile == null) return;

        // Полностью очищаем слои перед новым рендерингом
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < state.mapWidth; x++)
        {
            for (int y = 0; y < state.mapHeight; y++)
            {
                if (state.grid[x, y] == DungeonTile.Wall)
                {
                    wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
                else if (state.grid[x, y] == DungeonTile.Floor || state.grid[x, y] == DungeonTile.Corridor)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }
}
