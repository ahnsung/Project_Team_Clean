using System;
using System.Collections.Generic;

[Serializable]
public class ChestItemData
{
    public int itemID;
    public int amount;

    public ChestItemData(int itemID, int amount)
    {
        this.itemID = itemID;
        this.amount = amount;
    }
}

[Serializable]
public class ChestTileData
{
    public int x;
    public int y;

    public List<ChestItemData> items =
        new List<ChestItemData>();
}