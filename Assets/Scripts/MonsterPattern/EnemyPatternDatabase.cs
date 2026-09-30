using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPatternDatabase : MonoBehaviour
{
    public static EnemyPatternDatabase Instance { get; private set; }

    [Header("Enemy Pattern Data")]
    [SerializeField]
    private List<EnemyPatternData> patternRows =
        new List<EnemyPatternData>();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RegisterDefaultPatterns();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // =========================================================
    // 기본 패턴 데이터 등록
    // =========================================================

    private void RegisterDefaultPatterns()
    {
        patternRows.Clear();

        // -----------------------------------------------------
        // Enemy 3001
        // -----------------------------------------------------

        // 5001 - 기본 공격
        RegisterPattern(
            enemyId: 3001,
            patternId: 5001,
            patternName: "기본 공격",
            patternPossibility: 50,
            condition: 0,
            hp: 0,
            beforePatternId: 0,
            effectType: 0,
            effectPower: 0f,
            statusId: 0,
            order: 0,
            target: 0
        );

        // 5002 - 위액 뿌리기
        // Effect_Type 17
        // Status 2132 = 명중률 감소
        // Target 0 = 플레이어
        RegisterPattern(
            enemyId: 3001,
            patternId: 5002,
            patternName: "위액 뿌리기",
            patternPossibility: 50,
            condition: 0,
            hp: 0,
            beforePatternId: 0,
            effectType: 17,
            effectPower: 0f,
            statusId: 2132,
            order: 0,
            target: 0
        );

        // 5003 - 강화 공격 1
        // 5002 다음에 발동
        RegisterPattern(
            enemyId: 3001,
            patternId: 5003,
            patternName: "강화 공격 1",
            patternPossibility: 0,
            condition: 1,
            hp: 0,
            beforePatternId: 5002,
            effectType: 1,
            effectPower: 3f,
            statusId: 0,
            order: 0,
            target: 0
        );

        // 5004 - 강화 공격 2
        // 5003 다음에 발동
        RegisterPattern(
            enemyId: 3001,
            patternId: 5004,
            patternName: "강화 공격 2",
            patternPossibility: 0,
            condition: 1,
            hp: 0,
            beforePatternId: 5003,
            effectType: 1,
            effectPower: 6f,
            statusId: 0,
            order: 0,
            target: 0
        );

        // 5005 - 강화 공격 3
        // 5004 다음에 발동
        RegisterPattern(
            enemyId: 3001,
            patternId: 5005,
            patternName: "강화 공격 3",
            patternPossibility: 0,
            condition: 1,
            hp: 0,
            beforePatternId: 5004,
            effectType: 1,
            effectPower: 12f,
            statusId: 0,
            order: 0,
            target: 0
        );

        Debug.Log(
            "[EnemyPatternDatabase] 기본 패턴 데이터 등록 완료." +
            " 총 행 수: " +
            patternRows.Count
        );
    }

    private void RegisterPattern(
        int enemyId,
        int patternId,
        string patternName,
        int patternPossibility,
        int condition,
        int hp,
        int beforePatternId,
        int effectType,
        float effectPower,
        int statusId,
        int order,
        int target)
    {
        EnemyPatternData data =
            new EnemyPatternData(
                enemyId,
                patternId,
                patternName,
                patternPossibility,
                condition,
                hp,
                beforePatternId,
                effectType,
                effectPower,
                statusId,
                order,
                target
            );

        patternRows.Add(data);
    }

    // =========================================================
    // 패턴 선택
    // =========================================================

    public EnemyPatternData SelectNextPattern(
        int enemyId,
        int previousPatternId)
    {
        // -----------------------------------------------------
        // 1. 조건 패턴 우선
        // -----------------------------------------------------

        if (previousPatternId > 0)
        {
            List<EnemyPatternData> chainPatterns =
                GetChainPatternHeaders(
                    enemyId,
                    previousPatternId
                );

            if (chainPatterns.Count > 0)
            {
                EnemyPatternData selected =
                    chainPatterns[0];

                Debug.Log(
                    "[EnemyPatternDatabase] 연계 패턴 선택" +
                    " | Enemy: " + enemyId +
                    " | Previous: " + previousPatternId +
                    " | Pattern: " + selected.patternId +
                    " (" + selected.patternName + ")"
                );

                return selected;
            }
        }

        // -----------------------------------------------------
        // 2. 조건 패턴이 없으면 일반 패턴
        // -----------------------------------------------------

        List<EnemyPatternData> normalPatterns =
            GetNormalPatternHeaders(enemyId);

        if (normalPatterns.Count == 0)
        {
            Debug.LogWarning(
                "[EnemyPatternDatabase] 일반 패턴이 없습니다." +
                " Enemy_ID: " +
                enemyId
            );

            return null;
        }

        return SelectRandomNormalPattern(
            enemyId,
            normalPatterns
        );
    }

    // =========================================================
    // 효과 목록
    // =========================================================

    public List<EnemyPatternData> GetPatternEffects(
        int enemyId,
        int patternId)
    {
        return patternRows
            .Where(
                row =>
                    row != null &&
                    row.enemyId == enemyId &&
                    row.patternId == patternId
            )
            .OrderBy(row => row.order)
            .ToList();
    }

    public List<EnemyPatternData> GetEnemyPatternRows(
        int enemyId)
    {
        return patternRows
            .Where(
                row =>
                    row != null &&
                    row.enemyId == enemyId
            )
            .OrderBy(row => row.patternId)
            .ThenBy(row => row.order)
            .ToList();
    }

    public EnemyPatternData GetPattern(
        int enemyId,
        int patternId)
    {
        return patternRows
            .Where(
                row =>
                    row != null &&
                    row.enemyId == enemyId &&
                    row.patternId == patternId
            )
            .OrderBy(row => row.order)
            .FirstOrDefault();
    }

    // =========================================================
    // 일반 패턴
    // =========================================================

    private List<EnemyPatternData>
        GetNormalPatternHeaders(
            int enemyId)
    {
        return patternRows
            .Where(
                row =>
                    row != null &&
                    row.enemyId == enemyId &&
                    row.condition == 0
            )
            .GroupBy(row => row.patternId)
            .Select(
                group =>
                    group
                        .OrderBy(row => row.order)
                        .First()
            )
            .OrderBy(row => row.patternId)
            .ToList();
    }

    // =========================================================
    // 연계 패턴
    // =========================================================

    private List<EnemyPatternData>
        GetChainPatternHeaders(
            int enemyId,
            int previousPatternId)
    {
        return patternRows
            .Where(
                row =>
                    row != null &&
                    row.enemyId == enemyId &&
                    row.condition == 1 &&
                    row.beforePatternId ==
                    previousPatternId
            )
            .GroupBy(row => row.patternId)
            .Select(
                group =>
                    group
                        .OrderBy(row => row.order)
                        .First()
            )
            .OrderBy(row => row.patternId)
            .ToList();
    }

    // =========================================================
    // 확률 선택
    // =========================================================

    private EnemyPatternData SelectRandomNormalPattern(
        int enemyId,
        List<EnemyPatternData> normalPatterns)
    {
        int totalPossibility = 0;

        foreach (
            EnemyPatternData pattern
            in normalPatterns)
        {
            totalPossibility +=
                Mathf.Max(
                    0,
                    pattern.patternPossibility
                );
        }

        if (totalPossibility <= 0)
        {
            Debug.LogWarning(
                "[EnemyPatternDatabase] " +
                "일반 패턴 확률 합계가 0입니다." +
                " Enemy_ID: " +
                enemyId
            );

            return normalPatterns[0];
        }

        int roll =
            Random.Range(
                0,
                totalPossibility
            );

        int accumulated = 0;

        foreach (
            EnemyPatternData pattern
            in normalPatterns)
        {
            accumulated +=
                Mathf.Max(
                    0,
                    pattern.patternPossibility
                );

            if (roll < accumulated)
            {
                Debug.Log(
                    "[EnemyPatternDatabase] 일반 패턴 선택" +
                    " | Enemy: " + enemyId +
                    " | Pattern: " +
                    pattern.patternId +
                    " (" +
                    pattern.patternName +
                    ")" +
                    " | Roll: " +
                    roll +
                    "/" +
                    totalPossibility
                );

                return pattern;
            }
        }

        return normalPatterns[
            normalPatterns.Count - 1
        ];
    }

    // =========================================================
    // 데이터 검사
    // =========================================================

    [ContextMenu("패턴 데이터 검사")]
    private void ValidatePatternData()
    {
        if (patternRows == null ||
            patternRows.Count == 0)
        {
            Debug.LogWarning(
                "[EnemyPatternDatabase] 등록된 패턴이 없습니다."
            );

            return;
        }

        IEnumerable<int> enemyIds =
            patternRows
                .Where(row => row != null)
                .Select(row => row.enemyId)
                .Distinct()
                .OrderBy(id => id);

        foreach (int enemyId in enemyIds)
        {
            List<EnemyPatternData> normalPatterns =
                GetNormalPatternHeaders(enemyId);

            int total =
                normalPatterns.Sum(
                    pattern =>
                        Mathf.Max(
                            0,
                            pattern.patternPossibility
                        )
                );

            Debug.Log(
                "[EnemyPatternDatabase] 데이터 검사" +
                " | Enemy_ID: " +
                enemyId +
                " | 일반 패턴: " +
                normalPatterns.Count +
                " | 확률 합계: " +
                total +
                "%"
            );

            if (normalPatterns.Count > 0 &&
                total != 100)
            {
                Debug.LogWarning(
                    "[EnemyPatternDatabase] " +
                    enemyId +
                    " 일반 패턴 확률 합계가 " +
                    total +
                    "% 입니다."
                );
            }
        }
    }

    // =========================================================
    // 3001 연계 테스트
    // =========================================================

    [ContextMenu("3001 패턴 연계 테스트")]
    private void TestEnemy3001PatternChain()
    {
        Debug.Log(
            "========== 3001 패턴 연계 테스트 =========="
        );

        EnemyPatternData p5003 =
            SelectNextPattern(
                3001,
                5002
            );

        EnemyPatternData p5004 =
            SelectNextPattern(
                3001,
                5003
            );

        EnemyPatternData p5005 =
            SelectNextPattern(
                3001,
                5004
            );

        EnemyPatternData after5005 =
            SelectNextPattern(
                3001,
                5005
            );

        Debug.Log(
            "5002 다음 → " +
            GetPatternDebugName(p5003)
        );

        Debug.Log(
            "5003 다음 → " +
            GetPatternDebugName(p5004)
        );

        Debug.Log(
            "5004 다음 → " +
            GetPatternDebugName(p5005)
        );

        Debug.Log(
            "5005 다음 → 일반 패턴 복귀 → " +
            GetPatternDebugName(after5005)
        );

        Debug.Log(
            "=========================================="
        );
    }

    private string GetPatternDebugName(
        EnemyPatternData pattern)
    {
        if (pattern == null)
            return "NULL";

        return
            pattern.patternId +
            " (" +
            pattern.patternName +
            ")";
    }
}