using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LobbyScene의 상점 전체 화면 UI 관리자.
///
/// 기능:
/// - 상점 열기 / 닫기
/// - 구매 / 판매 모드 전환
/// - 현재 솔라스톤 표시
/// - 구매 모드에서 현재 상점 재고 표시
/// - 판매 모드에서 판매 가능한 플레이어 인벤토리 표시
/// </summary>
public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    public enum ShopMode
    {
        Buy,
        Sell
    }


    // =========================================================
    // Inspector
    // =========================================================

    [Header("Root")]
    [SerializeField]
    private GameObject shopRoot;

    [Header("Mode")]
    [SerializeField]
    private TMP_Text modeButtonText;

    [Header("Currency")]
    [SerializeField]
    private TMP_Text solastoneText;

    [Header("Shop Inventory Grid")]
    [SerializeField]
    private RectTransform itemGrid;

    [SerializeField]
    private GameObject shopSlotPrefab;

    [Header("Shop Grid Settings")]
    [SerializeField]
    private int shopGridWidth = 8;

    [SerializeField]
    private int shopGridHeight = 5;

    [Header("Shop Grid Size - Inspector에서 직접 조절")]
    [Min(20f)]
    public float shopCellSize = 110f;

    [Min(0f)]
    public float shopCellGap = 4f;

    [SerializeField]
    private Vector2 shopGridOffset = Vector2.zero;

    [Header("Legacy Sell Display")]
    [SerializeField]
    private ShopItemUI shopItemPrefab;

    [SerializeField]
    private int columns = 3;

    [SerializeField]
    private float itemWidth = 260f;

    [SerializeField]
    private float itemHeight = 240f;

    [SerializeField]
    private float spacingX = 25f;

    [SerializeField]
    private float spacingY = 30f;

    [SerializeField]
    private bool centerIncompleteRow = true;

    [Header("Settings")]
    [SerializeField]
    private bool hideOnStart = true;

    [Header("Buy Confirm")]
    [SerializeField]
    private GameObject buyConfirmPanel;

    [SerializeField]
    private TMP_Text buyConfirmItemNameText;

    [SerializeField]
    private TMP_Text buyConfirmDescriptionText;

    [SerializeField]
    private TMP_Text buyConfirmPriceText;

    [Header("Notice")]
    [SerializeField] private TMP_Text noticeText;
    [SerializeField] private float noticeDuration = 2f;

    private Coroutine noticeCoroutine;


    // =========================================================
    // Runtime
    // =========================================================

    private ShopMode currentMode = ShopMode.Buy;

    private ShopItemData selectedBuyShopItem;
    private ItemData selectedBuyItemData;

    private readonly List<ShopItemUI> spawnedShopItems =
        new List<ShopItemUI>();

    private readonly List<GameObject> spawnedShopSlots =
        new List<GameObject>();

    private readonly List<ShopInventoryItemUI> spawnedShopInventoryItems =
        new List<ShopInventoryItemUI>();

    private bool[,] shopOccupied;

    public ShopMode CurrentMode => currentMode;

    public bool IsShopOpen =>
        shopRoot != null &&
        shopRoot.activeSelf;


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

        // shopCellSize / shopCellGap은 Inspector에서 설정한 값을 그대로 사용한다.
    }


    private void Start()
    {
        SetMode(ShopMode.Buy);

        RefreshSolastone();

        if (buyConfirmPanel != null)
        {
            buyConfirmPanel.SetActive(false);
        }

        if (noticeText != null)
        {
            noticeText.gameObject.SetActive(false);
        }

        if (hideOnStart &&
            shopRoot != null)
        {
            shopRoot.SetActive(false);
        }
    }


    private void OnEnable()
    {
        SubscribeSolastoneEvent();
    }


    private void OnDisable()
    {
        UnsubscribeSolastoneEvent();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnsubscribeSolastoneEvent();
    }


    // =========================================================
    // Open / Close
    // =========================================================

    public void OpenShop()
    {
        if (shopRoot == null)
        {
            Debug.LogError(
                "[ShopUIManager] ShopRoot가 연결되어 있지 않습니다."
            );

            return;
        }

        shopRoot.SetActive(true);

        CloseBuyConfirm();

        SubscribeSolastoneEvent();

        RefreshSolastone();

        SetMode(ShopMode.Buy);

        Debug.Log(
            "[ShopUIManager] 상점 열기"
        );
    }


    public void CloseShop()
    {
        if (shopRoot == null)
            return;

        CloseBuyConfirm();

        shopRoot.SetActive(false);

        Debug.Log(
            "[ShopUIManager] 상점 닫기"
        );
    }


    // =========================================================
    // Mode
    // =========================================================

    public void ToggleMode()
    {
        if (currentMode == ShopMode.Buy)
        {
            SetMode(ShopMode.Sell);
        }
        else
        {
            SetMode(ShopMode.Buy);
        }
    }


    public void SetMode(ShopMode mode)
    {
        CloseBuyConfirm();

        currentMode = mode;

        RefreshModeUI();
        RefreshItemDisplay();

        Debug.Log(
            "[ShopUIManager] 상점 모드 변경: " +
            currentMode
        );
    }


    private void RefreshModeUI()
    {
        if (modeButtonText == null)
            return;

        if (currentMode == ShopMode.Buy)
        {
            modeButtonText.text =
                "판매하기";
        }
        else
        {
            modeButtonText.text =
                "구매하기";
        }
    }


    // =========================================================
    // Buy Confirm
    // =========================================================

    /// <summary>
    /// 구매 상품을 클릭했을 때 기획서의 구매 상세 UI를 연다.
    /// 실제 구매는 여기서 하지 않고 ConfirmPurchase()에서 처리한다.
    /// </summary>
    public void SelectBuyItem(
        ShopItemData shopItem,
        ItemData item)
    {
        OpenBuyConfirm(shopItem, item);
    }


    public void OpenBuyConfirm(
        ShopItemData shopItem,
        ItemData item)
    {
        if (shopItem == null ||
            item == null)
        {
            Debug.LogWarning(
                "[ShopUIManager] 구매 확인창을 열 상품 데이터가 없습니다."
            );

            return;
        }

        if (buyConfirmPanel == null)
        {
            Debug.LogError(
                "[ShopUIManager] BuyConfirmPanel이 연결되어 있지 않습니다."
            );

            return;
        }

        selectedBuyShopItem = shopItem;
        selectedBuyItemData = item;

        if (buyConfirmItemNameText != null)
        {
            buyConfirmItemNameText.text =
                item.itemName;
        }

        if (buyConfirmDescriptionText != null)
        {
            buyConfirmDescriptionText.text =
                item.effectDescription;
        }

        if (buyConfirmPriceText != null)
        {
            buyConfirmPriceText.text =
                item.buyPrice.ToString();
        }

        buyConfirmPanel.SetActive(true);

        Debug.Log(
            "[ShopUIManager] 구매 확인 UI 열기\n" +
            $"ShopItemId: {shopItem.shopItemId}\n" +
            $"ItemId: {item.id}\n" +
            $"ItemName: {item.itemName}\n" +
            $"BuyPrice: {item.buyPrice}"
        );
    }


    /// <summary>
    /// 구매 상세 UI의 X 버튼에서 호출한다.
    /// </summary>
    public void CloseBuyConfirm()
    {
        selectedBuyShopItem = null;
        selectedBuyItemData = null;

        if (buyConfirmPanel != null)
        {
            buyConfirmPanel.SetActive(false);
        }
    }


    /// <summary>
    /// 구매 상세 UI의 구매 버튼에서 호출한다.
    /// 기존 ShopPurchaseManager의 실제 구매 로직을 그대로 사용한다.
    /// </summary>
    public void ConfirmPurchase()
    {
        if (selectedBuyShopItem == null || selectedBuyItemData == null)
        {
            Debug.LogWarning("[ShopUIManager] 선택된 구매 상품이 없습니다.");
            return;
        }

        if (ShopPurchaseManager.Instance == null)
        {
            Debug.LogError("[ShopUIManager] ShopPurchaseManager.Instance가 없습니다.");
            return;
        }

        if (SolastoneManager.Instance == null)
        {
            Debug.LogError("[ShopUIManager] SolastoneManager.Instance가 없습니다.");
            return;
        }

        int shopItemId = selectedBuyShopItem.shopItemId;
        int itemId = selectedBuyItemData.id;
        string itemName = selectedBuyItemData.itemName;
        int buyPrice = selectedBuyItemData.buyPrice;

        if (!SolastoneManager.Instance.CanAfford(buyPrice))
        {
            ShowNotice("솔라스톤이 부족합니다.", true);
            return;
        }

        bool success = ShopPurchaseManager.Instance.Purchase(shopItemId);

        if (!success)
        {
            Debug.Log(
                "[ShopUIManager] 구매 실패\n" +
                $"ShopItemId: {shopItemId}\n" +
                $"ItemId: {itemId}\n" +
                $"ItemName: {itemName}"
            );

            ShowNotice("인벤토리 칸이 부족합니다.", true);
            return;
        }

        Debug.Log(
            "[ShopUIManager] 구매 성공\n" +
            $"ShopItemId: {shopItemId}\n" +
            $"ItemId: {itemId}\n" +
            $"ItemName: {itemName}"
        );

        CloseBuyConfirm();
        RefreshSolastone();
        RefreshItemDisplay();

        ShowNotice($"“{itemName}”을 구매했습니다.", false);
    }


    // =========================================================
    // Notice
    // =========================================================

    private void ShowNotice(string message, bool isFailure)
    {
        if (noticeText == null)
        {
            Debug.LogWarning(
                "[ShopUIManager] NoticeText가 연결되어 있지 않습니다.\n" +
                $"표시하려던 문구: {message}"
            );
            return;
        }

        if (noticeCoroutine != null)
        {
            StopCoroutine(noticeCoroutine);
            noticeCoroutine = null;
        }

        noticeText.text = message;
        noticeText.color = isFailure ? Color.red : Color.white;
        noticeText.gameObject.SetActive(true);

        noticeCoroutine = StartCoroutine(HideNoticeAfterDelay());
    }


    private IEnumerator HideNoticeAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, noticeDuration));

        if (noticeText != null)
        {
            noticeText.gameObject.SetActive(false);
        }

        noticeCoroutine = null;
    }


    // =========================================================
    // Item Display
    // =========================================================

    /// <summary>
    /// 현재 구매/판매 모드에 맞게
    /// ItemGrid의 내용을 다시 만든다.
    /// </summary>
    public void RefreshItemDisplay()
    {
        ClearItemDisplay();

        switch (currentMode)
        {
            case ShopMode.Buy:

                BuildBuyItemDisplay();
                break;


            case ShopMode.Sell:

                BuildSellItemDisplay();
                break;
        }
    }


    // =========================================================
    // Buy Display
    // =========================================================

    /// <summary>
    /// ShopManager의 현재 재고를 읽어서
    /// 구매 상품 UI를 생성한다.
    /// </summary>
    private void BuildBuyItemDisplay()
    {
        if (itemGrid == null)
        {
            Debug.LogError(
                "[ShopUIManager] ShopGridRoot(Item Grid)가 연결되어 있지 않습니다."
            );
            return;
        }

        if (shopSlotPrefab == null)
        {
            Debug.LogError(
                "[ShopUIManager] Shop Slot Prefab이 연결되어 있지 않습니다."
            );
            return;
        }

        if (ShopManager.Instance == null ||
            ShopDatabase.Instance == null ||
            ItemDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopUIManager] 상점 데이터 Manager 중 하나가 없습니다."
            );
            return;
        }

        if (!ShopManager.Instance.IsInitialized)
        {
            Debug.LogWarning(
                "[ShopUIManager] ShopManager가 아직 초기화되지 않았습니다."
            );
            return;
        }

        BuildShopSlots();

        List<ShopItemData> currentStock =
            ShopManager.Instance.GetCurrentStock();

        if (currentStock == null ||
            currentStock.Count == 0)
        {
            Debug.Log(
                "[ShopUIManager] 현재 상점 재고가 없습니다."
            );
            return;
        }

        shopOccupied =
            new bool[
                Mathf.Max(1, shopGridWidth),
                Mathf.Max(1, shopGridHeight)
            ];

        int createdCount = 0;

        foreach (ShopItemData shopItem in currentStock)
        {
            if (shopItem == null)
                continue;

            ItemData itemData =
                ItemDatabase.Instance.GetItem(
                    shopItem.sellItemId
                );

            if (itemData == null)
            {
                Debug.LogWarning(
                    "[ShopUIManager] ItemDatabase에 상품 아이템이 없습니다.\n" +
                    $"ShopItemId: {shopItem.shopItemId}\n" +
                    $"ItemId: {shopItem.sellItemId}"
                );
                continue;
            }

            if (!TryFindShopPosition(
                    itemData,
                    out Vector2Int position))
            {
                Debug.LogWarning(
                    "[ShopUIManager] 상점 그리드에 상품을 배치할 공간이 없습니다.\n" +
                    $"ShopItemId: {shopItem.shopItemId}\n" +
                    $"ItemId: {itemData.id}"
                );
                continue;
            }

            MarkShopOccupied(
                itemData,
                position
            );

            CreateShopInventoryItem(
                shopItem,
                itemData,
                position
            );

            createdCount++;
        }

        Debug.Log(
            "[ShopUIManager] 8x5 구매 그리드 생성 완료\n" +
            $"현재 재고: {currentStock.Count}\n" +
            $"생성 성공: {createdCount}"
        );
    }


    private void BuildShopSlots()
    {
        int width = Mathf.Max(1, shopGridWidth);
        int height = Mathf.Max(1, shopGridHeight);

        // 구매 인벤토리의 실제 8x5 배경 크기를 그리드 크기에 맞춘다.
        // 슬롯 배경(shopSlotPrefab)이 화면 뒤에 정상적으로 보이도록 한다.
        float gridWidth =
            width * shopCellSize +
            Mathf.Max(0, width - 1) * shopCellGap;

        float gridHeight =
            height * shopCellSize +
            Mathf.Max(0, height - 1) * shopCellGap;

        itemGrid.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            gridWidth
        );

        itemGrid.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            gridHeight
        );

        itemGrid.anchorMin = new Vector2(0.5f, 0.5f);
        itemGrid.anchorMax = new Vector2(0.5f, 0.5f);
        itemGrid.pivot = new Vector2(0.5f, 0.5f);
        itemGrid.anchoredPosition = shopGridOffset;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject slot =
                    Instantiate(
                        shopSlotPrefab,
                        itemGrid
                    );

                slot.name =
                    "ShopSlot_" + x + "_" + y;

                RectTransform rect =
                    slot.GetComponent<RectTransform>();

                if (rect != null)
                {
                    rect.anchorMin =
                        new Vector2(0.5f, 0.5f);
                    rect.anchorMax =
                        new Vector2(0.5f, 0.5f);
                    rect.pivot =
                        new Vector2(0.5f, 0.5f);
                    rect.sizeDelta =
                        new Vector2(
                            shopCellSize,
                            shopCellSize
                        );
                    rect.anchoredPosition =
                        ShopCellToLocalPosition(
                            new Vector2Int(x, y)
                        );
                }

                InventorySlotUI inventorySlot =
                    slot.GetComponent<InventorySlotUI>();

                if (inventorySlot != null)
                {
                    inventorySlot.enabled = false;
                }

                Image image =
                    slot.GetComponent<Image>();

                if (image != null)
                {
                    image.raycastTarget = false;
                }

                spawnedShopSlots.Add(slot);
            }
        }
    }


    private void CreateShopInventoryItem(
        ShopItemData shopItem,
        ItemData itemData,
        Vector2Int position)
    {
        GameObject itemObject =
            new GameObject(
                "ShopInventoryItem_" +
                shopItem.shopItemId +
                "_" +
                itemData.id,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(ShopInventoryItemUI)
            );

        itemObject.transform.SetParent(
            itemGrid,
            false
        );

        RectTransform rect =
            itemObject.GetComponent<RectTransform>();

        Vector2Int shapeSize =
            GetShopShapeSize(itemData);

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);
        rect.anchorMax =
            new Vector2(0.5f, 0.5f);
        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(
                shapeSize.x * shopCellSize +
                (shapeSize.x - 1) * shopCellGap,
                shapeSize.y * shopCellSize +
                (shapeSize.y - 1) * shopCellGap
            );

        Vector2 baseCellPosition =
            ShopCellToLocalPosition(position);

        float step =
            shopCellSize + shopCellGap;

        rect.anchoredPosition =
            new Vector2(
                baseCellPosition.x +
                ((shapeSize.x - 1) * step / 2f),
                baseCellPosition.y -
                ((shapeSize.y - 1) * step / 2f)
            );

        Image clickArea =
            itemObject.GetComponent<Image>();

        clickArea.color =
            new Color(1f, 1f, 1f, 0f);

        clickArea.raycastTarget = true;

        ShopInventoryItemUI ui =
            itemObject.GetComponent<ShopInventoryItemUI>();

        ui.Init(
            shopItem,
            itemData,
            shopCellSize
        );

        spawnedShopInventoryItems.Add(ui);

        itemObject.transform.SetAsLastSibling();
    }


    private bool TryFindShopPosition(
        ItemData itemData,
        out Vector2Int position)
    {
        int width = Mathf.Max(1, shopGridWidth);
        int height = Mathf.Max(1, shopGridHeight);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int candidate =
                    new Vector2Int(x, y);

                if (CanPlaceShopItem(
                        itemData,
                        candidate))
                {
                    position = candidate;
                    return true;
                }
            }
        }

        position =
            new Vector2Int(-1, -1);

        return false;
    }


    private bool CanPlaceShopItem(
        ItemData itemData,
        Vector2Int position)
    {
        if (itemData == null ||
            itemData.shape == null ||
            itemData.shape.Count == 0 ||
            shopOccupied == null)
        {
            return false;
        }

        int width = Mathf.Max(1, shopGridWidth);
        int height = Mathf.Max(1, shopGridHeight);

        foreach (Vector2Int cell in itemData.shape)
        {
            int x =
                position.x + cell.x;

            int y =
                position.y + cell.y;

            if (x < 0 ||
                x >= width ||
                y < 0 ||
                y >= height)
            {
                return false;
            }

            if (shopOccupied[x, y])
                return false;
        }

        return true;
    }


    private void MarkShopOccupied(
        ItemData itemData,
        Vector2Int position)
    {
        if (itemData == null ||
            itemData.shape == null ||
            shopOccupied == null)
        {
            return;
        }

        foreach (Vector2Int cell in itemData.shape)
        {
            int x =
                position.x + cell.x;

            int y =
                position.y + cell.y;

            if (x >= 0 &&
                x < shopOccupied.GetLength(0) &&
                y >= 0 &&
                y < shopOccupied.GetLength(1))
            {
                shopOccupied[x, y] = true;
            }
        }
    }


    private Vector2Int GetShopShapeSize(
        ItemData itemData)
    {
        if (itemData == null ||
            itemData.shape == null ||
            itemData.shape.Count == 0)
        {
            return Vector2Int.one;
        }

        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int cell in itemData.shape)
        {
            maxX =
                Mathf.Max(maxX, cell.x);

            maxY =
                Mathf.Max(maxY, cell.y);
        }

        return new Vector2Int(
            maxX + 1,
            maxY + 1
        );
    }


    private Vector2 ShopCellToLocalPosition(
        Vector2Int cell)
    {
        if (itemGrid == null)
            return Vector2.zero;

        Rect rect =
            itemGrid.rect;

        float step =
            shopCellSize + shopCellGap;

        float x =
            rect.xMin +
            shopCellSize / 2f +
            cell.x * step;

        float y =
            rect.yMax -
            shopCellSize / 2f -
            cell.y * step;

        return new Vector2(x, y);
    }


    // =========================================================
    // Sell Display
    // =========================================================

    /// <summary>
    /// 플레이어가 실제로 보유하고 있는 인벤토리 아이템 중
    /// 판매 가격이 1 이상인 아이템만 판매 목록에 표시한다.
    ///
    /// 같은 Item ID를 여러 개 가지고 있어도
    /// InventoryItem 각각을 별도의 판매 상품으로 생성한다.
    /// </summary>
    private void BuildSellItemDisplay()
    {
        if (itemGrid == null)
        {
            Debug.LogError(
                "[ShopUIManager] ItemGrid가 연결되어 있지 않습니다."
            );
            return;
        }

        if (shopItemPrefab == null)
        {
            Debug.LogError(
                "[ShopUIManager] ShopItemPrefab이 연결되어 있지 않습니다."
            );
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[ShopUIManager] InventoryManager.Instance가 없습니다."
            );
            return;
        }

        // 구매 화면과 완전히 같은 8x5 배경 슬롯을 먼저 만든다.
        BuildShopSlots();

        List<InventoryItem> inventoryItems =
            InventoryManager.Instance.items;

        if (inventoryItems == null ||
            inventoryItems.Count == 0)
        {
            Debug.Log(
                "[ShopUIManager] 판매 화면 인벤토리가 비어 있습니다."
            );
            return;
        }

        int createdCount = 0;

        foreach (InventoryItem inventoryItem in inventoryItems)
        {
            if (inventoryItem == null ||
                inventoryItem.data == null)
            {
                continue;
            }

            // 판매 불가 아이템도 인벤토리에는 보이게 한다.
            // 실제 클릭 시 ShopItemUI가 sellPrice <= 0을 검사한다.
            ShopItemUI ui =
                Instantiate(
                    shopItemPrefab,
                    itemGrid
                );

            ui.gameObject.SetActive(true);

            ui.name =
                "SellInventoryItem_" +
                inventoryItem.data.id +
                "_" +
                createdCount;

            // 상점 8x5 칸 크기와 아이템 Shape 크기를 동일하게 맞춘다.
            ui.SetCellSize(
                shopCellSize,
                Mathf.Min(6f, shopCellSize * 0.08f)
            );

            ui.InitSell(
                inventoryItem
            );

            PositionInventorySellItem(
                ui.GetComponent<RectTransform>(),
                inventoryItem
            );

            spawnedShopItems.Add(ui);
            createdCount++;
        }

        Debug.Log(
            "[ShopUIManager] 판매 화면 8x5 인벤토리 생성 완료\n" +
            $"현재 인벤토리 아이템: {createdCount}"
        );
    }


    private void PositionInventorySellItem(
        RectTransform rect,
        InventoryItem inventoryItem)
    {
        if (rect == null ||
            inventoryItem == null ||
            inventoryItem.data == null)
        {
            return;
        }

        int width = 1;
        int height = 1;

        if (inventoryItem.data.shape != null &&
            inventoryItem.data.shape.Count > 0)
        {
            int maxX = 0;
            int maxY = 0;

            foreach (Vector2Int cell in inventoryItem.data.shape)
            {
                if (cell.x > maxX) maxX = cell.x;
                if (cell.y > maxY) maxY = cell.y;
            }

            width = maxX + 1;
            height = maxY + 1;
        }

        float step =
            shopCellSize + shopCellGap;

        Rect gridRect =
            itemGrid.rect;

        float startX =
            gridRect.xMin +
            shopCellSize / 2f;

        float startY =
            gridRect.yMax -
            shopCellSize / 2f;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(
                width * shopCellSize +
                Mathf.Max(0, width - 1) * shopCellGap,

                height * shopCellSize +
                Mathf.Max(0, height - 1) * shopCellGap
            );

        rect.anchoredPosition =
            new Vector2(
                startX +
                inventoryItem.position.x * step +
                (width - 1) * step * 0.5f,

                startY -
                inventoryItem.position.y * step -
                (height - 1) * step * 0.5f
            );
    }


    private void RelayoutSpawnedItems()
    {
        int totalCount = spawnedShopItems.Count;

        for (int i = 0; i < totalCount; i++)
        {
            ShopItemUI ui = spawnedShopItems[i];

            if (ui == null)
                continue;

            PositionShopItemFinal(
                ui.GetComponent<RectTransform>(),
                i,
                totalCount
            );
        }
    }


    private void PositionShopItemFinal(
        RectTransform rect,
        int index,
        int totalCount)
    {
        if (rect == null)
            return;

        int safeColumns = Mathf.Max(1, columns);
        int column = index % safeColumns;
        int row = index / safeColumns;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(itemWidth, itemHeight);

        int rowStartIndex = row * safeColumns;

        int itemsInThisRow =
            Mathf.Min(
                safeColumns,
                Mathf.Max(
                    0,
                    totalCount - rowStartIndex
                )
            );

        float rowOffsetX = 0f;

        if (centerIncompleteRow &&
            itemsInThisRow > 0 &&
            itemsInThisRow < safeColumns)
        {
            float fullRowWidth =
                safeColumns * itemWidth +
                (safeColumns - 1) * spacingX;

            float currentRowWidth =
                itemsInThisRow * itemWidth +
                (itemsInThisRow - 1) * spacingX;

            rowOffsetX =
                (fullRowWidth - currentRowWidth) * 0.5f;
        }

        float x =
            rowOffsetX +
            column * (itemWidth + spacingX);

        float y =
            -row * (itemHeight + spacingY);

        rect.anchoredPosition =
            new Vector2(x, y);
    }


    // =========================================================
    // Position
    // =========================================================

    /// <summary>
    /// 상품 UI를 ItemGrid 내부에
    /// 일정한 간격으로 배치한다.
    ///
    /// GridLayoutGroup을 사용하지 않는 이유:
    /// 상점 상품 내부에서 아이템 Shape를 직접 표시하기 때문이다.
    /// </summary>
    private void PositionShopItem(
        RectTransform rect,
        int index)
    {
        if (rect == null)
            return;


        int safeColumns =
            Mathf.Max(1, columns);


        int column =
            index % safeColumns;


        int row =
            index / safeColumns;


        rect.anchorMin =
            new Vector2(0f, 1f);


        rect.anchorMax =
            new Vector2(0f, 1f);


        rect.pivot =
            new Vector2(0f, 1f);


        rect.sizeDelta =
            new Vector2(
                itemWidth,
                itemHeight
            );


        float x =
            column *
            (itemWidth + spacingX);


        float y =
            -row *
            (itemHeight + spacingY);


        rect.anchoredPosition =
            new Vector2(
                x,
                y
            );
    }


    // =========================================================
    // Clear
    // =========================================================

    /// <summary>
    /// 현재 ItemGrid에 생성했던 상품 UI를 제거한다.
    /// </summary>
    private void ClearItemDisplay()
    {
        foreach (ShopItemUI ui in spawnedShopItems)
        {
            if (ui != null)
            {
                Destroy(ui.gameObject);
            }
        }

        spawnedShopItems.Clear();


        foreach (
            ShopInventoryItemUI ui
            in spawnedShopInventoryItems)
        {
            if (ui != null)
            {
                Destroy(ui.gameObject);
            }
        }

        spawnedShopInventoryItems.Clear();


        foreach (
            GameObject slot
            in spawnedShopSlots)
        {
            if (slot != null)
            {
                Destroy(slot);
            }
        }

        spawnedShopSlots.Clear();

        shopOccupied = null;
    }


    // =========================================================
    // Solastone
    // =========================================================

    private void SubscribeSolastoneEvent()
    {
        if (SolastoneManager.Instance == null)
            return;


        SolastoneManager.Instance.OnAmountChanged -=
            HandleSolastoneChanged;


        SolastoneManager.Instance.OnAmountChanged +=
            HandleSolastoneChanged;
    }


    private void UnsubscribeSolastoneEvent()
    {
        if (SolastoneManager.Instance == null)
            return;


        SolastoneManager.Instance.OnAmountChanged -=
            HandleSolastoneChanged;
    }


    public void RefreshSolastone()
    {
        int amount = 0;


        if (SolastoneManager.Instance != null)
        {
            amount =
                SolastoneManager.Instance.CurrentAmount;
        }


        UpdateSolastoneText(
            amount
        );
    }


    private void HandleSolastoneChanged(
        int amount)
    {
        UpdateSolastoneText(
            amount
        );
    }


    private void UpdateSolastoneText(
        int amount)
    {
        if (solastoneText == null)
            return;


        solastoneText.text =
            $"솔라스톤 : {amount}";
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug/Open Shop")]
    private void DebugOpenShop()
    {
        OpenShop();
    }


    [ContextMenu("Debug/Close Shop")]
    private void DebugCloseShop()
    {
        CloseShop();
    }


    [ContextMenu("Debug/Toggle Buy Sell")]
    private void DebugToggleMode()
    {
        ToggleMode();
    }


    [ContextMenu("Debug/Refresh Solastone")]
    private void DebugRefreshSolastone()
    {
        RefreshSolastone();
    }


    [ContextMenu("Debug/Refresh Items")]
    private void DebugRefreshItems()
    {
        RefreshItemDisplay();
    }
}