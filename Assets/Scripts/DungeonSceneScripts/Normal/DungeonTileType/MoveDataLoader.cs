using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-850)]
public class MoveDataLoader : MonoBehaviour
{
    public static MoveDataLoader Instance
    {
        get;
        private set;
    }


    [Header("CSV")]
    [SerializeField]
    private TextAsset moveDataCsv;


    private readonly Dictionary<
        Vector2Int,
        Dictionary<MoveDirection, MoveData>
    > moveDataByPosition
        =
        new Dictionary<
            Vector2Int,
            Dictionary<MoveDirection, MoveData>
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


    // =========================================================
    // Unity
    // =========================================================

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


        LoadMoveData();
    }


    // =========================================================
    // Load
    // =========================================================

    public void LoadMoveData()
    {
        moveDataByPosition.Clear();


        Count = 0;

        IsLoaded = false;


        if (moveDataCsv == null)
        {
            Debug.LogError(
                "[MoveDataLoader] " +
                "Move Data CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            moveDataCsv.text;


        if (
            string.IsNullOrWhiteSpace(
                csvText
            )
        )
        {
            Debug.LogError(
                "[MoveDataLoader] " +
                "Move Data CSV 내용이 비어 있습니다."
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


        if (lines.Length <= 1)
        {
            Debug.LogError(
                "[MoveDataLoader] " +
                "Move Data CSV에 데이터가 없습니다."
            );

            return;
        }


        int successCount = 0;

        int skippedCount = 0;

        int errorCount = 0;


        for (
            int i = 1;
            i < lines.Length;
            i++
        )
        {
            string line =
                lines[i].Trim();


            if (
                string.IsNullOrWhiteSpace(
                    line
                )
            )
            {
                skippedCount++;

                continue;
            }


            string[] columns =
                line.Split(',');


            if (columns.Length < 5)
            {
                skippedCount++;

                continue;
            }


            string xText =
                CleanValue(
                    columns[0]
                );


            string yText =
                CleanValue(
                    columns[1]
                );


            string directionText =
                CleanValue(
                    columns[2]
                );


            string typeText =
                CleanValue(
                    columns[3]
                );


            string passableText =
                CleanValue(
                    columns[4]
                );


            // 설명/메모 행 무시
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


                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Y 변환 실패: " +
                    yText
                );


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


                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Direction 오류: " +
                    directionText
                );


                continue;
            }


            if (
                !TryParsePathType(
                    typeText,
                    out MovePathType pathType
                )
            )
            {
                errorCount++;


                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Type 오류: " +
                    typeText
                );


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


                Debug.LogWarning(
                    "[MoveDataLoader] " +
                    $"{i + 1}번째 줄 Passable 오류: " +
                    passableText
                );


                continue;
            }


            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            MoveData moveData =
                new MoveData(
                    x,
                    y,
                    direction,
                    pathType,
                    passable
                );


            if (
                !moveDataByPosition.TryGetValue(
                    position,
                    out Dictionary<
                        MoveDirection,
                        MoveData
                    > directionMap
                )
            )
            {
                directionMap =
                    new Dictionary<
                        MoveDirection,
                        MoveData
                    >();


                moveDataByPosition.Add(
                    position,
                    directionMap
                );
            }


            directionMap[direction] =
                moveData;


            successCount++;
        }


        Count =
            successCount;


        IsLoaded =
            successCount > 0;


        Debug.Log(
            "[MoveDataLoader] " +
            $"Move Data 로드 완료: {successCount}개\n" +
            $"좌표 수: {moveDataByPosition.Count}개\n" +
            $"무시: {skippedCount}개\n" +
            $"오류: {errorCount}개"
        );


        ValidateData();
    }


    // =========================================================
    // Get Move Data
    // =========================================================

    public MoveData GetMoveData(
        Vector2Int position,
        MoveDirection direction)
    {
        if (
            !moveDataByPosition.TryGetValue(
                position,
                out Dictionary<
                    MoveDirection,
                    MoveData
                > directionMap
            )
        )
        {
            return null;
        }


        if (
            !directionMap.TryGetValue(
                direction,
                out MoveData moveData
            )
        )
        {
            return null;
        }


        return moveData;
    }


    public MoveData GetMoveData(
        int x,
        int y,
        MoveDirection direction)
    {
        return
            GetMoveData(
                new Vector2Int(
                    x,
                    y
                ),
                direction
            );
    }


    public bool HasMoveData(
        Vector2Int position,
        MoveDirection direction)
    {
        return
            GetMoveData(
                position,
                direction
            ) != null;
    }


    // =========================================================
    // Passable
    // =========================================================

    public bool IsPassable(
        Vector2Int position,
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                position,
                direction
            );


        if (data == null)
        {
            return false;
        }


        return
            data.Passable;
    }


    // =========================================================
    // Path Type
    // =========================================================

    public MovePathType GetPathType(
        Vector2Int position,
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                position,
                direction
            );


        if (data == null)
        {
            return
                MovePathType.Wall;
        }


        return
            data.PathType;
    }


    // =========================================================
    // Destination
    // =========================================================

    public Vector2Int GetDestination(
        Vector2Int position,
        MoveDirection direction)
    {
        /*
         * 기획 데이터 좌표계
         *
         * North = Y - 1
         * South = Y + 1
         * West  = X - 1
         * East  = X + 1
         */

        switch (direction)
        {
            case MoveDirection.Up:

                return
                    new Vector2Int(
                        position.x,
                        position.y - 1
                    );


            case MoveDirection.Down:

                return
                    new Vector2Int(
                        position.x,
                        position.y + 1
                    );


            case MoveDirection.Left:

                return
                    new Vector2Int(
                        position.x - 1,
                        position.y
                    );


            case MoveDirection.Right:

                return
                    new Vector2Int(
                        position.x + 1,
                        position.y
                    );


            default:

                return
                    position;
        }
    }


    // =========================================================
    // Validation
    // =========================================================

    private void ValidateData()
    {
        int incompletePositionCount =
            0;


        foreach (
            KeyValuePair<
                Vector2Int,
                Dictionary<
                    MoveDirection,
                    MoveData
                >
            > pair
            in moveDataByPosition
        )
        {
            Dictionary<
                MoveDirection,
                MoveData
            > directionMap =
                pair.Value;


            if (
                !directionMap.ContainsKey(
                    MoveDirection.Up
                ) ||

                !directionMap.ContainsKey(
                    MoveDirection.Down
                ) ||

                !directionMap.ContainsKey(
                    MoveDirection.Left
                ) ||

                !directionMap.ContainsKey(
                    MoveDirection.Right
                )
            )
            {
                incompletePositionCount++;
            }
        }


        if (
            incompletePositionCount == 0
        )
        {
            Debug.Log(
                "[MoveDataLoader] " +
                "모든 좌표의 4방향 이동 데이터 확인 완료."
            );
        }
        else
        {
            Debug.LogWarning(
                "[MoveDataLoader] " +
                "4방향 데이터가 부족한 좌표 수: " +
                incompletePositionCount
            );
        }
    }


    // =========================================================
    // DEBUG - LockedDoor
    // =========================================================

    [ContextMenu(
        "DEBUG - LockedDoor 전부 출력"
    )]
    private void DebugPrintAllLockedDoors()
    {
        if (!IsLoaded)
        {
            Debug.LogWarning(
                "[MoveDataLoader] " +
                "Move Data가 아직 로드되지 않았습니다."
            );

            return;
        }


        int count = 0;


        Debug.Log(
            "========== LOCKED DOOR 목록 =========="
        );


        foreach (
            KeyValuePair<
                Vector2Int,
                Dictionary<
                    MoveDirection,
                    MoveData
                >
            > positionPair
            in moveDataByPosition
        )
        {
            Vector2Int position =
                positionPair.Key;


            Dictionary<
                MoveDirection,
                MoveData
            > directionMap =
                positionPair.Value;


            foreach (
                KeyValuePair<
                    MoveDirection,
                    MoveData
                > directionPair
                in directionMap
            )
            {
                MoveDirection direction =
                    directionPair.Key;


                MoveData data =
                    directionPair.Value;


                if (
                    data == null ||
                    data.PathType !=
                        MovePathType.LockedDoor
                )
                {
                    continue;
                }


                Vector2Int destination =
                    GetDestination(
                        position,
                        direction
                    );


                count++;


                Debug.Log(
                    $"[LockedDoor #{count}]\n" +
                    $"현재 좌표: {position}\n" +
                    $"방향: {direction}\n" +
                    $"목적지: {destination}\n" +
                    $"Passable: {data.Passable}"
                );
            }
        }


        Debug.Log(
            "========== LOCKED DOOR 검색 완료 ==========\n" +
            $"총 LockedDoor 방향 데이터: {count}개"
        );


        if (count == 0)
        {
            Debug.LogWarning(
                "[MoveDataLoader] " +
                "현재 Move Data CSV에는 " +
                "LockedDoor가 하나도 없습니다."
            );
        }
    }


    // =========================================================
    // DEBUG - GimmickDoor
    // =========================================================

    [ContextMenu(
        "DEBUG - GimmickDoor 전부 출력"
    )]
    private void DebugPrintAllGimmickDoors()
    {
        if (!IsLoaded)
        {
            Debug.LogWarning(
                "[MoveDataLoader] " +
                "Move Data가 아직 로드되지 않았습니다."
            );

            return;
        }


        int count =
            0;


        Debug.Log(
            "========== GIMMICK DOOR 목록 =========="
        );


        foreach (
            KeyValuePair<
                Vector2Int,
                Dictionary<
                    MoveDirection,
                    MoveData
                >
            > positionPair
            in moveDataByPosition
        )
        {
            Vector2Int position =
                positionPair.Key;


            foreach (
                KeyValuePair<
                    MoveDirection,
                    MoveData
                > directionPair
                in positionPair.Value
            )
            {
                MoveData data =
                    directionPair.Value;


                if (
                    data == null ||
                    data.PathType !=
                        MovePathType.GimmickDoor
                )
                {
                    continue;
                }


                MoveDirection direction =
                    directionPair.Key;


                Vector2Int destination =
                    GetDestination(
                        position,
                        direction
                    );


                count++;


                Debug.Log(
                    $"[GimmickDoor #{count}]\n" +
                    $"현재 좌표: {position}\n" +
                    $"방향: {direction}\n" +
                    $"목적지: {destination}\n" +
                    $"Passable: {data.Passable}"
                );
            }
        }


        Debug.Log(
            "========== GIMMICK DOOR 검색 완료 ==========\n" +
            $"총 GimmickDoor 방향 데이터: {count}개"
        );
    }


    // =========================================================
    // Utility
    // =========================================================

    private string CleanValue(
        string value)
    {
        if (value == null)
        {
            return
                string.Empty;
        }


        return
            value
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
            value
                .Trim()
                .ToLowerInvariant()
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


    private bool TryParsePathType(
        string value,
        out MovePathType pathType)
    {
        pathType =
            MovePathType.Wall;


        switch (
            value
                .Trim()
                .ToLowerInvariant()
        )
        {
            case "open":

                pathType =
                    MovePathType.Open;

                return true;


            case "wall":

                pathType =
                    MovePathType.Wall;

                return true;


            case "door":

                pathType =
                    MovePathType.Door;

                return true;


            case "oneway":

                pathType =
                    MovePathType.OneWay;

                return true;


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
        passable =
            false;


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

                passable =
                    true;

                return true;


            case "x":
            case "false":
            case "0":
            case "no":

                passable =
                    false;

                return true;
        }


        return false;
    }


    // =========================================================
    // Inspector
    // =========================================================

    [ContextMenu(
        "현재 Move Data 다시 로드"
    )]
    private void ReloadFromInspector()
    {
        LoadMoveData();
    }
}