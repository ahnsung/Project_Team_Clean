using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 상점에 표시되는 아이템 1개의 UI.
///
/// 구매 모드:
/// - ShopItemData 기준
/// - 구매 가격 표시
/// - 클릭 시 실제 구매
///
/// 판매 모드:
/// - InventoryItem 기준
/// - 판매 가격 표시
/// - 현재 단계에서는 표시만 하고 실제 판매는 아직 하지 않음
/// </summary>
public class ShopItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Shape")]
    [SerializeField] private RectTransform shapeRoot;

    [Header("Settings")]
    [SerializeField] private float cellSize = 60f;
    [SerializeField] private float cellPadding = 6f;


    // =========================================================
    // Runtime Data
    // =========================================================

    private ShopItemData shopItemData;
    private InventoryItem inventoryItem;
    private ItemData itemData;

    private ShopUIManager.ShopMode currentMode =
        ShopUIManager.ShopMode.Buy;

    // 구매 중복 클릭 방지
    private bool isProcessingPurchase = false;

    private readonly List<GameObject> cellVisuals =
        new List<GameObject>();


    // =========================================================
    // Init - Buy
    // =========================================================

    /// <summary>
    /// 구매 상품 초기화.
    /// 기존 구매 기능에서 사용한다.
    /// </summary>
    public void Init(
        ShopItemData shopData,
        ItemData data)
    {
        currentMode =
            ShopUIManager.ShopMode.Buy;

        shopItemData = shopData;
        inventoryItem = null;
        itemData = data;

        isProcessingPurchase = false;

        RefreshText();
        RebuildShapeVisual();
    }


    // =========================================================
    // Init - Sell
    // =========================================================

    /// <summary>
    /// 판매 상품 초기화.
    /// 실제 플레이어 인벤토리의 InventoryItem 하나를 받는다.
    /// </summary>
    public void InitSell(
        InventoryItem item)
    {
        currentMode =
            ShopUIManager.ShopMode.Sell;

        shopItemData = null;
        inventoryItem = item;

        if (inventoryItem != null)
        {
            itemData =
                inventoryItem.data;
        }
        else
        {
            itemData = null;
        }

        isProcessingPurchase = false;

        RefreshText();
        RebuildShapeVisual();
    }


    // =========================================================
    // Text
    // =========================================================

    private void RefreshText()
    {
        if (itemData == null)
            return;

        if (itemNameText != null)
        {
            itemNameText.text =
                itemData.itemName;
        }

        if (priceText != null)
        {
            if (currentMode ==
                ShopUIManager.ShopMode.Buy)
            {
                priceText.text =
                    itemData.buyPrice.ToString();
            }
            else
            {
                priceText.text =
                    itemData.sellPrice.ToString();
            }
        }
    }


    // =========================================================
    // Shape Visual
    // =========================================================

    private void RebuildShapeVisual()
    {
        ClearShapeVisual();

        if (itemData == null)
            return;

        if (shapeRoot == null)
            return;

        if (itemData.shape == null ||
            itemData.shape.Count == 0)
        {
            Debug.LogWarning(
                "[ShopItemUI] 아이템 Shape가 없습니다.\n" +
                $"ItemId: {itemData.id}"
            );

            return;
        }


        // -----------------------------------------------------
        // Shape 크기 계산
        // -----------------------------------------------------

        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int cell in itemData.shape)
        {
            if (cell.x > maxX)
                maxX = cell.x;

            if (cell.y > maxY)
                maxY = cell.y;
        }

        int width =
            maxX + 1;

        int height =
            maxY + 1;


        shapeRoot.sizeDelta =
            new Vector2(
                width * cellSize,
                height * cellSize
            );


        float startX =
            -(width * cellSize) / 2f +
            cellSize / 2f;

        float startY =
            (height * cellSize) / 2f -
            cellSize / 2f;


        // -----------------------------------------------------
        // Shape 셀 생성
        // -----------------------------------------------------

        foreach (Vector2Int cell in itemData.shape)
        {
            GameObject cellObject =
                new GameObject(
                    "ShopItemCell_" +
                    cell.x +
                    "_" +
                    cell.y
                );


            cellObject.transform.SetParent(
                shapeRoot,
                false
            );


            Image image =
                cellObject.AddComponent<Image>();


            image.sprite =
                itemData.icon;

            image.color =
                Color.white;


            // 클릭은 부모 ShopItemUI가 받는다.
            image.raycastTarget =
                false;


            RectTransform rect =
                cellObject.GetComponent<RectTransform>();


            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );


            rect.sizeDelta =
                new Vector2(
                    cellSize - cellPadding,
                    cellSize - cellPadding
                );


            rect.anchoredPosition =
                new Vector2(
                    startX +
                    cell.x * cellSize,

                    startY -
                    cell.y * cellSize
                );


            Outline outline =
                cellObject.AddComponent<Outline>();


            outline.effectColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.8f
                );


            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                );


            cellVisuals.Add(
                cellObject
            );
        }
    }


    // =========================================================
    // Click
    // =========================================================

    public void OnPointerClick(
        PointerEventData eventData)
    {
        // 왼쪽 클릭만 처리
        if (eventData.button !=
            PointerEventData.InputButton.Left)
        {
            return;
        }


        // -----------------------------------------------------
        // 구매 모드
        // -----------------------------------------------------

        if (currentMode ==
            ShopUIManager.ShopMode.Buy)
        {
            HandlePurchaseClick();
            return;
        }


        // -----------------------------------------------------
        // 판매 모드
        // -----------------------------------------------------

        if (currentMode ==
            ShopUIManager.ShopMode.Sell)
        {
            HandleSellClick();
        }
    }


    // =========================================================
    // Purchase
    // =========================================================

    private void HandlePurchaseClick()
    {
        if (shopItemData == null ||
            itemData == null)
        {
            Debug.LogWarning(
                "[ShopItemUI] 상품 데이터가 없어 구매 확인창을 열 수 없습니다."
            );

            return;
        }


        if (ShopUIManager.Instance == null)
        {
            Debug.LogError(
                "[ShopItemUI] ShopUIManager.Instance가 없습니다."
            );

            return;
        }


        Debug.Log(
            "[ShopItemUI] 구매 상품 선택\n" +
            $"ShopItemId: {shopItemData.shopItemId}\n" +
            $"ItemId: {itemData.id}\n" +
            $"ItemName: {itemData.itemName}\n" +
            $"BuyPrice: {itemData.buyPrice}"
        );


        // 실제 구매는 하지 않는다.
        // 기획서대로 구매 상세 UI를 먼저 연다.
        ShopUIManager.Instance.OpenBuyConfirm(
            shopItemData,
            itemData
        );
    }


    // =========================================================
    // Sell
    // =========================================================

    // =========================================================
    // Sell
    // =========================================================

    private void HandleSellClick()
    {
        if (inventoryItem == null ||
            itemData == null)
        {
            Debug.LogWarning(
                "[ShopItemUI] 판매할 인벤토리 아이템 데이터가 없습니다."
            );

            return;
        }


        // -----------------------------------------------------
        // 판매 가능 가격 확인
        // -----------------------------------------------------

        int sellPrice =
            itemData.sellPrice;


        if (sellPrice <= 0)
        {
            Debug.LogWarning(
                "[ShopItemUI] 판매할 수 없는 아이템입니다.\n" +
                $"ItemId: {itemData.id}\n" +
                $"ItemName: {itemData.itemName}\n" +
                $"SellPrice: {sellPrice}"
            );

            return;
        }


        // -----------------------------------------------------
        // 필수 Manager 확인
        // -----------------------------------------------------

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[ShopItemUI] InventoryManager.Instance가 없습니다."
            );

            return;
        }


        if (SolastoneManager.Instance == null)
        {
            Debug.LogError(
                "[ShopItemUI] SolastoneManager.Instance가 없습니다."
            );

            return;
        }


        // -----------------------------------------------------
        // 실제 인벤토리에 아직 존재하는지 확인
        // -----------------------------------------------------

        if (!InventoryManager.Instance.ContainsItem(inventoryItem))
        {
            Debug.LogWarning(
                "[ShopItemUI] 판매하려는 아이템이 " +
                "더 이상 인벤토리에 존재하지 않습니다.\n" +
                $"ItemId: {itemData.id}\n" +
                $"ItemName: {itemData.itemName}"
            );

            if (ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.RefreshItemDisplay();
            }

            return;
        }


        Debug.Log(
            "[ShopItemUI] 아이템 판매 요청\n" +
            $"ItemId: {itemData.id}\n" +
            $"ItemName: {itemData.itemName}\n" +
            $"SellPrice: {sellPrice}"
        );


        // -----------------------------------------------------
        // 인벤토리에서 아이템 제거
        // -----------------------------------------------------

        InventoryItem soldItem =
            inventoryItem;


        InventoryManager.Instance.RemoveItem(
            soldItem
        );


        // -----------------------------------------------------
        // 솔라스톤 지급
        // -----------------------------------------------------

        SolastoneManager.Instance.Add(
            sellPrice
        );


        Debug.Log(
            "[ShopItemUI] 아이템 판매 완료\n" +
            $"ItemId: {itemData.id}\n" +
            $"ItemName: {itemData.itemName}\n" +
            $"획득 Solastone: {sellPrice}"
        );


        // 이미 판매된 InventoryItem을
        // 다시 클릭하지 못하도록 참조 제거
        inventoryItem = null;


        // -----------------------------------------------------
        // 판매 목록 갱신
        // -----------------------------------------------------

        if (ShopUIManager.Instance != null)
        {
            ShopUIManager.Instance.RefreshItemDisplay();
        }
        else
        {
            Debug.LogWarning(
                "[ShopItemUI] ShopUIManager.Instance가 없어 " +
                "판매 목록을 즉시 갱신하지 못했습니다."
            );
        }
    }
    // =========================================================
    // Clear
    // =========================================================

    private void ClearShapeVisual()
    {
        foreach (GameObject obj in cellVisuals)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        cellVisuals.Clear();
    }


    // =========================================================
    // Public Getters
    // =========================================================

    public int GetShopItemId()
    {
        if (shopItemData == null)
            return -1;

        return shopItemData.shopItemId;
    }


    public int GetItemId()
    {
        if (itemData == null)
            return -1;

        return itemData.id;
    }


    public ItemData GetItemData()
    {
        return itemData;
    }


    public InventoryItem GetInventoryItem()
    {
        return inventoryItem;
    }


    public bool IsBuyMode()
    {
        return currentMode ==
               ShopUIManager.ShopMode.Buy;
    }


    public bool IsSellMode()
    {
        return currentMode ==
               ShopUIManager.ShopMode.Sell;
    }
}