using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTileEventManager : MonoBehaviour
{
    public static DungeonTileEventManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Used Tiles
    // =========================================================

    private readonly HashSet<Vector2Int>
        usedChestTiles =
            new HashSet<Vector2Int>();


    private readonly HashSet<Vector2Int>
        usedKeyTiles =
            new HashSet<Vector2Int>();


    private readonly HashSet<Vector2Int>
        usedFarmingTiles =
            new HashSet<Vector2Int>();


    // =========================================================
    // References
    // =========================================================

    [Header("References")]

    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private BattleManager battleManager;

    [SerializeField]
    private FadeController fadeController;


    // =========================================================
    // Boss Battle Background
    // =========================================================

    [Header("Boss Battle Background")]

    [Tooltip("기존 일반 배경 오브젝트 5개를 순서대로 연결합니다.")]
    [SerializeField]
    private GameObject[] normalBackgrounds;

    [Tooltip("보스 전용 배경 오브젝트 5개를 순서대로 연결합니다.")]
    [SerializeField]
    private GameObject[] bossBackgrounds;

    private bool[] savedNormalBackgroundStates;


    // =========================================================
    // General Battle
    // =========================================================

    [Header("General Battle")]

    [Range(0f, 100f)]
    [SerializeField]
    private float generalBattleStartChance = 10f;


    [Range(0f, 100f)]
    [SerializeField]
    private float generalBattleIncreaseAmount = 10f;


    [Tooltip("effect_type 24(전투 발생률 증가) 상태가 있을 때 General 전투 확률에 더할 값입니다. 예: 20이면 +20%p")]
    [Range(0f, 100f)]
    [SerializeField]
    private float statusBattleChanceIncrease = 20f;


    private float currentGeneralBattleChance;


    public float CurrentGeneralBattleChance
    {
        get
        {
            return currentGeneralBattleChance;
        }
    }


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


        ResetGeneralBattleChance();
    }


    private void Start()
    {
        ResolveReferences();
    }


    private void ResolveReferences()
    {
        if (dungeonManager == null)
        {
            dungeonManager =
                DungeonManager.Instance;
        }


        if (battleManager == null)
        {
            battleManager =
                FindFirstObjectByType<
                    BattleManager
                >();
        }


        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<
                    FadeController
                >();
        }
    }


    // =========================================================
    // Enter Event
    // =========================================================

    public IEnumerator ExecuteEnterEvent()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonTileEventManager] " +
                "DungeonManager가 없습니다."
            );

            yield break;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] " +
                "현재 타일 데이터가 없습니다."
            );

            yield break;
        }


        Debug.Log(
            "[DungeonTileEventManager] 타일 진입: " +
            $"({tile.X}, {tile.Y}) / " +
            $"{tile.TileType}"
        );


        switch (tile.TileType)
        {
            case DungeonTileType.General:

                yield return
                    HandleGeneralEnter(
                        tile
                    );

                break;


            case DungeonTileType.Trap:

                yield return
                    HandleTrapEnter(
                        tile
                    );

                break;


            case DungeonTileType.Teleport:

                yield return
                    HandleTeleportEnter(
                        tile
                    );

                break;


            // =====================================
            // Shop Update
            // =====================================

            case DungeonTileType.ShopUpdate:

                yield return
                    HandleShopUpdateEnter(
                        tile
                    );

                break;


            case DungeonTileType.Boss:

                yield return
                    HandleBoss(
                        tile
                    );

                break;


            default:

                break;
        }
    }


    // =========================================================
    // E Interaction
    // =========================================================

    public IEnumerator ExecuteInteraction()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            yield break;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
            yield break;
        }


        Debug.Log(
            "[DungeonTileEventManager] E 상호작용: " +
            $"({tile.X}, {tile.Y}) / " +
            $"{tile.TileType}"
        );


        switch (tile.TileType)
        {
            case DungeonTileType.Farming:

                yield return
                    HandleFarming(
                        tile
                    );

                break;


            case DungeonTileType.Key:

                yield return
                    HandleKey(
                        tile
                    );

                break;


            case DungeonTileType.Chest:

                yield return
                    HandleChest(
                        tile
                    );

                break;


            case DungeonTileType.PuzzleLetter:

                yield return
                    HandlePuzzleLetter(
                        tile
                    );

                break;


            case DungeonTileType.EventHint:

                yield return
                    HandleEventHint(
                        tile
                    );

                break;


            case DungeonTileType.Rest:

                yield return
                    HandleRest(
                        tile
                    );

                break;


            default:

                Debug.Log(
                    "[TileEvent] 현재 타일은 " +
                    "E 상호작용 대상이 아닙니다: " +
                    tile.TileType
                );

                break;
        }
    }


    // =========================================================
    // Can Interact
    // =========================================================

    public bool CanInteractCurrentTile()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            return false;
        }


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        if (tile == null)
        {
            return false;
        }


        switch (tile.TileType)
        {
            case DungeonTileType.Farming:
            case DungeonTileType.Key:
            case DungeonTileType.Chest:
            case DungeonTileType.PuzzleLetter:
            case DungeonTileType.EventHint:
            case DungeonTileType.Rest:

                return true;


            default:

                return false;
        }
    }


    // =========================================================
    // General
    // =========================================================

    private IEnumerator HandleGeneralEnter(
        DungeonTileData tile)
    {
        ResolveReferences();


        Debug.Log(
            "[General] 타일 진입\n" +
            $"좌표: ({tile.X}, {tile.Y})"
        );


        float roll =
            Random.Range(
                0f,
                100f
            );


        bool battleChanceUpActive =
            HasBattleEncounterRateUpStatus();


        float statusBonus =
            battleChanceUpActive
                ? statusBattleChanceIncrease
                : 0f;


        float itemChanceMultiplier = 1f;
        float itemChanceFlatModifier = 0f;

        if (ItemRuntimeEffectManager.Instance != null)
        {
            itemChanceMultiplier =
                ItemRuntimeEffectManager.Instance
                    .GetEncounterChanceMultiplier();

            itemChanceFlatModifier =
                ItemRuntimeEffectManager.Instance
                    .GetEncounterChanceFlatModifier();
        }

        float finalBattleChance =
            Mathf.Clamp(
                currentGeneralBattleChance *
                itemChanceMultiplier +
                itemChanceFlatModifier +
                statusBonus,
                0f,
                100f
            );


        bool battleOccurs =
            roll <
            finalBattleChance;


        Debug.Log(
            "[General] 전투 판정\n" +
            $"기본 현재 전투 확률: " +
            $"{currentGeneralBattleChance:F0}%\n" +
            $"상태이상 24 보너스: " +
            $"{statusBonus:F0}%p\n" +
            $"최종 전투 확률: " +
            $"{finalBattleChance:F0}%\n" +
            $"주사위 값: {roll:F2}"
        );


        if (!battleOccurs)
        {
            float oldChance =
                currentGeneralBattleChance;


            currentGeneralBattleChance =
                Mathf.Clamp(
                    currentGeneralBattleChance +
                    generalBattleIncreaseAmount,
                    0f,
                    100f
                );


            Debug.Log(
                "[General] 전투가 발생하지 않았습니다.\n" +
                $"이번 기본 확률: {oldChance:F0}%\n" +
                $"이번 최종 확률: {finalBattleChance:F0}%\n" +
                $"다음 General 기본 전투 확률: " +
                $"{currentGeneralBattleChance:F0}%"
            );


            yield break;
        }


        if (battleManager == null)
        {
            Debug.LogWarning(
                "[General] 전투 판정 성공했지만 " +
                "BattleManager가 없습니다."
            );

            yield break;
        }


        Debug.Log(
            "[General] 전투 발생!\n" +
            $"기본 확률: " +
            $"{currentGeneralBattleChance:F0}%\n" +
            $"상태이상 보너스: {statusBonus:F0}%p\n" +
            $"최종 확률: {finalBattleChance:F0}%"
        );


        // =====================================================
        // Encounter Table
        // =====================================================
        // 전투 발생 확률 판정은 위의 기존 General 로직을 그대로 사용한다.
        // 여기서는 전투가 발생하기로 결정된 뒤,
        // 현재 좌표에 맞는 Encounter Group / 몬스터 조합만 선택한다.

        EncounterManager encounterManager =
            EncounterManager.Instance;

        if (encounterManager == null)
        {
            encounterManager =
                FindFirstObjectByType<
                    EncounterManager
                >();
        }


        if (encounterManager == null)
        {
            Debug.LogError(
                "[General] EncounterManager를 찾을 수 없습니다.\n" +
                "씬에 EncounterManager 컴포넌트를 추가해주세요."
            );

            yield break;
        }


        Vector2Int encounterPosition =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (
            !encounterManager.TryRollEncounter(
                encounterPosition,
                out int encounterGroupId,
                out int[] encounterEnemyIds
            )
        )
        {
            Debug.LogWarning(
                "[General] 현재 좌표의 인카운터 몬스터 조합을 " +
                "선택하지 못했습니다.\n" +
                $"좌표: {encounterPosition}"
            );

            yield break;
        }


        Debug.Log(
            "[General] 인카운터 테이블 선택 완료\n" +
            $"좌표: {encounterPosition}\n" +
            $"Encounter Group: {encounterGroupId}\n" +
            $"Enemy IDs: {string.Join(", ", encounterEnemyIds)}"
        );


        // 몬스터 조합까지 정상적으로 정해진 뒤에만
        // 누적 전투 확률을 초기값으로 되돌린다.
        ResetGeneralBattleChance();


        yield return StartCoroutine(
            battleManager
                .StartBattleEncounter(
                    encounterGroupId,
                    encounterEnemyIds
                )
        );


        while (
            battleManager.IsBattleRunning()
        )
        {
            yield return null;
        }


        Debug.Log(
            "[General] 전투 종료"
        );
    }


    private bool HasBattleEncounterRateUpStatus()
    {
        StatusEffectController controller =
            null;


        // 전투용 플레이어 유닛에 연결된 상태이상 컨트롤러를 우선 사용한다.
        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null
        )
        {
            controller =
                BattleManager.Instance
                    .playerUnit
                    .GetComponent<
                        StatusEffectController
                    >();


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >();
            }
        }


        // 전투가 시작되기 전 General 타일 진입에서도
        // 상태이상을 확인할 수 있도록 Scene에서 한 번 더 찾는다.
        if (controller == null)
        {
            controller =
                FindFirstObjectByType<
                    StatusEffectController
                >();
        }


        if (controller == null)
        {
            return false;
        }


        return
            controller.HasStatusEffect(
                StatusEffectType
                    .BattleEncounterRateUp
            );
    }


    public void ResetGeneralBattleChance()
    {
        currentGeneralBattleChance =
            Mathf.Clamp(
                generalBattleStartChance,
                0f,
                100f
            );
    }


    // =========================================================
    // Farming
    // =========================================================

    private IEnumerator HandleFarming(
        DungeonTileData tile)
    {
        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (
            usedFarmingTiles.Contains(
                position
            )
        )
        {
            Debug.Log(
                "[Farming] 이미 파밍한 장소입니다."
            );

            yield break;
        }


        FarmingDataLoader loader =
            FarmingDataLoader.Instance;


        if (loader == null)
        {
            Debug.LogError(
                "[Farming] FarmingDataLoader가 없습니다."
            );

            yield break;
        }


        FarmingTileData data =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (data == null)
        {
            Debug.LogWarning(
                "[Farming] Farming_Data가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        FarmingItemGroupDatabase
            groupDatabase =
                FarmingItemGroupDatabase.Instance;


        if (groupDatabase == null)
        {
            yield break;
        }


        List<int> itemIDs =
            groupDatabase.GetItemIDs(
                data.itemGroup
            );


        if (
            itemIDs == null ||
            itemIDs.Count == 0
        )
        {
            yield break;
        }


        int amount =
            Random.Range(
                data.minItemQuantity,
                data.maxItemQuantity + 1
            );


        int baseAmount =
            amount;

        float farmingItemMultiplier =
            ItemRuntimeEffectManager.Instance != null
                ? ItemRuntimeEffectManager.Instance
                    .GetFarmingQuantityMultiplier()
                : 1f;

        amount =
            Mathf.RoundToInt(
                amount *
                farmingItemMultiplier
            );

        int farmingStatusBonus =
            GetFarmingItemAcquisitionBonus();


        amount +=
            farmingStatusBonus;


        amount =
            Mathf.Clamp(
                amount,
                1,
                6
            );


        Debug.Log(
            "[Farming] 획득 개수 계산\n" +
            $"기본 획득 개수: " +
            $"{baseAmount}\n" +
            $"아이템 획득량 배율: x" +
            $"{farmingItemMultiplier:F2}\n" +
            $"상태이상 2130 추가 획득: " +
            $"+{farmingStatusBonus}\n" +
            $"최종 획득 개수: {amount}"
        );


        List<ChestItemData> rewards =
            new List<ChestItemData>();


        for (
            int i = 0;
            i < amount;
            i++
        )
        {
            int itemID =
                itemIDs[
                    Random.Range(
                        0,
                        itemIDs.Count
                    )
                ];


            if (itemID <= 0)
            {
                continue;
            }


            rewards.Add(
                new ChestItemData(
                    itemID,
                    1
                )
            );
        }


        if (rewards.Count == 0)
        {
            yield break;
        }


        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            yield break;
        }


        yield return
            rewardUI.ShowChestRewards(
                rewards
            );


        if (rewardUI.AnyItemAcquired)
        {
            usedFarmingTiles.Add(
                position
            );


            Debug.Log(
                "[Farming] 파밍 완료: " +
                position
            );
        }
    }


    private int GetFarmingItemAcquisitionBonus()
    {
        StatusEffectController controller =
            null;


        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null
        )
        {
            controller =
                BattleManager.Instance
                    .playerUnit
                    .GetComponent<
                        StatusEffectController
                    >();


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >();
            }
        }


        if (controller == null)
        {
            controller =
                FindFirstObjectByType<
                    StatusEffectController
                >();
        }


        if (controller == null)
        {
            return 0;
        }


        // 기획서 상태이상 2130:
        // "파밍 시 아이템을 추가로 1개 획득한다."
        // 실제 DB에 등록된 2130 상태가 있을 때만 +1.
        if (
            controller.HasStatusEffectById(
                2130
            )
        )
        {
            return 1;
        }


        return 0;
    }


    // =========================================================
    // Trap
    // =========================================================

    private IEnumerator HandleTrapEnter(
    DungeonTileData tile)
    {
        TrapDataLoader trapLoader =
            TrapDataLoader.Instance;


        if (trapLoader == null)
        {
            Debug.LogError(
                "[Trap] TrapDataLoader를 찾을 수 없습니다."
            );

            yield break;
        }


        TrapTileData trapData =
            trapLoader.GetData(
                tile.X,
                tile.Y
            );


        if (trapData == null)
        {
            Debug.LogWarning(
                "[Trap] Trap 데이터가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        // =========================================================
        // 발동 확률
        // =========================================================

        int possibility =
            Mathf.Clamp(
                trapData.trapPossibility,
                0,
                100
            );


        int roll =
            Random.Range(
                0,
                100
            );


        bool triggered =
            roll < possibility;


        Debug.Log(
            "[Trap] 함정 판정\n" +
            $"좌표: ({trapData.x}, {trapData.y})\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"발동 확률: {possibility}%\n" +
            $"Roll: {roll}\n" +
            $"Amount: {trapData.trapAmount}"
        );


        // =========================================================
        // 회피
        // =========================================================

        if (!triggered)
        {
            Debug.Log(
                "[Trap] 함정을 피했습니다."
            );

            yield break;
        }


        // =========================================================
        // 발동
        // =========================================================

        Debug.Log(
            "[Trap] 함정 발동!\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"Amount: {trapData.trapAmount}"
        );


        ApplyTrapEffect(
            trapData
        );


        yield break;
    }

    // =========================================================
    // Trap Status Effect
    // =========================================================

    private void ApplyTrapStatusEffect(
        TrapTileData trapData)
    {
        if (trapData == null)
            return;


        // =====================================================
        // 상태이상 데이터 생성
        // =====================================================

        StatusEffectData statusData =
            TrapStatusEffectFactory.Create(
                trapData.trapType,
                trapData.trapAmount
            );


        if (statusData == null)
        {
            Debug.LogWarning(
                "[Trap] 아직 실제 효과가 연결되지 않은 함정입니다.\n" +
                $"TrapType: {trapData.trapType}"
            );

            return;
        }


        // =====================================================
        // Player StatusEffectController 찾기
        // =====================================================

        StatusEffectController controller =
            null;


        /*
         * BattleManager에 플레이어가 연결되어 있으면
         * 가장 확실하게 여기서 가져온다.
         */
        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null
        )
        {
            controller =
                BattleManager.Instance
                    .playerUnit
                    .GetComponent<
                        StatusEffectController
                    >();


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >();
            }
        }


        /*
         * 전투가 발생하지 않은 상태에서도
         * 함정은 작동해야 하므로 Scene에서도 찾아본다.
         */
        if (controller == null)
        {
            controller =
                FindFirstObjectByType<
                    StatusEffectController
                >();
        }


        if (controller == null)
        {
            Debug.LogError(
                "[Trap] 플레이어의 " +
                "StatusEffectController를 찾을 수 없습니다."
            );

            return;
        }


        // =====================================================
        // 상태이상 부여
        // =====================================================

        bool added =
            controller.AddStatusEffect(
                statusData
            );


        if (!added)
        {
            Debug.LogWarning(
                "[Trap] 상태이상 적용 실패\n" +
                $"TrapType: {trapData.trapType}"
            );

            return;
        }


        Debug.Log(
            "[Trap] 상태이상 적용 완료\n" +
            $"이름: {statusData.buffName}\n" +
            $"EffectType: {statusData.effectType}\n" +
            $"EffectPower: {statusData.effectPower}\n" +
            $"지속시간: {statusData.buffDuration}"
        );
    }
    // =========================================================
    // Trap Effect
    // =========================================================

    // =========================================================
    // Trap Effect
    // =========================================================

    private void ApplyTrapEffect(
        TrapTileData trapData)
    {
        if (trapData == null)
        {
            Debug.LogWarning(
                "[Trap] TrapTileData가 null입니다."
            );

            return;
        }


        Debug.Log(
            "[Trap] 효과 적용 시도\n" +
            $"TrapType: {trapData.trapType}\n" +
            $"Amount: {trapData.trapAmount}"
        );


        // =====================================================
        // 상태이상 함정 처리
        // =====================================================

        ApplyTrapStatusEffect(
            trapData
        );
    }

    // =========================================================
    // TELEPORT
    // =========================================================

    // =========================================================
    // Shop Update
    // =========================================================

    private IEnumerator HandleShopUpdateEnter(
        DungeonTileData tile)
    {
        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] " +
                "Shop_Update 타일 데이터가 없습니다."
            );

            yield break;
        }


        // -----------------------------------------------------
        // ShopTileDataLoader 확인
        // -----------------------------------------------------

        if (ShopTileDataLoader.Instance == null)
        {
            Debug.LogError(
                "[DungeonTileEventManager] " +
                "ShopTileDataLoader.Instance가 없습니다."
            );

            yield break;
        }


        // -----------------------------------------------------
        // 현재 좌표의 Shop Level 확인
        // -----------------------------------------------------

        if (!ShopTileDataLoader.Instance.TryGetShopLevel(
                tile.X,
                tile.Y,
                out int shopLevel))
        {
            Debug.LogWarning(
                "[DungeonTileEventManager] " +
                "Shop_Update 좌표에 해당하는 " +
                "Shop Level 데이터가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }


        // -----------------------------------------------------
        // ShopManager 확인
        // -----------------------------------------------------

        if (ShopManager.Instance == null)
        {
            Debug.LogError(
                "[DungeonTileEventManager] " +
                "ShopManager.Instance가 없습니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})\n" +
                $"Shop Level: {shopLevel}"
            );

            yield break;
        }


        // -----------------------------------------------------
        // 상점 레벨 해금
        // -----------------------------------------------------

        Debug.Log(
            "[DungeonTileEventManager] " +
            "Shop_Update 타일 발동\n" +
            $"좌표: ({tile.X}, {tile.Y})\n" +
            $"Shop Level: {shopLevel}"
        );


        ShopManager.Instance.UnlockShopLevel(
            shopLevel
        );


        Debug.Log(
            "[DungeonTileEventManager] " +
            "Shop_Update 처리 완료\n" +
            $"Shop Level: {shopLevel}"
        );


        yield break;
    }

    private IEnumerator HandleTeleportEnter(
        DungeonTileData tile)
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[Teleport] DungeonManager가 없습니다."
            );

            yield break;
        }


        TeleportDataLoader loader =
            TeleportDataLoader.Instance;


        if (loader == null)
        {
            Debug.LogError(
                "[Teleport] " +
                "TeleportDataLoader가 없습니다."
            );

            yield break;
        }


        Vector2Int currentPosition =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        TeleportTileData teleportData =
            loader.GetData(
                currentPosition
            );


        if (teleportData == null)
        {
            Debug.LogWarning(
                "[Teleport] Teleport 타일인데 " +
                "Teleport_Data가 없습니다.\n" +
                $"좌표: {currentPosition}"
            );

            yield break;
        }


        if (
            !loader.TryGetDestination(
                currentPosition,
                out Vector2Int destination
            )
        )
        {
            Debug.LogError(
                "[Teleport] 연결된 목적지를 찾지 못했습니다.\n" +
                $"현재 위치: {currentPosition}\n" +
                $"ID: {teleportData.connectedTeleportID}"
            );

            yield break;
        }


        Debug.Log(
            "[Teleport] 위치 변이기 발동\n" +
            $"ID: {teleportData.connectedTeleportID}\n" +
            $"출발: {currentPosition}\n" +
            $"도착: {destination}"
        );


        // Fade Out
        if (fadeController != null)
        {
            yield return
                fadeController.FadeOut();
        }


        bool moved =
            dungeonManager.TeleportToRoom(
                destination
            );


        if (!moved)
        {
            Debug.LogError(
                "[Teleport] 텔레포트 이동 실패"
            );


            if (fadeController != null)
            {
                yield return
                    fadeController.FadeIn();
            }


            yield break;
        }


        yield return null;


        // Fade In
        if (fadeController != null)
        {
            yield return
                fadeController.FadeIn();
        }


        /*
         * 여기서 ExecuteEnterEvent()를
         * 다시 호출하지 않는다.
         *
         * 도착지 역시 Teleport 타일이라
         * 재호출하면
         *
         * A -> B -> A -> B...
         *
         * 무한 왕복하기 때문.
         */


        Debug.Log(
            "[Teleport] 이동 완료\n" +
            $"현재 위치: " +
            $"{dungeonManager.CurrentRoom}"
        );
    }


    // =========================================================
    // Key
    // =========================================================

    private IEnumerator HandleKey(
        DungeonTileData tile)
    {
        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (
            usedKeyTiles.Contains(
                position
            )
        )
        {
            Debug.Log(
                "[Key] 이미 획득한 열쇠입니다: " +
                position
            );

            yield break;
        }


        KeyDataLoader loader =
            KeyDataLoader.Instance;


        if (loader == null)
        {
            yield break;
        }


        KeyTileData keyData =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (keyData == null)
        {
            yield break;
        }


        KeyRewardDatabase
            rewardDatabase =
                KeyRewardDatabase.Instance;


        if (rewardDatabase == null)
        {
            yield break;
        }


        if (
            !rewardDatabase.TryGetItemID(
                keyData.keyID,
                out int itemID
            )
        )
        {
            yield break;
        }


        if (ItemDatabase.Instance == null)
        {
            yield break;
        }


        ItemData itemData =
            ItemDatabase.Instance.GetItem(
                itemID
            );


        if (itemData == null)
        {
            yield break;
        }


        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            yield break;
        }


        List<ChestItemData> rewards =
            new List<ChestItemData>
            {
                new ChestItemData(
                    itemID,
                    1
                )
            };


        yield return
            rewardUI.ShowChestRewards(
                rewards
            );


        if (rewardUI.AnyItemAcquired)
        {
            usedKeyTiles.Add(
                position
            );


            Debug.Log(
                "[Key] 열쇠 획득 완료\n" +
                $"KeyID: {keyData.keyID}\n" +
                $"ItemID: {itemID}"
            );


            if (SaveManager.Instance != null)
            {
                SaveManager.Instance
                    .SaveGameplayData();
            }
        }
    }


    // =========================================================
    // Chest
    // =========================================================

    private IEnumerator HandleChest(
        DungeonTileData tile)
    {
        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (
            usedChestTiles.Contains(
                position
            )
        )
        {
            Debug.Log(
                "[Chest] 이미 사용한 상자입니다: " +
                position
            );

            yield break;
        }


        ChestDataLoader loader =
            ChestDataLoader.Instance;


        if (loader == null)
        {
            yield break;
        }


        ChestTileData data =
            loader.GetData(
                tile.X,
                tile.Y
            );


        if (data == null)
        {
            yield break;
        }


        DungeonRewardUI rewardUI =
            DungeonRewardUI.Instance;


        if (rewardUI == null)
        {
            yield break;
        }


        yield return
            rewardUI.ShowChestRewards(
                data.items
            );


        if (rewardUI.AnyItemAcquired)
        {
            usedChestTiles.Add(
                position
            );


            Debug.Log(
                "[Chest] 상자 사용 완료: " +
                position
            );


            if (SaveManager.Instance != null)
            {
                SaveManager.Instance
                    .SaveGameplayData();
            }
        }
    }


    // =========================================================
    // Puzzle / Letter
    // =========================================================

    private IEnumerator HandlePuzzleLetter(
        DungeonTileData tile)
    {
        PuzzleLetterUI puzzleUI =
            PuzzleLetterUI.Instance;


        if (puzzleUI == null)
        {
            yield break;
        }


        string message =
            "단서를 발견했습니다.\n\n" +
            $"좌표: ({tile.X}, {tile.Y})";


        yield return StartCoroutine(
            puzzleUI.ShowMessage(
                message
            )
        );
    }


    // =========================================================
    // Event / Hint
    // =========================================================

    private IEnumerator HandleEventHint(
        DungeonTileData tile)
    {
        Debug.Log(
            "[TileEvent] Event/Hint 예정: " +
            $"({tile.X}, {tile.Y})"
        );


        yield break;
    }


    // =========================================================
    // Rest
    // =========================================================

    private IEnumerator HandleRest(
        DungeonTileData tile)
    {
        RestTileManager restManager =
            RestTileManager.Instance;


        if (restManager == null)
        {
            yield break;
        }


        Vector2Int position =
            new Vector2Int(
                tile.X,
                tile.Y
            );


        if (
            !restManager.CanRest(
                position
            )
        )
        {
            Debug.Log(
                "[Rest] 이미 휴식한 장소입니다."
            );

            yield break;
        }


        RestConfirmUI confirmUI =
            RestConfirmUI.Instance;


        if (confirmUI == null)
        {
            yield break;
        }


        yield return StartCoroutine(
            confirmUI.ShowConfirm()
        );


        if (!confirmUI.GetResult())
        {
            yield break;
        }


        restManager.Rest(
            position
        );
    }


    // =========================================================
    // Boss
    // =========================================================

    private IEnumerator HandleBoss(
        DungeonTileData tile)
    {
        ResolveReferences();

        if (tile == null)
        {
            Debug.LogWarning(
                "[Boss] Boss 타일 데이터가 없습니다."
            );

            yield break;
        }

        if (battleManager == null)
        {
            Debug.LogError(
                "[Boss] BattleManager를 찾을 수 없습니다."
            );

            yield break;
        }

        // 현재 기획 기준: (15, 31)에서 Enemy 3005 보스전
        if (tile.X != 15 || tile.Y != 31)
        {
            Debug.LogWarning(
                "[Boss] 등록되지 않은 Boss 타일입니다.\n" +
                $"좌표: ({tile.X}, {tile.Y})"
            );

            yield break;
        }

        if (battleManager.IsBattleRunning())
        {
            Debug.LogWarning(
                "[Boss] 이미 전투가 진행 중입니다."
            );

            yield break;
        }

        int[] bossEnemyIds =
            new int[] { 3005 };

        Debug.Log(
            "[Boss] 보스 전투 시작\n" +
            $"좌표: ({tile.X}, {tile.Y})\n" +
            "Enemy ID: 3005"
        );

        // 보스전 직전 일반 배경의 현재 활성 상태를 저장한 뒤
        // 일반 배경은 끄고 보스 전용 배경을 켠다.
        ShowBossBattleBackground();

        // Encounter Group을 넘기지 않으므로
        // 일반 인카운터 보상 그룹과 분리된 보스 전투로 시작한다.
        yield return StartCoroutine(
            battleManager.StartBattleEncounter(
                bossEnemyIds
            )
        );

        while (battleManager.IsBattleRunning())
        {
            yield return null;
        }

        // 보스전이 완전히 끝난 뒤 보스 배경을 끄고
        // 전투 시작 전의 일반 배경 활성 상태를 그대로 복구한다.
        RestoreNormalBackground();

        Debug.Log(
            "[Boss] 보스 전투 종료\n" +
            $"좌표: ({tile.X}, {tile.Y})"
        );
    }


    private void ShowBossBattleBackground()
    {
        if (normalBackgrounds != null)
        {
            savedNormalBackgroundStates =
                new bool[normalBackgrounds.Length];

            for (int i = 0; i < normalBackgrounds.Length; i++)
            {
                GameObject background =
                    normalBackgrounds[i];

                if (background == null)
                {
                    savedNormalBackgroundStates[i] = false;
                    continue;
                }

                savedNormalBackgroundStates[i] =
                    background.activeSelf;

                background.SetActive(false);
            }
        }

        if (bossBackgrounds != null)
        {
            for (int i = 0; i < bossBackgrounds.Length; i++)
            {
                if (bossBackgrounds[i] != null)
                {
                    bossBackgrounds[i]
                        .SetActive(true);
                }
            }
        }

        Debug.Log(
            "[Boss] 보스 전용 배경 ON"
        );
    }


    private void RestoreNormalBackground()
    {
        if (bossBackgrounds != null)
        {
            for (int i = 0; i < bossBackgrounds.Length; i++)
            {
                if (bossBackgrounds[i] != null)
                {
                    bossBackgrounds[i]
                        .SetActive(false);
                }
            }
        }

        if (
            normalBackgrounds != null &&
            savedNormalBackgroundStates != null
        )
        {
            int count =
                Mathf.Min(
                    normalBackgrounds.Length,
                    savedNormalBackgroundStates.Length
                );

            for (int i = 0; i < count; i++)
            {
                if (normalBackgrounds[i] != null)
                {
                    normalBackgrounds[i]
                        .SetActive(
                            savedNormalBackgroundStates[i]
                        );
                }
            }
        }

        savedNormalBackgroundStates = null;

        Debug.Log(
            "[Boss] 일반 배경 상태 복구"
        );
    }


    // =========================================================
    // Chest Save
    // =========================================================

    public List<string>
        GetUsedChestTilesForSave()
    {
        List<string> result =
            new List<string>();


        foreach (
            Vector2Int position
            in usedChestTiles
        )
        {
            result.Add(
                $"{position.x},{position.y}"
            );
        }


        return result;
    }


    public void RestoreUsedChestTiles(
        List<string> savedTiles)
    {
        usedChestTiles.Clear();


        RestorePositionList(
            savedTiles,
            usedChestTiles
        );


        Debug.Log(
            "[Chest] 사용 완료 상자 복구: " +
            usedChestTiles.Count +
            "개"
        );
    }


    public void ClearUsedChestTiles()
    {
        usedChestTiles.Clear();
    }


    // =========================================================
    // Key Save
    // =========================================================

    public List<string>
        GetUsedKeyTilesForSave()
    {
        List<string> result =
            new List<string>();


        foreach (
            Vector2Int position
            in usedKeyTiles
        )
        {
            result.Add(
                $"{position.x},{position.y}"
            );
        }


        return result;
    }


    public void RestoreUsedKeyTiles(
        List<string> savedTiles)
    {
        usedKeyTiles.Clear();


        RestorePositionList(
            savedTiles,
            usedKeyTiles
        );


        Debug.Log(
            "[Key] 사용 완료 열쇠 타일 복구: " +
            usedKeyTiles.Count +
            "개"
        );
    }


    public void ClearUsedKeyTiles()
    {
        usedKeyTiles.Clear();


        Debug.Log(
            "[Key] 사용 기록 초기화"
        );
    }


    // =========================================================
    // Farming
    // =========================================================

    public void ClearUsedFarmingTiles()
    {
        usedFarmingTiles.Clear();


        Debug.Log(
            "[Farming] 사용 기록 초기화"
        );
    }


    // =========================================================
    // Save Parse Helper
    // =========================================================

    private void RestorePositionList(
        List<string> savedTiles,
        HashSet<Vector2Int> target)
    {
        if (
            savedTiles == null ||
            target == null
        )
        {
            return;
        }


        foreach (
            string entry
            in savedTiles
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    entry
                )
            )
            {
                continue;
            }


            string[] parts =
                entry.Split(',');


            if (parts.Length != 2)
            {
                continue;
            }


            if (
                !int.TryParse(
                    parts[0],
                    out int x
                )
            )
            {
                continue;
            }


            if (
                !int.TryParse(
                    parts[1],
                    out int y
                )
            )
            {
                continue;
            }


            target.Add(
                new Vector2Int(
                    x,
                    y
                )
            );
        }
    }
    // =========================================================
    // Farming Visual State
    // =========================================================

    public bool IsFarmingUsed(
        Vector2Int position)
    {
        return
            usedFarmingTiles.Contains(
                position
            );
    }
    // =========================================================
    // Visual State Query
    // =========================================================

    public bool IsFarmingTileUsed(
        Vector2Int position)
    {
        return
            usedFarmingTiles.Contains(
                position
            );
    }


    public bool IsChestTileUsed(
        Vector2Int position)
    {
        return
            usedChestTiles.Contains(
                position
            );
    }


    public bool IsKeyTileUsed(
        Vector2Int position)
    {
        return
            usedKeyTiles.Contains(
                position
            );
    }
}