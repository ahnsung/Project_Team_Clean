using System;
using UnityEngine;

public enum MovePathType
{
    Open,
    Wall,
    Door,
    OneWay,
    LockedDoor,
    GimmickDoor
}

[Serializable]
public class MoveData
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


    public MoveData(
        int x,
        int y,
        MoveDirection direction,
        MovePathType pathType,
        bool passable)
    {
        this.x = x;
        this.y = y;
        this.direction = direction;
        this.pathType = pathType;
        this.passable = passable;
    }


    public override string ToString()
    {
        return
            $"({x}, {y}) / " +
            $"{direction} / " +
            $"{pathType} / " +
            $"Passable: {passable}";
    }
}