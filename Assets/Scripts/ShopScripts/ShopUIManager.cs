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

    [SerializeField]
    private float shopCellSize = 60f;

    [SerializeField]
    private float shopCellGap = 0f;

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

    // 판매 모드에서 현재 선택한 실제 인벤토리 아이템
    private InventoryItem selectedSellInventoryItem;

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
    // Confirm Button Text
    // =========================================================

    /// <summary>
    /// BuyConfirmPanel 안의 BuyButton 텍스트를
    /// 현재 동작에 맞게 구매/판매로 바꾼다.
    /// Inspector에 별도 TMP_Text 연결이 없어도 동작한다.
    /// </summary>
    private void SetConfirmButtonText(string text)
    {
        if (buyConfirmPanel == null)
            return;

        Transform buyButtonTransform =
            buyConfirmPanel.transform.Find("BuyButton");

        if (buyButtonTransform == null)
        {
            Debug.LogWarning(
                "[ShopUIManager] BuyConfirmPanel 아래에서 BuyButton을 찾지 못했습니다."
            );
            return;
        }

        TMP_Text buttonText =
            buyButtonTransform.GetComponentInChildren<TMP_Text>(
                true
            );

        if (buttonText == null)
        {
            Debug.LogWarning(
                "[ShopUIManager] BuyButton 아래에서 TMP_Text를 찾지 못했습니다."
            );
            return;
        }

        buttonText.text = text;
    }


    // =========================================================
    // Buy Confirm
    // =========================================================

    /// <summary>
    /// 구매 상품을 클릭했을 때 기획서의 구매 상세 UI를 연다.
    /// 실제 구매는 여기서 하지 않고 ConfirmPurchase()에서 처리한다.
    /// </summary>
    /// <summary>
    /// ShopInventoryItemUI에서 선택한 구매 상품을
    /// 기존 구매 확인 UI로 전달한다.
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

        SetConfirmButtonText("구매");

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
        selectedSellInventoryItem = null;

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
        // 기존 BuyConfirmPanel의 구매 버튼 OnClick 연결을 그대로 사용한다.
        // 판매 모드에서는 같은 버튼이 판매 확정 버튼으로 동작한다.
        if (currentMode == ShopMode.Sell)
        {
            ConfirmSale();
            return;
        }

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
    // Sell Confirm
    // =========================================================

    /// <summary>
    /// 판매 화면의 실제 InventoryItem을 클릭했을 때 호출한다.
    /// sellPrice가 0 이하인 아이템은 확인창을 열지 않는다.
    /// </summary>
    public void SelectSellItem(InventoryItem inventoryItem)
    {
        if (currentMode != ShopMode.Sell)
            return;

        if (inventoryItem == null ||
            inventoryItem.data == null)
        {
            return;
        }

        ItemData itemData =
            inventoryItem.data;

        if (itemData.sellPrice <= 0)
        {
            ShowNotice(
                "판매할 수 없는 아이템입니다.",
                true
            );
            return;
        }

        if (InventoryManager.Instance == null ||
            !InventoryManager.Instance.ContainsItem(inventoryItem))
        {
            ShowNotice(
                "인벤토리에 존재하지 않는 아이템입니다.",
                true
            );

            RefreshItemDisplay();
            return;
        }

        if (buyConfirmPanel == null)
        {
            Debug.LogError(
                "[ShopUIManager] BuyConfirmPanel이 연결되어 있지 않습니다."
            );
            return;
        }

        selectedBuyShopItem = null;
        selectedBuyItemData = null;
        selectedSellInventoryItem =
            inventoryItem;

        if (buyConfirmItemNameText != null)
        {
            buyConfirmItemNameText.text =
                itemData.itemName;
        }

        if (buyConfirmDescriptionText != null)
        {
            buyConfirmDescriptionText.text =
                itemData.effectDescription;
        }

        if (buyConfirmPriceText != null)
        {
            buyConfirmPriceText.text =
                itemData.sellPrice.ToString();
        }

        SetConfirmButtonText("판매");

        buyConfirmPanel.SetActive(true);

        Debug.Log(
            "[ShopUIManager] 판매 확인 UI 열기\n" +
            $"ItemId: {itemData.id}\n" +
            $"ItemName: {itemData.itemName}\n" +
            $"SellPrice: {itemData.sellPrice}"
        );
    }


    /// <summary>
    /// 판매 확인창의 기존 구매 버튼을 눌렀을 때
    /// 판매 모드라면 여기로 들어온다.
    /// </summary>
    private void ConfirmSale()
    {
        if (selectedSellInventoryItem == null ||
            selectedSellInventoryItem.data == null)
        {
            Debug.LogWarning(
                "[ShopUIManager] 선택된 판매 아이템이 없습니다."
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

        if (SolastoneManager.Instance == null)
        {
            Debug.LogError(
                "[ShopUIManager] SolastoneManager.Instance가 없습니다."
            );
            return;
        }

        InventoryItem soldItem =
            selectedSellInventoryItem;

        ItemData itemData =
            soldItem.data;

        int sellPrice =
            itemData.sellPrice;

        if (sellPrice <= 0)
        {
            CloseBuyConfirm();

            ShowNotice(
                "판매할 수 없는 아이템입니다.",
                true
            );
            return;
        }

        if (!InventoryManager.Instance.ContainsItem(soldItem))
        {
            CloseBuyConfirm();

            ShowNotice(
                "인벤토리에 존재하지 않는 아이템입니다.",
                true
            );

            RefreshItemDisplay();
            return;
        }

        string itemName =
            itemData.itemName;

        int itemId =
            itemData.id;

        // 기획대로 판매한 아이템은 상점 재고로 돌아가지 않는다.
        InventoryManager.Instance.RemoveItem(
            soldItem
        );

        SolastoneManager.Instance.Add(
            sellPrice
        );

        Debug.Log(
            "[ShopUIManager] 판매 완료\n" +
            $"ItemId: {itemId}\n" +
            $"ItemName: {itemName}\n" +
            $"SellPrice: {sellPrice}\n" +
            $"Current Solastone: {SolastoneManager.Instance.CurrentAmount}"
        );

        CloseBuyConfirm();
        RefreshSolastone();
        RefreshItemDisplay();

        ShowNotice(
            $"“{itemName}”을 판매했습니다.",
            false
        );
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
        if (itemGrid == null)
            return;

        int width = Mathf.Max(1, shopGridWidth);
        int height = Mathf.Max(1, shopGridHeight);

        float step = shopCellSize + shopCellGap;

        // ShopGridRoot의 크기/Anchor/Pivot 상태와 무관하게
        // 8x5 그리드 전체를 자기 중심 기준으로 직접 배치한다.
        float totalWidth =
            width * shopCellSize +
            (width - 1) * shopCellGap;

        float totalHeight =
            height * shopCellSize +
            (height - 1) * shopCellGap;

        float startX =
            -totalWidth / 2f +
            shopCellSize / 2f +
            shopGridOffset.x;

        float startY =
            totalHeight / 2f -
            shopCellSize / 2f +
            shopGridOffset.y;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject slot =
                    new GameObject(
                        "ShopSlot_" + x + "_" + y,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image)
                    );

                slot.transform.SetParent(
                    itemGrid,
                    false
                );

                RectTransform rect =
                    slot.GetComponent<RectTransform>();

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
                    new Vector2(
                        startX + x * step,
                        startY - y * step
                    );

                Image image =
                    slot.GetComponent<Image>();

                // 기획서의 어두운 인벤토리 칸처럼 보이도록
                // 별도 Sprite 없이 색만 사용한다.
                image.sprite = null;
                image.color =
                    new Color(
                        0.12f,
                        0.12f,
                        0.12f,
                        0.72f
                    );

                image.raycastTarget = false;

                Outline outline =
                    slot.AddComponent<Outline>();

                outline.effectColor =
                    new Color(
                        0f,
                        0f,
                        0f,
                        0.9f
                    );

                outline.effectDistance =
                    new Vector2(1f, -1f);

                spawnedShopSlots.Add(slot);
            }
        }

        Debug.Log(
            "[ShopUIManager] 상점 전용 슬롯 생성 완료: " +
            (width * height)
        );
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
        int width = Mathf.Max(1, shopGridWidth);
        int height = Mathf.Max(1, shopGridHeight);

        float step =
            shopCellSize + shopCellGap;

        float totalWidth =
            width * shopCellSize +
            (width - 1) * shopCellGap;

        float totalHeight =
            height * shopCellSize +
            (height - 1) * shopCellGap;

        float startX =
            -totalWidth / 2f +
            shopCellSize / 2f +
            shopGridOffset.x;

        float startY =
            totalHeight / 2f -
            shopCellSize / 2f +
            shopGridOffset.y;

        return new Vector2(
            startX + cell.x * step,
            startY - cell.y * step
        );
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

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[ShopUIManager] InventoryManager.Instance가 없습니다."
            );
            return;
        }

        // -----------------------------------------------------
        // 판매 화면도 구매 화면과 같은 8x5 슬롯을 사용한다.
        // 기존 ShopItemUI 카드 목록은 더 이상 만들지 않는다.
        // -----------------------------------------------------
        BuildShopSlots();

        List<InventoryItem> inventoryItems =
            InventoryManager.Instance.items;

        if (inventoryItems == null ||
            inventoryItems.Count == 0)
        {
            Debug.Log(
                "[ShopUIManager] 판매 화면 인벤토리 표시 완료 / 아이템 0개"
            );
            return;
        }

        int createdItemCount = 0;
        int createdCellCount = 0;
        int skippedCount = 0;

        foreach (InventoryItem inventoryItem in inventoryItems)
        {
            if (inventoryItem == null ||
                inventoryItem.data == null)
            {
                skippedCount++;
                continue;
            }

            ItemData itemData =
                inventoryItem.data;

            // 중요:
            // sellPrice == 0이어도 플레이어 인벤토리에서는 숨기지 않는다.
            // 현재 단계에서는 실제 인벤토리 모양을 그대로 보여주는 것만 한다.
            //
            // GetOccupiedCells(position)은 InventoryItem의 현재 rotation을
            // 반영한 실제 점유 칸을 돌려주므로 별도로 회전 계산하지 않는다.
            List<Vector2Int> occupiedCells =
                inventoryItem.GetOccupiedCells(
                    inventoryItem.position
                );

            if (occupiedCells == null ||
                occupiedCells.Count == 0)
            {
                skippedCount++;
                continue;
            }

            foreach (Vector2Int cell in occupiedCells)
            {
                // 저장 데이터가 잘못되어 인벤토리 범위를 벗어난 경우
                // UI가 화면 밖에 생기지 않도록 건너뛴다.
                if (cell.x < 0 ||
                    cell.x >= shopGridWidth ||
                    cell.y < 0 ||
                    cell.y >= shopGridHeight)
                {
                    Debug.LogWarning(
                        "[ShopUIManager] 판매 화면 아이템 점유 칀이 " +
                        "상점 그리드 범위를 벗어났습니다.\n" +
                        $"ItemId: {itemData.id}\n" +
                        $"ItemName: {itemData.itemName}\n" +
                        $"Cell: {cell}"
                    );

                    continue;
                }

                GameObject cellObject =
                    new GameObject(
                        "SellInventoryCell_" +
                        itemData.id + "_" +
                        cell.x + "_" +
                        cell.y,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image),
                        typeof(Button)
                    );

                cellObject.transform.SetParent(
                    itemGrid,
                    false
                );

                RectTransform rect =
                    cellObject.GetComponent<RectTransform>();

                rect.anchorMin =
                    new Vector2(0.5f, 0.5f);
                rect.anchorMax =
                    new Vector2(0.5f, 0.5f);
                rect.pivot =
                    new Vector2(0.5f, 0.5f);

                // 기존 InventoryItemUI가 셀보다 6 작게 그리던 방식과 동일
                float visualCellSize =
                    Mathf.Max(1f, shopCellSize - 6f);

                rect.sizeDelta =
                    new Vector2(
                        visualCellSize,
                        visualCellSize
                    );

                rect.anchoredPosition =
                    ShopCellToLocalPosition(cell);

                Image image =
                    cellObject.GetComponent<Image>();

                image.sprite =
                    itemData.icon;

                image.preserveAspect = true;

                image.color = Color.white;

                // 판매 화면에서는 드래그하지 않고 클릭만 받는다.
                image.raycastTarget = true;

                Button button =
                    cellObject.GetComponent<Button>();

                button.transition =
                    Selectable.Transition.None;

                InventoryItem capturedItem =
                    inventoryItem;

                button.onClick.AddListener(
                    () => SelectSellItem(capturedItem)
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
                    new Vector2(1f, -1f);

                // ClearItemDisplay()에서 함께 정리되도록
                // 기존 슬롯 리스트에 등록한다.
                spawnedShopSlots.Add(
                    cellObject
                );

                cellObject.transform.SetAsLastSibling();

                createdCellCount++;
            }

            createdItemCount++;
        }

        Debug.Log(
            "[ShopUIManager] 판매 화면 실제 인벤토리 표시 완료\n" +
            $"인벤토리 전체 아이템: {inventoryItems.Count}\n" +
            $"표시 아이템: {createdItemCount}\n" +
            $"표시 Shape 셀: {createdCellCount}\n" +
            $"잘못된 데이터: {skippedCount}"
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