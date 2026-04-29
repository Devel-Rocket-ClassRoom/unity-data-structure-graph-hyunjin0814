using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefabs;
    private GameObject[] tileObjs;

    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    [Range(0f, 0.9f)]
    public float lakePercent = 0.1f;
    [Range(0f, 0.9f)]
    public float treePercent = 0.1f;
    [Range(0f, 0.9f)]
    public float hillPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float mountainPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float townPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float monsterPercent = 0.1f;

    public int erodeIterations = 2;

    public Vector2 tileSize = new Vector2(16, 16);

    public Sprite[] islandSprites;

    public Sprite[] fowSprites;

    private Map map;

    public Map Map => map;

    public PlayerMovement playerPrefab;
    private PlayerMovement player;

    private int prevTileId = -1;
    public int PrevTileId => prevTileId;

    public int FowRange = 3;

    public Vector3 FirstTilePos 
    {
        get
        {
            var pos = transform.position;
            pos.x -= (mapWidth * tileSize.x * 0.5f);
            pos.y += (mapHeight * tileSize.y * 0.5f);
            pos.x += tileSize.x * 0.5f;
            pos.y -= tileSize.y * 0.5f;
            return pos;
        }
    }

    TileSearch tileSearch = new TileSearch();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetStage();
        }

        if (tileObjs != null)
        {
            int currentTileId = ScreenPosToTileId(Input.mousePosition);
            if (prevTileId != currentTileId)
            {
                tileObjs[currentTileId].GetComponent<SpriteRenderer>().color = Color.green;
                if (prevTileId >= 0 && prevTileId < tileObjs.Length)
                {
                    tileObjs[prevTileId].GetComponent<SpriteRenderer>().color = Color.white;
                }
                prevTileId = currentTileId;
            }
        }

        // 마우스 클릭 시 타일 아이디만 넘겨서 이동 자체는 플레이어에서 처리하도록 변경 해야함
        if (Input.GetMouseButtonDown(0))
        {
            if (player == null)
                return;

            int currentTileId = ScreenPosToTileId(Input.mousePosition);

            if (tileSearch.AStar(map.tiles[player.CurrentTileId], map.tiles[currentTileId]))
            {
                player.FollowPath(new List<Tile>(tileSearch.path));
            }
        }
    }

    private void ResetStage()
    {
        while (true)
        {
            map = new Map();
            map.Init(mapHeight, mapWidth);
            tileSearch.Init(Map);
            map.CreateIsland(erodePercent, erodeIterations, lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent);
            CreateGrid();

            if (tileSearch.AStar(map.startTile, map.castleTile))
            {
                //stagePath = tileSearch.path;
                break;
            }
        }

        CreatePlayer();
        OnTileVisited(map.tiles[map.startTile.id]);
        //GoPath();
        //UpdateVisibility(map.startTile.id);
    }

    //private void GoPath()
    //{
    //    player.FollowPath(stagePath);
    //}

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        player = Instantiate(playerPrefab);
        player.SetPosition(map.startTile.id);
    }

    private void CreateGrid()
    {
        if (tileObjs != null)
        {
            foreach (var tile in tileObjs)
            {
                Destroy(tile.gameObject);
            }
        }

        tileObjs = new GameObject[mapWidth * mapHeight];

        var position = FirstTilePos;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                int tileId = i * mapWidth + j;
                var newGo = Instantiate(tilePrefabs, transform);
                newGo.transform.position = position; 
                position.x += tileSize.x;

                tileObjs[tileId] = newGo;
                DecorateTile(tileId);
            }
            position.x = FirstTilePos.x;
            position.y -= tileSize.y;
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var tileGo = tileObjs[tileId];
        var ren = tileGo.GetComponent<SpriteRenderer>();

        if (tile.isVisited)
        {
            if (tile.autoTileId != (int)TileTypes.Empty)
            {
                ren.sprite = islandSprites[tile.autoTileId];
            }
            else
            {
                ren.sprite = null;
            }
        }
        else
        {
            ren.sprite = fowSprites[tile.fowTilleId];
        }
    }


    //public void DecorateTile(int tileId)
    //{
    //    var tile = map.tiles[tileId];
    //    var tileGo = tileObjs[tileId];
    //    var ren = tileGo.GetComponent<SpriteRenderer>();

    //    // 방문한 타일은 기존 로직
    //    if (tile.isVisited)
    //    {
    //        if (tile.autoTileId != (int)TileTypes.Empty)
    //        {
    //            ren.sprite = islandSprites[tile.autoTileId];
    //        }
    //        else
    //        {
    //            ren.sprite = null;
    //        }
    //    }
    //    // 방문하지 않은 타일은 FOW 타일로 업데이트
    //    else
    //    {
    //        tile.UpdateFowTileId(map);

    //        if (fowSprites != null && tile.fowTilleId < fowSprites.Length)
    //        {
    //            ren.sprite = fowSprites[tile.fowTilleId];
    //        }
    //    }
    //}

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - Camera.main.transform.position.z);

        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return WorldPosToTileId(worldPos);
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        var first = FirstTilePos;

        int x = Mathf.FloorToInt((worldPos.x - first.x) / tileSize.x + 0.5f);
        int y = Mathf.FloorToInt((first.y - worldPos.y) / tileSize.y + 0.5f);

        x = Mathf.Clamp(x, 0, mapWidth - 1);
        y = Mathf.Clamp(y, 0, mapHeight - 1);

        return y * mapWidth + x;
    }

    public Vector3 GetTilePos(int y, int x)
    {
        return FirstTilePos + new Vector3(x * tileSize.x, -y * tileSize.y, 0);
    }
  

    public Vector3 GetTilePos(int tileId)
    {
        int y = tileId / mapWidth;
        int x = tileId % mapWidth;

        return GetTilePos(y, x);
    }

    public void OnTileVisited(int tileId)
    {
        OnTileVisited(map.tiles[tileId]);
    }

    public void OnTileVisited(Tile tile)
    {
        int centerX = tile.id % mapWidth;
        int centerY = tile.id / mapWidth;

        for (int i = -FowRange; i <= FowRange; i++)
        {
            for (int j = -FowRange; j <= FowRange; j++)
            {
                int x = centerX + j;
                int y = centerY + i;
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    continue;

                int id = y * mapWidth + x;
                map.tiles[id].isVisited = true;
                DecorateTile(id);
            }
        }

        int range = FowRange + 1;

        for (int i = -range; i <= range; i++)
        {
            for (int j = -range; j <= range; j++)
            {
                if (i == range || i == -range || j == range || j == -range)
                {
                    int x = centerX + j;
                    int y = centerY + i;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        continue;

                    int id = y * mapWidth + x;
                    map.tiles[id].UpdateFowTileId();
                    DecorateTile(id);
                }
            }
        }
    }

    //public void UpdateVisibility(int centerTileId)
    //{
    //    int centerY = centerTileId / mapWidth;
    //    int centerX = centerTileId % mapWidth;

    //    // 중심 타일을 기준으로 FOW 범위 내의 타일을 방문 처리
    //    for (int y = centerY - FowRange; y <= centerY + FowRange; y++)
    //    {
    //        for (int x = centerX - FowRange; x <= centerX + FowRange; x++)
    //        {
    //            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
    //                continue;

    //            int tileId = y * mapWidth + x;
    //            map.tiles[tileId].isVisited = true;
    //        }
    //    }

    //    // FOW 범위 + 경계 타일까지 DecorateTile() 호출해서 안개 표현
    //    int visualRange = FowRange + 1;
    //    for (int y = centerY - visualRange; y <= centerY + visualRange; y++)
    //    {
    //        for (int x = centerX - visualRange; x <= centerX + visualRange; x++)
    //        {
    //            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
    //                continue;

    //            int tileId = y * mapWidth + x;
    //            DecorateTile(tileId);
    //        }
    //    }
    //}
}
