using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance;

    [Header("Max Resource")]
    [SerializeField]
    private int maxHealth = 100;

    [SerializeField]
    private int maxMental = 75;

    [SerializeField]
    private int maxHunger = 125;

    [Header("Current Resource")]
    [SerializeField]
    private int currentHealth = 100;

    [SerializeField]
    private int currentMental = 75;

    [SerializeField]
    private int currentHunger = 125;

    [Header("Turn Rule")]
    [SerializeField]
    private int mentalDecreaseTurnInterval = 50;

    [SerializeField]
    private int mentalDecreaseAmount = 10;

    [SerializeField]
    private int hungerDecreaseTurnInterval = 10;

    [SerializeField]
    private int hungerDecreaseAmount = 10;

    private int lastProcessedTurn;

    private const string HEALTH_KEY =
        "PLAYER_HEALTH";

    private const string MENTAL_KEY =
        "PLAYER_MENTAL";

    private const string HUNGER_KEY =
        "PLAYER_HUNGER";

    private const string LAST_PROCESSED_TURN_KEY =
        "PLAYER_LAST_PROCESSED_TURN";

    public int MaxHealth => maxHealth;
    public int MaxMental => maxMental;
    public int MaxHunger => maxHunger;

    public int CurrentHealth => currentHealth;
    public int CurrentMental => currentMental;
    public int CurrentHunger => currentHunger;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        /*
         * 여기서는 최대치만 계산한다.
         * 저장하면 기존 플레이 데이터가 덮어써질 수 있으므로
         * saveData를 false로 둔다.
         */
        ApplyMaxResourceFromStats(false);

        Load();
        ClampAll();
    }

    private void Start()
    {
        SubscribeDungeonTurnEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeDungeonTurnEvent();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SubscribeDungeonTurnEvent()
    {
        if (DungeonManager.Instance == null)
            return;

        DungeonManager.Instance.OnTurnChanged -=
            HandleTurnChanged;

        DungeonManager.Instance.OnTurnChanged +=
            HandleTurnChanged;

        int currentTurn =
            DungeonManager.Instance.CurrentTurn;

        if (lastProcessedTurn > currentTurn)
        {
            lastProcessedTurn =
                currentTurn;
        }
    }

    private void UnsubscribeDungeonTurnEvent()
    {
        if (DungeonManager.Instance == null)
            return;

        DungeonManager.Instance.OnTurnChanged -=
            HandleTurnChanged;
    }

    public void ApplyMaxResourceFromStats()
    {
        ApplyMaxResourceFromStats(true);
    }

    public void ApplyMaxResourceFromStats(
        bool saveData)
    {
        if (PlayerStats.Instance != null)
        {
            maxHealth =
                PlayerStats.Instance.MaxHealth;

            maxHunger =
                PlayerStats.Instance.MaxHunger;

            maxMental =
                PlayerStats.Instance.MaxMental;
        }

        ClampAll();

        if (saveData)
        {
            Save();
        }
    }

    private void HandleTurnChanged(
        int newTurn)
    {
        if (newTurn < lastProcessedTurn)
        {
            lastProcessedTurn =
                newTurn;

            Save();
            return;
        }

        for (
            int turn = lastProcessedTurn + 1;
            turn <= newTurn;
            turn++)
        {
            if (mentalDecreaseTurnInterval > 0 &&
                turn %
                mentalDecreaseTurnInterval == 0)
            {
                ChangeMental(
                    -ApplyDecreasePenalty(
                        mentalDecreaseAmount
                    ),
                    $"정신력 감소 규칙 발동 ({turn}턴)"
                );
            }

            if (hungerDecreaseTurnInterval > 0 &&
                turn %
                hungerDecreaseTurnInterval == 0)
            {
                ChangeHunger(
                    -ApplyDecreasePenalty(
                        hungerDecreaseAmount
                    ),
                    $"배고픔 감소 규칙 발동 ({turn}턴)"
                );
            }
        }

        lastProcessedTurn =
            newTurn;

        Save();
    }

    public void ChangeHealth(
        int amount,
        string reason = "")
    {
        int finalAmount =
            amount;

        if (amount < 0)
        {
            finalAmount =
                -ApplyDecreasePenalty(
                    Mathf.Abs(amount)
                );
        }

        if (amount > 0 &&
            IsHungerHealthHealPenaltyActive())
        {
            finalAmount =
                Mathf.RoundToInt(
                    amount * 0.5f
                );
        }

        if (amount > 0)
        {
            StatusEffectController controller =
                GetPlayerStatusController();

            if (controller != null)
            {
                finalAmount =
                    Mathf.RoundToInt(
                        finalAmount *
                        controller
                            .GetHealthHealingMultiplier()
                    );
            }
        }

        int before =
            currentHealth;

        currentHealth =
            Mathf.Clamp(
                currentHealth + finalAmount,
                0,
                maxHealth
            );

        Debug.Log(
            $"[Resource] 체력: {before} -> " +
            $"{currentHealth} / 변화량: " +
            $"{finalAmount} / 이유: {reason}"
        );

        RefreshStatusResourceConditions();

        Save();
    }

    public void ChangeMental(
        int amount,
        string reason = "")
    {
        int finalAmount =
            amount;

        if (amount < 0)
        {
            finalAmount =
                -ApplyDecreasePenalty(
                    Mathf.Abs(amount)
                );
        }

        if (amount > 0 &&
            IsHungerMentalHealPenaltyActive())
        {
            finalAmount =
                Mathf.RoundToInt(
                    amount * 0.5f
                );
        }
        if (amount > 0)
        {
            StatusEffectController controller =
                GetPlayerStatusController();

            if (controller != null)
            {
                finalAmount =
                    Mathf.RoundToInt(
                        finalAmount *
                        controller
                            .GetMentalHealingMultiplier()
                    );
            }
        }

        int before =
            currentMental;

        currentMental =
            Mathf.Clamp(
                currentMental + finalAmount,
                0,
                maxMental
            );

        Debug.Log(
            $"[Resource] 정신력: {before} -> " +
            $"{currentMental} / 변화량: " +
            $"{finalAmount} / 이유: {reason}"
        );

        RefreshStatusResourceConditions();

        Save();
    }

    public void ChangeHunger(
        int amount,
        string reason = "")
    {
        int finalAmount =
            amount;

        if (amount < 0)
        {
            finalAmount =
                -ApplyDecreasePenalty(
                    Mathf.Abs(amount)
                );
        }

        if (amount > 0)
        {
            StatusEffectController controller =
                GetPlayerStatusController();

            if (controller != null)
            {
                finalAmount =
                    Mathf.RoundToInt(
                        finalAmount *
                        controller
                            .GetHungerHealingMultiplier()
                    );
            }
        }

        int before =
            currentHunger;

        currentHunger =
            Mathf.Clamp(
                currentHunger + finalAmount,
                0,
                maxHunger
            );

        Debug.Log(
            $"[Resource] 배고픔: {before} -> " +
            $"{currentHunger} / 변화량: " +
            $"{finalAmount} / 이유: {reason}"
        );

        RefreshStatusResourceConditions();

        Save();
    }

    public void HealHealthItem(
        int amount)
    {
        ChangeHealth(
            amount,
            "아이템 체력 회복"
        );
    }

    public void HealHungerItem(
        int amount)
    {
        ChangeHunger(
            amount,
            "아이템 배고픔 회복"
        );
    }

    public void HealMentalItem(
        int amount)
    {
        ChangeMental(
            amount,
            "아이템 정신력 회복"
        );
    }

    private int ApplyDecreasePenalty(
        int amount)
    {
        amount = Mathf.Max(0, amount);

        float multiplier = 1f;

        // 기존 배고픔 25% 이하 패널티 유지
        if (IsHungerAllDecreasePenaltyActive())
        {
            multiplier *= 1.5f;
        }

        // 2016 - 정신력 25%
        // 모든 자원의 감소량 50% 증가
        StatusEffectController controller =
            GetPlayerStatusController();

        if (controller != null)
        {
            multiplier *=
                controller.GetResourceDecreaseMultiplier();
        }

        return Mathf.RoundToInt(
            amount * multiplier
        );
    }

    public int GetMentalAccuracyPenalty()
    {
        float mentalRatio =
            GetMentalRatio();

        if (mentalRatio > 0.75f)
            return 0;

        float lostRatio =
            1f - mentalRatio;

        int lostTenPercentCount =
            Mathf.FloorToInt(
                lostRatio / 0.1f
            );

        return lostTenPercentCount * 5;
    }

    public bool IsMentalDamagePenaltyActive()
    {
        return GetMentalRatio() <= 0.25f;
    }

    public bool IsHungerHealthHealPenaltyActive()
    {
        return GetHungerRatio() <= 0.70f;
    }

    public bool IsHungerMentalHealPenaltyActive()
    {
        return GetHungerRatio() <= 0.50f;
    }

    public bool IsHungerAllDecreasePenaltyActive()
    {
        return GetHungerRatio() <= 0.25f;
    }

    private float GetMentalRatio()
    {
        if (maxMental <= 0)
            return 0f;

        return
            (float)currentMental /
            maxMental;
    }

    private float GetHungerRatio()
    {
        if (maxHunger <= 0)
            return 0f;

        return
            (float)currentHunger /
            maxHunger;
    }

    // =========================================================
    // Death Recovery
    // =========================================================

    public void RestoreHealthToMaxForDeath()
    {
        /*
         * 사망 후 로비 복귀용.
         *
         * ChangeHealth()를 사용하지 않는다.
         * 배고픔 / 상태이상에 의한 회복량 감소 효과를
         * 적용받지 않고 HP만 정확히 최대치로 복구한다.
         *
         * Mental / Hunger / lastProcessedTurn은 건드리지 않는다.
         */
        ApplyMaxResourceFromStats(false);

        int before =
            currentHealth;

        currentHealth =
            maxHealth;

        ClampAll();
        RefreshStatusResourceConditions();
        Save();

        Debug.Log(
            "[Resource] 사망 복귀 HP 회복 완료 " +
            $"{before} -> {currentHealth}/{maxHealth} " +
            $"(Mental {currentMental}/{maxMental}, " +
            $"Hunger {currentHunger}/{maxHunger} 유지)"
        );
    }


    public void ResetResourceToMax()
    {
        ResetForNewGame(true);
    }

    public void ResetForNewGame(
        bool saveData = true)
    {
        ApplyMaxResourceFromStats(false);

        currentHealth =
            maxHealth;

        currentMental =
            maxMental;

        currentHunger =
            maxHunger;

        lastProcessedTurn =
            0;

        ClampAll();

        if (saveData)
        {
            Save();
        }

        Debug.Log(
            "[Resource] 새 게임 리소스 초기화 완료 " +
            $"HP {currentHealth}/{maxHealth}, " +
            $"Mental {currentMental}/{maxMental}, " +
            $"Hunger {currentHunger}/{maxHunger}"
        );
    }

    private void ClampAll()
    {
        maxHealth =
            Mathf.Max(0, maxHealth);

        maxMental =
            Mathf.Max(0, maxMental);

        maxHunger =
            Mathf.Max(0, maxHunger);

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maxHealth
            );

        currentMental =
            Mathf.Clamp(
                currentMental,
                0,
                maxMental
            );

        currentHunger =
            Mathf.Clamp(
                currentHunger,
                0,
                maxHunger
            );
    }

    private void RefreshStatusResourceConditions()
    {
        StatusEffectController controller =
            GetPlayerStatusController();

        if (controller != null)
        {
            controller.RefreshResourceConditionEffects();
        }
    }


    private void Save()
    {
        PlayerPrefs.SetInt(
            HEALTH_KEY,
            currentHealth
        );

        PlayerPrefs.SetInt(
            MENTAL_KEY,
            currentMental
        );

        PlayerPrefs.SetInt(
            HUNGER_KEY,
            currentHunger
        );

        PlayerPrefs.SetInt(
            LAST_PROCESSED_TURN_KEY,
            lastProcessedTurn
        );

        PlayerPrefs.Save();
    }

    private void Load()
    {
        currentHealth =
            PlayerPrefs.GetInt(
                HEALTH_KEY,
                maxHealth
            );

        currentMental =
            PlayerPrefs.GetInt(
                MENTAL_KEY,
                maxMental
            );

        currentHunger =
            PlayerPrefs.GetInt(
                HUNGER_KEY,
                maxHunger
            );

        lastProcessedTurn =
            PlayerPrefs.GetInt(
                LAST_PROCESSED_TURN_KEY,
                0
            );
    }
    private StatusEffectController
    GetPlayerStatusController()
    {
        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null)
        {
            StatusEffectController controller =
                BattleManager.Instance.playerUnit
                    .GetComponent<StatusEffectController>();

            if (controller != null)
            {
                return controller;
            }
        }

        return null;
    }
}