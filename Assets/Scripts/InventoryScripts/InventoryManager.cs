using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Size")]
    public int width = 8;
    public int height = 8;

    [Header("Unlocked Area")]
    public int unlockedWidth = 8;
    public int unlockedHeight = 1;

    public List<InventoryItem> items =
        new List<InventoryItem>();

    private InventoryItem[,] grid;

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

        if (items == null)
        {
            items =
                new List<InventoryItem>();
        }

        int removedCount =
            items.RemoveAll(
                item =>
                    item == null ||
                    item.data == null ||
                    item.data.id <= 0
            );

        if (removedCount > 0)
        {
            Debug.LogWarning(
                "[InventoryManager] 씬에 저장돼 있던 " +
                $"잘못된 빈 아이템 {removedCount}개를 제거했습니다."
            );
        }

        grid =
            new InventoryItem[
                width,
                height
            ];
    }

    private void Start()
    {
        ApplyCapacityFromStats();
        RefreshGrid();
    }

    public void ApplyCapacityFromStats()
    {
        if (PlayerStats.Instance == null)
            return;

        int capacity =
            PlayerStats.Instance
                .InventoryCapacity;

        int calculatedHeight =
            Mathf.CeilToInt(
                (float)capacity /
                Mathf.Max(1, width)
            );

        unlockedWidth =
            Mathf.Clamp(
                unlockedWidth,
                1,
                width
            );

        unlockedHeight =
            Mathf.Clamp(
                calculatedHeight,
                1,
                height
            );

        RefreshGrid();

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .RebuildAndRefresh();
        }
    }

    public void AddTestBandage()
    {
        AddItem(1001);
    }

    public void AddTestLongBandage()
    {
        AddItem(1004);
    }

    public void AddTestMedKit()
    {
        AddItem(1007);
    }

    public void AddTestScrap()
    {
        AddItem(1005);
    }

    public void AddTestWeapon()
    {
        AddItem(2001);
    }

    public void AddTestArmor()
    {
        AddItem(2002);
    }

    public bool AddItem(int itemId)
    {
        return AddItem(itemId, 1);
    }

    // 지정 수량만큼 아이템을 획득한다.
    // 1020(솔라스톤)은 인벤토리 칸 대신 별도 화폐 수치로 지급한다.
    public bool AddItem(int itemId, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning(
                "[InventoryManager] 추가할 아이템 수량은 1 이상이어야 합니다.\\n" +
                $"Item ID: {itemId}\\n" +
                $"Amount: {amount}"
            );
            return false;
        }

        if (itemId == 1020)
        {
            if (SolastoneManager.Instance == null)
            {
                Debug.LogError(
                    "[InventoryManager] SolastoneManager.Instance가 없습니다.\\n" +
                    "StartScene의 SolastoneManager 오브젝트를 확인해주세요."
                );
                return false;
            }

            SolastoneManager.Instance.Add(amount);

            Debug.Log(
                "[InventoryManager] 솔라스톤 획득 처리\\n" +
                $"획득량: {amount}\\n" +
                $"현재 보유량: {SolastoneManager.Instance.CurrentAmount}"
            );

            return true;
        }

        if (ItemDatabase.Instance == null)
        {
            Debug.LogError(
                "ItemDatabase.Instance가 없습니다."
            );
            return false;
        }

        ItemData data =
            ItemDatabase.Instance
                .GetItem(itemId);

        if (data == null)
            return false;

        int addedCount = 0;

        for (int i = 0; i < amount; i++)
        {
            InventoryItem newItem =
                new InventoryItem(data);

            bool found =
                TryFindEmptyPosition(
                    newItem,
                    true,
                    out Vector2Int position,
                    out int rotation
                );

            if (!found)
            {
                Debug.Log(
                    "인벤토리 공간이 부족합니다. " +
                    "초과 칸은 다음 단계에서 연결합니다."
                );
                break;
            }

            newItem.SetRotation(rotation);
            items.Add(newItem);

            PlaceItem(
                newItem,
                position
            );

            addedCount++;
        }

        if (addedCount > 0)
            RefreshUI();

        if (addedCount < amount)
        {
            Debug.LogWarning(
                "[InventoryManager] 요청한 아이템 수량을 전부 추가하지 못했습니다.\\n" +
                $"Item ID: {itemId}\\n" +
                $"요청 수량: {amount}\\n" +
                $"추가 수량: {addedCount}"
            );
        }

        return addedCount == amount;
    }

    public bool TryMoveItem(
        InventoryItem movingItem,
        Vector2Int targetPosition)
    {
        if (movingItem == null ||
            !items.Contains(movingItem))
        {
            return false;
        }

        Vector2Int originalPosition =
            movingItem.position;

        int originalRotation =
            movingItem.rotation;

        RemoveFromGrid(movingItem);

        List<InventoryItem> overlappedItems =
            GetOverlappedItems(
                movingItem,
                targetPosition
            );

        if (overlappedItems.Count == 0)
        {
            if (CanPlaceItem(
                movingItem,
                targetPosition,
                null))
            {
                PlaceItem(
                    movingItem,
                    targetPosition
                );

                RefreshUI();

                return true;
            }
        }
        else if (overlappedItems.Count == 1)
        {
            InventoryItem swapItem =
                overlappedItems[0];

            Vector2Int swapOriginalPosition =
                swapItem.position;

            int swapOriginalRotation =
                swapItem.rotation;

            RemoveFromGrid(swapItem);

            bool canMove =
                CanPlaceItem(
                    movingItem,
                    targetPosition,
                    null
                );

            bool canSwap =
                CanPlaceItem(
                    swapItem,
                    originalPosition,
                    null
                );

            if (canMove && canSwap)
            {
                PlaceItem(
                    movingItem,
                    targetPosition
                );

                PlaceItem(
                    swapItem,
                    originalPosition
                );

                RefreshUI();

                return true;
            }

            swapItem.SetRotation(
                swapOriginalRotation
            );

            PlaceItem(
                swapItem,
                swapOriginalPosition
            );
        }

        movingItem.SetRotation(
            originalRotation
        );

        PlaceItem(
            movingItem,
            originalPosition
        );

        RefreshUI();

        return false;
    }

    public bool CanPlaceItem(
        InventoryItem item,
        Vector2Int targetPosition,
        InventoryItem ignoreItem)
    {
        if (item == null)
            return false;

        foreach (
            Vector2Int cell
            in item.GetOccupiedCells(
                targetPosition
            ))
        {
            if (!IsInsideUnlocked(cell))
                return false;

            InventoryItem occupying =
                grid[cell.x, cell.y];

            if (occupying != null &&
                occupying != ignoreItem)
            {
                return false;
            }
        }

        return true;
    }

    public List<InventoryItem>
        GetOverlappedItems(
            InventoryItem movingItem,
            Vector2Int targetPosition)
    {
        List<InventoryItem> result =
            new List<InventoryItem>();

        foreach (
            Vector2Int cell
            in movingItem.GetOccupiedCells(
                targetPosition
            ))
        {
            if (!IsInside(cell))
                continue;

            InventoryItem other =
                grid[cell.x, cell.y];

            if (other != null &&
                other != movingItem &&
                !result.Contains(other))
            {
                result.Add(other);
            }
        }

        return result;
    }

    public void UseItem(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            item.data.IsEquipment)
        {
            return;
        }

        StatusEffectController playerStatus =
    null;

        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null)
        {
            playerStatus =
                BattleManager.Instance.playerUnit
                    .GetComponent<StatusEffectController>();
        }

        if (
            playerStatus != null &&
            !playerStatus.CanUseItem())
        {
            Debug.Log(
                "[InventoryManager] 혼란 상태이므로 " +
                "아이템을 사용할 수 없습니다."
            );

            return;
        }

        if (BattleManager.Instance != null &&
            BattleManager.Instance
                .IsBattleRunning() &&
            !BattleManager.Instance
                .CanPlayerUseItem())
        {
            Debug.Log(
                "지금은 플레이어 턴이 아니라 " +
                "아이템을 사용할 수 없습니다."
            );

            return;
        }

        if (!CanUseItemInCurrentContext(item))
        {
            return;
        }

        // 1013 벽돌은 적을 먼저 선택한 뒤 실제로 소비한다.
        if (item.data.id == 1013)
        {
            if (BattleManager.Instance != null &&
                BattleManager.Instance.BeginTargetDamageItem(
                    1013,
                    10
                ))
            {
                Debug.Log(
                    "[InventoryManager] 벽돌 대상 선택 대기"
                );
            }

            return;
        }

        // 1019 개조 장치는 스탯 선택 후 실제 소비한다.
        if (item.data.id == 1019)
        {
            if (ItemStatSelectionUI.Instance == null)
            {
                Debug.LogWarning(
                    "[InventoryManager] ItemStatSelectionUI가 없어 개조 장치를 사용할 수 없습니다."
                );
                return;
            }

            ItemStatSelectionUI.Instance
                .Show();

            return;
        }

        ApplyItemEffect(item);

        item.remainUseCount--;

        if (item.remainUseCount <= 0)
        {
            RemoveItem(item);
        }
        else
        {
            RefreshUI();
        }

        if (BattleManager.Instance != null &&
            BattleManager.Instance
                .IsBattleRunning())
        {
            BattleManager.Instance
                .OnPlayerUsedItem();
        }
        else if (
            DungeonManager.Instance != null &&
            item.data.consumeTurnOnUse)
        {
            DungeonManager.Instance
                .AddTurn("아이템 사용");
        }
    }

    // =========================================================
    // Item use condition
    // =========================================================

    private bool CanUseItemInCurrentContext(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null)
        {
            return false;
        }

        int id = item.data.id;

        bool inBattle =
            BattleManager.Instance != null &&
            BattleManager.Instance.IsBattleRunning();

        switch (id)
        {
            // 던전 이동 중 사용
            case 1011:
            case 1023:
                if (inBattle ||
                    DungeonManager.Instance == null)
                {
                    Debug.Log(
                        "[InventoryManager] 이 아이템은 던전 이동 중에만 사용할 수 있습니다."
                    );
                    return false;
                }
                break;

            // 휴식 그리드에서만 사용
            case 1012:
                if (inBattle ||
                    DungeonManager.Instance == null ||
                    DungeonManager.Instance.GetCurrentTileType() !=
                        DungeonTileType.Rest)
                {
                    Debug.Log(
                        "[InventoryManager] 텐트는 휴식 그리드에서만 사용할 수 있습니다."
                    );
                    return false;
                }

                if (RestTileManager.Instance == null ||
                    !RestTileManager.Instance.CanRest(
                        DungeonManager.Instance.CurrentRoom
                    ))
                {
                    Debug.Log(
                        "[InventoryManager] 현재 위치에서는 휴식할 수 없습니다."
                    );
                    return false;
                }
                break;

            // 전투 중에만 사용
            case 1013:
            case 1014:
            case 1015:
            case 1016:
            case 1017:
            case 1018:
            case 1022:
                if (!inBattle)
                {
                    Debug.Log(
                        "[InventoryManager] 이 아이템은 전투 중에만 사용할 수 있습니다."
                    );
                    return false;
                }
                break;

            // 던전 이동 중 사용
            case 1019:
                if (inBattle ||
                    DungeonManager.Instance == null)
                {
                    Debug.Log(
                        "[InventoryManager] 개조 장치는 던전 이동 중에만 사용할 수 있습니다."
                    );
                    return false;
                }
                break;

            // 상점/거래 시스템 연결 전
            case 1020:
                Debug.Log(
                    "[InventoryManager] 솔라스톤은 거래 시스템에서 사용합니다. 현재는 직접 사용할 수 없습니다."
                );
                return false;

            // 열쇠는 잠금문 선택 시 LockedDoorManager가 자동 소비
            case 1021:
                Debug.Log(
                    "[InventoryManager] 열쇠는 인벤토리에서 직접 사용하지 않고 잠금문을 열 때 자동 사용합니다."
                );
                return false;
        }

        return true;
    }


    // =========================================================
    // Item Effect
    // =========================================================

    private void ApplyItemEffect(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null)
        {
            return;
        }

        // ---------------------------------------------------------
        // 새 아이템 효과 테이블
        // ---------------------------------------------------------
        //
        // ItemEffectDatabase에 효과가 등록되어 있으면
        // 새 테이블 방식을 우선 사용한다.
        //
        // 아직 효과 테이블로 이전하지 않은 기존 회복 아이템은
        // 아래 Legacy 로직을 그대로 사용한다.
        // ---------------------------------------------------------

        if (ItemEffectDatabase.Instance != null &&
            ItemEffectDatabase.Instance.HasEffects(
                item.data.id))
        {
            List<ItemEffectData> effects =
                ItemEffectDatabase.Instance.GetEffects(
                    item.data.id
                );

            foreach (
                ItemEffectData effect
                in effects)
            {
                ExecuteItemEffect(
                    item,
                    effect
                );
            }

            return;
        }

        // ---------------------------------------------------------
        // 기존 아이템 효과
        // ---------------------------------------------------------

        ApplyLegacyItemEffect(
            item
        );
    }


    private void ExecuteItemEffect(
        InventoryItem item,
        ItemEffectData effect)
    {
        if (item == null ||
            item.data == null ||
            effect == null)
        {
            return;
        }

        Debug.Log(
            "[InventoryManager] 아이템 효과 실행\n" +
            $"Item ID: {item.data.id}\n" +
            $"Effect Type: {effect.effectType}\n" +
            $"Effect Value: {effect.effectValue}\n" +
            $"Status ID: {effect.statusId}\n" +
            $"Order: {effect.order}"
        );


        switch (effect.effectType)
        {
            // =====================================================
            // effect_type 1 = 체력 회복
            // =====================================================

            case 1:

                // target 1/2 + 전투 조건이면 회복이 아니라
                // 원본 아이템 효과 테이블의 '데미지' 용도.
                if (effect.useCondition == 1 &&
                    effect.target == 2)
                {
                    if (BattleManager.Instance != null)
                    {
                        BattleManager.Instance
                            .UseAllEnemyDamageItem(
                                Mathf.RoundToInt(
                                    effect.effectValue
                                )
                            );
                    }

                    break;
                }

                // target 1인 1013은 UseItem에서 대상 선택 후
                // BattleManager가 직접 처리하므로 여기서는 실행하지 않는다.
                if (effect.useCondition == 1 &&
                    effect.target == 1)
                {
                    break;
                }

                if (PlayerResourceManager.Instance == null)
                {
                    Debug.LogError(
                        "[InventoryManager] PlayerResourceManager.Instance가 없습니다."
                    );

                    break;
                }

                PlayerResourceManager.Instance
                    .HealHealthItem(
                        Mathf.RoundToInt(effect.effectValue)
                    );

                break;


            // =====================================================
            // effect_type 2 = 배고픔 회복
            // =====================================================

            case 2:

                if (PlayerResourceManager.Instance == null)
                {
                    Debug.LogError(
                        "[InventoryManager] PlayerResourceManager.Instance가 없습니다."
                    );

                    break;
                }

                PlayerResourceManager.Instance
                    .HealHungerItem(
                        Mathf.RoundToInt(effect.effectValue)
                    );

                break;


            // =====================================================
            // effect_type 3 = 정신력 회복
            // =====================================================

            case 3:

                if (PlayerResourceManager.Instance == null)
                {
                    Debug.LogError(
                        "[InventoryManager] PlayerResourceManager.Instance가 없습니다."
                    );

                    break;
                }

                PlayerResourceManager.Instance
                    .HealMentalItem(
                        Mathf.RoundToInt(effect.effectValue)
                    );

                break;


            // =====================================================
            // effect_type 8 = 적 명중률
            // 1022 연막탄: 다음 적 턴 명중률 -30
            // =====================================================
            case 8:

                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance
                        .SetEnemyAccuracyModifierForNextEnemyTurn(
                            Mathf.RoundToInt(
                                effect.effectValue
                            )
                        );
                }

                break;


            // =====================================================
            // effect_type 9 = 회피율/명중 관련
            // 1017 자석의 설명은 "다음 공격 혹은 무기 스킬 확정 명중".
            // 현재 이미 검증된 2125 확정명중 상태를 재사용한다.
            // =====================================================
            case 9:

                ApplyStatusEffectFromItem(
                    2125
                );

                break;


            // =====================================================
            // effect_type 10 = 이동 시 전투 확률
            // =====================================================
            case 10:

                ItemRuntimeEffectManager
                    .EnsureExists()
                    .AddEncounterEffect(
                        effect.effectValue,
                        effect.duration,
                        effect.stackable
                    );

                break;


            // =====================================================
            // effect_type 11 = 공격력
            // 1018 자극제: 다음 일반 공격 피해 2배
            // =====================================================
            case 11:

                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance
                        .ActivateDoubleNextNormalAttackDamage();
                }

                break;


            // =====================================================
            // effect_type 12 = 영구 스탯 증가
            // 실제 선택/적용은 ItemStatSelectionUI에서 처리.
            // =====================================================
            case 12:
                break;


            // =====================================================
            // effect_type 13 = 잠금 그리드 해제
            // 잠금문을 선택할 때 LockedDoorManager에서 자동 소비.
            // =====================================================
            case 13:
                break;


            // =====================================================
            // effect_type 14 = 공격 무시
            // =====================================================
            case 14:

                if (BattleManager.Instance != null)
                {
                    if (effect.duration > 0)
                    {
                        BattleManager.Instance
                            .ActivateIgnoreAllEnemyHitsThisTurn();
                    }
                    else
                    {
                        BattleManager.Instance
                            .ActivateIgnoreNextEnemyHit();
                    }
                }

                break;


            // =====================================================
            // effect_type 15 = 아이템 획득량
            // =====================================================
            case 15:

                ItemRuntimeEffectManager
                    .EnsureExists()
                    .AddFarmingMultiplier(
                        effect.effectValue,
                        effect.duration,
                        effect.stackable
                    );

                break;


            // =====================================================
            // effect_type 16 = 휴식
            // =====================================================
            case 16:

                if (RestTileManager.Instance != null &&
                    DungeonManager.Instance != null)
                {
                    RestTileManager.Instance.Rest(
                        DungeonManager.Instance.CurrentRoom
                    );
                }

                break;


            // =====================================================
            // effect_type 17
            //
            // 아이템 효과 테이블에서의 17은
            // StatusEffectDatabase 참조를 의미한다.
            //
            // StatusEffectType의 숫자 17과는 별개이다.
            // =====================================================

            case 17:

                ApplyStatusEffectFromItem(
                    effect.statusId
                );

                break;


            default:

                Debug.LogWarning(
                    "[InventoryManager] 아직 처리되지 않은 " +
                    "아이템 Effect Type입니다.\n" +
                    $"Item ID: {item.data.id}\n" +
                    $"Effect Type: {effect.effectType}"
                );

                break;
        }
    }


    private void ApplyStatusEffectFromItem(
        int statusId)
    {
        if (statusId <= 0)
        {
            Debug.LogWarning(
                "[InventoryManager] 상태이상 아이템의 " +
                "Status ID가 올바르지 않습니다.\n" +
                $"Status ID: {statusId}"
            );

            return;
        }


        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[InventoryManager] " +
                "StatusEffectDatabase.Instance가 없습니다."
            );

            return;
        }


        StatusEffectData statusData =
            StatusEffectDatabase.Instance
                .GetStatusEffect(
                    statusId
                );


        if (statusData == null)
        {
            Debug.LogError(
                "[InventoryManager] " +
                "StatusEffectDatabase에서 상태이상을 " +
                "찾지 못했습니다.\n" +
                $"Status ID: {statusId}"
            );

            return;
        }


        StatusEffectController playerStatus =
            GetPlayerStatusEffectController();


        if (playerStatus == null)
        {
            Debug.LogError(
                "[InventoryManager] 플레이어의 " +
                "StatusEffectController를 찾지 못했습니다."
            );

            return;
        }


        bool added =
            playerStatus.AddStatusEffect(
                statusData
            );


        Debug.Log(
            "[InventoryManager] 아이템 상태이상 적용\n" +
            $"Status ID: {statusId}\n" +
            $"상태이상: {statusData.buffName}\n" +
            $"적용 결과: {added}"
        );
    }


    private StatusEffectController
        GetPlayerStatusEffectController()
    {
        // ---------------------------------------------------------
        // 1. 전투 중 플레이어
        // ---------------------------------------------------------

        if (BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null)
        {
            StatusEffectController controller =
                BattleManager.Instance.playerUnit
                    .GetComponent<StatusEffectController>();

            if (controller == null)
            {
                controller =
                    BattleManager.Instance.playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >(
                            true
                        );
            }

            if (controller == null)
            {
                controller =
                    BattleManager.Instance.playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }

            if (controller != null)
            {
                return controller;
            }
        }


        // ---------------------------------------------------------
        // 2. 던전 탐험 중 플레이어
        // ---------------------------------------------------------

        if (PlayerResourceManager.Instance != null)
        {
            StatusEffectController controller =
                PlayerResourceManager.Instance
                    .GetComponent<StatusEffectController>();

            if (controller == null)
            {
                controller =
                    PlayerResourceManager.Instance
                        .GetComponentInChildren<
                            StatusEffectController
                        >(
                            true
                        );
            }

            if (controller == null)
            {
                controller =
                    PlayerResourceManager.Instance
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }

            if (controller != null)
            {
                return controller;
            }
        }


        return null;
    }


    // =========================================================
    // Legacy Item Effect
    // =========================================================
    //
    // 현재 사용 중인 기존 회복 아이템을 깨뜨리지 않기 위해
    // ItemEffectDatabase로 이전하기 전까지 유지한다.
    // =========================================================

    private void ApplyLegacyItemEffect(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null ||
            PlayerResourceManager.Instance == null)
        {
            return;
        }


        switch (item.data.id)
        {
            // 체력 회복
            case 1001:
            case 1002:
            case 1003:

                PlayerResourceManager.Instance
                    .HealHealthItem(
                        10
                    );

                break;


            // 배고픔 회복
            case 1004:

                PlayerResourceManager.Instance
                    .HealHungerItem(
                        10
                    );

                break;


            // 정신력 회복
            case 1007:

                PlayerResourceManager.Instance
                    .HealMentalItem(
                        10
                    );

                break;
        }
    }


    public void RemoveItem(
        InventoryItem item)
    {
        if (item == null)
            return;

        RemoveFromGrid(item);

        items.Remove(item);

        RefreshUI();
    }

    public bool ContainsItem(
        InventoryItem item)
    {
        return item != null &&
               items.Contains(item);
    }

    public bool TryTakeItemForEquipment(
        InventoryItem item)
    {
        if (!ContainsItem(item))
            return false;

        RemoveFromGrid(item);

        items.Remove(item);

        RefreshUI();

        return true;
    }

    public bool TryReturnEquipmentToInventory(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null)
        {
            return false;
        }

        if (items.Contains(item))
            return true;

        bool found =
            TryFindEmptyPosition(
                item,
                true,
                out Vector2Int position,
                out int rotation
            );

        if (!found)
            return false;

        item.SetRotation(rotation);

        items.Add(item);

        PlaceItem(
            item,
            position
        );

        RefreshUI();

        return true;
    }

    public bool TryRestoreItem(
        InventoryItem item,
        Vector2Int originalPosition,
        int originalRotation)
    {
        if (item == null)
            return false;

        item.SetRotation(
            originalRotation
        );

        if (!CanPlaceItem(
            item,
            originalPosition,
            null))
        {
            return false;
        }

        if (!items.Contains(item))
        {
            items.Add(item);
        }

        PlaceItem(
            item,
            originalPosition
        );

        RefreshUI();

        return true;
    }

    public bool TryFindEmptyPosition(
        InventoryItem item,
        bool allowRotation,
        out Vector2Int foundPosition,
        out int foundRotation)
    {
        foundPosition =
            Vector2Int.zero;

        foundRotation =
            item != null
                ? item.rotation
                : 0;

        if (item == null)
            return false;

        int originalRotation =
            item.rotation;

        int rotationCount =
            allowRotation
                ? 4
                : 1;

        for (
            int rotationIndex = 0;
            rotationIndex < rotationCount;
            rotationIndex++)
        {
            item.SetRotation(
                allowRotation
                    ? rotationIndex
                    : originalRotation
            );

            for (
                int y = 0;
                y < unlockedHeight;
                y++)
            {
                for (
                    int x = 0;
                    x < unlockedWidth;
                    x++)
                {
                    Vector2Int target =
                        new Vector2Int(
                            x,
                            y
                        );

                    if (!CanPlaceItem(
                        item,
                        target,
                        null))
                    {
                        continue;
                    }

                    foundPosition =
                        target;

                    foundRotation =
                        item.rotation;

                    item.SetRotation(
                        originalRotation
                    );

                    return true;
                }
            }
        }

        item.SetRotation(
            originalRotation
        );

        return false;
    }

    public InventoryItem FindFirstEquipment(
        EquipmentType equipmentType)
    {
        foreach (
            InventoryItem item
            in items)
        {
            if (item == null ||
                item.data == null ||
                !item.data.IsEquipment)
            {
                continue;
            }

            if (item.data.equipmentType ==
                equipmentType)
            {
                return item;
            }
        }

        return null;
    }

    public List<InventoryItem>
        FindEquipmentByType(
            EquipmentType equipmentType)
    {
        List<InventoryItem> result =
            new List<InventoryItem>();

        foreach (
            InventoryItem item
            in items)
        {
            if (item != null &&
                item.data != null &&
                item.data.IsEquipment &&
                item.data.equipmentType ==
                equipmentType)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private void PlaceItem(
        InventoryItem item,
        Vector2Int position)
    {
        item.position = position;

        foreach (
            Vector2Int cell
            in item.GetOccupiedCells(
                position
            ))
        {
            if (IsInside(cell))
            {
                grid[cell.x, cell.y] =
                    item;
            }
        }
    }

    private void RemoveFromGrid(
        InventoryItem item)
    {
        if (grid == null)
            return;

        for (
            int y = 0;
            y < height;
            y++)
        {
            for (
                int x = 0;
                x < width;
                x++)
            {
                if (grid[x, y] == item)
                {
                    grid[x, y] =
                        null;
                }
            }
        }
    }

    private void RefreshGrid()
    {
        grid =
            new InventoryItem[
                width,
                height
            ];

        items.RemoveAll(
            item =>
                item == null ||
                item.data == null ||
                item.data.id <= 0
        );

        foreach (
            InventoryItem item
            in items)
        {
            if (CanPlaceItem(
                item,
                item.position,
                null))
            {
                PlaceItem(
                    item,
                    item.position
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{item.data.itemName}의 " +
                    "저장 위치가 현재 인벤토리 범위와 맞지 않습니다."
                );
            }
        }
    }

    private bool IsInside(
        Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < width &&
               cell.y >= 0 &&
               cell.y < height;
    }

    private bool IsInsideUnlocked(
        Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < unlockedWidth &&
               cell.y >= 0 &&
               cell.y < unlockedHeight;
    }

    public bool IsOverCapacity()
    {
        foreach (
            InventoryItem item
            in items)
        {
            foreach (
                Vector2Int cell
                in item.GetOccupiedCells(
                    item.position
                ))
            {
                if (!IsInsideUnlocked(cell))
                    return true;
            }
        }

        return false;
    }

    private void RefreshUI()
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .RefreshUI();
        }
    }


    // =============================
    // Save / Load
    // =============================

    public void ClearForLoad()
    {
        items.Clear();
        grid = new InventoryItem[width, height];
    }

    public bool AddRestoredItem(InventoryItem item)
    {
        if (item == null || item.data == null)
            return false;

        EnsureGridCreated();

        if (!CanPlaceItem(item, item.position, null))
            return false;

        if (!items.Contains(item))
            items.Add(item);

        PlaceItem(item, item.position);
        return true;
    }

    public bool AddRestoredItemToEmptySpace(InventoryItem item)
    {
        if (item == null || item.data == null)
            return false;

        EnsureGridCreated();

        if (!TryFindEmptyPosition(
                item,
                true,
                out Vector2Int position,
                out int rotation))
        {
            return false;
        }

        item.SetRotation(rotation);

        if (!items.Contains(item))
            items.Add(item);

        PlaceItem(item, position);
        return true;
    }

    public void FinishLoad()
    {
        RefreshGrid();
        RefreshUI();
    }

    private void EnsureGridCreated()
    {
        if (grid == null ||
            grid.GetLength(0) != width ||
            grid.GetLength(1) != height)
        {
            grid = new InventoryItem[width, height];
        }
    }
    // =========================================================
    // Item ID Search / Consume
    // =========================================================

    public void CompleteModificationDeviceUse(
        string statName)
    {
        InventoryItem item =
            FindFirstItemById(1019);

        if (item == null ||
            PlayerStats.Instance == null)
        {
            return;
        }

        switch (statName)
        {
            case "STR":
                PlayerStats.Instance.AddSTR();
                break;

            case "DEX":
                PlayerStats.Instance.AddDEX();
                break;

            case "CON":
                PlayerStats.Instance.AddCON();
                break;

            case "INT":
                PlayerStats.Instance.AddINT();
                break;

            default:
                Debug.LogWarning(
                    "[InventoryManager] 알 수 없는 스탯: " +
                    statName
                );
                return;
        }

        item.remainUseCount--;

        if (item.remainUseCount <= 0)
            RemoveItem(item);
        else
            RefreshUI();

        if (DungeonManager.Instance != null &&
            item.data.consumeTurnOnUse)
        {
            DungeonManager.Instance
                .AddTurn("개조 장치 사용");
        }

        Debug.Log(
            "[InventoryManager] 개조 장치 사용 완료 → " +
            statName +
            " +1"
        );
    }


    public InventoryItem FindFirstItemById(int itemId)
    {
        if (items == null)
            return null;

        foreach (InventoryItem item in items)
        {
            if (item == null ||
                item.data == null)
            {
                continue;
            }

            if (item.data.id == itemId)
            {
                return item;
            }
        }

        return null;
    }


    public bool HasItemById(int itemId)
    {
        return FindFirstItemById(itemId) != null;
    }


    public bool ConsumeItemById(int itemId)
    {
        InventoryItem item =
            FindFirstItemById(itemId);

        if (item == null)
        {
            Debug.Log(
                "[InventoryManager] 아이템을 찾지 못했습니다.\n" +
                $"Item ID: {itemId}"
            );

            return false;
        }


        Debug.Log(
            "[InventoryManager] 아이템 소비\n" +
            $"Item ID: {itemId}\n" +
            $"아이템: {item.data.itemName}"
        );


        RemoveItem(item);

        return true;
    }
}