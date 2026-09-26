using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmingDataLoader : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static FarmingDataLoader Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Inspector
    // =========================================================

    [Header("Farming Data CSV")]
    [SerializeField]
    private TextAsset farmingDataCsv;


    // =========================================================
    // Runtime Data
    // =========================================================

    private readonly Dictionary<
        Vector2Int,
        FarmingTileData
    > dataByPosition =
        new Dictionary<
            Vector2Int,
            FarmingTileData
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


        LoadFarmingData();
    }


    // =========================================================
    // Load
    // =========================================================

    public void LoadFarmingData()
    {
        IsLoaded = false;


        dataByPosition.Clear();


        // -----------------------------------------------------
        // CSV 확인
        // -----------------------------------------------------

        if (farmingDataCsv == null)
        {
            Debug.LogError(
                "[FarmingDataLoader] " +
                "Farming CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            farmingDataCsv.text;


        if (
            string.IsNullOrWhiteSpace(
                csvText
            )
        )
        {
            Debug.LogError(
                "[FarmingDataLoader] " +
                "Farming CSV 내용이 비어 있습니다."
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
                "[FarmingDataLoader] " +
                "Farming CSV에 데이터 행이 없습니다."
            );

            return;
        }


        // =====================================================
        // Header 분석
        // =====================================================

        string[] headerColumns =
            lines[0].Split(',');


        int xIndex = -1;
        int yIndex = -1;
        int typeIndex = -1;
        int itemGroupIndex = -1;
        int minIndex = -1;
        int maxIndex = -1;


        for (
            int i = 0;
            i < headerColumns.Length;
            i++
        )
        {
            string header =
                NormalizeHeader(
                    headerColumns[i]
                );


            switch (header)
            {
                case "x":

                    xIndex = i;
                    break;


                case "y":

                    yIndex = i;
                    break;


                case "type":
                case "tiletype":

                    typeIndex = i;
                    break;


                case "itemgroup":
                case "group":

                    itemGroupIndex = i;
                    break;


                case "minitemquantity":
                case "minquantity":
                case "min":

                    minIndex = i;
                    break;


                case "maxitemquantity":
                case "maxquantity":
                case "max":

                    maxIndex = i;
                    break;
            }
        }


        // =====================================================
        // Header fallback
        // =====================================================

        /*
         * 신형:
         *
         * X,Y,Type,ItemGroup,MinItemQuantity,MaxItemQuantity
         *
         * 구형:
         *
         * X,Y,ItemGroup,MinItemQuantity,MaxItemQuantity
         */


        bool newFormat =
            headerColumns.Length >= 6;


        if (xIndex < 0)
        {
            xIndex = 0;
        }


        if (yIndex < 0)
        {
            yIndex = 1;
        }


        if (itemGroupIndex < 0)
        {
            itemGroupIndex =
                newFormat
                    ? 3
                    : 2;
        }


        if (minIndex < 0)
        {
            minIndex =
                newFormat
                    ? 4
                    : 3;
        }


        if (maxIndex < 0)
        {
            maxIndex =
                newFormat
                    ? 5
                    : 4;
        }


        if (
            typeIndex < 0 &&
            newFormat
        )
        {
            typeIndex = 2;
        }


        Debug.Log(
            "[FarmingDataLoader] CSV 형식 감지\n" +
            (
                newFormat
                    ? "신형 6열 Farming 데이터"
                    : "구형 5열 Farming 데이터"
            )
        );


        // =====================================================
        // Rows
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


            int requiredIndex =
                Mathf.Max(
                    xIndex,
                    yIndex
                );


            requiredIndex =
                Mathf.Max(
                    requiredIndex,
                    itemGroupIndex
                );


            requiredIndex =
                Mathf.Max(
                    requiredIndex,
                    minIndex
                );


            requiredIndex =
                Mathf.Max(
                    requiredIndex,
                    maxIndex
                );


            if (
                columns.Length <= requiredIndex
            )
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] 행 형식 오류\n" +
                    $"Line: {i + 1}\n" +
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
                 * 설명 행/메모 행은 무시
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
                    "[FarmingDataLoader] Y 숫자 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {yText}"
                );

                errorCount++;
                continue;
            }


            // -------------------------------------------------
            // Type
            // -------------------------------------------------

            if (
                typeIndex >= 0 &&
                typeIndex < columns.Length
            )
            {
                string tileType =
                    CleanValue(
                        columns[typeIndex]
                    );


                /*
                 * Farming 전용 Loader이므로
                 * Type이 적혀있다면 Farming만 허용.
                 */

                if (
                    !string.IsNullOrWhiteSpace(
                        tileType
                    ) &&
                    !tileType.Equals(
                        "Farming",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    Debug.LogWarning(
                        "[FarmingDataLoader] " +
                        "Farming이 아닌 데이터가 들어있습니다.\n" +
                        $"Line: {i + 1}\n" +
                        $"Type: {tileType}"
                    );

                    skippedCount++;
                    continue;
                }
            }


            // -------------------------------------------------
            // ItemGroup
            // -------------------------------------------------

            string itemGroup =
                CleanValue(
                    columns[
                        itemGroupIndex
                    ]
                );


            if (
                string.IsNullOrWhiteSpace(
                    itemGroup
                )
            )
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "ItemGroup이 비어 있습니다.\n" +
                    $"좌표: ({x}, {y})"
                );

                errorCount++;
                continue;
            }


            // -------------------------------------------------
            // Min
            // -------------------------------------------------

            string minText =
                CleanValue(
                    columns[minIndex]
                );


            if (
                !int.TryParse(
                    minText,
                    out int minQuantity
                )
            )
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "MinItemQuantity 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {minText}"
                );

                errorCount++;
                continue;
            }


            // -------------------------------------------------
            // Max
            // -------------------------------------------------

            string maxText =
                CleanValue(
                    columns[maxIndex]
                );


            if (
                !int.TryParse(
                    maxText,
                    out int maxQuantity
                )
            )
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "MaxItemQuantity 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {maxText}"
                );

                errorCount++;
                continue;
            }


            // -------------------------------------------------
            // Quantity 검증
            // -------------------------------------------------

            if (minQuantity < 0)
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "MinItemQuantity가 0보다 작아 0으로 보정합니다.\n" +
                    $"좌표: ({x}, {y})"
                );

                minQuantity = 0;
            }


            if (maxQuantity < minQuantity)
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "MaxItemQuantity가 Min보다 작아서 서로 교환합니다.\n" +
                    $"좌표: ({x}, {y})"
                );


                int temp =
                    minQuantity;


                minQuantity =
                    maxQuantity;


                maxQuantity =
                    temp;
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
                    "[FarmingDataLoader] " +
                    "중복 Farming 좌표\n" +
                    $"좌표: {position}"
                );

                errorCount++;
                continue;
            }


            // -------------------------------------------------
            // 등록
            // -------------------------------------------------

            FarmingTileData data =
                new FarmingTileData
                {
                    x = x,

                    y = y,

                    itemGroup =
                        itemGroup,

                    minItemQuantity =
                        minQuantity,

                    maxItemQuantity =
                        maxQuantity
                };


            dataByPosition.Add(
                position,
                data
            );


            successCount++;
        }


        // =====================================================
        // Finish
        // =====================================================

        IsLoaded =
            successCount > 0;


        Debug.Log(
            "[FarmingDataLoader] " +
            $"Farming_Data 로드 완료: {successCount}개\n" +
            $"무시: {skippedCount}개\n" +
            $"오류: {errorCount}개"
        );


        ValidateData();
    }


    // =========================================================
    // Validation
    // =========================================================

    private void ValidateData()
    {
        int problemCount = 0;


        foreach (
            KeyValuePair<
                Vector2Int,
                FarmingTileData
            > pair
            in dataByPosition
        )
        {
            FarmingTileData data =
                pair.Value;


            if (
                data == null
            )
            {
                problemCount++;
                continue;
            }


            if (
                string.IsNullOrWhiteSpace(
                    data.itemGroup
                )
            )
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "ItemGroup 누락\n" +
                    $"좌표: {pair.Key}"
                );

                problemCount++;
            }


            if (
                data.maxItemQuantity <
                data.minItemQuantity
            )
            {
                Debug.LogWarning(
                    "[FarmingDataLoader] " +
                    "수량 범위 오류\n" +
                    $"좌표: {pair.Key}\n" +
                    $"Min: {data.minItemQuantity}\n" +
                    $"Max: {data.maxItemQuantity}"
                );

                problemCount++;
            }
        }


        if (problemCount == 0)
        {
            Debug.Log(
                "[FarmingDataLoader] " +
                "Farming 데이터 검증 완료. 문제 없음."
            );
        }
        else
        {
            Debug.LogWarning(
                "[FarmingDataLoader] " +
                "Farming 데이터 문제: " +
                problemCount +
                "개"
            );
        }
    }


    // =========================================================
    // Get Data
    // =========================================================

    public FarmingTileData GetData(
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


    public FarmingTileData GetData(
        Vector2Int position)
    {
        if (
            dataByPosition.TryGetValue(
                position,
                out FarmingTileData data
            )
        )
        {
            return data;
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
    // DEBUG
    // =========================================================

    public void DebugFarmingData(
        Vector2Int position)
    {
        FarmingTileData data =
            GetData(
                position
            );


        if (data == null)
        {
            Debug.Log(
                "[FarmingDataLoader] " +
                $"좌표 {position}: Farming 데이터 없음"
            );

            return;
        }


        Debug.Log(
            "[FarmingDataLoader] Farming Debug\n" +
            $"좌표: {position}\n" +
            $"ItemGroup: {data.itemGroup}\n" +
            $"수량: " +
            $"{data.minItemQuantity} ~ " +
            $"{data.maxItemQuantity}"
        );
    }


    [ContextMenu(
        "DEBUG - Reload Farming Data"
    )]
    private void DebugReload()
    {
        LoadFarmingData();
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


    private string NormalizeHeader(
        string value)
    {
        return
            CleanValue(
                value
            )
            .ToLowerInvariant()
            .Replace(" ", "")
            .Replace("_", "");
    }
}