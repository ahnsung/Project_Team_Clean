using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonMapData
{
    [SerializeField]
    private List<DungeonTileData> tiles = new List<DungeonTileData>();

    public IReadOnlyList<DungeonTileData> Tiles => tiles;

    public DungeonMapData()
    {
        tiles = new List<DungeonTileData>();
    }

    public DungeonMapData(List<DungeonTileData> tiles)
    {
        this.tiles = tiles ?? new List<DungeonTileData>();
    }

    public void AddTile(DungeonTileData tile)
    {
        if (tile == null)
            return;

        tiles.Add(tile);
    }

    public void Clear()
    {
        tiles.Clear();
    }
}