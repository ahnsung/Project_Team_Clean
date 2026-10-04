using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점 판매 품목 데이터베이스.
///
/// ShopItemData를 shopItemId 기준으로 관리한다.
///
/// 주의:
/// - shopItemId는 상점 판매 품목 자체의 고유 ID
/// - sellItemId는 실제 아이템 ID
/// - shopLevel은 해당 품목이 추가되는 상점 단계
/// - 가격은 ShopDatabase에서 관리하지 않는다.
/// - StartScene에서 생성된 뒤 씬이 바뀌어도 유지된다.
/// </summary>
public class ShopDatabase : MonoBehaviour
{
    public static ShopDatabase Instance { get; private set; }

    private readonly Dictionary<int, ShopItemData> shopItems =
        new Dictionary<int, ShopItemData>();


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // 이미 살아 있는 ShopDatabase가 있다면
        // 새로 생성된 중복 오브젝트는 제거한다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 변경되어도 ShopDatabase를 유지한다.
        DontDestroyOnLoad(gameObject);

        BuildDatabase();
    }


    // =========================================================
    // DATABASE BUILD
    // =========================================================

    /// <summary>
    /// 기획서의 상점 테이블을 등록한다.
    /// </summary>
    private void BuildDatabase()
    {
        shopItems.Clear();


        // =====================================================
        // SHOP LEVEL 0
        // 최초 기본 상점 재고
        // =====================================================

        AddShopItem(6001, 1001, 0);
        AddShopItem(6002, 1004, 0);
        AddShopItem(6003, 1007, 0);
        AddShopItem(6004, 1109, 0);
        AddShopItem(6005, 1014, 0);


        // =====================================================
        // SHOP LEVEL 1
        // =====================================================

        AddShopItem(6006, 1001, 1);
        AddShopItem(6007, 1004, 1);
        AddShopItem(6008, 1007, 1);
        AddShopItem(6009, 1001, 1);
        AddShopItem(6010, 1004, 1);
        AddShopItem(6011, 1007, 1);


        // =====================================================
        // SHOP LEVEL 2
        // =====================================================

        AddShopItem(6012, 1010, 2);
        AddShopItem(6013, 1110, 2);
        AddShopItem(6014, 1112, 2);
        AddShopItem(6015, 1002, 2);


        // =====================================================
        // SHOP LEVEL 3
        // =====================================================

        AddShopItem(6016, 1111, 3);
        AddShopItem(6017, 1014, 3);
        AddShopItem(6018, 1015, 3);
        AddShopItem(6019, 1005, 3);


        Debug.Log(
            $"[ShopDatabase] 상점 데이터 로드 완료 - 총 {shopItems.Count}개");
    }


    /// <summary>
    /// 상점 품목 하나를 데이터베이스에 등록한다.
    /// </summary>
    private void AddShopItem(
        int shopItemId,
        int sellItemId,
        int shopLevel)
    {
        if (shopItems.ContainsKey(shopItemId))
        {
            Debug.LogWarning(
                $"[ShopDatabase] 중복된 Shop Item ID입니다: {shopItemId}");

            return;
        }

        ShopItemData data = new ShopItemData(
            shopItemId,
            sellItemId,
            shopLevel);

        shopItems.Add(
            shopItemId,
            data);
    }


    // =========================================================
    // GET
    // =========================================================

    /// <summary>
    /// shopItemId로 특정 상점 품목을 가져온다.
    /// 존재하지 않으면 null.
    /// </summary>
    public ShopItemData GetShopItem(int shopItemId)
    {
        if (shopItems.TryGetValue(
            shopItemId,
            out ShopItemData data))
        {
            return data;
        }

        Debug.LogWarning(
            $"[ShopDatabase] 존재하지 않는 Shop Item ID입니다: " +
            $"{shopItemId}");

        return null;
    }


    /// <summary>
    /// 특정 Shop Level에 해당하는 품목만 반환한다.
    ///
    /// level 0 -> 6001~6005
    /// level 1 -> 6006~6011
    /// level 2 -> 6012~6015
    /// level 3 -> 6016~6019
    /// </summary>
    public List<ShopItemData> GetItemsByLevel(int level)
    {
        List<ShopItemData> result =
            new List<ShopItemData>();

        foreach (ShopItemData data in shopItems.Values)
        {
            if (data.shopLevel == level)
            {
                result.Add(data);
            }
        }

        result.Sort(
            (a, b) =>
                a.shopItemId.CompareTo(b.shopItemId));

        return result;
    }


    /// <summary>
    /// 현재 상점 레벨까지의 모든 품목을 반환한다.
    ///
    /// 주의:
    /// 이것은 실제 현재 재고가 아니다.
    /// 단순히 DB상 해금 가능한 품목을 조회하는 함수다.
    ///
    /// 실제 남아 있는 재고는 ShopManager에서 관리한다.
    /// </summary>
    public List<ShopItemData> GetUnlockedItems(
        int currentShopLevel)
    {
        List<ShopItemData> result =
            new List<ShopItemData>();

        foreach (ShopItemData data in shopItems.Values)
        {
            if (data.shopLevel <= currentShopLevel)
            {
                result.Add(data);
            }
        }

        result.Sort(
            (a, b) =>
                a.shopItemId.CompareTo(b.shopItemId));

        return result;
    }


    /// <summary>
    /// 데이터베이스에 등록된 모든 품목을 반환한다.
    /// </summary>
    public IReadOnlyDictionary<int, ShopItemData> GetAllItems()
    {
        return shopItems;
    }


    /// <summary>
    /// 데이터베이스에 해당 shopItemId가 존재하는지 확인한다.
    /// </summary>
    public bool ContainsShopItem(int shopItemId)
    {
        return shopItems.ContainsKey(shopItemId);
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("Debug/Print All Shop Items")]
    private void DebugPrintAllShopItems()
    {
        Debug.Log(
            "========== SHOP DATABASE ==========");

        List<ShopItemData> list =
            new List<ShopItemData>(shopItems.Values);

        list.Sort(
            (a, b) =>
                a.shopItemId.CompareTo(b.shopItemId));

        foreach (ShopItemData data in list)
        {
            Debug.Log(
                $"ShopItemId: {data.shopItemId} / " +
                $"ItemId: {data.sellItemId} / " +
                $"ShopLevel: {data.shopLevel}");
        }

        Debug.Log(
            $"총 상점 품목 수: {shopItems.Count}");

        Debug.Log(
            "===================================");
    }
}