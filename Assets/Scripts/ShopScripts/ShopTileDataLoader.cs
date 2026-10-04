using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shop_Update 타일 데이터.
///
/// 현재 기획 데이터:
/// (2, 30) -> Shop Level 1
/// (6, 31) -> Shop Level 2
///
/// 추후 ShopTiles CSV가 추가되면
/// 이 클래스 내부 데이터 생성 부분만 CSV 로딩 방식으로 교체하면 된다.
/// </summary>
public class ShopTileDataLoader : MonoBehaviour
{
    public static ShopTileDataLoader Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Runtime Data
    // =========================================================

    private readonly Dictionary<Vector2Int, int>
        shopLevels =
            new Dictionary<Vector2Int, int>();


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

        BuildData();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // Build
    // =========================================================

    private void BuildData()
    {
        shopLevels.Clear();


        // ---------------------------------------------
        // ShopTiles 기획 데이터
        // ---------------------------------------------

        AddData(
            new Vector2Int(2, 29),
            1
        );

        AddData(
            new Vector2Int(6, 31),
            2
        );


        Debug.Log(
            "[ShopTileDataLoader] " +
            "Shop_Update 데이터 등록 완료\n" +
            $"등록 개수: {shopLevels.Count}"
        );
    }


    private void AddData(
        Vector2Int position,
        int shopLevel)
    {
        if (shopLevel < 0)
        {
            Debug.LogWarning(
                "[ShopTileDataLoader] " +
                "잘못된 Shop Level입니다.\n" +
                $"좌표: {position}\n" +
                $"Level: {shopLevel}"
            );

            return;
        }


        if (shopLevels.ContainsKey(position))
        {
            Debug.LogWarning(
                "[ShopTileDataLoader] " +
                "중복된 Shop_Update 좌표입니다.\n" +
                $"좌표: {position}"
            );

            return;
        }


        shopLevels.Add(
            position,
            shopLevel
        );
    }


    // =========================================================
    // Query
    // =========================================================

    public bool TryGetShopLevel(
        int x,
        int y,
        out int shopLevel)
    {
        return TryGetShopLevel(
            new Vector2Int(x, y),
            out shopLevel
        );
    }


    public bool TryGetShopLevel(
        Vector2Int position,
        out int shopLevel)
    {
        return shopLevels.TryGetValue(
            position,
            out shopLevel
        );
    }


    public bool IsShopUpdateTile(
        int x,
        int y)
    {
        return shopLevels.ContainsKey(
            new Vector2Int(x, y)
        );
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug/Print Shop Tiles")]
    private void DebugPrintShopTiles()
    {
        Debug.Log(
            "[ShopTileDataLoader] " +
            $"등록된 Shop_Update 타일: {shopLevels.Count}개"
        );


        foreach (
            KeyValuePair<Vector2Int, int> pair
            in shopLevels)
        {
            Debug.Log(
                "[ShopTileDataLoader]\n" +
                $"좌표: {pair.Key}\n" +
                $"Shop Level: {pair.Value}"
            );
        }
    }
}