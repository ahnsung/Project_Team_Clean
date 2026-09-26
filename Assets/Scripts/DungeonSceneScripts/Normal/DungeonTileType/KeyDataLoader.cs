using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyDataLoader : MonoBehaviour
{
    public static KeyDataLoader Instance
    {
        get;
        private set;
    }


    [Header("Key Data CSV")]
    [SerializeField]
    private TextAsset keyDataCsv;


    private readonly Dictionary<
        Vector2Int,
        KeyTileData
    > keyDataLookup =
        new Dictionary<
            Vector2Int,
            KeyTileData
        >();


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
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadKeyData();
    }


    // =========================================================
    // Load
    // =========================================================

    public void LoadKeyData()
    {
        if (IsLoaded)
            return;


        keyDataLookup.Clear();


        if (keyDataCsv == null)
        {
            Debug.LogError(
                "[KeyDataLoader] " +
                "Key_Data CSV가 연결되지 않았습니다."
            );

            return;
        }


        string csvText =
            keyDataCsv.text;


        if (string.IsNullOrWhiteSpace(
            csvText))
        {
            Debug.LogError(
                "[KeyDataLoader] " +
                "Key_Data CSV가 비어 있습니다."
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
                StringSplitOptions
                    .RemoveEmptyEntries
            );


        for (
            int i = 0;
            i < lines.Length;
            i++)
        {
            string line =
                lines[i].Trim();


            if (string.IsNullOrWhiteSpace(
                line))
            {
                continue;
            }


            // 첫 줄 헤더
            if (i == 0 &&
                line.ToLowerInvariant()
                    .Contains("keyid"))
            {
                continue;
            }


            string[] columns =
                line.Split(',');


            if (columns.Length < 3)
            {
                Debug.LogWarning(
                    "[KeyDataLoader] " +
                    "잘못된 행 형식\n" +
                    $"Line: {i + 1}\n" +
                    $"내용: {line}"
                );

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

            string keyID =
                CleanValue(
                    columns[2]
                );


            if (!int.TryParse(
                xText,
                out int x))
            {
                Debug.LogWarning(
                    "[KeyDataLoader] " +
                    "X 좌표 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {xText}"
                );

                continue;
            }


            if (!int.TryParse(
                yText,
                out int y))
            {
                Debug.LogWarning(
                    "[KeyDataLoader] " +
                    "Y 좌표 변환 실패\n" +
                    $"Line: {i + 1}\n" +
                    $"값: {yText}"
                );

                continue;
            }


            if (string.IsNullOrWhiteSpace(
                keyID))
            {
                Debug.LogWarning(
                    "[KeyDataLoader] " +
                    "KeyID가 비어 있습니다.\n" +
                    $"좌표: ({x}, {y})"
                );

                continue;
            }


            Vector2Int position =
                new Vector2Int(
                    x,
                    y
                );


            if (keyDataLookup.ContainsKey(
                position))
            {
                Debug.LogWarning(
                    "[KeyDataLoader] " +
                    "중복 좌표입니다: " +
                    position
                );

                continue;
            }


            KeyTileData data =
                new KeyTileData
                {
                    x = x,
                    y = y,
                    keyID = keyID
                };


            keyDataLookup.Add(
                position,
                data
            );
        }


        IsLoaded =
            true;


        Debug.Log(
            "[KeyDataLoader] " +
            "Key_Data 로드 완료: " +
            keyDataLookup.Count +
            "개"
        );
    }


    // =========================================================
    // Get Data
    // =========================================================

    public KeyTileData GetData(
        int x,
        int y)
    {
        return GetData(
            new Vector2Int(
                x,
                y
            )
        );
    }


    public KeyTileData GetData(
        Vector2Int position)
    {
        if (keyDataLookup.TryGetValue(
            position,
            out KeyTileData data))
        {
            return data;
        }


        return null;
    }


    public bool HasData(
        int x,
        int y)
    {
        return keyDataLookup.ContainsKey(
            new Vector2Int(
                x,
                y
            )
        );
    }


    // =========================================================
    // CSV Utility
    // =========================================================

    private string CleanValue(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
            value))
        {
            return string.Empty;
        }


        value =
            value.Trim();


        if (value.StartsWith("\"") &&
            value.EndsWith("\"") &&
            value.Length >= 2)
        {
            value =
                value.Substring(
                    1,
                    value.Length - 2
                );
        }


        return value.Trim();
    }
}