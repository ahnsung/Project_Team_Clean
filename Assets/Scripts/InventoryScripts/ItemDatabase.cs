using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    // =========================================================
    // Item Sprites - 1001 ~ 1023
    // 에셋을 받으면 ItemDatabase 오브젝트의 슬롯에 Sprite만 넣으면 됩니다.
    // =========================================================

    [Header("Item Sprites - Recovery (1001 ~ 1010)")]
    [Tooltip("1001 붕대")] public Sprite item1001BandageIcon;
    [Tooltip("1002 미니 의료키트")] public Sprite item1002MiniMedicalKitIcon;
    [Tooltip("1003 구급 상자")] public Sprite item1003FirstAidKitIcon;
    [Tooltip("1004 통조림")] public Sprite item1004CannedFoodIcon;
    [Tooltip("1005 전투식량")] public Sprite item1005CombatRationIcon;
    [Tooltip("1006 스테이크")] public Sprite item1006SteakIcon;
    [Tooltip("1007 술")] public Sprite item1007AlcoholIcon;
    [Tooltip("1008 주사기")] public Sprite item1008SyringeIcon;
    [Tooltip("1009 급속 안정제")] public Sprite item1009RapidSedativeIcon;
    [Tooltip("1010 생수병")] public Sprite item1010WaterBottleIcon;

    [Header("Item Sprites - Utility / Battle (1011 ~ 1023)")]
    [Tooltip("1011 손전등")] public Sprite item1011FlashlightIcon;
    [Tooltip("1012 텐트")] public Sprite item1012TentIcon;
    [Tooltip("1013 벽돌")] public Sprite item1013BrickIcon;
    [Tooltip("1014 화염병")] public Sprite item1014MolotovIcon;
    [Tooltip("1015 1회용 에너지 보호막")] public Sprite item1015EnergyShieldIcon;
    [Tooltip("1016 강화 에너지 보호막")] public Sprite item1016ReinforcedEnergyShieldIcon;
    [Tooltip("1017 자석")] public Sprite item1017MagnetIcon;
    [Tooltip("1018 자극제")] public Sprite item1018StimulantIcon;
    [Tooltip("1019 개조 장치")] public Sprite item1019ModificationDeviceIcon;
    [Tooltip("1020 솔라스톤")] public Sprite item1020SolaStoneIcon;
    [Tooltip("1021 열쇠")] public Sprite item1021KeyIcon;
    [Tooltip("1022 연막탄")] public Sprite item1022SmokeBombIcon;
    [Tooltip("1023 투명 장치")] public Sprite item1023InvisibilityDeviceIcon;

    [Header("Equipment Test Sprites")]
    public Sprite testWeaponIcon;
    public Sprite testArmorIcon;

    [Header("Legacy Key Sprite (3001 ~ 3004)")]
    [Tooltip("기존 테스트용 K_1~K_4 공용 아이콘")]
    public Sprite keyIcon;


    private readonly Dictionary<int, ItemData> database =
        new Dictionary<int, ItemData>();


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

        CreateItems();
    }


    // =========================================================
    // Item Creation
    // =========================================================

    private void CreateItems()
    {
        database.Clear();


        // =====================================================
        // 회복 아이템 - 원본 "아이템 테이블" 기준
        // =====================================================

        CreateConsumableItem(
            1001,
            "붕대",
            "체력을 회복한다. (소)",
            item1001BandageIcon,
            2,
            true,
            true,
            CreateShape1()
        );

        CreateConsumableItem(
            1002,
            "미니 의료키트",
            "체력을 회복한다. (중)",
            item1002MiniMedicalKitIcon,
            2,
            true,
            true,
            CreateShape1()
        );

        CreateConsumableItem(
            1003,
            "구급 상자",
            "체력을 회복한다. (대)",
            item1003FirstAidKitIcon,
            1,
            true,
            true,
            CreateShape4_1()
        );

        CreateConsumableItem(
            1004,
            "통조림",
            "배고픔을 회복한다. (소)",
            item1004CannedFoodIcon,
            2,
            true,
            true,
            CreateShape1()
        );

        CreateConsumableItem(
            1005,
            "전투식량",
            "배고픔을 회복한다. (중)",
            item1005CombatRationIcon,
            2,
            true,
            true,
            CreateShape1()
        );

        CreateConsumableItem(
            1006,
            "스테이크",
            "배고픔을 회복한다. (대)",
            item1006SteakIcon,
            1,
            true,
            true,
            CreateShape4_1()
        );

        CreateConsumableItem(
            1007,
            "술",
            "정신력을 회복한다. (소)",
            item1007AlcoholIcon,
            2,
            true,
            true,
            CreateShape2()
        );

        CreateConsumableItem(
            1008,
            "주사기",
            "정신력을 회복한다. (중)",
            item1008SyringeIcon,
            1,
            true,
            true,
            CreateShape2()
        );

        CreateConsumableItem(
            1009,
            "급속 안정제",
            "정신력을 회복한다. (대)",
            item1009RapidSedativeIcon,
            1,
            true,
            true,
            CreateShape3_1()
        );

        CreateConsumableItem(
            1010,
            "생수병",
            "체력과 정신력을 회복한다. (소)",
            item1010WaterBottleIcon,
            2,
            true,
            true,
            CreateShape2()
        );


        // =====================================================
        // 기타 아이템 - 원본 "아이템 테이블" 기준 1011 ~ 1023
        // =====================================================

        CreateConsumableItem(1011, "손전등",
            "일정 턴 동안 전투 발생 확률을 올리지만 파밍 그리드에서의 아이템 획득량이 증가한다.",
            item1011FlashlightIcon, 3, true, true, CreateShape2());

        CreateConsumableItem(1012, "텐트",
            "휴식 그리드에서 사용 시 휴식할 수 있다.",
            item1012TentIcon, 1, false, true, CreateLinearShape(4));

        CreateConsumableItem(1013, "벽돌",
            "지정한 적에게 X 피해를 준다.",
            item1013BrickIcon, 1, true, true, CreateShape1());

        CreateConsumableItem(1014, "화염병",
            "적 전체에게 X 피해를 준다.",
            item1014MolotovIcon, 1, true, true, CreateShape2());

        CreateConsumableItem(1015, "1회용 에너지 보호막",
            "적의 공격을 1회 무시한다.",
            item1015EnergyShieldIcon, 1, true, true, CreateShape1());

        CreateConsumableItem(1016, "강화 에너지 보호막",
            "이번 턴 모든 공격을 무시한다.",
            item1016ReinforcedEnergyShieldIcon, 1, true, true, CreateShape1());

        CreateConsumableItem(1017, "자석",
            "다음 공격 혹은 무기 스킬이 무조건 적중한다.",
            item1017MagnetIcon, 1, true, true, CreateShape1());

        CreateConsumableItem(1018, "자극제",
            "다음 공격의 데미지가 두 배가 된다.",
            item1018StimulantIcon, 1, true, true, CreateShape1());

        CreateConsumableItem(1019, "개조 장치",
            "사용 시 원하는 스탯을 올릴 수 있다.",
            item1019ModificationDeviceIcon, 1, true, false, CreateShape1());

        CreateConsumableItem(1020, "솔라스톤",
            "적과의 거래에 사용.",
            item1020SolaStoneIcon, 1, true, false, CreateShape1());

        CreateConsumableItem(1021, "열쇠",
            "잠금 그리드에 사용 시 이동 가능.",
            item1021KeyIcon != null ? item1021KeyIcon : keyIcon, 1, true, false, CreateShape1());

        CreateConsumableItem(1022, "연막탄",
            "일정 턴 동안 적의 명중률이 감소한다.",
            item1022SmokeBombIcon, 1, true, true, CreateShape1());

        CreateConsumableItem(1023, "투명 장치",
            "일정 턴 동안 전투가 발생하지 않는다.",
            item1023InvisibilityDeviceIcon, 1, true, true, CreateShape1());


        // =====================================================
        // 테스트 무기
        // =====================================================

        ItemData testWeapon =
            NewBaseItem(
                2001,
                "Test Rifle",
                ItemCategory.Equipment,
                "공격력 +5, DEX +1을 제공하는 테스트 무기입니다.",
                testWeaponIcon
            );

        testWeapon.equipmentType =
            EquipmentType.Weapon;

        testWeapon.maxDurability =
            50;

        testWeapon.statModifier.dex =
            1;

        testWeapon.statModifier.attackPower =
            5;

        testWeapon.weaponSkillId =
            4005;

        testWeapon.weaponSkillDescription =
            "견디기 - 이번 라운드 동안 받는 피해가 50% 감소합니다.";

        testWeapon.shape.Add(
            new Vector2Int(0, 0)
        );

        testWeapon.shape.Add(
            new Vector2Int(1, 0)
        );

        testWeapon.shape.Add(
            new Vector2Int(2, 0)
        );

        database[
            testWeapon.id
        ] = testWeapon;


        // =====================================================
        // 테스트 갑옷
        // =====================================================

        ItemData testArmor =
            NewBaseItem(
                2002,
                "Test Armor",
                ItemCategory.Equipment,
                "명중률 +10, INT +1을 제공하는 테스트 갑옷입니다.",
                testArmorIcon
            );

        testArmor.equipmentType =
            EquipmentType.Armor;

        testArmor.maxDurability =
            50;

        testArmor.statModifier.intelligence =
            1;

        testArmor.statModifier.accuracyBonus =
            10;

        testArmor.shape.Add(
            new Vector2Int(0, 0)
        );

        testArmor.shape.Add(
            new Vector2Int(0, 1)
        );

        testArmor.shape.Add(
            new Vector2Int(1, 1)
        );

        database[
            testArmor.id
        ] = testArmor;


        // =====================================================
        // 기존 테스트 열쇠
        //
        // 실제 아이템 테이블의 1030~1034 열쇠 전환 전까지
        // 기존 문 시스템 호환을 위해 그대로 유지한다.
        // =====================================================

        CreateKeyItem(
            3001,
            "열쇠 K_1",
            "잠긴 문 K_1을 열기 위한 열쇠입니다.",
            keyIcon
        );

        CreateKeyItem(
            3002,
            "열쇠 K_2",
            "잠긴 문 K_2을 열기 위한 열쇠입니다.",
            keyIcon
        );

        CreateKeyItem(
            3003,
            "열쇠 K_3",
            "잠긴 문 K_3을 열기 위한 열쇠입니다.",
            keyIcon
        );

        CreateKeyItem(
            3004,
            "열쇠 K_4",
            "잠긴 문 K_4을 열기 위한 열쇠입니다.",
            keyIcon
        );


        Debug.Log(
            "[ItemDatabase] 아이템 등록 완료: " +
            database.Count +
            "개"
        );
    }


    // =========================================================
    // Base Item
    // =========================================================

    private ItemData NewBaseItem(
        int id,
        string itemName,
        ItemCategory category,
        string description,
        Sprite icon)
    {
        ItemData item =
            new ItemData
            {
                id = id,

                itemName =
                    itemName,

                category =
                    category,

                maxUseCount =
                    0,

                consumeTurnOnUse =
                    false,

                canDrop =
                    true,

                effectDescription =
                    description,

                icon =
                    icon,

                shape =
                    new List<Vector2Int>(),

                statModifier =
                    new EquipmentStatModifier()
            };


        return item;
    }


    // =========================================================
    // Recovery
    // =========================================================

    private void CreateRecoveryItem(
        int id,
        string itemName,
        string description,
        Sprite icon)
    {
        ItemData item =
            NewBaseItem(
                id,
                itemName,
                ItemCategory.Recovery,
                description,
                icon
            );


        item.maxUseCount =
            2;

        item.consumeTurnOnUse =
            true;


        item.shape.Add(
            Vector2Int.zero
        );


        database[
            id
        ] = item;
    }


    // =========================================================
    // Consumable
    // =========================================================

    private void CreateConsumableItem(
        int id,
        string itemName,
        string description,
        Sprite icon,
        int maxUseCount,
        bool consumeTurnOnUse,
        bool canDrop,
        List<Vector2Int> shape)
    {
        ItemData item =
            NewBaseItem(
                id,
                itemName,
                ItemCategory.Recovery,
                description,
                icon
            );

        item.maxUseCount =
            Mathf.Max(
                1,
                maxUseCount
            );

        item.consumeTurnOnUse =
            consumeTurnOnUse;

        item.canDrop =
            canDrop;

        item.shape =
            shape ??
            CreateShape1();

        item.EnsureValidShape();

        database[
            id
        ] = item;
    }


    // =========================================================
    // Shape Helpers
    // =========================================================

    private List<Vector2Int> CreateShape1()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 0)
        };
    }


    private List<Vector2Int> CreateShape2()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0)
        };
    }


    private List<Vector2Int> CreateShape3_1()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };
    }


    private List<Vector2Int> CreateLinearShape(int count)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int safeCount = Mathf.Max(1, count);

        for (int i = 0; i < safeCount; i++)
        {
            result.Add(new Vector2Int(i, 0));
        }

        return result;
    }


    private List<Vector2Int> CreateShape4_1()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };
    }


    // =========================================================
    // Key
    // =========================================================

    private void CreateKeyItem(
        int id,
        string itemName,
        string description,
        Sprite icon)
    {
        ItemData key =
            NewBaseItem(
                id,
                itemName,
                ItemCategory.Etc,
                description,
                icon
            );


        // 열쇠는 사용 아이템이 아님
        key.maxUseCount =
            0;

        key.consumeTurnOnUse =
            false;


        // 중요:
        // 열쇠는 플레이어가 버릴 수 없다.
        key.canDrop =
            false;


        // 인벤토리 1칸
        key.shape.Add(
            Vector2Int.zero
        );


        database[
            id
        ] = key;
    }


    // =========================================================
    // Get Item
    // =========================================================

    public ItemData GetItem(
        int id)
    {
        if (database.TryGetValue(
            id,
            out ItemData item))
        {
            return item;
        }


        Debug.LogError(
            "ItemDatabase에 없는 아이템 ID: " +
            id
        );


        return null;
    }


    // =========================================================
    // Utility
    // =========================================================

    public bool HasItem(
        int id)
    {
        return database.ContainsKey(
            id
        );
    }

    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("DEBUG - Add Water Item 1010")]
    private void DebugAddWaterItem1010()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[ItemDatabase] InventoryManager.Instance가 없습니다."
            );

            return;
        }

        if (!HasItem(1010))
        {
            Debug.LogError(
                "[ItemDatabase] 1010 생수병이 등록되어 있지 않습니다."
            );

            return;
        }

        bool added =
            InventoryManager.Instance.AddItem(
                1010
            );

        if (added)
        {
            Debug.Log(
                "[ItemDatabase] DEBUG - 1010 생수병을 인벤토리에 추가했습니다."
            );
        }
        else
        {
            Debug.LogWarning(
                "[ItemDatabase] DEBUG - 1010 생수병 추가 실패. 인벤토리 공간을 확인하세요."
            );
        }
    }


    [ContextMenu("DEBUG - Add All Items 1001~1023")]
    private void DebugAddAllItems1001To1023()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ItemDatabase] InventoryManager.Instance가 없습니다.");
            return;
        }

        int success = 0;
        int failed = 0;

        for (int id = 1001; id <= 1023; id++)
        {
            if (!HasItem(id))
            {
                Debug.LogWarning($"[ItemDatabase] Item ID {id}가 등록되어 있지 않습니다.");
                failed++;
                continue;
            }

            if (InventoryManager.Instance.AddItem(id))
                success++;
            else
                failed++;
        }

        Debug.Log(
            $"[ItemDatabase] DEBUG 일괄 지급 완료 / 성공: {success} / 실패: {failed}\n" +
            "인벤토리 공간이 부족하면 실패할 수 있습니다."
        );
    }

}