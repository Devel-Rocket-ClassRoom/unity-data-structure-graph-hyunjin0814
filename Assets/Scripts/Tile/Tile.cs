
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

    // 안개 타일 ID 계산
    public void UpdateFowTileId(Map map)
    {
        fowTilleId = 0;
        int r = id / map.cols;
        int c = id % map.cols;

        if (IsFog(map.GetTileAt(r - 1, c))) fowTilleId |= (1 << 0); // Top
        if (IsFog(map.GetTileAt(r, c - 1))) fowTilleId |= (1 << 1); // Left
        if (IsFog(map.GetTileAt(r, c + 1))) fowTilleId |= (1 << 2); // Right
        if (IsFog(map.GetTileAt(r + 1, c))) fowTilleId |= (1 << 3); // Bottom
    }

    // 안개 조건: 타일이 null, 방문하지 않은 타일
    private bool IsFog(Tile target)
    {
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
