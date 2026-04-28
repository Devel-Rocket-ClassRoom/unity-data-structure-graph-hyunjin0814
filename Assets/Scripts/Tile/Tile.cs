using UnityEngine;

// 0000
// 0001
// 0010
// 0011

// 1000

public enum Sides
{
    None = -1,
    Top, // 0
    Left,  // 1
    Right,   // 2
    Bottom,  // 3
}

public class Tile 
{
    public int id;
    public Tile[] adjacents = new Tile[4];
    public int autoTileId;
    public int fowTilleId;

    public bool isVisited = false;

    public bool CanMove => autoTileId != (int)TileTypes.Empty;

    public void UpdateAutoTileId()
    {
        autoTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] != null)
            {
                autoTileId |= (1 << i);
            }
        }
    }   

    public void UpdateFowTileId(Map map)
    {
        fowTilleId = 0;
        int r = id / map.cols;
        int c = id % map.cols;

        // Sides 순서에 맞춰 비트 계산 (Top, Left, Right, Bottom)
        // 맵 밖이거나(null), 방문하지 않았으면 안개 비트를 켭니다.
        if (IsFog(map.GetTileAt(r - 1, c))) fowTilleId |= (1 << 0); // Top
        if (IsFog(map.GetTileAt(r, c - 1))) fowTilleId |= (1 << 1); // Left
        if (IsFog(map.GetTileAt(r, c + 1))) fowTilleId |= (1 << 2); // Right
        if (IsFog(map.GetTileAt(r + 1, c))) fowTilleId |= (1 << 3); // Bottom

        //fowTilleId = 0;
        //for (int i = 0; i < adjacents.Length; i++)
        //{
        //    if (adjacents[i] == null || !adjacents[i].isVisited)
        //    {
        //        fowTilleId |= (1 << i);
        //    }
        //}
    }

    private bool IsFog(Tile target)
    {
        // 타일이 없거나(맵 끝), 방문하지 않았다면 안개(true)입니다.
        return target == null || !target.isVisited;
    }

    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null)
                continue;

            if (adjacents[i].id == tile.id)
            {
                adjacents[i] = null;
                UpdateAutoTileId();
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null)
                continue;

            adjacents[i].RemoveAdjacents(this);
            adjacents[i] = null;
        }
        UpdateAutoTileId();
    }
}
