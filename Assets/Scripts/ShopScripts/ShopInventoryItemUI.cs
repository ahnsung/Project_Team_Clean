using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopInventoryItemUI : MonoBehaviour,
    IPointerClickHandler
{
    [Header("Optional")]
    [SerializeField]
    private Image highlightImage;

    private ShopItemData shopItemData;
    private ItemData itemData;

    private RectTransform rectTransform;

    private float cellSize = 48f;

    private readonly List<GameObject> cellVisuals =
        new List<GameObject>();


    // =========================================================
    // Init
    // =========================================================

    public void Init(
        ShopItemData shopData,
        ItemData data,
        float newCellSize)
    {
        shopItemData = shopData;
        itemData = data;

        cellSize = newCellSize;

        rectTransform =
            GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            rectTransform =
                gameObject.AddComponent<RectTransform>();
        }

        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }

        RebuildShapeVisual();
    }


    // =========================================================
    // Click
    // =========================================================

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (shopItemData == null ||
            itemData == null)
        {
            return;
        }

        ShopUIManager shopUI =
            FindFirstObjectByType<ShopUIManager>();

        if (shopUI == null)
        {
            Debug.LogWarning(
                "[ShopInventoryItemUI] " +
                "ShopUIManager를 찾을 수 없습니다."
            );

            return;
        }

        /*
         * 기존에 완성해 둔
         *
         * 상품 클릭
         *      ↓
         * BuyConfirmPanel
         *
         * 흐름으로 전달한다.
         */
        shopUI.SelectBuyItem(
            shopItemData,
            itemData
        );

        Debug.Log(
            "[ShopInventoryItemUI] 구매 상품 선택\n" +
            "ShopItemId: " +
            shopItemData.shopItemId +
            "\nItemId: " +
            itemData.id
        );
    }


    // =========================================================
    // Shape
    // =========================================================

    private void RebuildShapeVisual()
    {
        ClearShapeVisual();

        if (itemData == null)
            return;

        if (itemData.shape == null ||
            itemData.shape.Count == 0)
        {
            Debug.LogWarning(
                "[ShopInventoryItemUI] " +
                "아이템 Shape가 없습니다.\n" +
                "ItemId: " +
                itemData.id
            );

            return;
        }

        List<Vector2Int> shape =
            itemData.shape;

        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int cell in shape)
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


        // =====================================================
        // Root Size
        // =====================================================

        if (rectTransform != null)
        {
            rectTransform.sizeDelta =
                new Vector2(
                    width * cellSize,
                    height * cellSize
                );
        }


        // =====================================================
        // Shape Start Position
        // =====================================================

        float startX =
            -(width * cellSize) / 2f +
            cellSize / 2f;

        float startY =
            (height * cellSize) / 2f -
            cellSize / 2f;


        // =====================================================
        // Cell Visual
        // =====================================================

        foreach (Vector2Int cell in shape)
        {
            GameObject cellObject =
                new GameObject(
                    "ShopItemCell_" +
                    cell.x +
                    "_" +
                    cell.y
                );

            cellObject.transform.SetParent(
                transform,
                false
            );


            Image image =
                cellObject.AddComponent<Image>();

            image.sprite =
                itemData.icon;

            image.color =
                Color.white;

            /*
             * 클릭은 부모
             * ShopInventoryItemUI가 받는다.
             */
            image.raycastTarget = false;


            RectTransform cellRect =
                cellObject.GetComponent<RectTransform>();

            cellRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            cellRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            cellRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            /*
             * 기존 InventoryItemUI와 동일하게
             * 슬롯보다 살짝 작게 표시.
             */
            cellRect.sizeDelta =
                new Vector2(
                    cellSize - 6f,
                    cellSize - 6f
                );

            cellRect.anchoredPosition =
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
    // Clear Shape
    // =========================================================

    private void ClearShapeVisual()
    {
        foreach (
            GameObject obj
            in cellVisuals)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        cellVisuals.Clear();
    }


    // =========================================================
    // Highlight
    // =========================================================

    public void SetHighlight(
        bool value)
    {
        foreach (
            GameObject obj
            in cellVisuals)
        {
            if (obj == null)
                continue;

            Image image =
                obj.GetComponent<Image>();

            if (image == null)
                continue;

            image.color =
                value
                    ? new Color(
                        1f,
                        0.9f,
                        0.45f,
                        1f
                    )
                    : Color.white;
        }
    }


    // =========================================================
    // Getters
    // =========================================================

    public ShopItemData GetShopItemData()
    {
        return shopItemData;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }
}