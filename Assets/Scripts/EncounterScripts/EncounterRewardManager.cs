using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기획서의 "전투 종료 시 보상" 테이블을 관리한다.
///
/// 역할:
/// 1) Encounter Group ID(7000~7005)를 받는다.
/// 2) 해당 그룹의 보상 확률표에서 아이템 1개를 추첨한다.
/// 3) 실제 인벤토리 지급/UI 표시는 담당하지 않는다.
///
/// 다음 단계에서 BattleManager 전투 승리 처리와 연결한다.
/// </summary>
public class EncounterRewardManager : MonoBehaviour
{
    public static EncounterRewardManager Instance
    {
        get;
        private set;
    }

    private class RewardEntry
    {
        public int itemId;
        public int possibility;

        public RewardEntry(
            int itemId,
            int possibility)
        {
            this.itemId = itemId;
            this.possibility = possibility;
        }
    }

    private readonly Dictionary<int, List<RewardEntry>>
        rewardTable =
            new Dictionary<int, List<RewardEntry>>();

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

        RegisterRewardTable();
        ValidateRewardTable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =========================================================
    // Reward Table
    // =========================================================

    private void RegisterRewardTable()
    {
        rewardTable.Clear();

        // 7000
        AddReward(7000, 1001, 20);
        AddReward(7000, 1004, 15);
        AddReward(7000, 1007, 15);
        AddReward(7000, 1020, 50);

        // 7001
        AddReward(7001, 1001, 20);
        AddReward(7001, 1004, 15);
        AddReward(7001, 1007, 15);
        AddReward(7001, 1020, 50);

        // 7002
        AddReward(7002, 1013, 40);
        AddReward(7002, 1110, 10);
        AddReward(7002, 1020, 50);

        // 7003
        AddReward(7003, 1001, 20);
        AddReward(7003, 1002, 20);
        AddReward(7003, 1004, 20);
        AddReward(7003, 1007, 20);
        AddReward(7003, 1020, 20);

        // 7004
        AddReward(7004, 1109, 20);
        AddReward(7004, 1005, 20);
        AddReward(7004, 1008, 20);
        AddReward(7004, 1101, 10);
        AddReward(7004, 1020, 30);

        // 7005
        AddReward(7005, 1104, 20);
        AddReward(7005, 1107, 20);
        AddReward(7005, 1002, 20);
        AddReward(7005, 1004, 20);
        AddReward(7005, 1006, 20);
    }

    private void AddReward(
        int encounterGroupId,
        int itemId,
        int possibility)
    {
        if (
            !rewardTable.TryGetValue(
                encounterGroupId,
                out List<RewardEntry> entries
            )
        )
        {
            entries =
                new List<RewardEntry>();

            rewardTable.Add(
                encounterGroupId,
                entries
            );
        }

        entries.Add(
            new RewardEntry(
                itemId,
                possibility
            )
        );
    }

    // =========================================================
    // Roll
    // =========================================================

    /// <summary>
    /// Encounter Group의 보상 아이템 1개를 확률에 따라 뽑는다.
    /// 성공하면 true와 itemId를 반환한다.
    /// </summary>
    public bool TryRollReward(
        int encounterGroupId,
        out int itemId)
    {
        itemId = 0;

        if (
            !rewardTable.TryGetValue(
                encounterGroupId,
                out List<RewardEntry> entries
            ) ||
            entries == null ||
            entries.Count == 0
        )
        {
            Debug.LogWarning(
                "[EncounterRewardManager] " +
                "보상 테이블이 없습니다. " +
                "Encounter Group: " +
                encounterGroupId
            );

            return false;
        }

        int totalPossibility = 0;

        foreach (RewardEntry entry in entries)
        {
            if (entry == null)
                continue;

            totalPossibility +=
                Mathf.Max(
                    0,
                    entry.possibility
                );
        }

        if (totalPossibility <= 0)
        {
            Debug.LogError(
                "[EncounterRewardManager] " +
                "보상 확률 합계가 0 이하입니다. " +
                "Encounter Group: " +
                encounterGroupId
            );

            return false;
        }

        int roll =
            Random.Range(
                0,
                totalPossibility
            );

        int accumulated = 0;

        foreach (RewardEntry entry in entries)
        {
            if (entry == null)
                continue;

            accumulated +=
                Mathf.Max(
                    0,
                    entry.possibility
                );

            if (roll < accumulated)
            {
                itemId =
                    entry.itemId;

                Debug.Log(
                    "[EncounterRewardManager] " +
                    "전투 보상 선택 완료" +
                    " / Group: " +
                    encounterGroupId +
                    " / Item ID: " +
                    itemId +
                    " / Roll: " +
                    roll +
                    " / Total: " +
                    totalPossibility
                );

                return true;
            }
        }

        return false;
    }

    // =========================================================
    // Query / Validation
    // =========================================================

    public bool HasRewardGroup(
        int encounterGroupId)
    {
        return
            rewardTable.ContainsKey(
                encounterGroupId
            );
    }

    private void ValidateRewardTable()
    {
        foreach (
            KeyValuePair<int, List<RewardEntry>> pair
            in rewardTable
        )
        {
            int total = 0;

            foreach (
                RewardEntry entry
                in pair.Value
            )
            {
                if (entry == null)
                    continue;

                total +=
                    Mathf.Max(
                        0,
                        entry.possibility
                    );
            }

            if (total != 100)
            {
                Debug.LogWarning(
                    "[EncounterRewardManager] " +
                    "보상 확률 합계가 100이 아닙니다." +
                    " / Group: " +
                    pair.Key +
                    " / Total: " +
                    total
                );
            }
        }
    }
}
