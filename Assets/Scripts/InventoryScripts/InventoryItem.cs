using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string uniqueId;

    public ItemData data;

    public int remainUseCount;

    public Vector2Int position;

    [Range(0, 3)]
    public int rotation;

    [Header("Equipment Runtime Data")]
    public int currentDurability;

    public InventoryItem(ItemData data)
    {
        uniqueId = System.Guid.NewGuid().ToString();

        this.data = data;

        remainUseCount =
            data != null
                ? Mathf.Max(0, data.maxUseCount)
                : 0;

        position = Vector2Int.zero;
        rotation = 0;

        currentDurability =
            data != null && data.IsEquipment
                ? data.SafeMaxDurability
                : 0;
    }

    public bool IsEquipment
    {
        get
        {
            return data != null &&
                   data.IsEquipment;
        }
    }

    public bool IsBroken
    {
        get
        {
            return IsEquipment &&
                   currentDurability <= 0;
        }
    }

    public void ReduceDurability(int amount)
    {
        if (!IsEquipment)
            return;

        if (amount <= 0)
            return;

        currentDurability =
            Mathf.Max(
                0,
                currentDurability - amount
            );
    }

    public List<Vector2Int> GetOccupiedCells(
        Vector2Int basePosition)
    {
        List<Vector2Int> result =
            new List<Vector2Int>();

        foreach (Vector2Int offset in GetRotatedShape())
        {
            result.Add(basePosition + offset);
        }

        return result;
    }

    public List<Vector2Int> GetRotatedShape()
    {
        List<Vector2Int> rotated =
            new List<Vector2Int>();

        if (data == null)
        {
            rotated.Add(Vector2Int.zero);
            return rotated;
        }

        data.EnsureValidShape();

        foreach (Vector2Int cell in data.shape)
        {
            Vector2Int point = cell;

            for (int i = 0; i < rotation; i++)
            {
                point =
                    new Vector2Int(
                        point.y,
                        -point.x
                    );
            }

            rotated.Add(point);
        }

        return NormalizeShape(rotated);
    }

    public void RotateClockwise()
    {
        rotation = (rotation + 1) % 4;
    }

    public void SetRotation(int value)
    {
        rotation =
            ((value % 4) + 4) % 4;
    }

    private List<Vector2Int> NormalizeShape(
        List<Vector2Int> source)
    {
        if (source == null ||
            source.Count == 0)
        {
            return new List<Vector2Int>
            {
                Vector2Int.zero
            };
        }

        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (Vector2Int cell in source)
        {
            minX = Mathf.Min(minX, cell.x);
            minY = Mathf.Min(minY, cell.y);
        }

        List<Vector2Int> result =
            new List<Vector2Int>();

        foreach (Vector2Int cell in source)
        {
            result.Add(
                new Vector2Int(
                    cell.x - minX,
                    cell.y - minY
                )
            );
        }

        return result;
    }

    public Vector2Int GetShapeSize()
    {
        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int cell in GetRotatedShape())
        {
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }

        return new Vector2Int(
            maxX + 1,
            maxY + 1
        );
    }
}