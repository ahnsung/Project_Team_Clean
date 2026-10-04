using System;
using UnityEngine;

/// <summary>
/// 솔라스톤 전용 화폐 관리자.
///
/// 기획상 솔라스톤(Item ID 1020)은
/// 일반 인벤토리 칸을 차지하지 않고 별도 수치로 관리한다.
///
/// 이후 연결 대상:
/// - 아이템 1020 획득
/// - 상점 구매
/// - 상점 판매
/// - 전투 보상
/// - 파밍 / 상자 보상
/// - 상점 UI
/// </summary>
public class SolastoneManager : MonoBehaviour
{
    public static SolastoneManager Instance { get; private set; }

    private const string SAVE_KEY = "SOLASTONE_AMOUNT";

    [Header("Debug")]
    [SerializeField]
    private int currentAmount = 0;

    /// <summary>
    /// 현재 보유 솔라스톤.
    /// 외부에서는 직접 수정하지 않고
    /// Add / Spend / SetAmount를 사용한다.
    /// </summary>
    public int CurrentAmount => currentAmount;

    /// <summary>
    /// 솔라스톤 수치가 변경됐을 때 호출.
    /// 이후 상점 UI의 보유량 텍스트를 이 이벤트에 연결할 수 있다.
    /// </summary>
    public event Action<int> OnAmountChanged;


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

        Load();
    }


    // =========================================================
    // Add
    // =========================================================

    /// <summary>
    /// 솔라스톤을 획득한다.
    /// </summary>
    public void Add(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning(
                "[SolastoneManager] 추가할 솔라스톤은 1 이상이어야 합니다.\n" +
                $"요청 수량: {amount}"
            );

            return;
        }

        currentAmount += amount;

        Save();

        OnAmountChanged?.Invoke(currentAmount);

        Debug.Log(
            "[SolastoneManager] 솔라스톤 획득\n" +
            $"획득량: {amount}\n" +
            $"현재 보유량: {currentAmount}"
        );
    }


    // =========================================================
    // Spend
    // =========================================================

    /// <summary>
    /// 솔라스톤을 소비한다.
    ///
    /// 보유량이 부족하면 소비하지 않고 false를 반환한다.
    /// </summary>
    public bool Spend(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning(
                "[SolastoneManager] 소비할 솔라스톤은 1 이상이어야 합니다.\n" +
                $"요청 수량: {amount}"
            );

            return false;
        }

        if (currentAmount < amount)
        {
            Debug.Log(
                "[SolastoneManager] 솔라스톤 부족\n" +
                $"필요량: {amount}\n" +
                $"현재 보유량: {currentAmount}"
            );

            return false;
        }

        currentAmount -= amount;

        Save();

        OnAmountChanged?.Invoke(currentAmount);

        Debug.Log(
            "[SolastoneManager] 솔라스톤 소비\n" +
            $"소비량: {amount}\n" +
            $"현재 보유량: {currentAmount}"
        );

        return true;
    }


    // =========================================================
    // Check
    // =========================================================

    /// <summary>
    /// 해당 가격을 지불할 수 있는지 확인한다.
    /// 실제 솔라스톤은 소비하지 않는다.
    /// </summary>
    public bool CanAfford(int amount)
    {
        if (amount < 0)
            return false;

        return currentAmount >= amount;
    }


    // =========================================================
    // Direct Set
    // =========================================================

    /// <summary>
    /// 저장 데이터 복원이나 테스트 등에서
    /// 솔라스톤 값을 직접 설정할 때 사용한다.
    /// 음수는 0으로 처리한다.
    /// </summary>
    public void SetAmount(int amount)
    {
        currentAmount = Mathf.Max(0, amount);

        Save();

        OnAmountChanged?.Invoke(currentAmount);

        Debug.Log(
            "[SolastoneManager] 솔라스톤 수치 설정\n" +
            $"현재 보유량: {currentAmount}"
        );
    }


    // =========================================================
    // Save / Load
    // =========================================================

    /// <summary>
    /// 현재 솔라스톤 보유량 저장.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetInt(
            SAVE_KEY,
            currentAmount
        );

        PlayerPrefs.Save();
    }


    /// <summary>
    /// 저장된 솔라스톤 보유량 불러오기.
    /// 저장값이 없으면 0부터 시작한다.
    /// </summary>
    public void Load()
    {
        currentAmount =
            Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    SAVE_KEY,
                    0
                )
            );

        OnAmountChanged?.Invoke(currentAmount);

        Debug.Log(
            "[SolastoneManager] 솔라스톤 불러오기\n" +
            $"현재 보유량: {currentAmount}"
        );
    }


    /// <summary>
    /// 새 게임용 초기화.
    ///
    /// 나중에 SaveManager의 새 게임/저장 삭제 흐름과 연결한다.
    /// </summary>
    public void ResetSolastone()
    {
        currentAmount = 0;

        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();

        OnAmountChanged?.Invoke(currentAmount);

        Debug.Log(
            "[SolastoneManager] 솔라스톤 초기화"
        );
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug/Add 10 Solastone")]
    private void DebugAdd10()
    {
        Add(10);
    }

    [ContextMenu("Debug/Spend 5 Solastone")]
    private void DebugSpend5()
    {
        Spend(5);
    }

    [ContextMenu("Debug/Reset Solastone")]
    private void DebugReset()
    {
        ResetSolastone();
    }
}