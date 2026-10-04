using System;

/// <summary>
/// 상점에 등록되는 판매 품목 데이터.
///
/// 기획서 상점 테이블 구조:
/// shop_item_id / sell_item_id / shop_level
/// </summary>
[Serializable]
public class ShopItemData
{
    /// <summary>
    /// 상점에서 판매하는 품목 자체의 고유 ID.
    ///
    /// 같은 아이템을 여러 번 상점 재고에 넣기 위해
    /// 실제 아이템 ID와 별도로 존재한다.
    /// </summary>
    public int shopItemId;

    /// <summary>
    /// 실제 아이템 ID.
    /// 기존 아이템/장비 데이터에서 해당 ID를 참조한다.
    /// </summary>
    public int sellItemId;

    /// <summary>
    /// 해당 판매 품목이 추가되는 상점 단계.
    /// </summary>
    public int shopLevel;


    public ShopItemData(
        int shopItemId,
        int sellItemId,
        int shopLevel)
    {
        this.shopItemId = shopItemId;
        this.sellItemId = sellItemId;
        this.shopLevel = shopLevel;
    }
}