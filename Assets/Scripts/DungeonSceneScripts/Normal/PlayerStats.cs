using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    // =========================================================
    // Base Stats
    // =========================================================

    [Header("Base Stats")]
    public int STR = 5;
    public int DEX = 5;
    public int CON = 5;
    public int INT = 5;


    // =========================================================
    // Status Effect
    // =========================================================

    [Header("Status Effect")]

    [Tooltip("플레이어에게 붙어 있는 StatusEffectController")]
    [SerializeField]
    private StatusEffectController statusEffectController;


    // =========================================================
    // Legacy Weapon
    // =========================================================

    [Header("Legacy Weapon")]

    [Tooltip("기존 프로젝트의 무기 공격력 필드입니다.")]
    public int weaponDamage = 0;


    // =========================================================
    // Equipment Bonus
    // =========================================================

    [Header("Equipment Bonus - Runtime")]

    [SerializeField]
    private int equipmentSTR;

    [SerializeField]
    private int equipmentDEX;

    [SerializeField]
    private int equipmentCON;

    [SerializeField]
    private int equipmentINT;

    [SerializeField]
    private int equipmentAttackPower;

    [SerializeField]
    private int equipmentAccuracyBonus;


    // =========================================================
    // Status Effect Bonus
    // =========================================================

    public int StatusSTRBonus
    {
        get
        {
            if (statusEffectController == null)
                return 0;

            return statusEffectController
                .GetStrengthBonus();
        }
    }


    public int StatusDEXBonus
    {
        get
        {
            if (statusEffectController == null)
                return 0;

            return statusEffectController
                .GetDexterityBonus();
        }
    }


    public int StatusCONBonus
    {
        get
        {
            if (statusEffectController == null)
                return 0;

            return statusEffectController
                .GetConstitutionBonus();
        }
    }


    public int StatusINTBonus
    {
        get
        {
            if (statusEffectController == null)
                return 0;

            return statusEffectController
                .GetIntelligenceBonus();
        }
    }


    // =========================================================
    // Total Stats
    // =========================================================

    public int TotalSTR
    {
        get
        {
            return Mathf.Max(
                0,
                STR +
                equipmentSTR +
                StatusSTRBonus
            );
        }
    }


    public int TotalDEX
    {
        get
        {
            return Mathf.Max(
                0,
                DEX +
                equipmentDEX +
                StatusDEXBonus
            );
        }
    }


    public int TotalCON
    {
        get
        {
            return Mathf.Max(
                0,
                CON +
                equipmentCON +
                StatusCONBonus
            );
        }
    }


    public int TotalINT
    {
        get
        {
            return Mathf.Max(
                0,
                INT +
                equipmentINT +
                StatusINTBonus
            );
        }
    }


    // =========================================================
    // Equipment
    // =========================================================

    public int EquipmentAttackPower
    {
        get
        {
            return equipmentAttackPower;
        }
    }


    public int EquipmentAccuracyBonus
    {
        get
        {
            return equipmentAccuracyBonus;
        }
    }


    // =========================================================
    // Resources
    // =========================================================

    public int MaxHealth
    {
        get
        {
            return 50 + TotalCON * 10;
        }
    }


    public int MaxHunger
    {
        get
        {
            return 100 + TotalCON * 5;
        }
    }


    public int MaxMental
    {
        get
        {
            return 50 + TotalINT * 5;
        }
    }


    public int InventoryCapacity
    {
        get
        {
            return 32;
        }
    }


    // =========================================================
    // Save Keys
    // =========================================================

    private const string STR_KEY =
        "STAT_STR";

    private const string DEX_KEY =
        "STAT_DEX";

    private const string CON_KEY =
        "STAT_CON";

    private const string INT_KEY =
        "STAT_INT";


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadStats();
    }


    private void Start()
    {
        ResolveStatusEffectController();

        if (statusEffectController != null)
        {
            statusEffectController
                .OnStatusEffectsChanged +=
                HandleStatusEffectsChanged;
        }

        ApplyStatChange();
    }


    private void OnDestroy()
    {
        if (
            statusEffectController != null
        )
        {
            statusEffectController
                .OnStatusEffectsChanged -=
                HandleStatusEffectsChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // Resolve Status Effect Controller
    // =========================================================

    private void ResolveStatusEffectController()
    {
        /*
         * Inspector에 직접 연결되어 있으면
         * 그것을 가장 우선 사용한다.
         */
        if (statusEffectController != null)
        {
            return;
        }


        /*
         * BattleManager에 연결된 플레이어에서 찾는다.
         */
        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null
        )
        {
            statusEffectController =
                BattleManager.Instance
                    .playerUnit
                    .GetComponent<
                        StatusEffectController
                    >();


            if (statusEffectController == null)
            {
                statusEffectController =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }


            if (statusEffectController == null)
            {
                statusEffectController =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >();
            }
        }


        if (statusEffectController == null)
        {
            Debug.LogWarning(
                "[PlayerStats] " +
                "플레이어 StatusEffectController가 연결되지 않았습니다.\n" +
                "PlayerStats Inspector의 Status Effect Controller에 " +
                "Player 오브젝트를 직접 연결하는 것을 권장합니다."
            );
        }
    }


    // =========================================================
    // Status Effect Changed
    // =========================================================

    private void HandleStatusEffectsChanged()
    {
        ApplyStatChange();

        Debug.Log(
            "[PlayerStats] 상태이상 스탯 갱신\n" +
            $"STR: {TotalSTR} ({StatusSTRBonus:+#;-#;0})\n" +
            $"DEX: {TotalDEX} ({StatusDEXBonus:+#;-#;0})\n" +
            $"CON: {TotalCON} ({StatusCONBonus:+#;-#;0})\n" +
            $"INT: {TotalINT} ({StatusINTBonus:+#;-#;0})"
        );
    }


    // =========================================================
    // Attack
    // =========================================================

    public int GetBaseAttackDamage()
    {
        return
            TotalSTR * 2 +
            weaponDamage +
            equipmentAttackPower;
    }


    public int GetFinalAttackDamage()
    {
        int damage =
            GetBaseAttackDamage();


        if (
            PlayerResourceManager.Instance != null &&
            PlayerResourceManager.Instance
                .IsMentalDamagePenaltyActive()
        )
        {
            damage =
                Mathf.RoundToInt(
                    damage * 0.5f
                );
        }


        return Mathf.Max(
            1,
            damage
        );
    }


    // =========================================================
    // Accuracy
    // =========================================================

    public int GetFinalAccuracy(
        int enemyEvasion)
    {
        int accuracy =
            100 -
            (
                enemyEvasion -
                TotalDEX * 3
            );


        accuracy +=
            equipmentAccuracyBonus;


        if (
            PlayerResourceManager.Instance != null
        )
        {
            accuracy -=
                PlayerResourceManager.Instance
                    .GetMentalAccuracyPenalty();
        }


        return Mathf.Clamp(
            accuracy,
            10,
            95
        );
    }


    // =========================================================
    // Evasion
    // =========================================================

    public int GetFinalEvasion(
        int enemyAccuracy)
    {
        int evasionChance =
            100 -
            (
                enemyAccuracy -
                TotalDEX * 3
            );


        return Mathf.Clamp(
            evasionChance,
            5,
            95
        );
    }


    // =========================================================
    // Run
    // =========================================================

    public int GetRunSuccessPercent()
    {
        return Mathf.Clamp(
            50 +
            TotalDEX * 2,
            0,
            95
        );
    }


    // =========================================================
    // Equipment Bonus
    // =========================================================

    public void SetEquipmentBonuses(
        EquipmentStatModifier modifier)
    {
        if (modifier == null)
        {
            equipmentSTR = 0;
            equipmentDEX = 0;
            equipmentCON = 0;
            equipmentINT = 0;

            equipmentAttackPower = 0;
            equipmentAccuracyBonus = 0;
        }
        else
        {
            equipmentSTR =
                modifier.str;

            equipmentDEX =
                modifier.dex;

            equipmentCON =
                modifier.con;

            equipmentINT =
                modifier.intelligence;

            equipmentAttackPower =
                modifier.attackPower;

            equipmentAccuracyBonus =
                modifier.accuracyBonus;
        }


        ApplyStatChange();
    }


    public void ClearEquipmentBonuses()
    {
        SetEquipmentBonuses(
            new EquipmentStatModifier()
        );
    }


    // =========================================================
    // Base Stat Change
    // =========================================================

    public void AddSTR()
    {
        STR++;

        SaveStats();
        ApplyStatChange();
    }


    public void SubSTR()
    {
        STR =
            Mathf.Max(
                0,
                STR - 1
            );

        SaveStats();
        ApplyStatChange();
    }


    public void AddDEX()
    {
        DEX++;

        SaveStats();
        ApplyStatChange();
    }


    public void SubDEX()
    {
        DEX =
            Mathf.Max(
                0,
                DEX - 1
            );

        SaveStats();
        ApplyStatChange();
    }


    public void AddCON()
    {
        CON++;

        SaveStats();
        ApplyStatChange();
    }


    public void SubCON()
    {
        CON =
            Mathf.Max(
                0,
                CON - 1
            );

        SaveStats();
        ApplyStatChange();
    }


    public void AddINT()
    {
        INT++;

        SaveStats();
        ApplyStatChange();
    }


    public void SubINT()
    {
        INT =
            Mathf.Max(
                0,
                INT - 1
            );

        SaveStats();
        ApplyStatChange();
    }


    // =========================================================
    // Apply Stat Change
    // =========================================================

    private void ApplyStatChange()
    {
        if (
            PlayerResourceManager.Instance != null
        )
        {
            PlayerResourceManager.Instance
                .ApplyMaxResourceFromStats();
        }


        if (
            InventoryManager.Instance != null
        )
        {
            InventoryManager.Instance
                .ApplyCapacityFromStats();
        }
    }


    // =========================================================
    // Save
    // =========================================================

    private void SaveStats()
    {
        PlayerPrefs.SetInt(
            STR_KEY,
            STR
        );

        PlayerPrefs.SetInt(
            DEX_KEY,
            DEX
        );

        PlayerPrefs.SetInt(
            CON_KEY,
            CON
        );

        PlayerPrefs.SetInt(
            INT_KEY,
            INT
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // Load
    // =========================================================

    private void LoadStats()
    {
        STR =
            PlayerPrefs.GetInt(
                STR_KEY,
                STR
            );


        DEX =
            PlayerPrefs.GetInt(
                DEX_KEY,
                DEX
            );


        CON =
            PlayerPrefs.GetInt(
                CON_KEY,
                CON
            );


        INT =
            PlayerPrefs.GetInt(
                INT_KEY,
                INT
            );
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("DEBUG - 현재 최종 스탯 출력")]
    private void DebugPrintFinalStats()
    {
        Debug.Log(
            "[PlayerStats] 현재 최종 스탯\n" +
            $"STR: {STR} + 장비 {equipmentSTR} + 상태 {StatusSTRBonus} = {TotalSTR}\n" +
            $"DEX: {DEX} + 장비 {equipmentDEX} + 상태 {StatusDEXBonus} = {TotalDEX}\n" +
            $"CON: {CON} + 장비 {equipmentCON} + 상태 {StatusCONBonus} = {TotalCON}\n" +
            $"INT: {INT} + 장비 {equipmentINT} + 상태 {StatusINTBonus} = {TotalINT}"
        );
    }
}