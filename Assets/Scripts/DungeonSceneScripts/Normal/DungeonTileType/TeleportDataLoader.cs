using System;
using System.Collections.Generic;
using UnityEngine;

public class TeleportDataLoader : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static TeleportDataLoader Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Inspector
    // =========================================================

    [Header("Teleport Data CSV")]
    [SerializeField]
    private TextAsset teleportDataCsv;


    // =========================================================
    // Runtime Data
    // =========================================================

    // 좌표 → Teleport 데이터
    private readonly Dictionary<
        Vector2Int,
        TeleportTileData
    > dataByPosition =
        new Dictionary<
            Vector2Int,
            TeleportTileData
        >();


    /*
     * 구형 CSV용
     *
     * 예:
     * X,Y,ConnectedTeleportID
     * 3,3,T_1
     * 10,3,T_1
     *
     * 같은 ID를 가진 좌표 2개를 묶는다.
     */
    private readonly Dictionary<
        string,
        List<TeleportTileData>
    > dataBySharedPairID =
        new Dictionary<
            string,
            List<TeleportTileData>
        >();


    /*
     * 신형 CSV용
     *
     * 예:
     * X,Y,TeleportID,ConnectedTeleportID
     *
     * 자기 ID → 좌표
     */
    private readonly Dictionary<
        string,
        Vector2Int
    > positionByTeleportID =
        new Dictionary<
            string,
            Vector2Int
        >();


    /*
     * 좌표 → 자기 TeleportID
     *
     * Debug / 검증용
     */
    private readonly Dictionary<
        Vector2Int,
        string
    > teleportIDByPosition =
        new Dictionary<
            Vector2Int,
            string
        >();


    // =========================================================
    // State
    // =========================================================

    public bool IsLoaded
    {
        get;
        private set;
    }


    public int Count
    {
        get
        {
            return dataByPosition.Count;
        }
    }


    /*
     * false:
     * 구형 3열 방식
     *
     * true:
     * 신형 ID → ConnectedID 방식
     */
    private bool usesExplicitTeleportIDs;


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


        LoadTeleportData();
    }


    // =========================================================
    // Load
    // =========================================================

    public void LoadTeleportData()
    {
        // 다시 로드 가능하게 처리
        IsLoaded = false;


        dataByPosition.Clear();

        dataBySharedPairID.Clear();

        positionByTeleportID.Clear();

        teleportIDByPosition.Clear();


        usesExplicitTeleportIDs = false;


        // -----------------------------------------------------
        // CSV 확인
        // -----------------------------------------------------

        if (teleportDataCsv == null)
        {
            Debug.LogError(
                "[TeleportDataLoader] " +
                "Teleport CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            teleportDataCsv.text;


        if (
            string.IsNullOrWhiteSpace(
                csvText
            )
        )
        {
            Debug.LogError(
                "[TeleportDataLoader] " +
                "Teleport CSV 내용이 비어 있습니다."
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
                "[TeleportDataLoader] " +
                "Teleport CSV에 데이터 행이 없습니다."
            );

            return;
        }


        // =====================================================
        // Header 분석
        // =====================================================

        string header =
            lines[0]
                .Trim()
                .ToLowerInvariant();


        string[] headerColumns =
            header.Split(',');


        int xIndex = -1;

        int yIndex = -1;

        int teleportIDIndex = -1;

        int connectedIDIndex = -1;


        for (
            int i = 0;
            i < headerColumns.Length;
            i++
        )
        {
            string column =
                CleanValue(
                    headerColumns[i]
                )
                .ToLowerInvariant()
                .Replace(" ", "")
                .Replace("_", "");


            if (column == "x")
            {
                xIndex = i;
            }
            else if (column == "y")
            {
                yIndex = i;
            }
            else if (
                column == "teleportid" ||
                column == "id"
            )
            {
                teleportIDIndex = i;
            }
            else if (
                column == "connectedteleportid" ||
                column == "connectedid" ||
                column == "targetteleportid"
            )
            {
                connectedIDIndex = i;
            }
        }


        // -----------------------------------------------------
        // 최소 좌표 열 검증
        // -----------------------------------------------------

        if (
            xIndex < 0 ||
            yIndex < 0
        )
        {
            /*
             * 혹시 헤더명이 이상해도
             * 첫 두 열은 X/Y라고 fallback.
             */
            xIndex = 0;
            yIndex = 1;
        }


        /*
         * 신형:
         * TeleportID + ConnectedTeleportID 둘 다 존재
         */
        usesExplicitTeleportIDs =
            teleportIDIndex >= 0 &&
            connectedIDIndex >= 0;


        /*
         * 구형:
         * X,Y,ConnectedTeleportID
         *
         * Header 분석이 실패한 경우
         * 3번째 열 사용
         */
        if (
            !usesExplicitTeleportIDs &&
            connectedIDIndex < 0
        )
        {
            connectedIDIndex = 2;
        }


        Debug.Log(
            "[TeleportDataLoader] CSV 형식 감지\n" +
            (
                usesExplicitTeleportIDs
                    ? "형식: TeleportID + ConnectedTeleportID"
                    : "형식: Shared ConnectedTeleportID"
            )
        );


        // =====================================================
        // Data Rows
        // =====================================================

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


            int requiredColumn =
                Mathf.Max(
                    xIndex,
                    yIndex
                );


            requiredColumn =
                Mathf.Max(
                    requiredColumn,
                    connectedIDIndex
                );


            if (usesExplicitTeleportIDs)
            {
                requiredColumn =
                    Mathf.Max(
                        requiredColumn,
                        teleportIDIndex
                    );
            }


            if (
                columns.Length <= requiredColumn
            )
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    $"행 형식 오류 - {i + 1}번째 줄\n" +
                    $"내용: {line}"
                );

                errorCount++;
                continue;
            }


            // -------------------------------------------------
            // X
            // -------------------------------------------------

            string xText =
                CleanValue(
                    columns[xIndex]
                );


            if (
                !int.TryParse(
                    xText,
                    out int x
                )
            )
            {
                /*
                 * 메모/설명 행일 가능성이 있으므로
                 * 단순 무시
                 */
                skippedCount++;
                continue;
            }


            // -------------------------------------------------
            // Y
            // -------------------------------------------------

            string yText =
                CleanValue(
                    columns[yIndex]
                );


            if (
                !int.TryParse(
                    yText,
                    out int y
                )
            )
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    $"Y 좌표 변환 실패 - {i + 1}번째 줄\n" +
                    $"값: {yText}"
                );

                errorCount++;
                continue;
            }


            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            // -------------------------------------------------
            // 좌표 중복
            // -------------------------------------------------

            if (
                dataByPosition.ContainsKey(
                    position
                )
            )
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "중복 Teleport 좌표\n" +
                    $"좌표: {position}"
                );

                errorCount++;
                continue;
            }


            // =================================================
            // 신형
            // X,Y,TeleportID,ConnectedTeleportID
            // =================================================

            if (usesExplicitTeleportIDs)
            {
                string teleportID =
                    CleanValue(
                        columns[
                            teleportIDIndex
                        ]
                    );


                string connectedID =
                    CleanValue(
                        columns[
                            connectedIDIndex
                        ]
                    );


                if (
                    string.IsNullOrWhiteSpace(
                        teleportID
                    )
                )
                {
                    Debug.LogWarning(
                        "[TeleportDataLoader] " +
                        "TeleportID가 비어 있습니다.\n" +
                        $"좌표: {position}"
                    );

                    errorCount++;
                    continue;
                }


                if (
                    string.IsNullOrWhiteSpace(
                        connectedID
                    )
                )
                {
                    Debug.LogWarning(
                        "[TeleportDataLoader] " +
                        "ConnectedTeleportID가 비어 있습니다.\n" +
                        $"좌표: {position}\n" +
                        $"TeleportID: {teleportID}"
                    );

                    errorCount++;
                    continue;
                }


                // ---------------------------------------------
                // 자기 ID 중복
                // ---------------------------------------------

                if (
                    positionByTeleportID
                        .ContainsKey(
                            teleportID
                        )
                )
                {
                    Debug.LogWarning(
                        "[TeleportDataLoader] " +
                        "중복 TeleportID\n" +
                        $"ID: {teleportID}"
                    );

                    errorCount++;
                    continue;
                }


                TeleportTileData data =
                    new TeleportTileData
                    {
                        x = x,

                        y = y,

                        connectedTeleportID =
                            connectedID
                    };


                dataByPosition.Add(
                    position,
                    data
                );


                positionByTeleportID.Add(
                    teleportID,
                    position
                );


                teleportIDByPosition.Add(
                    position,
                    teleportID
                );


                successCount++;
            }

            // =================================================
            // 구형
            // X,Y,ConnectedTeleportID
            // =================================================

            else
            {
                string pairID =
                    CleanValue(
                        columns[
                            connectedIDIndex
                        ]
                    );


                if (
                    string.IsNullOrWhiteSpace(
                        pairID
                    )
                )
                {
                    Debug.LogWarning(
                        "[TeleportDataLoader] " +
                        "ConnectedTeleportID가 비어 있습니다.\n" +
                        $"좌표: {position}"
                    );

                    errorCount++;
                    continue;
                }


                TeleportTileData data =
                    new TeleportTileData
                    {
                        x = x,

                        y = y,

                        connectedTeleportID =
                            pairID
                    };


                dataByPosition.Add(
                    position,
                    data
                );


                if (
                    !dataBySharedPairID
                        .TryGetValue(
                            pairID,
                            out List<TeleportTileData> list
                        )
                )
                {
                    list =
                        new List<
                            TeleportTileData
                        >();


                    dataBySharedPairID.Add(
                        pairID,
                        list
                    );
                }


                list.Add(
                    data
                );


                successCount++;
            }
        }


        IsLoaded =
            successCount > 0;


        Debug.Log(
            "[TeleportDataLoader] " +
            $"Teleport Data 로드 완료: {successCount}개\n" +
            $"무시: {skippedCount}개\n" +
            $"오류: {errorCount}개"
        );


        ValidatePairs();
    }


    // =========================================================
    // Validation
    // =========================================================

    private void ValidatePairs()
    {
        int problemCount = 0;


        // =====================================================
        // 신형 검증
        // =====================================================

        if (usesExplicitTeleportIDs)
        {
            foreach (
                KeyValuePair<
                    Vector2Int,
                    TeleportTileData
                > pair
                in dataByPosition
            )
            {
                Vector2Int position =
                    pair.Key;


                TeleportTileData data =
                    pair.Value;


                if (
                    !teleportIDByPosition.TryGetValue(
                        position,
                        out string ownID
                    )
                )
                {
                    problemCount++;

                    continue;
                }


                string connectedID =
                    data.connectedTeleportID;


                if (
                    !positionByTeleportID
                        .ContainsKey(
                            connectedID
                        )
                )
                {
                    Debug.LogWarning(
                        "[TeleportDataLoader] " +
                        "연결 대상 ID가 존재하지 않습니다.\n" +
                        $"현재 좌표: {position}\n" +
                        $"현재 ID: {ownID}\n" +
                        $"Connected ID: {connectedID}"
                    );

                    problemCount++;

                    continue;
                }


                Vector2Int targetPosition =
                    positionByTeleportID[
                        connectedID
                    ];


                TeleportTileData targetData =
                    GetData(
                        targetPosition
                    );


                if (targetData == null)
                {
                    problemCount++;

                    continue;
                }


                /*
                 * A → B면
                 * B → A인지도 확인
                 */
                if (
                    teleportIDByPosition.TryGetValue(
                        targetPosition,
                        out string targetOwnID
                    )
                )
                {
                    if (
                        targetData.connectedTeleportID
                        != ownID
                    )
                    {
                        Debug.LogWarning(
                            "[TeleportDataLoader] " +
                            "텔레포트 연결이 서로 대칭이 아닙니다.\n" +
                            $"{ownID} -> {connectedID}\n" +
                            $"{targetOwnID} -> " +
                            $"{targetData.connectedTeleportID}"
                        );

                        problemCount++;
                    }
                }
            }
        }

        // =====================================================
        // 구형 검증
        // =====================================================

        else
        {
            foreach (
                KeyValuePair<
                    string,
                    List<TeleportTileData>
                > pair
                in dataBySharedPairID
            )
            {
                if (
                    pair.Value == null ||
                    pair.Value.Count != 2
                )
                {
                    Debug.LogWarning(
                        "[TeleportDataLoader] " +
                        "Teleport ID는 정확히 2개의 좌표가 필요합니다.\n" +
                        $"ID: {pair.Key}\n" +
                        $"현재 개수: " +
                        (
                            pair.Value == null
                                ? 0
                                : pair.Value.Count
                        )
                    );


                    problemCount++;
                }
            }
        }


        if (problemCount == 0)
        {
            Debug.Log(
                "[TeleportDataLoader] " +
                "Teleport 연결 검증 완료. 문제 없음."
            );
        }
        else
        {
            Debug.LogWarning(
                "[TeleportDataLoader] " +
                "Teleport 연결 문제: " +
                problemCount +
                "개"
            );
        }
    }


    // =========================================================
    // Get Data
    // =========================================================

    public TeleportTileData GetData(
        int x,
        int y)
    {
        return
            GetData(
                new Vector2Int(
                    x,
                    y
                )
            );
    }


    public TeleportTileData GetData(
        Vector2Int position)
    {
        if (
            dataByPosition.TryGetValue(
                position,
                out TeleportTileData data
            )
        )
        {
            return data;
        }


        return null;
    }


    // =========================================================
    // Destination
    // =========================================================

    public bool TryGetDestination(
        Vector2Int currentPosition,
        out Vector2Int destination)
    {
        destination =
            currentPosition;


        TeleportTileData currentData =
            GetData(
                currentPosition
            );


        if (currentData == null)
        {
            Debug.LogWarning(
                "[TeleportDataLoader] " +
                "현재 좌표의 Teleport Data가 없습니다.\n" +
                $"좌표: {currentPosition}"
            );

            return false;
        }


        // =====================================================
        // 신형 방식
        // =====================================================

        if (usesExplicitTeleportIDs)
        {
            string connectedID =
                currentData
                    .connectedTeleportID;


            if (
                string.IsNullOrWhiteSpace(
                    connectedID
                )
            )
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "ConnectedTeleportID가 비어 있습니다.\n" +
                    $"현재 좌표: {currentPosition}"
                );

                return false;
            }


            if (
                !positionByTeleportID
                    .TryGetValue(
                        connectedID,
                        out Vector2Int target
                    )
            )
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "ConnectedTeleportID에 해당하는 " +
                    "목적지를 찾지 못했습니다.\n" +
                    $"현재 위치: {currentPosition}\n" +
                    $"Connected ID: {connectedID}"
                );

                return false;
            }


            if (
                target ==
                currentPosition
            )
            {
                Debug.LogWarning(
                    "[TeleportDataLoader] " +
                    "텔레포트 목적지가 자기 자신입니다.\n" +
                    $"좌표: {currentPosition}"
                );

                return false;
            }


            destination =
                target;


            return true;
        }


        // =====================================================
        // 구형 방식
        // =====================================================

        string sharedPairID =
            currentData
                .connectedTeleportID;


        if (
            string.IsNullOrWhiteSpace(
                sharedPairID
            )
        )
        {
            return false;
        }


        if (
            !dataBySharedPairID
                .TryGetValue(
                    sharedPairID,
                    out List<TeleportTileData> pair
                )
        )
        {
            return false;
        }


        if (
            pair == null ||
            pair.Count != 2
        )
        {
            Debug.LogWarning(
                "[TeleportDataLoader] " +
                "Teleport Pair가 정확히 2개가 아닙니다.\n" +
                $"ID: {sharedPairID}\n" +
                $"개수: " +
                (
                    pair == null
                        ? 0
                        : pair.Count
                )
            );

            return false;
        }


        foreach (
            TeleportTileData data
            in pair
        )
        {
            Vector2Int position =
                new Vector2Int(
                    data.x,
                    data.y
                );


            if (
                position !=
                currentPosition
            )
            {
                destination =
                    position;


                return true;
            }
        }


        return false;
    }


    // =========================================================
    // Has
    // =========================================================

    public bool HasData(
        int x,
        int y)
    {
        return
            HasData(
                new Vector2Int(
                    x,
                    y
                )
            );
    }


    public bool HasData(
        Vector2Int position)
    {
        return
            dataByPosition.ContainsKey(
                position
            );
    }


    // =========================================================
    // Debug
    // =========================================================

    public void DebugTeleport(
        Vector2Int position)
    {
        TeleportTileData data =
            GetData(
                position
            );


        if (data == null)
        {
            Debug.Log(
                "[TeleportDataLoader] " +
                $"좌표 {position}: 데이터 없음"
            );

            return;
        }


        string ownID =
            "(구형 공유 ID 방식)";


        if (
            usesExplicitTeleportIDs &&
            teleportIDByPosition
                .TryGetValue(
                    position,
                    out string id
                )
        )
        {
            ownID = id;
        }


        Debug.Log(
            "[TeleportDataLoader] Teleport Debug\n" +
            $"좌표: {position}\n" +
            $"자기 ID: {ownID}\n" +
            $"Connected ID: " +
            $"{data.connectedTeleportID}"
        );


        if (
            TryGetDestination(
                position,
                out Vector2Int destination
            )
        )
        {
            Debug.Log(
                "[TeleportDataLoader] " +
                $"목적지: {destination}"
            );
        }
        else
        {
            Debug.LogWarning(
                "[TeleportDataLoader] " +
                "목적지를 찾지 못했습니다."
            );
        }
    }


    [ContextMenu(
        "DEBUG - Reload Teleport Data"
    )]
    private void DebugReload()
    {
        LoadTeleportData();
    }


    // =========================================================
    // Utility
    // =========================================================

    private string CleanValue(
        string value)
    {
        if (
            string.IsNullOrWhiteSpace(
                value
            )
        )
        {
            return string.Empty;
        }


        value =
            value.Trim();


        if (
            value.StartsWith("\"") &&
            value.EndsWith("\"") &&
            value.Length >= 2
        )
        {
            value =
                value.Substring(
                    1,
                    value.Length - 2
                );
        }


        return
            value.Trim();
    }
}