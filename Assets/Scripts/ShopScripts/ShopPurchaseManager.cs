using UnityEngine;

/// <summary>
/// 상점 구매 처리를 담당한다.
///
/// 구매 흐름:
/// 1. ShopItemId 재고 확인
/// 2. ShopItemData 확인
/// 3. 실제 ItemData 확인
/// 4. 구매 가격 확인
/// 5. 솔라스톤 보유량 확인
/// 6. 인벤토리에 아이템 추가
/// 7. 솔라스톤 차감
/// 8. 해당 ShopItemId 재고 제거
///
/// 같은 실제 아이템이라도 ShopItemId가 다르면
/// 서로 다른 상점 재고로 취급한다.
/// </summary>
public class ShopPurchaseManager : MonoBehaviour
{
    public static ShopPurchaseManager Instance { get; private set; }


    // =========================================================
    // UNITY
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

        DontDestroyOnLoad(gameObject);
    }


    // =========================================================
    // PURCHASE
    // =========================================================

    /// <summary>
    /// ShopItemId에 해당하는 상품을 1개 구매한다.
    ///
    /// 성공하면 true,
    /// 실패하면 false를 반환한다.
    /// </summary>
    public bool Purchase(int shopItemId)
    {
        Debug.Log(
            "[ShopPurchaseManager] 구매 시도\n" +
            $"ShopItemId: {shopItemId}"
        );


        // ---------------------------------------------------------
        // 1. 필수 Manager 확인
        // ---------------------------------------------------------

        if (ShopManager.Instance == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] ShopManager.Instance가 없습니다."
            );

            return false;
        }


        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] ShopDatabase.Instance가 없습니다."
            );

            return false;
        }


        if (ItemDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] ItemDatabase.Instance가 없습니다."
            );

            return false;
        }


        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] InventoryManager.Instance가 없습니다."
            );

            return false;
        }


        if (SolastoneManager.Instance == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] SolastoneManager.Instance가 없습니다."
            );

            return false;
        }


        // ---------------------------------------------------------
        // 2. 상점 초기화 확인
        // ---------------------------------------------------------

        if (!ShopManager.Instance.IsInitialized)
        {
            Debug.LogWarning(
                "[ShopPurchaseManager] " +
                "ShopManager 초기화가 아직 완료되지 않았습니다."
            );

            return false;
        }


        // ---------------------------------------------------------
        // 3. 현재 재고 확인
        // ---------------------------------------------------------

        if (!ShopManager.Instance.HasStock(shopItemId))
        {
            Debug.LogWarning(
                "[ShopPurchaseManager] " +
                "현재 상점에 존재하지 않는 재고입니다.\n" +
                $"ShopItemId: {shopItemId}"
            );

            return false;
        }


        // ---------------------------------------------------------
        // 4. ShopItemData 확인
        // ---------------------------------------------------------

        ShopItemData shopItem =
            ShopDatabase.Instance.GetShopItem(
                shopItemId
            );

        if (shopItem == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] " +
                "ShopDatabase에서 상품 정보를 찾지 못했습니다.\n" +
                $"ShopItemId: {shopItemId}"
            );

            return false;
        }


        // ---------------------------------------------------------
        // 5. 실제 ItemData 확인
        // ---------------------------------------------------------

        int itemId =
            shopItem.sellItemId;

        ItemData itemData =
            ItemDatabase.Instance.GetItem(
                itemId
            );

        if (itemData == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] " +
                "ItemDatabase에서 실제 아이템을 찾지 못했습니다.\n" +
                $"ShopItemId: {shopItemId}\n" +
                $"ItemId: {itemId}"
            );

            return false;
        }


        // ---------------------------------------------------------
        // 6. 구매 가격 확인
        // ---------------------------------------------------------

        int price =
            itemData.buyPrice;

        if (price <= 0)
        {
            Debug.LogWarning(
                "[ShopPurchaseManager] " +
                "구매 가격이 올바르지 않습니다.\n" +
                $"ItemId: {itemId}\n" +
                $"ItemName: {itemData.itemName}\n" +
                $"BuyPrice: {price}"
            );

            return false;
        }


        // ---------------------------------------------------------
        // 7. 솔라스톤 확인
        // ---------------------------------------------------------

        if (!SolastoneManager.Instance.CanAfford(price))
        {
            Debug.Log(
                "[ShopPurchaseManager] 솔라스톤이 부족합니다.\n" +
                $"상품: {itemData.itemName}\n" +
                $"필요 솔라스톤: {price}\n" +
                $"현재 솔라스톤: " +
                $"{SolastoneManager.Instance.CurrentAmount}"
            );

            return false;
        }


        // ---------------------------------------------------------
        // 8. 구매 전 인벤토리 상태 기록
        //
        // AddItem 성공 후 다른 단계가 실패하면
        // 방금 추가된 아이템을 찾아 되돌리기 위해 사용한다.
        // ---------------------------------------------------------

        int inventoryCountBefore =
            InventoryManager.Instance.items.Count;


        // ---------------------------------------------------------
        // 9. 인벤토리에 아이템 추가
        //
        // 공간이 없으면 AddItem이 false를 반환하므로
        // 돈과 재고는 건드리지 않는다.
        // ---------------------------------------------------------

        bool itemAdded =
            InventoryManager.Instance.AddItem(
                itemId
            );

        if (!itemAdded)
        {
            Debug.Log(
                "[ShopPurchaseManager] 구매 실패 - " +
                "인벤토리 공간이 부족하거나 아이템 추가에 실패했습니다.\n" +
                $"ItemId: {itemId}\n" +
                $"ItemName: {itemData.itemName}"
            );

            return false;
        }


        // ---------------------------------------------------------
        // 10. 솔라스톤 차감
        // ---------------------------------------------------------

        bool spent =
            SolastoneManager.Instance.Spend(
                price
            );

        if (!spent)
        {
            Debug.LogError(
                "[ShopPurchaseManager] " +
                "아이템 추가 후 솔라스톤 차감에 실패했습니다.\n" +
                "구매를 취소하고 추가된 아이템을 회수합니다."
            );

            RollbackAddedItem(
                itemId,
                inventoryCountBefore
            );

            return false;
        }


        // ---------------------------------------------------------
        // 11. 정확한 ShopItemId 재고 제거
        // ---------------------------------------------------------

        bool stockRemoved =
            ShopManager.Instance.RemoveStock(
                shopItemId
            );

        if (!stockRemoved)
        {
            Debug.LogError(
                "[ShopPurchaseManager] " +
                "솔라스톤 차감 후 상점 재고 제거에 실패했습니다.\n" +
                "구매를 취소하고 아이템 및 솔라스톤을 복구합니다."
            );


            // 방금 구매한 아이템 회수
            RollbackAddedItem(
                itemId,
                inventoryCountBefore
            );


            // 사용한 솔라스톤 복구
            SolastoneManager.Instance.Add(
                price
            );


            return false;
        }


        // ---------------------------------------------------------
        // 12. 구매 완료
        // ---------------------------------------------------------

        Debug.Log(
            "[ShopPurchaseManager] 구매 완료\n" +
            $"ShopItemId: {shopItemId}\n" +
            $"ItemId: {itemId}\n" +
            $"ItemName: {itemData.itemName}\n" +
            $"Price: {price}\n" +
            $"Remaining Solastone: " +
            $"{SolastoneManager.Instance.CurrentAmount}"
        );


        return true;
    }


    // =========================================================
    // ROLLBACK
    // =========================================================

    /// <summary>
    /// AddItem 성공 이후 구매 과정에서 문제가 발생했을 때
    /// 방금 추가된 아이템을 인벤토리에서 제거한다.
    ///
    /// 구매 전에 존재하던 같은 ID의 아이템을 잘못 지우지 않도록
    /// 구매 전 items.Count를 기준으로 새로 추가된 영역만 검사한다.
    /// </summary>
    private void RollbackAddedItem(
        int itemId,
        int inventoryCountBefore)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] " +
                "Rollback 실패 - InventoryManager.Instance가 없습니다."
            );

            return;
        }


        if (InventoryManager.Instance.items == null)
        {
            Debug.LogError(
                "[ShopPurchaseManager] " +
                "Rollback 실패 - Inventory items가 없습니다."
            );

            return;
        }


        for (
            int i =
                InventoryManager.Instance.items.Count - 1;
            i >= inventoryCountBefore;
            i--)
        {
            InventoryItem item =
                InventoryManager.Instance.items[i];

            if (item == null ||
                item.data == null)
            {
                continue;
            }


            if (item.data.id != itemId)
            {
                continue;
            }


            InventoryManager.Instance.RemoveItem(
                item
            );


            Debug.Log(
                "[ShopPurchaseManager] " +
                "구매 Rollback으로 아이템을 회수했습니다.\n" +
                $"ItemId: {itemId}"
            );


            return;
        }


        Debug.LogError(
            "[ShopPurchaseManager] " +
            "Rollback할 구매 아이템을 찾지 못했습니다.\n" +
            $"ItemId: {itemId}"
        );
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("Debug/Purchase ShopItem 6001")]
    private void DebugPurchase6001()
    {
        Purchase(6001);
    }


    [ContextMenu("Debug/Purchase ShopItem 6002")]
    private void DebugPurchase6002()
    {
        Purchase(6002);
    }


    [ContextMenu("Debug/Purchase ShopItem 6003")]
    private void DebugPurchase6003()
    {
        Purchase(6003);
    }


    [ContextMenu("Debug/Purchase ShopItem 6004")]
    private void DebugPurchase6004()
    {
        Purchase(6004);
    }


    [ContextMenu("Debug/Purchase ShopItem 6005")]
    private void DebugPurchase6005()
    {
        Purchase(6005);
    }
}