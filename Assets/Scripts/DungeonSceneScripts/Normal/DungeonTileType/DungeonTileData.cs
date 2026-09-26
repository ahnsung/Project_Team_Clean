using System;
using UnityEngine;

[Serializable]
public class DungeonTileData
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private DungeonTileType tileType;

    public int X => x;
    public int Y => y;
    public DungeonTileType TileType => tileType;

    public Vector2Int Position => new Vector2Int(x, y);

    public DungeonTileData(
        int x,
        int y,
        DungeonTileType tileType)
    {
        this.x = x;
        this.y = y;
        this.tileType = tileType;
    }

    public bool IsValidTile()
    {
        return tileType != DungeonTileType.None;
    }
}