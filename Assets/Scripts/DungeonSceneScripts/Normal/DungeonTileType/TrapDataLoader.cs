using System;
using System.Collections.Generic;
using UnityEngine;

public class TrapDataLoader : MonoBehaviour
{
    public static TrapDataLoader Instance { get; private set; }

    [Header("CSV Data")]
    [SerializeField]
    private TextAsset trapDataFile;

    [Header("Default Values")]
    [Tooltip("CSV에서 TrapType이 비어 있을 때 사용할 기본값")]
    [SerializeField]
    private int defaultTrapType = 1;

    [Tooltip("CSV에서 발동 확률이 비어 있을 때 사용할 기본값")]
    [SerializeField]
    private int defaultTrapPossibility = 100;

    [Tooltip("CSV에서 TrapAmount가 비어 있을 때 사용할 기본값")]
    [SerializeField]
    private int defaultTrapAmount = 1;

    [Header("Debug")]
    [SerializeField]
    private bool printLog = true;


    private readonly Dictionary<Vector2Int, TrapTileData> trapData =
        new Dictionary<Vector2Int, TrapTileData>();


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        LoadData();
    }


    // =========================================================
    // CSV Load
    // =========================================================

    private void LoadData()
    {
        trapData.Clear();


        if (trapDataFile == null)
        {
            Debug.LogError(
                "[TrapDataLoader] Trap CSV가 연결되지 않았습니다."
            );

            return;
        }


        string[] lines =
            trapDataFile.text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries
            );


        if (lines.Length <= 1)
        {
            Debug.LogWarning(
                "[TrapDataLoader] Trap CSV에 데이터가 없습니다."
            );

            return;
        }


        /*
         * 실제 CSV 구조:
         *
         * X,Y,Tile Type,TrapType,TrapPossiblity,TrapAmount
         *
         * 예:
         *
         * 2,26,Trap,1,70,3
         * 7,24,Trap,,,
         */


        for (int i = 1; i < lines.Length; i++)
        {
            string line =
                lines[i].Trim();


            if (string.IsNullOrWhiteSpace(line))
                continue;


            string[] values =
                line.Split(',');


            if (values.Length < 3)
            {
                Debug.LogWarning(
                    "[TrapDataLoader] 잘못된 CSV 형식\n" +
                    $"줄: {i + 1}\n" +
                    $"내용: {line}"
                );

                continue;
            }


            // =================================================
            // 좌표
            // =================================================

            if (
                !int.TryParse(
                    values[0].Trim(),
                    out int x
                ) ||
                !int.TryParse(
                    values[1].Trim(),
                    out int y
                )
            )
            {
                Debug.LogWarning(
                    "[TrapDataLoader] 좌표 변환 실패\n" +
                    $"줄: {i + 1}\n" +
                    $"내용: {line}"
                );

                continue;
            }


            // =================================================
            // Tile Type
            // =================================================

            string tileType =
                values[2].Trim();


            /*
             * 혹시 CSV에 다른 종류의 타일이 섞여 있어도
             * Trap만 로드한다.
             */
            if (
                !string.Equals(
                    tileType,
                    "Trap",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }


            // =================================================
            // TrapType
            // =================================================

            int trapType =
                defaultTrapType;


            if (
                values.Length > 3 &&
                !string.IsNullOrWhiteSpace(values[3])
            )
            {
                if (
                    !int.TryParse(
                        values[3].Trim(),
                        out trapType
                    )
                )
                {
                    trapType =
                        defaultTrapType;
                }
            }


            // =================================================
            // TrapPossibility
            // =================================================

            int trapPossibility =
                defaultTrapPossibility;


            if (
                values.Length > 4 &&
                !string.IsNullOrWhiteSpace(values[4])
            )
            {
                if (
                    !int.TryParse(
                        values[4].Trim(),
                        out trapPossibility
                    )
                )
                {
                    trapPossibility =
                        defaultTrapPossibility;
                }
            }


            trapPossibility =
                Mathf.Clamp(
                    trapPossibility,
                    0,
                    100
                );


            // =================================================
            // TrapAmount
            // =================================================

            int trapAmount =
                defaultTrapAmount;


            if (
                values.Length > 5 &&
                !string.IsNullOrWhiteSpace(values[5])
            )
            {
                if (
                    !int.TryParse(
                        values[5].Trim(),
                        out trapAmount
                    )
                )
                {
                    trapAmount =
                        defaultTrapAmount;
                }
            }


            // =================================================
            // 생성
            // =================================================

            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            if (
                trapData.ContainsKey(
                    position
                )
            )
            {
                Debug.LogWarning(
                    "[TrapDataLoader] 중복 좌표 발견: " +
                    position
                );

                continue;
            }


            TrapTileData data =
                new TrapTileData();


            data.x =
                x;

            data.y =
                y;

            data.trapType =
                trapType;

            data.trapPossibility =
                trapPossibility;

            data.trapAmount =
                trapAmount;


            trapData.Add(
                position,
                data
            );


            if (printLog)
            {
                Debug.Log(
                    "[TrapDataLoader] Trap 등록\n" +
                    $"좌표: ({x}, {y})\n" +
                    $"TrapType: {trapType}\n" +
                    $"Possibility: {trapPossibility}\n" +
                    $"Amount: {trapAmount}"
                );
            }
        }


        Debug.Log(
            "[TrapDataLoader] Trap 데이터 로드 완료: " +
            trapData.Count +
            "개"
        );
    }


    // =========================================================
    // Get Data
    // =========================================================

    public TrapTileData GetData(
        int x,
        int y)
    {
        Vector2Int position =
            new Vector2Int(
                x,
                y
            );


        if (
            trapData.TryGetValue(
                position,
                out TrapTileData data
            )
        )
        {
            return data;
        }


        if (printLog)
        {
            Debug.LogWarning(
                "[TrapDataLoader] Trap 데이터 없음: " +
                position
            );
        }


        return null;
    }


    // =========================================================
    // Has Data
    // =========================================================

    public bool HasData(
        int x,
        int y)
    {
        return
            trapData.ContainsKey(
                new Vector2Int(
                    x,
                    y
                )
            );
    }


    // =========================================================
    // Debug Reload
    // =========================================================

    [ContextMenu("Reload Trap Data")]
    private void DebugReload()
    {
        LoadData();
    }
}