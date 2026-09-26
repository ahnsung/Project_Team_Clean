using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestDataLoader : MonoBehaviour
{
    public static ChestDataLoader Instance { get; private set; }

    [Header("CSV Data")]
    [SerializeField] private TextAsset chestDataFile;

    private readonly Dictionary<Vector2Int, ChestTileData> chestData =
        new Dictionary<Vector2Int, ChestTileData>();

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

    private void LoadData()
    {
        chestData.Clear();

        if (chestDataFile == null)
        {
            Debug.LogError(
                "[ChestDataLoader] Chest_Data.csv가 연결되지 않았습니다."
            );

            return;
        }

        string[] lines =
            chestDataFile.text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries
            );

        if (lines.Length <= 1)
        {
            Debug.LogWarning(
                "[ChestDataLoader] Chest_Data.csv에 데이터가 없습니다."
            );

            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] values = line.Split(',');

            if (values.Length < 8)
            {
                Debug.LogWarning(
                    $"[ChestDataLoader] 잘못된 데이터 형식 - {i + 1}번째 줄\n" +
                    line
                );

                continue;
            }

            if (!int.TryParse(values[0].Trim(), out int x) ||
                !int.TryParse(values[1].Trim(), out int y))
            {
                Debug.LogWarning(
                    $"[ChestDataLoader] 좌표 변환 실패 - {i + 1}번째 줄\n" +
                    line
                );

                continue;
            }

            ChestTileData data =
                new ChestTileData();

            data.x = x;
            data.y = y;

            data.items =
                new List<ChestItemData>();

            for (int itemIndex = 0; itemIndex < 3; itemIndex++)
            {
                int idColumn =
                    2 + itemIndex * 2;

                int amountColumn =
                    idColumn + 1;

                if (!int.TryParse(
                        values[idColumn].Trim(),
                        out int itemID))
                {
                    continue;
                }

                if (!int.TryParse(
                        values[amountColumn].Trim(),
                        out int amount))
                {
                    continue;
                }

                ChestItemData item =
                    new ChestItemData(
                        itemID,
                        amount
                    );

                data.items.Add(item);
            }

            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );

            if (chestData.ContainsKey(position))
            {
                Debug.LogWarning(
                    $"[ChestDataLoader] 중복 좌표 발견: {position}"
                );

                continue;
            }

            chestData.Add(
                position,
                data
            );
        }

        Debug.Log(
            $"[ChestDataLoader] Chest_Data 로드 완료: {chestData.Count}개"
        );
    }

    public ChestTileData GetData(
        int x,
        int y)
    {
        Vector2Int position =
            new Vector2Int(
                x,
                y
            );

        if (chestData.TryGetValue(
            position,
            out ChestTileData data))
        {
            return data;
        }

        return null;
    }

    public bool HasData(
        int x,
        int y)
    {
        return chestData.ContainsKey(
            new Vector2Int(x, y)
        );
    }
}