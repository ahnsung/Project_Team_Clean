using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-840)]
public class DoorDataLoader : MonoBehaviour
{
    public static DoorDataLoader Instance
    {
        get;
        private set;
    }


    [Header("CSV")]
    [SerializeField]
    private TextAsset doorDataCsv;


    private readonly Dictionary<
        Vector2Int,
        Dictionary<MoveDirection, DoorData>
    > doorDataByPosition
        =
        new Dictionary<
            Vector2Int,
            Dictionary<MoveDirection, DoorData>
        >();


    public int Count
    {
        get;
        private set;
    }


    public bool IsLoaded
    {
        get;
        private set;
    }


    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        LoadDoorData();
    }


    public void LoadDoorData()
    {
        doorDataByPosition.Clear();

        Count = 0;
        IsLoaded = false;


        if (doorDataCsv == null)
        {
            Debug.LogError(
                "[DoorDataLoader] Door Tiles CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            doorDataCsv.text;


        if (string.IsNullOrWhiteSpace(csvText))
        {
            Debug.LogError(
                "[DoorDataLoader] Door Tiles CSV 내용이 비어 있습니다."
            );

            return;
        }


        string[] lines =
            csvText.Split(
                new[]
                {
                    "\r\n",
                    "\n",
                    "\r"
                },
                StringSplitOptions.None
            );


        int successCount = 0;
        int skippedCount = 0;
        int errorCount = 0;

        int lockedDoorCount = 0;
        int gimmickDoorCount = 0;


        for (
            int i = 1;
            i < lines.Length;
            i++
        )
        {
            string line =
                lines[i].Trim();


            if (string.IsNullOrWhiteSpace(line))
            {
                skippedCount++;
                continue;
            }


            string[] columns =
                line.Split(',');


            if (columns.Length < 6)
            {
                errorCount++;
                continue;
            }


            string xText =
                CleanValue(columns[0]);

            string yText =
                CleanValue(columns[1]);

            string directionText =
                CleanValue(columns[2]);

            string typeText =
                CleanValue(columns[3]);

            string passableText =
                CleanValue(columns[4]);

            string needKeyText =
                CleanValue(columns[5]);


            if (
                !int.TryParse(
                    xText,
                    out int x
                )
            )
            {
                skippedCount++;
                continue;
            }


            if (
                !int.TryParse(
                    yText,
                    out int y
                )
            )
            {
                errorCount++;
                continue;
            }


            if (
                !TryParseDirection(
                    directionText,
                    out MoveDirection direction
                )
            )
            {
                errorCount++;
                continue;
            }


            if (
                !TryParseDoorType(
                    typeText,
                    out MovePathType pathType
                )
            )
            {
                errorCount++;
                continue;
            }


            if (
                !TryParsePassable(
                    passableText,
                    out bool passable
                )
            )
            {
                errorCount++;
                continue;
            }


            if (
                string.Equals(
                    needKeyText,
                    "nan",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    needKeyText,
                    "null",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                needKeyText =
                    string.Empty;
            }


            DoorData doorData =
                new DoorData(
                    x,
                    y,
                    direction,
                    pathType,
                    passable,
                    needKeyText
                );


            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            if (
                !doorDataByPosition.TryGetValue(
                    position,
                    out Dictionary<
                        MoveDirection,
                        DoorData
                    > directionMap
                )
            )
            {
                directionMap =
                    new Dictionary<
                        MoveDirection,
                        DoorData
                    >();


                doorDataByPosition.Add(
                    position,
                    directionMap
                );
            }


            directionMap[direction] =
                doorData;


            if (
                pathType ==
                MovePathType.LockedDoor
            )
            {
                lockedDoorCount++;
            }
            else if (
                pathType ==
                MovePathType.GimmickDoor
            )
            {
                gimmickDoorCount++;
            }


            successCount++;
        }


        Count = successCount;

        IsLoaded =
            successCount > 0;


        Debug.Log(
            "[DoorDataLoader] " +
            $"Door Tiles 로드 완료: {successCount}개\n" +
            $"LockedDoor 방향 데이터: {lockedDoorCount}개\n" +
            $"GimmickDoor 방향 데이터: {gimmickDoorCount}개\n" +
            $"오류: {errorCount}개\n" +
            $"빈 줄 무시: {skippedCount}개"
        );


        ValidateAgainstMoveData();
        ValidateDoorPairs();
    }


    public DoorData GetDoorData(
        Vector2Int position,
        MoveDirection direction)
    {
        if (
            !doorDataByPosition.TryGetValue(
                position,
                out Dictionary<
                    MoveDirection,
                    DoorData
                > directionMap
            )
        )
        {
            return null;
        }


        if (
            !directionMap.TryGetValue(
                direction,
                out DoorData doorData
            )
        )
        {
            return null;
        }


        return doorData;
    }


    public DoorData GetDoorData(
        int x,
        int y,
        MoveDirection direction)
    {
        return GetDoorData(
            new Vector2Int(
                x,
                y
            ),
            direction
        );
    }


    public bool HasDoorData(
        Vector2Int position,
        MoveDirection direction)
    {
        return
            GetDoorData(
                position,
                direction
            ) != null;
    }


    public string GetRequiredKey(
        Vector2Int position,
        MoveDirection direction)
    {
        DoorData data =
            GetDoorData(
                position,
                direction
            );


        if (data == null)
        {
            return string.Empty;
        }


        return data.NeedKey;
    }


    public bool IsLockedDoor(
        Vector2Int position,
        MoveDirection direction)
    {
        DoorData data =
            GetDoorData(
                position,
                direction
            );


        return
            data != null &&
            data.IsLockedDoor;
    }


    public bool IsGimmickDoor(
        Vector2Int position,
        MoveDirection direction)
    {
        DoorData data =
            GetDoorData(
                position,
                direction
            );


        return
            data != null &&
            data.IsGimmickDoor;
    }


    private void ValidateAgainstMoveData()
    {
        MoveDataLoader moveLoader =
            MoveDataLoader.Instance;


        if (
            moveLoader == null ||
            !moveLoader.IsLoaded
        )
        {
            Debug.LogWarning(
                "[DoorDataLoader] " +
                "Move Data 교차 검증을 건너뜁니다."
            );

            return;
        }


        int mismatchCount = 0;


        foreach (
            KeyValuePair<
                Vector2Int,
                Dictionary<MoveDirection, DoorData>
            > positionPair
            in doorDataByPosition
        )
        {
            foreach (
                KeyValuePair<
                    MoveDirection,
                    DoorData
                > directionPair
                in positionPair.Value
            )
            {
                MoveData moveData =
                    moveLoader.GetMoveData(
                        positionPair.Key,
                        directionPair.Key
                    );


                DoorData doorData =
                    directionPair.Value;


                if (moveData == null)
                {
                    mismatchCount++;
                    continue;
                }


                if (
                    moveData.PathType !=
                    doorData.PathType
                )
                {
                    mismatchCount++;
                }


                if (
                    moveData.Passable !=
                    doorData.Passable
                )
                {
                    mismatchCount++;
                }
            }
        }


        if (mismatchCount == 0)
        {
            Debug.Log(
                "[DoorDataLoader] " +
                "Move Data와 Door Tiles 교차 검증 완료. " +
                "불일치 없음."
            );
        }
        else
        {
            Debug.LogWarning(
                "[DoorDataLoader] " +
                "Move Data / Door Tiles 불일치: " +
                mismatchCount +
                "개"
            );
        }
    }


    private void ValidateDoorPairs()
    {
        int pairErrorCount = 0;


        foreach (
            KeyValuePair<
                Vector2Int,
                Dictionary<MoveDirection, DoorData>
            > positionPair
            in doorDataByPosition
        )
        {
            Vector2Int position =
                positionPair.Key;


            foreach (
                KeyValuePair<
                    MoveDirection,
                    DoorData
                > directionPair
                in positionPair.Value
            )
            {
                MoveDirection direction =
                    directionPair.Key;


                DoorData current =
                    directionPair.Value;


                Vector2Int destination =
                    GetDestination(
                        position,
                        direction
                    );


                MoveDirection opposite =
                    GetOppositeDirection(
                        direction
                    );


                DoorData oppositeData =
                    GetDoorData(
                        destination,
                        opposite
                    );


                if (oppositeData == null)
                {
                    pairErrorCount++;


                    Debug.LogWarning(
                        "[DoorDataLoader] " +
                        "반대편 Door 데이터가 없습니다.\n" +
                        $"현재: {position} / {direction}\n" +
                        $"예상: {destination} / {opposite}"
                    );


                    continue;
                }


                if (
                    current.PathType !=
                    oppositeData.PathType
                )
                {
                    pairErrorCount++;
                }


                if (
                    current.IsLockedDoor &&
                    current.NeedKey !=
                    oppositeData.NeedKey
                )
                {
                    pairErrorCount++;
                }
            }
        }


        if (pairErrorCount == 0)
        {
            Debug.Log(
                "[DoorDataLoader] " +
                "Door 양방향 데이터 검증 완료. 문제 없음."
            );
        }
        else
        {
            Debug.LogWarning(
                "[DoorDataLoader] " +
                "Door 양방향 데이터 문제: " +
                pairErrorCount +
                "개"
            );
        }
    }


    private Vector2Int GetDestination(
        Vector2Int position,
        MoveDirection direction)
    {
        /*
         * 새 기획 데이터 좌표계
         *
         * North = Y - 1
         * South = Y + 1
         */

        switch (direction)
        {
            case MoveDirection.Up:

                return new Vector2Int(
                    position.x,
                    position.y - 1
                );


            case MoveDirection.Down:

                return new Vector2Int(
                    position.x,
                    position.y + 1
                );


            case MoveDirection.Left:

                return new Vector2Int(
                    position.x - 1,
                    position.y
                );


            case MoveDirection.Right:

                return new Vector2Int(
                    position.x + 1,
                    position.y
                );


            default:

                return position;
        }
    }


    private MoveDirection GetOppositeDirection(
        MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return MoveDirection.Down;

            case MoveDirection.Down:
                return MoveDirection.Up;

            case MoveDirection.Left:
                return MoveDirection.Right;

            case MoveDirection.Right:
                return MoveDirection.Left;

            default:
                return direction;
        }
    }


    private string CleanValue(
        string value)
    {
        if (value == null)
        {
            return string.Empty;
        }


        return value
            .Trim()
            .Trim('"')
            .Trim();
    }


    private bool TryParseDirection(
        string value,
        out MoveDirection direction)
    {
        direction =
            MoveDirection.Up;


        switch (
            value.Trim().ToLowerInvariant()
        )
        {
            case "north":

                direction =
                    MoveDirection.Up;

                return true;


            case "south":

                direction =
                    MoveDirection.Down;

                return true;


            case "west":

                direction =
                    MoveDirection.Left;

                return true;


            case "east":

                direction =
                    MoveDirection.Right;

                return true;
        }


        return false;
    }


    private bool TryParseDoorType(
        string value,
        out MovePathType pathType)
    {
        pathType =
            MovePathType.Wall;


        switch (
            value.Trim().ToLowerInvariant()
        )
        {
            case "lockeddoor":

                pathType =
                    MovePathType.LockedDoor;

                return true;


            case "gimmickdoor":

                pathType =
                    MovePathType.GimmickDoor;

                return true;
        }


        return false;
    }


    private bool TryParsePassable(
        string value,
        out bool passable)
    {
        passable = false;


        string lower =
            value
                .Trim()
                .ToLowerInvariant();


        switch (lower)
        {
            case "o":
            case "true":
            case "1":
            case "yes":

                passable = true;
                return true;


            case "x":
            case "false":
            case "0":
            case "no":

                passable = false;
                return true;
        }


        return false;
    }


    [ContextMenu("Door Tiles 다시 로드")]
    private void ReloadFromInspector()
    {
        LoadDoorData();
    }
}