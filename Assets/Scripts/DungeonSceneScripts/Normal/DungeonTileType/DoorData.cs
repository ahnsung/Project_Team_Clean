using System;
using UnityEngine;

[Serializable]
public class DoorData
{
    [SerializeField]
    private int x;

    [SerializeField]
    private int y;

    [SerializeField]
    private MoveDirection direction;

    [SerializeField]
    private MovePathType pathType;

    [SerializeField]
    private bool passable;

    [SerializeField]
    private string needKey;


    // =========================================================
    // Properties
    // =========================================================

    public int X
    {
        get { return x; }
    }

    public int Y
    {
        get { return y; }
    }

    public Vector2Int Position
    {
        get
        {
            return new Vector2Int(
                x,
                y
            );
        }
    }

    public MoveDirection Direction
    {
        get { return direction; }
    }

    public MovePathType PathType
    {
        get { return pathType; }
    }

    public bool Passable
    {
        get { return passable; }
    }

    public string NeedKey
    {
        get { return needKey; }
    }


    public bool RequiresKey
    {
        get
        {
            return
                !string.IsNullOrWhiteSpace(
                    needKey
                );
        }
    }


    public bool IsLockedDoor
    {
        get
        {
            return
                pathType ==
                MovePathType.LockedDoor;
        }
    }


    public bool IsGimmickDoor
    {
        get
        {
            return
                pathType ==
                MovePathType.GimmickDoor;
        }
    }


    // =========================================================
    // Constructor
    // =========================================================

    public DoorData(
        int x,
        int y,
        MoveDirection direction,
        MovePathType pathType,
        bool passable,
        string needKey)
    {
        this.x = x;
        this.y = y;
        this.direction = direction;
        this.pathType = pathType;
        this.passable = passable;
        this.needKey = needKey;
    }


    // =========================================================
    // Debug
    // =========================================================

    public override string ToString()
    {
        string keyText =
            string.IsNullOrWhiteSpace(needKey)
                ? "None"
                : needKey;


        return
            $"({x}, {y}) / " +
            $"{direction} / " +
            $"{pathType} / " +
            $"Passable: {passable} / " +
            $"NeedKey: {keyText}";
    }
}