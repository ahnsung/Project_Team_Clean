using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점의 현재 진행 상태와 재고를 관리한다.
///
/// 담당 기능:
/// 1. 현재 상점 레벨 관리
/// 2. 현재 남아 있는 상점 재고 관리
/// 3. 구매한 ShopItemId 재고에서 제거
/// 4. Shop Update 시 해당 레벨 재고 추가
/// 5. 상점 상태 저장 / 불러오기
///
/// 실제 구매/판매 가격 계산과 UI는
/// 이후 별도 시스템에서 담당한다.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    private const string SHOP_LEVEL_KEY = "SHOP_CURRENT_LEVEL";
    private const string SHOP_STOCK_KEY = "SHOP_CURRENT_STOCK";
    private const string SHOP_INITIALIZED_KEY = "SHOP_INITIALIZED";

    // 이전 테스트에서 초기화 순서 문제로
    // Level 0 / Stock 0이 저장된 데이터를 구분하기 위한 키.
    private const string SHOP_VALID_INITIALIZATION_KEY =
        "SHOP_VALID_INITIALIZATION";

    [Header("Current Shop State")]
    [SerializeField]
    private int currentShopLevel = 0;

    /// <summary>
    /// 현재 상점에 남아 있는 shopItemId 목록.
    ///
    /// 실제 Item ID가 아니라
    /// ShopItemData.shopItemId를 저장한다.
    /// </summary>
    private readonly HashSet<int> currentStock =
        new HashSet<int>();

    private bool isInitialized = false;

    public int CurrentShopLevel => currentShopLevel;

    public bool IsInitialized => isInitialized;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // 중요:
        // 여기서는 Load를 하지 않는다.
        //
        // ShopDatabase.Awake()보다 ShopManager.Awake()가
        // 먼저 실행될 수 있기 때문이다.
    }


    private void Start()
    {
        Initialize();
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    /// <summary>
    /// ShopDatabase의 Awake가 끝난 뒤
    /// 상점 상태를 초기화한다.
    /// </summary>
    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopManager] ShopDatabase.Instance가 없습니다. " +
                "ShopDatabase 오브젝트가 씬에 존재하는지 확인해주세요.");

            return;
        }

        Load();

        isInitialized = true;

        Debug.Log(
            $"[ShopManager] 초기화 완료 / " +
            $"Level: {currentShopLevel} / " +
            $"Stock: {currentStock.Count}");
    }


    /// <summary>
    /// 완전히 새로운 상점 데이터를 만든다.
    ///
    /// 기본 재고는 Shop Level 0의 품목이다.
    /// </summary>
    private void InitializeNewShop()
    {
        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopManager] 새 상점을 초기화할 수 없습니다. " +
                "ShopDatabase.Instance가 없습니다.");

            return;
        }

        currentShopLevel = 0;
        currentStock.Clear();

        AddLevelStock(0);

        PlayerPrefs.SetInt(
            SHOP_INITIALIZED_KEY,
            1);

        PlayerPrefs.SetInt(
            SHOP_VALID_INITIALIZATION_KEY,
            1);

        Save();

        Debug.Log(
            $"[ShopManager] 새 상점 초기화 완료 / " +
            $"Level: {currentShopLevel} / " +
            $"Stock: {currentStock.Count}");
    }


    /// <summary>
    /// 이전 코드의 초기화 순서 문제 때문에
    /// Level 0 / Stock 0이 저장된 경우를 복구한다.
    ///
    /// SHOP_VALID_INITIALIZATION_KEY가 없는
    /// 기존 테스트 데이터에 대해서만 실행된다.
    ///
    /// 따라서 앞으로 실제 플레이 중
    /// Lv0 상품 5개를 전부 구매해서 Stock 0이 된 경우에는
    /// 재고를 다시 생성하지 않는다.
    /// </summary>
    private void RepairOldInvalidInitializationIfNeeded()
    {
        bool hasValidInitialization =
            PlayerPrefs.GetInt(
                SHOP_VALID_INITIALIZATION_KEY,
                0) == 1;

        if (hasValidInitialization)
        {
            return;
        }

        if (currentShopLevel == 0 &&
            currentStock.Count == 0)
        {
            Debug.LogWarning(
                "[ShopManager] 이전 초기화 순서 문제로 생성된 " +
                "Level 0 / Stock 0 데이터를 발견했습니다. " +
                "Lv0 기본 재고를 복구합니다.");

            currentStock.Clear();

            AddLevelStock(0);

            PlayerPrefs.SetInt(
                SHOP_VALID_INITIALIZATION_KEY,
                1);

            Save();

            Debug.Log(
                $"[ShopManager] 기존 상점 데이터 복구 완료 / " +
                $"Level: {currentShopLevel} / " +
                $"Stock: {currentStock.Count}");

            return;
        }

        // 기존 데이터가 정상적인 형태라면
        // 유효한 저장 데이터로 간주한다.
        PlayerPrefs.SetInt(
            SHOP_VALID_INITIALIZATION_KEY,
            1);

        PlayerPrefs.Save();
    }


    // =========================================================
    // STOCK
    // =========================================================

    /// <summary>
    /// 특정 Shop Level에 등록된 재고를 추가한다.
    ///
    /// HashSet을 사용하기 때문에 같은 shopItemId가
    /// 중복으로 추가되지는 않는다.
    /// </summary>
    private void AddLevelStock(int level)
    {
        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopManager] ShopDatabase.Instance가 없습니다.");

            return;
        }

        List<ShopItemData> levelItems =
            ShopDatabase.Instance.GetItemsByLevel(level);

        foreach (ShopItemData data in levelItems)
        {
            currentStock.Add(data.shopItemId);
        }
    }


    /// <summary>
    /// 특정 shopItemId가 현재 상점 재고에 있는지 확인한다.
    /// </summary>
    public bool HasStock(int shopItemId)
    {
        return currentStock.Contains(shopItemId);
    }


    /// <summary>
    /// 현재 남아 있는 상점 재고 개수를 반환한다.
    /// </summary>
    public int GetCurrentStockCount()
    {
        return currentStock.Count;
    }


    /// <summary>
    /// 현재 남아 있는 상점 재고를 반환한다.
    ///
    /// shopItemId 순서로 정렬한다.
    /// </summary>
    public List<ShopItemData> GetCurrentStock()
    {
        List<ShopItemData> result =
            new List<ShopItemData>();

        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopManager] ShopDatabase.Instance가 없습니다.");

            return result;
        }

        foreach (int shopItemId in currentStock)
        {
            ShopItemData data =
                ShopDatabase.Instance.GetShopItem(shopItemId);

            if (data != null)
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
    /// 구매가 성공한 뒤
    /// 해당 shopItemId를 상점 재고에서 제거한다.
    ///
    /// sellItemId가 아니라
    /// shopItemId를 제거해야 한다.
    ///
    /// 같은 실제 아이템이라도
    /// 서로 다른 ShopItemId라면 별개의 재고이다.
    /// </summary>
    public bool RemoveStock(int shopItemId)
    {
        if (!currentStock.Contains(shopItemId))
        {
            Debug.LogWarning(
                $"[ShopManager] 이미 없거나 존재하지 않는 재고입니다. " +
                $"ShopItemId: {shopItemId}");

            return false;
        }

        currentStock.Remove(shopItemId);

        Save();

        Debug.Log(
            $"[ShopManager] 상점 재고 제거 / " +
            $"ShopItemId: {shopItemId} / " +
            $"Remaining Stock: {currentStock.Count}");

        return true;
    }


    // =========================================================
    // SHOP UPDATE
    // =========================================================

    /// <summary>
    /// Shop Update 타일에서 호출한다.
    ///
    /// 해당 Shop Level의 품목을 현재 재고에 추가한다.
    ///
    /// 이미 해금한 레벨을 다시 호출해도
    /// 구매해서 사라진 품목을 복구하지 않는다.
    /// </summary>
    public bool UnlockShopLevel(int level)
    {
        if (!isInitialized)
        {
            Debug.LogWarning(
                "[ShopManager] 아직 상점 초기화가 완료되지 않았습니다.");

            return false;
        }

        if (level <= currentShopLevel)
        {
            Debug.Log(
                $"[ShopManager] 이미 해금된 상점 레벨입니다. " +
                $"현재 Level: {currentShopLevel}, " +
                $"요청 Level: {level}");

            return false;
        }

        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopManager] ShopDatabase.Instance가 없습니다.");

            return false;
        }

        List<ShopItemData> levelItems =
            ShopDatabase.Instance.GetItemsByLevel(level);

        if (levelItems.Count == 0)
        {
            Debug.LogWarning(
                $"[ShopManager] Level {level}에 등록된 " +
                "상점 품목이 없습니다.");

            return false;
        }

        foreach (ShopItemData data in levelItems)
        {
            currentStock.Add(data.shopItemId);
        }

        currentShopLevel = level;

        Save();

        Debug.Log(
            $"[ShopManager] 상점 업데이트 완료 / " +
            $"Level: {currentShopLevel} / " +
            $"Current Stock: {currentStock.Count}");

        return true;
    }


    // =========================================================
    // SAVE
    // =========================================================

    /// <summary>
    /// 현재 상점 레벨과 남아 있는 재고를 저장한다.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetInt(
            SHOP_LEVEL_KEY,
            currentShopLevel);

        PlayerPrefs.SetInt(
            SHOP_INITIALIZED_KEY,
            1);

        List<int> sortedStock =
            new List<int>(currentStock);

        sortedStock.Sort();

        string stockData =
            string.Join(",", sortedStock);

        PlayerPrefs.SetString(
            SHOP_STOCK_KEY,
            stockData);

        PlayerPrefs.Save();
    }


    // =========================================================
    // LOAD
    // =========================================================

    /// <summary>
    /// 저장된 상점 상태를 불러온다.
    ///
    /// 저장 데이터가 없다면
    /// Shop Level 0 재고로 새로 시작한다.
    /// </summary>
    public void Load()
    {
        if (ShopDatabase.Instance == null)
        {
            Debug.LogError(
                "[ShopManager] 상점 데이터를 불러올 수 없습니다. " +
                "ShopDatabase.Instance가 없습니다.");

            return;
        }

        if (!PlayerPrefs.HasKey(SHOP_INITIALIZED_KEY))
        {
            InitializeNewShop();
            return;
        }

        currentShopLevel =
            PlayerPrefs.GetInt(
                SHOP_LEVEL_KEY,
                0);

        currentStock.Clear();

        string stockData =
            PlayerPrefs.GetString(
                SHOP_STOCK_KEY,
                "");

        if (!string.IsNullOrEmpty(stockData))
        {
            string[] split =
                stockData.Split(',');

            foreach (string value in split)
            {
                if (int.TryParse(
                    value,
                    out int shopItemId))
                {
                    // 현재 ShopDatabase에 실제로 존재하는
                    // shopItemId만 불러온다.
                    ShopItemData data =
                        ShopDatabase.Instance.GetShopItem(
                            shopItemId);

                    if (data != null)
                    {
                        currentStock.Add(shopItemId);
                    }
                }
            }
        }

        RepairOldInvalidInitializationIfNeeded();

        Debug.Log(
            $"[ShopManager] 상점 데이터 불러오기 완료 / " +
            $"Level: {currentShopLevel} / " +
            $"Stock: {currentStock.Count}");
    }


    // =========================================================
    // RESET
    // =========================================================

    /// <summary>
    /// 새 게임용 상점 데이터 초기화.
    ///
    /// 추후 SaveManager의 새 게임 / 저장 삭제와 연결한다.
    /// </summary>
    public void ResetShop()
    {
        PlayerPrefs.DeleteKey(
            SHOP_LEVEL_KEY);

        PlayerPrefs.DeleteKey(
            SHOP_STOCK_KEY);

        PlayerPrefs.DeleteKey(
            SHOP_INITIALIZED_KEY);

        PlayerPrefs.DeleteKey(
            SHOP_VALID_INITIALIZATION_KEY);

        currentStock.Clear();
        currentShopLevel = 0;

        InitializeNewShop();

        isInitialized = true;

        Debug.Log(
            "[ShopManager] 상점 데이터가 초기화되었습니다.");
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("Debug/Print Current Stock")]
    private void DebugPrintCurrentStock()
    {
        Debug.Log(
            $"========== SHOP STOCK / LEVEL " +
            $"{currentShopLevel} ==========");

        List<ShopItemData> stock =
            GetCurrentStock();

        foreach (ShopItemData data in stock)
        {
            Debug.Log(
                $"ShopItemId: {data.shopItemId} / " +
                $"ItemId: {data.sellItemId} / " +
                $"Level: {data.shopLevel}");
        }

        Debug.Log(
            $"Current Stock Count: {stock.Count}");

        Debug.Log(
            "==========================================");
    }


    [ContextMenu("Debug/Unlock Level 1")]
    private void DebugUnlockLevel1()
    {
        UnlockShopLevel(1);
    }


    [ContextMenu("Debug/Unlock Level 2")]
    private void DebugUnlockLevel2()
    {
        UnlockShopLevel(2);
    }


    [ContextMenu("Debug/Unlock Level 3")]
    private void DebugUnlockLevel3()
    {
        UnlockShopLevel(3);
    }


    [ContextMenu("Debug/Remove ShopItem 6001")]
    private void DebugRemove6001()
    {
        RemoveStock(6001);
    }


    [ContextMenu("Debug/Reset Shop")]
    private void DebugResetShop()
    {
        ResetShop();
    }
}