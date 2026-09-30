using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private enum BattleState
    {
        None,
        PlayerTurn,
        SelectingTarget,
        EnemyTurn,
        BattleEnd
    }

    [Header("Cut In")]
    public BattleAttackCutInController cutInController;

    [Header("Player")]
    public BattleUnit playerUnit;

    [Header("Monster Spawn")]
    public BattleMonsterData[] monsterPool;
    public Transform enemyGroup;
    public Transform[] enemySpawnPoints;

    [Min(1)]
    public int minEnemyCount = 1;

    [Min(1)]
    public int maxEnemyCount = 1;

    [Header("UI")]
    public BattleUIManager uiManager;
    public GameObject encounterPanel;
    public TextMeshProUGUI encounterText;

    [Header("Enemy Pattern UI")]
    [SerializeField] private GameObject enemyPatternPanel;
    [SerializeField] private TextMeshProUGUI enemyPatternText;
    [SerializeField] private float enemyPatternMessageTime = 1.6f;

    [Header("Floating Text")]
    public TextMeshProUGUI floatingTextPrefab;
    public Canvas worldCanvas;

    [Header("Battle Timing")]
    public float encounterMessageTime = 1f;
    public float enemyAttackDelay = 0.6f;
    public float afterHitDelay = 0.2f;
    public float actionDelay = 0.6f;

    [Header("Battle Setting")]
    public int runSuccessPercent = 50;

    [Header("Weapon Skill Audio")]
    [Tooltip("무기 스킬 사용 불가 효과음을 재생할 AudioSource입니다. 비워두면 같은 오브젝트의 AudioSource를 자동으로 찾습니다.")]
    public AudioSource weaponSkillAudioSource;

    [Tooltip("메인 무기가 없거나 무기 스킬이 없을 때 재생할 효과음입니다. 리소스를 받은 뒤 여기에 넣으면 됩니다.")]
    public AudioClip weaponSkillUnavailableClip;

    [Range(0f, 1f)]
    public float weaponSkillUnavailableVolume = 1f;

    [Header("Equipment Durability")]
    [Min(0)]
    public int weaponDurabilityCost = 10;

    [Min(0)]
    public int armorDurabilityCost = 10;

    [Header("Guard Status")]
    [Range(1, 100)]
    public int guardDamageReductionPercent = 50;

    [Min(1)]
    public int guardArmorDurabilityMultiplier = 2;

    private const int GuardStatusId = 9001;
    private const int GuardStunStatusId = 9002;

    private BattleState state =
        BattleState.None;

    private bool battleRunning;

    // 적 지정이 필요한 무기 스킬을 선택했을 때 보관한다.
    // null이면 일반 공격 대상 선택 상태다.
    private WeaponSkillData pendingTargetWeaponSkill;

    // =========================================================
    // Item runtime actions (1013~1022)
    // =========================================================
    private int pendingTargetItemId = 0;
    private int pendingTargetItemDamage = 0;

    private bool ignoreNextEnemyHit = false;
    private bool ignoreAllEnemyHitsThisTurn = false;
    private bool doubleNextNormalAttackDamage = false;
    private int enemyAccuracyModifierThisTurn = 0;

    private readonly List<BattleUnit> enemies =
        new List<BattleUnit>();

    // =========================================================
    // Enemy Pattern Runtime
    // =========================================================
    // 생성된 BattleUnit이 어떤 BattleMonsterData에서 왔는지 보관한다.
    // BattleUnit 자체를 수정하지 않고 Enemy_ID를 안전하게 찾기 위한 매핑이다.
    private readonly Dictionary<BattleUnit, BattleMonsterData> enemyMonsterData =
        new Dictionary<BattleUnit, BattleMonsterData>();

    // 적 개체별 마지막으로 실제 발동한 Pattern_ID.
    // 기절로 행동을 건너뛴 턴에는 값을 변경하지 않는다.
    private readonly Dictionary<BattleUnit, int> enemyPreviousPatternIds =
        new Dictionary<BattleUnit, int>();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (weaponSkillAudioSource == null)
        {
            weaponSkillAudioSource =
                GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (uiManager != null)
            uiManager.HideBattleUI();

        if (encounterPanel != null)
            encounterPanel.SetActive(false);

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsBattleRunning()
    {
        return battleRunning;
    }

    public bool CanPlayerUseItem()
    {
        return battleRunning &&
               state == BattleState.PlayerTurn;
    }

    public bool CanPlayerSwapWeapon()
    {
        return battleRunning &&
               state == BattleState.PlayerTurn;
    }

    public IEnumerator StartBattleEncounter()
    {
        if (battleRunning)
            yield break;

        battleRunning = true;
        state = BattleState.None;

        if (uiManager != null)
            uiManager.HideBattleUI();

        if (encounterPanel != null)
            encounterPanel.SetActive(true);

        if (encounterText != null)
            encounterText.text = "전투 발생!";

        yield return new WaitForSeconds(
            encounterMessageTime
        );

        if (encounterPanel != null)
            encounterPanel.SetActive(false);

        SpawnEnemies();

        if (enemies.Count == 0)
        {
            Debug.LogError(
                "[BattleManager] 생성된 몬스터가 없습니다."
            );

            EndBattleImmediately();
            yield break;
        }

        StartBattle();
    }

    private void StartBattle()
    {
        pendingTargetWeaponSkill = null;
        pendingTargetItemId = 0;
        pendingTargetItemDamage = 0;
        state = BattleState.PlayerTurn;

        if (uiManager != null)
        {
            uiManager.ShowBattleUI();
            uiManager.ShowMainBattleMenu();
        }
    }

    private void SpawnEnemies()
    {
        ClearEnemies();

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(true);

        if (monsterPool == null ||
            monsterPool.Length == 0)
        {
            Debug.LogError(
                "[BattleManager] Monster Pool이 비어 있습니다."
            );

            return;
        }

        if (enemySpawnPoints == null ||
            enemySpawnPoints.Length == 0)
        {
            Debug.LogError(
                "[BattleManager] Enemy Spawn Points가 비어 있습니다."
            );

            return;
        }

        int safeMinimum =
            Mathf.Max(1, minEnemyCount);

        int safeMaximum =
            Mathf.Max(safeMinimum, maxEnemyCount);

        int count =
            Random.Range(
                safeMinimum,
                safeMaximum + 1
            );

        count =
            Mathf.Clamp(
                count,
                1,
                enemySpawnPoints.Length
            );

        for (int i = 0; i < count; i++)
        {
            BattleMonsterData data =
                GetRandomValidMonsterData();

            if (data == null ||
                data.monsterPrefab == null)
            {
                Debug.LogWarning(
                    "[BattleManager] 사용할 수 있는 몬스터 데이터가 없습니다."
                );

                continue;
            }

            Transform spawnPoint =
                enemySpawnPoints[i];

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "[BattleManager] Enemy Spawn Point가 비어 있습니다."
                );

                continue;
            }

            GameObject enemyObject =
                Instantiate(
                    data.monsterPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    enemyGroup
                );

            BattleUnit unit =
                enemyObject.GetComponent<BattleUnit>();

            if (unit == null)
            {
                unit =
                    enemyObject.AddComponent<BattleUnit>();
            }

            // ==========================================
            // 몬스터 데이터 연결
            // ==========================================
            //
            // 기존:
            //
            // unit.Setup(
            //     data.monsterName,
            //     data.maxHP,
            //     data.attackPower,
            //     data.accuracy,
            //     data.evasion
            // );
            //
            // 변경:
            //
            // BattleUnit 내부에 BattleMonsterData까지 저장한다.
            // 컷인 컨트롤러에서 Rat / Dog / 기타 몬스터의
            // 공격/피격 이미지를 구분하기 위해 필요하다.
            // ==========================================

            unit.SetupMonster(data);

            StatusEffectController enemyStatusController =
                enemyObject.GetComponent<StatusEffectController>();

            if (enemyStatusController == null)
            {
                enemyStatusController =
                    enemyObject.AddComponent<StatusEffectController>();
            }

            BattleEnemyClick enemyClick =
                enemyObject.GetComponent<BattleEnemyClick>();

            if (enemyClick == null)
            {
                enemyClick =
                    enemyObject.AddComponent<BattleEnemyClick>();
            }

            enemyClick.enemyUnit = unit;
            enemyClick.battleManager = this;

            Collider2D collider =
                enemyObject.GetComponent<Collider2D>();

            if (collider == null)
            {
                BoxCollider2D boxCollider =
                    enemyObject.AddComponent<BoxCollider2D>();

                boxCollider.isTrigger = true;
            }

            enemies.Add(unit);

            enemyMonsterData[unit] = data;
            enemyPreviousPatternIds[unit] = 0;
        }
    }

    private BattleMonsterData GetRandomValidMonsterData()
    {
        if (monsterPool == null ||
            monsterPool.Length == 0)
        {
            return null;
        }

        List<BattleMonsterData> validData =
            new List<BattleMonsterData>();

        foreach (
            BattleMonsterData data
            in monsterPool)
        {
            if (data != null &&
                data.monsterPrefab != null)
            {
                validData.Add(data);
            }
        }

        if (validData.Count == 0)
            return null;

        return validData[
            Random.Range(0, validData.Count)
        ];
    }

    private void ClearEnemies()
    {
        HideEnemyPatternMessage();

        enemies.Clear();
        enemyMonsterData.Clear();
        enemyPreviousPatternIds.Clear();

        if (enemyGroup == null)
            return;

        for (
            int i = enemyGroup.childCount - 1;
            i >= 0;
            i--)
        {
            Transform child =
                enemyGroup.GetChild(i);

            if (child != null)
                Destroy(child.gameObject);
        }
    }

    public void OnClickBattleButton()
    {
        if (state != BattleState.PlayerTurn)
            return;

        if (uiManager != null)
            uiManager.ShowActionMenu();
    }

    public void OnClickAttackButton()
    {
        if (state != BattleState.PlayerTurn)
            return;

        // 일반 공격 대상 선택이므로
        // 대기 중인 무기 스킬은 비운다.
        pendingTargetWeaponSkill = null;
        pendingTargetItemId = 0;
        pendingTargetItemDamage = 0;

        state = BattleState.SelectingTarget;
    }

    // =========================================================
    // 무기 스킬
    //
    // 현재 1차 연결:
    // - 장착된 주무기의 weaponSkillId 조회
    // - 4001 확정 명중 사용
    // - Status 2125 적용
    // - 스킬 내구도 비용 소모
    // - 적 턴 진행
    //
    // 이후 4002~4005는 이 진입점을 그대로 확장한다.
    // =========================================================

    /// <summary>
    /// 메인 무기가 없거나 현재 무기에 무기 스킬이 없을 때
    /// 사용할 효과음을 재생한다.
    ///
    /// AudioClip이 아직 없어도 오류 없이 동작한다.
    /// 나중에 Inspector의 Weapon Skill Unavailable Clip에
    /// 리소스만 넣으면 바로 소리가 난다.
    /// </summary>
    private void PlayWeaponSkillUnavailableSound()
    {
        if (weaponSkillUnavailableClip == null)
            return;

        if (weaponSkillAudioSource == null)
        {
            weaponSkillAudioSource =
                GetComponent<AudioSource>();
        }

        if (weaponSkillAudioSource == null)
        {
            Debug.LogWarning(
                "[BattleManager] Weapon Skill Audio Source가 없습니다. " +
                "BattleManager 오브젝트에 AudioSource를 추가하거나 Inspector에 연결해주세요."
            );

            return;
        }

        weaponSkillAudioSource.PlayOneShot(
            weaponSkillUnavailableClip,
            weaponSkillUnavailableVolume
        );
    }


    public void OnClickSkillButton()
    {
        if (!battleRunning)
            return;

        if (state != BattleState.PlayerTurn)
            return;

        StatusEffectController playerStatus =
            GetPlayerStatusController();

        if (playerStatus != null &&
            !playerStatus.CanUseSkill())
        {
            Debug.Log(
                "[BattleManager] 현재 상태에서는 스킬을 사용할 수 없습니다."
            );

            return;
        }

        if (EquipmentManager.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] EquipmentManager가 없습니다."
            );

            return;
        }

        InventoryItem mainWeapon =
            EquipmentManager.Instance.MainWeapon;

        if (mainWeapon == null ||
            mainWeapon.data == null ||
            !mainWeapon.data.IsWeapon)
        {
            Debug.Log(
                "[BattleManager] 무기가 장착되어 있지 않습니다!"
            );

            PlayWeaponSkillUnavailableSound();
            return;
        }

        if (!mainWeapon.data.HasWeaponSkill)
        {
            Debug.Log(
                "[BattleManager] 현재 주무기에 무기 스킬이 없습니다."
            );

            PlayWeaponSkillUnavailableSound();
            return;
        }

        if (WeaponSkillDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] WeaponSkillDatabase가 없습니다."
            );

            return;
        }

        WeaponSkillData skill =
            WeaponSkillDatabase.Instance.GetSkill(
                mainWeapon.data.weaponSkillId
            );

        if (skill == null)
        {
            Debug.LogError(
                "[BattleManager] 무기 스킬 데이터를 찾을 수 없습니다." +
                " / Skill ID: " +
                mainWeapon.data.weaponSkillId
            );

            return;
        }

        switch (skill.skillId)
        {
            case 4001:
                StartCoroutine(
                    UseGuaranteedHitSkillRoutine(
                        skill
                    )
                );
                break;

            case 4002:
                StartCoroutine(
                    UseAllAttackSkillRoutine(
                        skill
                    )
                );
                break;

            case 4003:
                BeginTargetedWeaponSkill(
                    skill
                );
                break;

            case 4004:
                BeginTargetedWeaponSkill(
                    skill
                );
                break;

            case 4005:
                StartCoroutine(
                    UseEndureSkillRoutine(
                        skill
                    )
                );
                break;

            default:
                Debug.Log(
                    "[BattleManager] 아직 실행이 연결되지 않은 무기 스킬: " +
                    skill.skillId +
                    " / " +
                    skill.skillName
                );
                break;
        }
    }


    // =========================================================
    // 4005 - 견디기
    //
    // - 대상 선택 없음
    // - 플레이어에게 Status 2127 적용
    // - 받는 피해 50% 감소
    // - 무기 스킬 내구도 15 소모
    // - 적 턴 진행
    // - 전체 TurnEnd에서 지속시간 1 -> 0, 제거
    // =========================================================

    private IEnumerator UseEndureSkillRoutine(
        WeaponSkillData skill)
    {
        state =
            BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        StatusEffectController playerStatus =
            GetPlayerStatusController();

        if (playerStatus == null)
        {
            Debug.LogError(
                "[BattleManager] 플레이어 StatusEffectController가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] StatusEffectDatabase가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        StatusEffectData endureStatus =
            StatusEffectDatabase.Instance
                .GetStatusEffect(
                    2127
                );

        if (endureStatus == null)
        {
            Debug.LogError(
                "[BattleManager] 상태이상 2127(견디기)을 찾을 수 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        bool added =
            playerStatus.AddStatusEffect(
                endureStatus
            );

        if (!added)
        {
            Debug.LogError(
                "[BattleManager] 견디기 상태 2127 적용에 실패했습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        ConsumePlayerWeaponSkillDurability(
            skill.useDurability
        );

        Debug.Log(
            "[BattleManager] 무기 스킬 사용: " +
            skill.skillName +
            " / Status ID: 2127" +
            " / 받는 피해 50% 감소" +
            " / 내구도 비용: " +
            skill.useDurability
        );

        yield return new WaitForSeconds(
            actionDelay
        );

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }


    // =========================================================
    // 적 지정형 무기 스킬 시작
    //
    // 4003 기절, 이후 4004 강타에서도 재사용한다.
    // =========================================================

    private void BeginTargetedWeaponSkill(
        WeaponSkillData skill)
    {
        if (skill == null)
            return;

        pendingTargetWeaponSkill =
            skill;

        state =
            BattleState.SelectingTarget;

        Debug.Log(
            "[BattleManager] 무기 스킬 대상 선택 시작: " +
            skill.skillName +
            " / Skill ID: " +
            skill.skillId
        );
    }


    private IEnumerator UseGuaranteedHitSkillRoutine(
        WeaponSkillData skill)
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        StatusEffectController playerStatus =
            GetPlayerStatusController();

        if (playerStatus == null)
        {
            Debug.LogError(
                "[BattleManager] 플레이어 StatusEffectController가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] StatusEffectDatabase가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        StatusEffectData guaranteedHit =
            StatusEffectDatabase.Instance
                .GetStatusEffect(
                    2125
                );

        if (guaranteedHit == null)
        {
            Debug.LogError(
                "[BattleManager] 상태이상 2125(확정 명중)를 찾을 수 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        bool added =
            playerStatus.AddStatusEffect(
                guaranteedHit
            );

        if (!added)
        {
            Debug.LogError(
                "[BattleManager] 확정 명중 상태 적용에 실패했습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        ConsumePlayerWeaponSkillDurability(
            skill.useDurability
        );

        Debug.Log(
            "[BattleManager] 무기 스킬 사용: " +
            skill.skillName +
            " / Skill ID: " +
            skill.skillId +
            " / Status ID: 2125" +
            " / 내구도 비용: " +
            skill.useDurability
        );

        yield return new WaitForSeconds(
            actionDelay
        );

        // 자기 자신에게 버프를 적용하는 행동도
        // 플레이어의 행동 1회로 처리한다.
        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }


    // =========================================================
    // 4002 - 전체 공격
    //
    // 기획 데이터:
    // - effect_id 5002
    // - effect_type 0 (Damage)
    // - effect_value 10
    // - 적 선택 없음
    // - 살아있는 모든 적에게 10 피해
    // - 무기 내구도 15 소모
    // - 사용 후 적 턴
    // =========================================================

    private IEnumerator UseAllAttackSkillRoutine(
        WeaponSkillData skill)
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        WeaponSkillEffectData damageEffect = null;

        List<WeaponSkillEffectData> orderedEffects =
            skill.GetOrderedEffects();

        foreach (
            WeaponSkillEffectData effect
            in orderedEffects)
        {
            if (effect == null)
                continue;

            if (effect.effectType ==
                WeaponSkillEffectType.Damage)
            {
                damageEffect = effect;
                break;
            }
        }

        if (damageEffect == null)
        {
            Debug.LogError(
                "[BattleManager] 4002 전체 공격의 Damage 효과를 찾을 수 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        int damage =
            Mathf.Max(
                0,
                damageEffect.effectValue
            );

        BattleUnit[] targets =
            enemies.ToArray();

        foreach (
            BattleUnit target
            in targets)
        {
            if (!IsValidLivingEnemy(target))
                continue;

            target.TakeDamage(
                damage
            );

            ShowFloatingText(
                target.transform.position,
                damage.ToString()
            );

            Debug.Log(
                "[BattleManager] 전체 공격 → " +
                target.unitName +
                " / 피해: " +
                damage
            );
        }

        ConsumePlayerWeaponSkillDurability(
            skill.useDurability
        );

        Debug.Log(
            "[BattleManager] 무기 스킬 사용: " +
            skill.skillName +
            " / 모든 적 피해: " +
            damage +
            " / 내구도 비용: " +
            skill.useDurability
        );

        yield return new WaitForSeconds(
            afterHitDelay
        );

        RemoveDeadEnemies();

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn(
                        "전투 승리"
                    );
            }

            EndBattle();
            yield break;
        }

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }


    // =========================================================
    // 4003 - 기절
    //
    // - SkillButton 클릭 후 적 선택
    // - 선택한 적에게 Status 2001 적용
    // - 무기 내구도 15 소모
    // - 적 턴 진행
    // - 2001은 SelfTurnEnd에서 1 -> 0으로 감소/삭제
    // =========================================================

    private IEnumerator UseStunSkillRoutine(
        WeaponSkillData skill,
        BattleUnit target)
    {
        state =
            BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        if (!IsValidLivingEnemy(target))
        {
            Debug.LogWarning(
                "[BattleManager] 기절 스킬 대상이 유효하지 않습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] StatusEffectDatabase가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        StatusEffectData stunData =
            StatusEffectDatabase.Instance
                .GetStatusEffect(
                    2001
                );

        if (stunData == null)
        {
            Debug.LogError(
                "[BattleManager] 상태이상 2001(기절)을 찾을 수 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        StatusEffectController targetStatus =
            GetOrAddStatusController(
                target
            );

        if (targetStatus == null)
        {
            Debug.LogError(
                "[BattleManager] 대상의 StatusEffectController를 가져올 수 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        bool added =
            targetStatus.AddStatusEffect(
                stunData
            );

        if (!added)
        {
            Debug.LogError(
                "[BattleManager] 대상에게 기절 상태 적용에 실패했습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        ConsumePlayerWeaponSkillDurability(
            skill.useDurability
        );

        ShowFloatingText(
            target.transform.position,
            "STUN"
        );

        Debug.Log(
            "[BattleManager] 무기 스킬 사용: " +
            skill.skillName +
            " / 대상: " +
            target.unitName +
            " / Status ID: 2001" +
            " / 내구도 비용: " +
            skill.useDurability
        );

        yield return new WaitForSeconds(
            actionDelay
        );

        // 기절된 적을 포함한 적 팀 턴을 시작한다.
        // EnemyTurnRoutine에서 Stun을 확인하므로
        // 해당 적은 행동하지 못하고 SelfTurnEnd에서
        // 기절 지속시간이 감소한다.
        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }


    // =========================================================
    // 4004 - 강타
    //
    // 기획 순서:
    // order 0 : 플레이어에게 Status 2126 적용
    // order 1 : 선택한 적에게 일반 공격의 2배 피해
    //
    // - 일반 공격 내구도 비용은 추가로 소모하지 않는다.
    // - 스킬 내구도 비용 15만 소모한다.
    // - 공격이 끝난 뒤 2126 스택 하나를 소비한다.
    // =========================================================

    private IEnumerator UsePowerStrikeSkillRoutine(
        WeaponSkillData skill,
        BattleUnit target)
    {
        state =
            BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        if (!IsValidLivingEnemy(target))
        {
            Debug.LogWarning(
                "[BattleManager] 강타 대상이 유효하지 않습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        StatusEffectController playerStatus =
            GetPlayerStatusController();

        if (playerStatus == null)
        {
            Debug.LogError(
                "[BattleManager] 플레이어 StatusEffectController가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] StatusEffectDatabase가 없습니다."
            );

            ReturnToPlayerTurnIfPossible();
            yield break;
        }

        List<WeaponSkillEffectData> orderedEffects =
            skill.GetOrderedEffects();

        bool powerStrikeStatusApplied =
            false;

        foreach (
            WeaponSkillEffectData effect
            in orderedEffects)
        {
            if (effect == null)
                continue;

            // ---------------------------------------------
            // order 0 / effect 5004
            // 자신에게 2126 적용
            // ---------------------------------------------
            if (effect.effectType ==
                    WeaponSkillEffectType.StatusEffect &&
                effect.target ==
                    WeaponSkillTarget.Self &&
                effect.statusId ==
                    2126)
            {
                StatusEffectData powerStrikeStatus =
                    StatusEffectDatabase.Instance
                        .GetStatusEffect(
                            effect.statusId
                        );

                if (powerStrikeStatus == null)
                {
                    Debug.LogError(
                        "[BattleManager] 상태이상 2126(강타)을 찾을 수 없습니다."
                    );

                    ReturnToPlayerTurnIfPossible();
                    yield break;
                }

                powerStrikeStatusApplied =
                    playerStatus.AddStatusEffect(
                        powerStrikeStatus
                    );

                if (!powerStrikeStatusApplied)
                {
                    Debug.LogError(
                        "[BattleManager] 강타 상태 2126 적용에 실패했습니다."
                    );

                    ReturnToPlayerTurnIfPossible();
                    yield break;
                }

                continue;
            }

            // ---------------------------------------------
            // order 1 / effect 5005
            // 선택한 적 공격
            // ---------------------------------------------
            if (effect.effectType ==
                    WeaponSkillEffectType.Damage &&
                effect.target ==
                    WeaponSkillTarget.Enemy)
            {
                // 강타도 플레이어가 실제로 공격하는 기술이므로
                // 기존 플레이어 공격 컷인을 그대로 사용한다.
                if (cutInController != null &&
                    playerUnit != null)
                {
                    yield return cutInController
                        .PlayPlayerAttackCutIn(
                            playerUnit,
                            target
                        );
                }
                else
                {
                    if (playerUnit != null)
                        playerUnit.PlayAttackAnimation();

                    yield return new WaitForSeconds(
                        0.6f
                    );
                }

                int baseDamage =
                    PlayerStats.Instance != null
                        ? PlayerStats.Instance
                            .GetFinalAttackDamage()
                        : playerUnit != null
                            ? Mathf.Max(
                                1,
                                playerUnit.attackPower
                            )
                            : 1;

                float attackMultiplier =
                    playerStatus
                        .GetAttackPowerMultiplier();

                int damage =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            baseDamage *
                            attackMultiplier
                        )
                    );

                // 2126 강타 효과:
                // 일반 공격 최종 공격력 계산값의 2배.
                damage =
                    Mathf.Max(
                        1,
                        damage * 2
                    );

                StatusEffectController targetStatus =
                    target.GetComponent<
                        StatusEffectController>();

                if (targetStatus != null)
                {
                    damage =
                        Mathf.Max(
                            1,
                            Mathf.RoundToInt(
                                damage *
                                targetStatus
                                    .GetDamageTakenMultiplier()
                            )
                        );
                }

                target.TakeDamage(
                    damage
                );

                ShowFloatingText(
                    target.transform.position,
                    damage.ToString()
                );

                Debug.Log(
                    "[BattleManager] 강타 데미지: " +
                    baseDamage +
                    " x 공격력배율 " +
                    attackMultiplier +
                    " x 2 = " +
                    damage
                );
            }
        }

        // 강타는 일반 공격 내구도 비용을 별도로 소모하지 않는다.
        ConsumePlayerWeaponSkillDurability(
            skill.useDurability
        );

        // 실제 강타 공격이 끝났으므로 2126 한 스택 소비.
        if (powerStrikeStatusApplied)
        {
            playerStatus
                .ConsumeAttackTriggeredStatusById(
                    2126
                );
        }

        yield return new WaitForSeconds(
            afterHitDelay
        );

        RemoveDeadEnemies();

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn(
                        "전투 승리"
                    );
            }

            EndBattle();
            yield break;
        }

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }


    public void OnClickGuardButton()
    {
        if (!battleRunning)
            return;

        if (state != BattleState.PlayerTurn &&
            state != BattleState.SelectingTarget)
        {
            return;
        }

        if (state == BattleState.SelectingTarget)
        {
            ClearEnemyArrows();

            pendingTargetWeaponSkill =
                null;

            state = BattleState.PlayerTurn;

            Debug.Log(
                "[BattleManager] 대상 선택 취소 → 방어로 변경"
            );
        }

        StatusEffectController playerStatusController =
            GetPlayerStatusController();

        if (playerStatusController == null)
        {
            Debug.LogError(
                "[BattleManager] 플레이어 StatusEffectController가 없습니다."
            );

            return;
        }

        playerStatusController.AddStatusEffect(
            CreateGuardStatusData()
        );

        StartCoroutine(
            PlayerGuardRoutine()
        );
    }

    public void OnClickRunButton()
    {
        if (state != BattleState.PlayerTurn)
            return;

        StartCoroutine(
            RunRoutine()
        );
    }

    public void HoverEnemy(
        BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget)
            return;

        if (!IsValidLivingEnemy(enemy))
            return;

        enemy.SetArrow(true);
    }

    public void ExitHoverEnemy(
        BattleUnit enemy)
    {
        if (enemy == null)
            return;

        enemy.SetArrow(false);
    }

    public void ClickEnemy(
        BattleUnit enemy)
    {
        if (state != BattleState.SelectingTarget)
            return;

        if (!IsValidLivingEnemy(enemy))
            return;

        // 1013 벽돌처럼 아이템이 적 선택을 기다리는 중이면
        // 일반 공격/무기 스킬보다 먼저 아이템 효과를 처리한다.
        if (pendingTargetItemId > 0)
        {
            int itemId = pendingTargetItemId;
            int damage = pendingTargetItemDamage;

            pendingTargetItemId = 0;
            pendingTargetItemDamage = 0;

            StartCoroutine(
                UseTargetDamageItemRoutine(
                    itemId,
                    enemy,
                    damage
                )
            );
            return;
        }

        // 적 지정형 무기 스킬이 대기 중이면
        // 일반 공격 대신 해당 스킬을 실행한다.
        if (pendingTargetWeaponSkill != null)
        {
            WeaponSkillData selectedSkill =
                pendingTargetWeaponSkill;

            pendingTargetWeaponSkill =
                null;

            switch (selectedSkill.skillId)
            {
                case 4003:
                    StartCoroutine(
                        UseStunSkillRoutine(
                            selectedSkill,
                            enemy
                        )
                    );
                    return;

                case 4004:
                    StartCoroutine(
                        UsePowerStrikeSkillRoutine(
                            selectedSkill,
                            enemy
                        )
                    );
                    return;

                default:
                    Debug.LogWarning(
                        "[BattleManager] 대상 지정 실행이 아직 연결되지 않은 무기 스킬: " +
                        selectedSkill.skillId
                    );

                    state =
                        BattleState.PlayerTurn;

                    return;
            }
        }

        StartCoroutine(
            PlayerAttackRoutine(enemy)
        );
    }

    // =========================================================
    // Item APIs
    // =========================================================

    public bool BeginTargetDamageItem(
        int itemId,
        int damage)
    {
        if (!battleRunning ||
            state != BattleState.PlayerTurn)
        {
            return false;
        }

        pendingTargetWeaponSkill = null;
        pendingTargetItemId = 0;
        pendingTargetItemDamage = 0;
        pendingTargetItemId = itemId;
        pendingTargetItemDamage = Mathf.Max(0, damage);

        state = BattleState.SelectingTarget;

        ClearEnemyArrows();

        foreach (BattleUnit enemy in enemies)
        {
            if (IsValidLivingEnemy(enemy))
                enemy.SetArrow(true);
        }

        Debug.Log(
            "[BattleManager] 아이템 대상 선택 시작 / Item ID: " +
            itemId +
            " / Damage: " +
            damage
        );

        return true;
    }

    public bool UseAllEnemyDamageItem(
        int damage)
    {
        if (!battleRunning ||
            state != BattleState.PlayerTurn)
        {
            return false;
        }

        damage = Mathf.Max(0, damage);

        foreach (BattleUnit enemy in enemies.ToArray())
        {
            if (!IsValidLivingEnemy(enemy))
                continue;

            enemy.TakeDamage(damage);

            ShowFloatingText(
                enemy.transform.position,
                damage.ToString()
            );
        }

        RemoveDeadEnemies();

        return true;
    }

    public void ActivateIgnoreNextEnemyHit()
    {
        ignoreNextEnemyHit = true;
        Debug.Log("[BattleManager] 다음 적 공격 1회 무효 활성화");
    }

    public void ActivateIgnoreAllEnemyHitsThisTurn()
    {
        ignoreAllEnemyHitsThisTurn = true;
        Debug.Log("[BattleManager] 이번 적 턴 모든 공격 무효 활성화");
    }

    public void ActivateDoubleNextNormalAttackDamage()
    {
        doubleNextNormalAttackDamage = true;
        Debug.Log("[BattleManager] 다음 일반 공격 피해 2배 활성화");
    }

    public void SetEnemyAccuracyModifierForNextEnemyTurn(
        int modifier)
    {
        enemyAccuracyModifierThisTurn = modifier;

        Debug.Log(
            "[BattleManager] 다음 적 턴 명중률 보정: " +
            modifier
        );
    }

    private IEnumerator UseTargetDamageItemRoutine(
        int itemId,
        BattleUnit target,
        int damage)
    {
        state = BattleState.EnemyTurn;
        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        if (IsValidLivingEnemy(target))
        {
            target.TakeDamage(
                Mathf.Max(0, damage)
            );

            ShowFloatingText(
                target.transform.position,
                Mathf.Max(0, damage).ToString()
            );
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .ConsumeItemById(itemId);
        }

        yield return new WaitForSeconds(
            afterHitDelay
        );

        RemoveDeadEnemies();

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn("아이템으로 전투 승리");
            }

            EndBattle();
            yield break;
        }

        yield return StartCoroutine(
            EnemyTurnRoutine(false)
        );

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .AddTurn("아이템 사용");
        }

        ReturnToPlayerTurnIfPossible();
    }


    public void OnPlayerUsedItem()
    {
        if (!battleRunning)
            return;

        if (state != BattleState.PlayerTurn)
            return;

        StartCoroutine(
            PlayerUseItemRoutine()
        );
    }

    private IEnumerator PlayerGuardRoutine()
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        if (uiManager != null)
            uiManager.HideBattleUI();

        Debug.Log(
            "[BattleManager] 방어 사용 - 이번 적 팀 공격에 방어 효과 적용"
        );

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }

    private IEnumerator PlayerUseItemRoutine()
    {
        state = BattleState.EnemyTurn;

        if (uiManager != null)
            uiManager.HideBattleUI();

        yield return StartCoroutine(
            EnemyTurnRoutine(false)
        );

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .AddTurn("아이템 사용");
        }

        ReturnToPlayerTurnIfPossible();
    }

    private IEnumerator PlayerAttackRoutine(
        BattleUnit target)
    {
        state = BattleState.EnemyTurn;

        ClearEnemyArrows();

        StatusEffectController playerStatus =
            playerUnit != null
                ? playerUnit.GetComponent<StatusEffectController>()
                : null;

        int playerAccuracy =
            PlayerStats.Instance != null
                ? PlayerStats.Instance
                    .GetFinalAccuracy(target.evasion)
                : playerUnit != null
                    ? playerUnit.accuracy
                    : 90;

        if (playerStatus != null)
        {
            playerAccuracy +=
                playerStatus.GetAccuracyBonus();
        }

        playerAccuracy =
            Mathf.Clamp(
                playerAccuracy,
                0,
                100
            );

        // =====================================================
        // 2125 - 확정 명중
        //
        // 상태가 하나라도 존재하면 이번 공격은
        // 명중률/랜덤 판정과 관계없이 반드시 명중한다.
        // 스택은 공격 처리가 끝난 뒤 하나만 소비한다.
        // =====================================================

        bool guaranteedHit =
            playerStatus != null &&
            playerStatus.HasStatusEffectById(
                2125
            );

        bool hit =
            guaranteedHit ||
            Random.Range(0, 100) <
            playerAccuracy;

        if (guaranteedHit)
        {
            Debug.Log(
                "[Battle] 확정 명중(2125) 적용 → 이번 공격은 반드시 명중"
            );
        }
        else
        {
            Debug.Log(
                "[Battle] 플레이어 최종 명중률: " +
                playerAccuracy
            );
        }

        if (cutInController != null &&
            playerUnit != null)
        {
            yield return cutInController
                .PlayPlayerAttackCutIn(
                    playerUnit,
                    target
                );
        }
        else
        {
            if (playerUnit != null)
                playerUnit.PlayAttackAnimation();

            yield return new WaitForSeconds(
                0.6f
            );
        }

        if (hit)
        {
            int baseDamage =
                PlayerStats.Instance != null
                    ? PlayerStats.Instance
                        .GetFinalAttackDamage()
                    : playerUnit != null
                        ? Mathf.Max(
                            1,
                            playerUnit.attackPower
                        )
                        : 1;

            float attackMultiplier = 1f;

            if (playerStatus != null)
            {
                attackMultiplier =
                    playerStatus
                        .GetAttackPowerMultiplier();
            }

            int damage =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        baseDamage *
                        attackMultiplier
                    )
                );

            StatusEffectController targetStatus =
                target != null
                    ? target.GetComponent<
                        StatusEffectController>()
                    : null;

            if (targetStatus != null)
            {
                damage =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            damage *
                            targetStatus
                                .GetDamageTakenMultiplier()
                        )
                    );
            }

            if (doubleNextNormalAttackDamage)
            {
                damage =
                    Mathf.Max(
                        1,
                        damage * 2
                    );

                doubleNextNormalAttackDamage = false;

                Debug.Log(
                    "[BattleManager] 자극제 적용 → 이번 일반 공격 피해 2배"
                );
            }

            Debug.Log(
                "[Battle] 공격 데미지 계산: " +
                baseDamage +
                " x " +
                attackMultiplier +
                " = " +
                damage
            );

            target.TakeDamage(
                damage
            );

            ShowFloatingText(
                target.transform.position,
                damage.ToString()
            );
        }
        else
        {
            ShowFloatingText(
                target.transform.position,
                "MISS"
            );
        }

        // 자극제는 '다음 공격'에 반응하므로 빗나가도 소비한다.
        if (!hit && doubleNextNormalAttackDamage)
        {
            doubleNextNormalAttackDamage = false;
        }

        ConsumePlayerWeaponDurability();

        // =====================================================
        // 2125 - 확정 명중 스택 소비
        //
        // 실제 공격 1회가 끝난 뒤 가장 먼저 들어온
        // 2125 스택 하나만 지속시간 1 감소.
        // 지속시간이 0이 되면 그 스택만 제거된다.
        // =====================================================

        if (guaranteedHit &&
            playerStatus != null)
        {
            playerStatus
                .ConsumeAttackTriggeredStatusById(
                    2125
                );
        }

        yield return new WaitForSeconds(
            afterHitDelay
        );

        RemoveDeadEnemies();

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn("전투 승리");
            }

            EndBattle();
            yield break;
        }

        yield return StartCoroutine(
            EnemyTurnRoutine(true)
        );
    }

    private IEnumerator EnemyTurnRoutine(
        bool addTurnAtEnd)
    {
        state = BattleState.EnemyTurn;

        RemoveDeadEnemies();

        StatusEffectController playerStatusController =
            GetPlayerStatusController();

        BattleUnit[] aliveEnemies =
            enemies.ToArray();

        foreach (
            BattleUnit enemy
            in aliveEnemies)
        {
            if (!IsValidLivingEnemy(enemy))
                continue;

            StatusEffectController enemyStatusController =
                GetOrAddStatusController(enemy);

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.SelfTurnStart
                );
            }

            bool enemyIsStunned =
                enemyStatusController != null &&
                enemyStatusController.HasStatusEffect(
                    StatusEffectType.Stun
                );

            if (enemyIsStunned)
            {
                Debug.Log(
                    "[BattleManager] " +
                    enemy.unitName +
                    "은(는) 기절하여 행동할 수 없습니다."
                );

                ShowFloatingText(
                    enemy.transform.position,
                    "STUN"
                );

                yield return new WaitForSeconds(
                    actionDelay
                );

                if (enemyStatusController != null)
                {
                    enemyStatusController.ProcessTiming(
                        StatusEffectTiming.SelfTurnEnd
                    );
                }

                // 중요:
                // 기절한 턴에는 패턴을 선택하지도,
                // previousPatternId를 변경하지도 않는다.
                // 따라서 5002 -> 5003 같은 연계가 그대로 유지된다.
                continue;
            }

            // -------------------------------------------------
            // 패턴 선택
            // -------------------------------------------------
            EnemyPatternData selectedPattern =
                SelectEnemyPattern(enemy);

            if (selectedPattern != null)
            {
                Debug.Log(
                    "[BattleManager] " +
                    enemy.unitName +
                    " 패턴 발동: " +
                    selectedPattern.patternId +
                    " / " +
                    selectedPattern.patternName
                );

                ShowEnemyPatternMessage(
                    selectedPattern.patternName
                );

                yield return StartCoroutine(
                    ExecuteEnemyPatternRoutine(
                        enemy,
                        enemyStatusController,
                        playerStatusController,
                        selectedPattern
                    )
                );

                // 패턴을 실제로 발동한 뒤에만 진행도를 저장한다.
                enemyPreviousPatternIds[enemy] =
                    selectedPattern.patternId;
            }
            else
            {
                // 패턴 데이터가 없는 3002~3005 등은
                // 기존 기본 공격을 그대로 사용한다.
                yield return StartCoroutine(
                    ExecuteLegacyEnemyAttackRoutine(
                        enemy,
                        enemyStatusController,
                        playerStatusController,
                        0
                    )
                );
            }

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.SelfTurnEnd
                );
            }

            if (playerUnit != null &&
                playerUnit.IsDead)
            {
                EndBattle();
                yield break;
            }
        }

        // 적 팀 행동이 끝났으므로 1턴성 아이템 효과 종료
        ignoreAllEnemyHitsThisTurn = false;
        enemyAccuracyModifierThisTurn = 0;

        RemoveDeadEnemies();

        if (playerStatusController != null)
        {
            playerStatusController.ProcessTiming(
                StatusEffectTiming.EnemyTeamEnd
            );
        }

        foreach (
            BattleUnit enemy
            in enemies)
        {
            StatusEffectController enemyStatusController =
                GetStatusController(enemy);

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.EnemyTeamEnd
                );
            }
        }

        RemoveDeadEnemies();

        if (playerUnit != null &&
            playerUnit.IsDead)
        {
            EndBattle();
            yield break;
        }

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn(
                        "전투 승리"
                    );
            }

            EndBattle();
            yield break;
        }

        if (addTurnAtEnd &&
            DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .AddTurn(
                    "전투 라운드 종료"
                );
        }

        if (playerStatusController != null)
        {
            playerStatusController.ProcessTiming(
                StatusEffectTiming.TurnEnd
            );
        }

        foreach (
            BattleUnit enemy
            in enemies)
        {
            StatusEffectController enemyStatusController =
                GetStatusController(enemy);

            if (enemyStatusController != null)
            {
                enemyStatusController.ProcessTiming(
                    StatusEffectTiming.TurnEnd
                );
            }
        }

        RemoveDeadEnemies();

        if (playerUnit != null &&
            playerUnit.IsDead)
        {
            EndBattle();
            yield break;
        }

        if (AllEnemiesDead())
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn(
                        "전투 승리"
                    );
            }

            EndBattle();
            yield break;
        }

        ReturnToPlayerTurnIfPossible();
    }

    // =========================================================
    // Enemy Pattern UI
    // =========================================================

    private Coroutine enemyPatternMessageCoroutine;

    private void ShowEnemyPatternMessage(string patternName)
    {
        if (enemyPatternPanel == null ||
            enemyPatternText == null)
        {
            return;
        }

        if (enemyPatternMessageCoroutine != null)
        {
            StopCoroutine(
                enemyPatternMessageCoroutine
            );
        }

        enemyPatternMessageCoroutine =
            StartCoroutine(
                EnemyPatternMessageRoutine(
                    patternName
                )
            );
    }

    private IEnumerator EnemyPatternMessageRoutine(
        string patternName)
    {
        enemyPatternText.text =
            "적이 \"" +
            patternName +
            "\"을 발동했다.";

        enemyPatternPanel.SetActive(true);

        yield return new WaitForSeconds(
            enemyPatternMessageTime
        );

        enemyPatternPanel.SetActive(false);
        enemyPatternMessageCoroutine = null;
    }

    private void HideEnemyPatternMessage()
    {
        if (enemyPatternMessageCoroutine != null)
        {
            StopCoroutine(
                enemyPatternMessageCoroutine
            );

            enemyPatternMessageCoroutine = null;
        }

        if (enemyPatternPanel != null)
        {
            enemyPatternPanel.SetActive(false);
        }
    }

    // =========================================================
    // Enemy Pattern
    // =========================================================

    private EnemyPatternData SelectEnemyPattern(
        BattleUnit enemy)
    {
        if (enemy == null ||
            EnemyPatternDatabase.Instance == null)
        {
            return null;
        }

        if (!enemyMonsterData.TryGetValue(
                enemy,
                out BattleMonsterData monsterData) ||
            monsterData == null)
        {
            return null;
        }

        int enemyId =
            monsterData.enemyId;

        int previousPatternId = 0;

        enemyPreviousPatternIds.TryGetValue(
            enemy,
            out previousPatternId
        );

        return EnemyPatternDatabase.Instance
            .SelectNextPattern(
                enemyId,
                previousPatternId
            );
    }

    private IEnumerator ExecuteEnemyPatternRoutine(
        BattleUnit enemy,
        StatusEffectController enemyStatusController,
        StatusEffectController playerStatusController,
        EnemyPatternData selectedPattern)
    {
        if (enemy == null ||
            selectedPattern == null)
        {
            yield break;
        }

        if (!enemyMonsterData.TryGetValue(
                enemy,
                out BattleMonsterData monsterData) ||
            monsterData == null)
        {
            yield return StartCoroutine(
                ExecuteLegacyEnemyAttackRoutine(
                    enemy,
                    enemyStatusController,
                    playerStatusController,
                    0
                )
            );

            yield break;
        }

        List<EnemyPatternData> effects =
            EnemyPatternDatabase.Instance != null
                ? EnemyPatternDatabase.Instance
                    .GetPatternEffects(
                        monsterData.enemyId,
                        selectedPattern.patternId
                    )
                : null;

        if (effects == null ||
            effects.Count == 0)
        {
            yield return StartCoroutine(
                ExecuteLegacyEnemyAttackRoutine(
                    enemy,
                    enemyStatusController,
                    playerStatusController,
                    0
                )
            );

            yield break;
        }

        bool attackAnimationPlayed = false;

        foreach (EnemyPatternData effect in effects)
        {
            if (effect == null)
                continue;

            // target 0 = 플레이어
            if (effect.target != 0)
            {
                Debug.LogWarning(
                    "[BattleManager] 아직 지원하지 않는 적 패턴 target: " +
                    effect.target +
                    " / Pattern_ID: " +
                    effect.patternId
                );

                continue;
            }

            switch (effect.effectType)
            {
                // 5001 기본 공격.
                // 현재 패턴 테이블에서 기본 공격 행에 사용하는 타입.
                case 0:
                    if (!attackAnimationPlayed)
                    {
                        attackAnimationPlayed = true;

                        yield return StartCoroutine(
                            ExecuteLegacyEnemyAttackRoutine(
                                enemy,
                                enemyStatusController,
                                playerStatusController,
                                0
                            )
                        );
                    }
                    break;

                // 5003~5005 강화 공격.
                // Effect_Power 3 / 6 / 12를
                // 기존 공격력에 더하는 추가 피해값으로 사용한다.
                case 1:
                    if (!attackAnimationPlayed)
                    {
                        attackAnimationPlayed = true;

                        int bonusDamage =
                            Mathf.RoundToInt(
                                effect.effectPower
                            );

                        yield return StartCoroutine(
                            ExecuteLegacyEnemyAttackRoutine(
                                enemy,
                                enemyStatusController,
                                playerStatusController,
                                bonusDamage
                            )
                        );
                    }
                    break;

                // 상태이상 적용.
                // 현재 5002 위액 뿌리기 -> status 2132 -> target 0.
                case 17:
                    yield return StartCoroutine(
                        ApplyEnemyPatternStatusRoutine(
                            enemy,
                            playerStatusController,
                            effect
                        )
                    );
                    break;

                default:
                    Debug.LogWarning(
                        "[BattleManager] 아직 지원하지 않는 적 패턴 Effect_Type: " +
                        effect.effectType +
                        " / Pattern_ID: " +
                        effect.patternId
                    );
                    break;
            }
        }
    }

    private IEnumerator ApplyEnemyPatternStatusRoutine(
        BattleUnit enemy,
        StatusEffectController playerStatusController,
        EnemyPatternData effect)
    {
        // 상태이상 패턴도 적의 행동이므로 기존 공격 컷인을 사용한다.
        if (cutInController != null &&
            playerUnit != null)
        {
            yield return cutInController
                .PlayEnemyAttackCutIn(
                    enemy,
                    playerUnit
                );
        }
        else
        {
            enemy.PlayAttackAnimation();

            yield return new WaitForSeconds(
                enemyAttackDelay
            );
        }

        if (effect.statusId <= 0)
            yield break;

        if (playerStatusController == null)
        {
            Debug.LogError(
                "[BattleManager] 플레이어 StatusEffectController가 없습니다."
            );

            yield break;
        }

        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleManager] StatusEffectDatabase가 없습니다."
            );

            yield break;
        }

        StatusEffectData statusData =
            StatusEffectDatabase.Instance
                .GetStatusEffect(
                    effect.statusId
                );

        if (statusData == null)
        {
            Debug.LogError(
                "[BattleManager] 적 패턴 상태이상을 찾을 수 없습니다." +
                " / Status ID: " +
                effect.statusId
            );

            yield break;
        }

        bool added =
            playerStatusController
                .AddStatusEffect(
                    statusData
                );

        Debug.Log(
            "[BattleManager] 적 패턴 상태이상 적용" +
            " / Pattern: " +
            effect.patternName +
            " / Status ID: " +
            effect.statusId +
            " / 성공: " +
            added
        );

        if (added &&
            playerUnit != null)
        {
            ShowFloatingText(
                playerUnit.transform.position,
                statusData.buffName
            );
        }

        yield return new WaitForSeconds(
            afterHitDelay
        );
    }

    // =========================================================
    // 기존 적 공격 처리
    // =========================================================
    // 기존 EnemyTurnRoutine의 공격 계산을 그대로 분리했다.
    // bonusDamage는 패턴 강화 공격에서만 추가한다.
    private IEnumerator ExecuteLegacyEnemyAttackRoutine(
        BattleUnit enemy,
        StatusEffectController enemyStatusController,
        StatusEffectController playerStatusController,
        int bonusDamage)
    {
        if (enemy == null)
            yield break;

        if (cutInController != null &&
            playerUnit != null)
        {
            yield return cutInController
                .PlayEnemyAttackCutIn(
                    enemy,
                    playerUnit
                );
        }
        else
        {
            enemy.PlayAttackAnimation();

            yield return new WaitForSeconds(
                enemyAttackDelay
            );
        }

        int finalEnemyAccuracy =
            enemy.accuracy +
            enemyAccuracyModifierThisTurn;

        if (playerStatusController != null)
        {
            int evasionBonus =
                playerStatusController
                    .GetEvasionBonus();

            finalEnemyAccuracy -=
                evasionBonus;
        }

        finalEnemyAccuracy =
            Mathf.Clamp(
                finalEnemyAccuracy,
                0,
                100
            );

        Debug.Log(
            "[BattleManager] 적 최종 명중률: " +
            finalEnemyAccuracy
        );

        bool hit =
            RollEnemyHit(
                finalEnemyAccuracy
            );

        bool itemShieldBlocked =
            hit &&
            (
                ignoreAllEnemyHitsThisTurn ||
                ignoreNextEnemyHit
            );

        if (itemShieldBlocked)
        {
            if (ignoreNextEnemyHit)
                ignoreNextEnemyHit = false;

            hit = false;

            if (playerUnit != null)
            {
                ShowFloatingText(
                    playerUnit.transform.position,
                    "BLOCK"
                );
            }

            Debug.Log(
                "[BattleManager] 에너지 보호막으로 적 공격 무효"
            );
        }

        bool guardWasActive =
            playerStatusController != null &&
            playerStatusController.HasStatusEffect(
                StatusEffectType.Guard
            );

        bool applyGuardStunAfterTurn =
            false;

        if (hit)
        {
            int damage =
                Mathf.Max(
                    1,
                    enemy.attackPower +
                    bonusDamage
                );

            if (bonusDamage != 0)
            {
                Debug.Log(
                    "[BattleManager] 강화 공격 추가 피해: +" +
                    bonusDamage +
                    " / 기본 공격력: " +
                    enemy.attackPower
                );
            }

            if (enemyStatusController != null)
            {
                float attackMultiplier =
                    enemyStatusController
                        .GetAttackPowerMultiplier();

                damage =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            damage *
                            attackMultiplier
                        )
                    );

                Debug.Log(
                    "[BattleManager] 적 공격력 상태이상: " +
                    (enemy.attackPower + bonusDamage) +
                    " x " +
                    attackMultiplier +
                    " = " +
                    damage
                );
            }

            if (PlayerResourceManager.Instance != null &&
                PlayerResourceManager.Instance
                    .IsHungerAllDecreasePenaltyActive())
            {
                damage =
                    Mathf.RoundToInt(
                        damage * 1.5f
                    );

                Debug.Log(
                    "[BattleManager] " +
                    "배고픔 패널티 적용 → 피해 1.5배"
                );
            }

            if (playerStatusController != null)
            {
                float defenseMultiplier =
                    playerStatusController
                        .GetDefenseMultiplier();

                if (defenseMultiplier > 0f)
                {
                    damage =
                        Mathf.Max(
                            1,
                            Mathf.RoundToInt(
                                damage /
                                defenseMultiplier
                            )
                        );
                }

                Debug.Log(
                    "[BattleManager] " +
                    "플레이어 방어 상태이상 배율: " +
                    defenseMultiplier
                );
            }

            if (playerStatusController != null)
            {
                float damageTakenMultiplier =
                    playerStatusController
                        .GetDamageTakenMultiplier();

                damage =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            damage *
                            damageTakenMultiplier
                        )
                    );

                Debug.Log(
                    "[BattleManager] " +
                    "받는 피해 상태이상 배율: " +
                    damageTakenMultiplier
                );
            }

            if (guardWasActive)
            {
                float guardMultiplier =
                    Mathf.Clamp01(
                        1f -
                        guardDamageReductionPercent /
                        100f
                    );

                damage =
                    Mathf.Max(
                        1,
                        Mathf.CeilToInt(
                            damage *
                            guardMultiplier
                        )
                    );

                applyGuardStunAfterTurn =
                    true;

                Debug.Log(
                    "[BattleManager] 방어 적용 - " +
                    "받는 피해 감소 / " +
                    "방어구 내구도 소모 " +
                    guardArmorDurabilityMultiplier +
                    "배"
                );
            }

            if (playerUnit != null)
            {
                playerUnit.TakeDamage(
                    damage
                );
            }

            if (PlayerResourceManager.Instance != null)
            {
                PlayerResourceManager.Instance
                    .ChangeHealth(
                        -damage,
                        guardWasActive
                            ? "적 공격 피해 (방어 적용)"
                            : "적 공격 피해"
                    );
            }

            if (playerUnit != null)
            {
                ShowFloatingText(
                    playerUnit.transform.position,
                    damage.ToString()
                );
            }

            ConsumePlayerArmorDurability(
                guardWasActive
            );
        }
        else if (!itemShieldBlocked)
        {
            if (playerUnit != null)
            {
                ShowFloatingText(
                    playerUnit.transform.position,
                    "MISS"
                );
            }
        }

        yield return new WaitForSeconds(
            afterHitDelay
        );

        if (applyGuardStunAfterTurn &&
            enemyStatusController != null)
        {
            enemyStatusController.AddStatusEffect(
                CreateGuardStunStatusData()
            );
        }
    }

    private void ConsumePlayerWeaponDurability()
    {
        if (EquipmentManager.Instance == null)
            return;

        int finalCost =
            weaponDurabilityCost;

        StatusEffectController controller =
            GetPlayerStatusController();

        if (controller != null)
        {
            finalCost =
                Mathf.RoundToInt(
                    finalCost *
                    controller
                        .GetDurabilityCostMultiplier()
                );
        }

        finalCost =
            Mathf.Max(
                0,
                finalCost
            );

        EquipmentManager.Instance
            .ConsumeMainWeaponDurability(
                finalCost
            );

        Debug.Log(
            "[BattleManager] 무기 내구도 소모: " +
            finalCost
        );
    }

    // =========================================================
    // 무기 스킬 내구도
    //
    // 현재 내구도가 비용보다 적어도 사용 가능하다.
    // 실제 0 처리/파괴는 EquipmentManager가 담당한다.
    // 부식 상태의 내구도 소모 배율도 일반 공격과 동일하게 적용.
    // =========================================================

    private void ConsumePlayerWeaponSkillDurability(
        int baseCost)
    {
        if (EquipmentManager.Instance == null)
            return;

        int finalCost =
            Mathf.Max(
                0,
                baseCost
            );

        StatusEffectController controller =
            GetPlayerStatusController();

        if (controller != null)
        {
            finalCost =
                Mathf.RoundToInt(
                    finalCost *
                    controller
                        .GetDurabilityCostMultiplier()
                );
        }

        finalCost =
            Mathf.Max(
                0,
                finalCost
            );

        EquipmentManager.Instance
            .ConsumeMainWeaponDurability(
                finalCost
            );

        Debug.Log(
            "[BattleManager] 무기 스킬 내구도 소모: " +
            finalCost +
            " / 기본 비용: " +
            baseCost
        );
    }


    private void ConsumePlayerArmorDurability(
        bool guardActive)
    {
        if (EquipmentManager.Instance == null)
            return;

        int finalCost =
            armorDurabilityCost;

        if (guardActive)
        {
            finalCost *=
                Mathf.Max(
                    1,
                    guardArmorDurabilityMultiplier
                );
        }

        StatusEffectController controller =
            GetPlayerStatusController();

        if (controller != null)
        {
            finalCost =
                Mathf.RoundToInt(
                    finalCost *
                    controller
                        .GetDurabilityCostMultiplier()
                );
        }

        finalCost =
            Mathf.Max(
                0,
                finalCost
            );

        EquipmentManager.Instance
            .ConsumeArmorDurability(
                finalCost
            );

        Debug.Log(
            "[BattleManager] 방어구 내구도 소모: " +
            finalCost +
            " / Guard: " +
            guardActive
        );
    }

    private StatusEffectController GetPlayerStatusController()
    {
        if (playerUnit == null)
            return null;

        StatusEffectController controller =
            playerUnit.GetComponent<StatusEffectController>();

        if (controller == null)
        {
            controller =
                playerUnit.GetComponentInParent<StatusEffectController>();
        }

        if (controller == null)
        {
            controller =
                playerUnit.GetComponentInChildren<StatusEffectController>();
        }

        return controller;
    }

    private StatusEffectController GetStatusController(
        BattleUnit unit)
    {
        if (unit == null)
            return null;

        return unit.GetComponent<StatusEffectController>();
    }

    private StatusEffectController GetOrAddStatusController(
        BattleUnit unit)
    {
        if (unit == null)
            return null;

        StatusEffectController controller =
            unit.GetComponent<StatusEffectController>();

        if (controller == null)
        {
            controller =
                unit.gameObject.AddComponent<StatusEffectController>();
        }

        return controller;
    }

    private StatusEffectData CreateGuardStatusData()
    {
        return new StatusEffectData
        {
            id = GuardStatusId,
            buffName = "방어",
            description =
                "이번 적 팀의 공격 피해를 50% 감소시키고, " +
                "피격 시 방어구 내구도 소모가 2배가 됩니다. " +
                "방어 중 공격한 적은 다음 행동 1회를 기절합니다.",
            tendency = StatusEffectTendency.Positive,
            effectType = StatusEffectType.Guard,
            effectPower = guardDamageReductionPercent,
            buffDuration = 1,
            whenDecreaseDuration = StatusEffectTiming.EnemyTeamEnd,
            whenBuffEffect = StatusEffectTiming.None,
            canStack = false,
            whenRemove = StatusEffectRemoveType.DurationEnded
        };
    }

    private StatusEffectData CreateGuardStunStatusData()
    {
        return new StatusEffectData
        {
            id = GuardStunStatusId,
            buffName = "기절",
            description =
                "다음 자신의 행동 1회를 수행할 수 없습니다.",
            tendency = StatusEffectTendency.Negative,
            effectType = StatusEffectType.Stun,
            effectPower = 0,
            buffDuration = 1,
            whenDecreaseDuration = StatusEffectTiming.SelfTurnEnd,
            whenBuffEffect = StatusEffectTiming.None,
            canStack = true,
            whenRemove = StatusEffectRemoveType.DurationEnded
        };
    }

    private void ReturnToPlayerTurnIfPossible()
    {
        if (!battleRunning ||
            state == BattleState.BattleEnd)
        {
            return;
        }

        state = BattleState.PlayerTurn;

        if (uiManager != null)
        {
            uiManager.ShowBattleUI();
            uiManager.ShowMainBattleMenu();
        }
    }

    private IEnumerator RunRoutine()
    {
        state = BattleState.EnemyTurn;

        int finalRunPercent =
            PlayerStats.Instance != null
                ? PlayerStats.Instance
                    .GetRunSuccessPercent()
                : runSuccessPercent;

        int roll =
            Random.Range(0, 100);

        if (roll < finalRunPercent)
        {
            yield return new WaitForSeconds(
                actionDelay
            );

            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .AddTurn("도망 성공");
            }

            EndBattle();
        }
        else
        {
            yield return new WaitForSeconds(
                actionDelay
            );

            yield return StartCoroutine(
                EnemyTurnRoutine(true)
            );
        }
    }

    private bool RollEnemyHit(
        int enemyAccuracy)
    {
        if (PlayerStats.Instance == null)
        {
            return Random.Range(0, 100) <
                   Mathf.Clamp(
                       enemyAccuracy,
                       10,
                       95
                   );
        }

        int playerEvasionChance =
            PlayerStats.Instance
                .GetFinalEvasion(
                    enemyAccuracy
                );

        int enemyHitChance =
            100 - playerEvasionChance;

        enemyHitChance =
            Mathf.Clamp(
                enemyHitChance,
                5,
                95
            );

        return Random.Range(0, 100) <
               enemyHitChance;
    }

    private bool IsValidLivingEnemy(
        BattleUnit enemy)
    {
        return enemy != null &&
               !enemy.IsDead &&
               enemy.gameObject.activeInHierarchy;
    }

    private void RemoveDeadEnemies()
    {
        enemies.RemoveAll(
            enemy =>
                enemy == null ||
                enemy.IsDead ||
                !enemy.gameObject.activeInHierarchy
        );

        List<BattleUnit> deadRuntimeKeys =
            new List<BattleUnit>();

        foreach (
            BattleUnit unit
            in enemyMonsterData.Keys)
        {
            if (unit == null ||
                !enemies.Contains(unit))
            {
                deadRuntimeKeys.Add(unit);
            }
        }

        foreach (
            BattleUnit unit
            in deadRuntimeKeys)
        {
            enemyMonsterData.Remove(unit);
            enemyPreviousPatternIds.Remove(unit);
        }
    }

    private bool AllEnemiesDead()
    {
        RemoveDeadEnemies();

        return enemies.Count == 0;
    }

    private void EndBattle()
    {
        if (state == BattleState.BattleEnd)
            return;

        state = BattleState.BattleEnd;

        ClearEnemyArrows();

        StartCoroutine(
            EndBattleRoutine()
        );
    }

    private IEnumerator EndBattleRoutine()
    {
        yield return new WaitForSeconds(1f);

        ClearEnemies();

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(false);

        if (uiManager != null)
            uiManager.HideBattleUI();

        pendingTargetWeaponSkill = null;
        state = BattleState.None;
        battleRunning = false;
    }

    private void EndBattleImmediately()
    {
        ClearEnemies();

        if (enemyGroup != null)
            enemyGroup.gameObject.SetActive(false);

        if (uiManager != null)
            uiManager.HideBattleUI();

        pendingTargetWeaponSkill = null;
        state = BattleState.None;
        battleRunning = false;
    }

    private void ClearEnemyArrows()
    {
        foreach (
            BattleUnit enemy
            in enemies)
        {
            if (enemy != null)
                enemy.SetArrow(false);
        }
    }

    private void ShowFloatingText(
        Vector3 worldPosition,
        string text)
    {
        if (floatingTextPrefab == null ||
            worldCanvas == null)
        {
            return;
        }

        TextMeshProUGUI floatingText =
            Instantiate(
                floatingTextPrefab,
                worldCanvas.transform
            );

        floatingText.text = text;

        Camera mainCamera =
            Camera.main;

        if (mainCamera != null)
        {
            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(
                    worldPosition +
                    Vector3.up * 1.5f
                );

            floatingText.transform.position =
                screenPosition;
        }

        Destroy(
            floatingText.gameObject,
            0.8f
        );
    }
}