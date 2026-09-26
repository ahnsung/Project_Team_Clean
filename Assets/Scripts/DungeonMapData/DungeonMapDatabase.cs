using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DungeonMapDatabase : MonoBehaviour
{
    public static DungeonMapDatabase Instance
    {
        get;
        private set;
    }

    private readonly Dictionary<
        Vector2Int,
        DungeonTileData
    > tileLookup =
        new Dictionary<
            Vector2Int,
            DungeonTileData
        >();

    private DungeonMapData mapData =
        new DungeonMapData();

    public DungeonMapData MapData =>
        mapData;

    public bool IsLoaded =>
        mapData != null &&
        mapData.Tiles != null &&
        mapData.Tiles.Count > 0;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// 새로운 맵 데이터를 Database에 등록한다.
    /// </summary>
    public void SetMapData(
        DungeonMapData newMapData)
    {
        if (newMapData == null)
        {
            Debug.LogError(
                "[DungeonMapDatabase] MapData가 null입니다."
            );

            return;
        }

        mapData = newMapData;

        RebuildLookup();
    }

    /// <summary>
    /// 좌표 검색용 Dictionary를 다시 만든다.
    /// </summary>
    private void RebuildLookup()
    {
        tileLookup.Clear();

        if (mapData == null ||
            mapData.Tiles == null)
        {
            Debug.LogError(
                "[DungeonMapDatabase] 등록할 Tile 데이터가 없습니다."
            );

            return;
        }

        foreach (
            DungeonTileData tile
            in mapData.Tiles)
        {
            if (tile == null)
                continue;

            Vector2Int position =
                tile.Position;

            if (tileLookup.ContainsKey(
                position))
            {
                Debug.LogWarning(
                    "[DungeonMapDatabase] " +
                    "중복 좌표 발견: " +
                    $"({position.x}, {position.y})"
                );

                continue;
            }

            tileLookup.Add(
                position,
                tile
            );
        }

        Debug.Log(
            "[DungeonMapDatabase] " +
            "맵 데이터 등록 완료. 총 " +
            $"{tileLookup.Count}개 좌표"
        );
    }

    /// <summary>
    /// 해당 좌표에 데이터가 존재하는지 확인.
    /// None 타일도 데이터 자체가 존재하면 true.
    /// </summary>
    public bool HasTileData(
        Vector2Int position)
    {
        return tileLookup.ContainsKey(
            position
        );
    }

    public bool HasTileData(
        int x,
        int y)
    {
        return HasTileData(
            new Vector2Int(x, y)
        );
    }

    /// <summary>
    /// 실제 이동 가능한 맵 타일인지 확인.
    /// 좌표가 없거나 None이면 false.
    /// </summary>
    public bool IsValidTile(
        Vector2Int position)
    {
        if (!tileLookup.TryGetValue(
            position,
            out DungeonTileData tile))
        {
            return false;
        }

        return tile.IsValidTile();
    }

    public bool IsValidTile(
        int x,
        int y)
    {
        return IsValidTile(
            new Vector2Int(x, y)
        );
    }

    /// <summary>
    /// 해당 좌표의 타일 데이터를 가져온다.
    /// 없으면 null.
    /// </summary>
    public DungeonTileData GetTile(
        Vector2Int position)
    {
        if (tileLookup.TryGetValue(
            position,
            out DungeonTileData tile))
        {
            return tile;
        }

        return null;
    }

    public DungeonTileData GetTile(
        int x,
        int y)
    {
        return GetTile(
            new Vector2Int(x, y)
        );
    }

    /// <summary>
    /// 해당 좌표의 TileType 반환.
    /// 데이터가 없으면 None.
    /// </summary>
    public DungeonTileType GetTileType(
        Vector2Int position)
    {
        DungeonTileData tile =
            GetTile(position);

        if (tile == null)
        {
            return DungeonTileType.None;
        }

        return tile.TileType;
    }

    public DungeonTileType GetTileType(
        int x,
        int y)
    {
        return GetTileType(
            new Vector2Int(x, y)
        );
    }
}