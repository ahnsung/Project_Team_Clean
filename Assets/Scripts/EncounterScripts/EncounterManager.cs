using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기획서 "상점 및 인카운터"의 EncounterTileTable / EncounterMonsterTiles 담당.
///
/// 역할:
/// 1) 현재 던전 좌표가 어느 encounter_group_id에 속하는지 반환
/// 2) 해당 그룹에서 확률에 따라 몬스터 조합 1개 선택
///
/// 주의:
/// - "1칸 이동할 때마다 전투 발생 확률 +10%" 판정은 이 클래스가 담당하지 않는다.
///   이 클래스는 전투가 발생하기로 결정된 뒤 "어떤 적이 나오는가"를 담당한다.
/// - 적은 최대 3마리.
/// </summary>
public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    [Serializable]
    private class EncounterArea
    {
        public int groupId;
        public int minX;
        public int minY;
        public int maxX;
        public int maxY;

        public EncounterArea(int groupId, int minX, int minY, int maxX, int maxY)
        {
            this.groupId = groupId;
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }

        public bool Contains(Vector2Int room)
        {
            return room.x >= minX &&
                   room.x <= maxX &&
                   room.y >= minY &&
                   room.y <= maxY;
        }
    }

    [Serializable]
    private class EncounterEntry
    {
        public int groupId;
        public int enemy1Id;
        public int enemy2Id;
        public int enemy3Id;
        public int possibility;

        public EncounterEntry(
            int groupId,
            int enemy1Id,
            int enemy2Id,
            int enemy3Id,
            int possibility)
        {
            this.groupId = groupId;
            this.enemy1Id = enemy1Id;
            this.enemy2Id = enemy2Id;
            this.enemy3Id = enemy3Id;
            this.possibility = possibility;
        }

        public int[] GetEnemyIds()
        {
            List<int> result = new List<int>(3);

            if (enemy1Id > 0)
                result.Add(enemy1Id);

            if (enemy2Id > 0)
                result.Add(enemy2Id);

            if (enemy3Id > 0)
                result.Add(enemy3Id);

            return result.ToArray();
        }
    }

    private readonly List<EncounterArea> encounterAreas =
        new List<EncounterArea>();

    private readonly List<EncounterEntry> encounterEntries =
        new List<EncounterEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RegisterEncounterAreas();
        RegisterEncounterEntries();
        ValidateEncounterData();
    }

    // =========================================================
    // EncounterTileTable
    // =========================================================

    private void RegisterEncounterAreas()
    {
        encounterAreas.Clear();

        // 7000 중앙지역
        AddArea(7000, 11, 21, 18, 32);

        // 7001 왼쪽지역
        // 같은 그룹 ID가 여러 사각형 범위를 가질 수 있다.
        AddArea(7001, 0, 23, 10, 35);
        AddArea(7001, 1, 36, 6, 36);

        // 7002 아래지역
        AddArea(7002, 7, 33, 20, 42);

        // 7003 위쪽지역
        AddArea(7003, 3, 0, 26, 17);
        AddArea(7003, 18, 13, 20, 15);

        // 7004 오른쪽 지역
        AddArea(7004, 19, 20, 26, 24);
        AddArea(7004, 27, 16, 43, 27);

        // 7005 오른쪽 지역2
        AddArea(7005, 23, 29, 45, 41);
        AddArea(7005, 19, 29, 22, 29);
    }

    private void AddArea(
        int groupId,
        int minX,
        int minY,
        int maxX,
        int maxY)
    {
        encounterAreas.Add(
            new EncounterArea(
                groupId,
                minX,
                minY,
                maxX,
                maxY
            )
        );
    }

    // =========================================================
    // EncounterMonsterTiles
    // =========================================================

    private void RegisterEncounterEntries()
    {
        encounterEntries.Clear();

        // -------------------------
        // 7000
        // -------------------------
        AddEntry(7000, 3001, 0, 0, 50);
        AddEntry(7000, 3002, 0, 0, 50);

        // -------------------------
        // 7001
        // -------------------------
        AddEntry(7001, 3001, 0, 0, 25);
        AddEntry(7001, 3002, 0, 0, 25);
        AddEntry(7001, 3001, 3001, 0, 15);
        AddEntry(7001, 3002, 3002, 0, 15);
        AddEntry(7001, 3001, 3002, 0, 10);

        // -------------------------
        // 7002
        // -------------------------
        AddEntry(7002, 3001, 0, 0, 20);
        AddEntry(7002, 3002, 0, 0, 20);
        AddEntry(7002, 3001, 3002, 0, 20);
        AddEntry(7002, 3002, 3002, 0, 20);
        AddEntry(7002, 3003, 0, 0, 20);

        // -------------------------
        // 7003
        // -------------------------
        AddEntry(7003, 3001, 3002, 0, 20);
        AddEntry(7003, 3003, 0, 0, 25);
        AddEntry(7003, 3004, 0, 0, 25);
        AddEntry(7003, 3003, 3001, 3001, 15);
        AddEntry(7003, 3004, 3002, 0, 15);

        // -------------------------
        // 7004
        // -------------------------
        AddEntry(7004, 3003, 0, 0, 25);
        AddEntry(7004, 3004, 0, 0, 25);
        AddEntry(7004, 3002, 3001, 3001, 25);
        AddEntry(7004, 3003, 3004, 0, 25);

        // -------------------------
        // 7005
        // -------------------------
        AddEntry(7005, 3003, 3004, 0, 25);
        AddEntry(7005, 3003, 3002, 0, 12);
        AddEntry(7005, 3004, 3002, 0, 13);
        AddEntry(7005, 3003, 3003, 0, 25);
        AddEntry(7005, 3004, 3004, 0, 25);
    }

    private void AddEntry(
        int groupId,
        int enemy1Id,
        int enemy2Id,
        int enemy3Id,
        int possibility)
    {
        encounterEntries.Add(
            new EncounterEntry(
                groupId,
                enemy1Id,
                enemy2Id,
                enemy3Id,
                possibility
            )
        );
    }

    // =========================================================
    // Public API
    // =========================================================

    /// <summary>
    /// 좌표가 속한 인카운터 그룹 ID.
    /// 어느 범위에도 속하지 않으면 -1.
    /// </summary>
    public int GetEncounterGroupId(Vector2Int room)
    {
        for (int i = 0; i < encounterAreas.Count; i++)
        {
            EncounterArea area = encounterAreas[i];

            if (area != null && area.Contains(room))
                return area.groupId;
        }

        return -1;
    }

    public int GetEncounterGroupId(int x, int y)
    {
        return GetEncounterGroupId(new Vector2Int(x, y));
    }

    public bool HasEncounterGroup(Vector2Int room)
    {
        return GetEncounterGroupId(room) >= 0;
    }

    /// <summary>
    /// 현재 좌표에 맞는 인카운터 조합을 하나 뽑는다.
    /// 성공하면 true + enemyIds.
    /// 해당 좌표에 그룹이 없거나 데이터가 잘못되었으면 false.
    /// </summary>
    public bool TryRollEncounter(
        Vector2Int room,
        out int encounterGroupId,
        out int[] enemyIds)
    {
        encounterGroupId = GetEncounterGroupId(room);
        enemyIds = Array.Empty<int>();

        if (encounterGroupId < 0)
        {
            Debug.LogWarning(
                "[EncounterManager] 현재 좌표에 해당하는 인카운터 그룹이 없습니다. " +
                "좌표: " + room
            );

            return false;
        }

        return TryRollEncounter(
            encounterGroupId,
            out enemyIds
        );
    }

    /// <summary>
    /// 이미 그룹 ID를 알고 있을 때 해당 그룹의 몬스터 조합을 뽑는다.
    /// </summary>
    public bool TryRollEncounter(
        int encounterGroupId,
        out int[] enemyIds)
    {
        enemyIds = Array.Empty<int>();

        List<EncounterEntry> candidates =
            new List<EncounterEntry>();

        int totalWeight = 0;

        for (int i = 0; i < encounterEntries.Count; i++)
        {
            EncounterEntry entry = encounterEntries[i];

            if (entry == null ||
                entry.groupId != encounterGroupId ||
                entry.possibility <= 0)
            {
                continue;
            }

            candidates.Add(entry);
            totalWeight += entry.possibility;
        }

        if (candidates.Count == 0 || totalWeight <= 0)
        {
            Debug.LogError(
                "[EncounterManager] 그룹 " +
                encounterGroupId +
                "의 EncounterMonsterTiles 데이터가 없습니다."
            );

            return false;
        }

        // 기획표의 possibility를 가중치로 사용한다.
        // 현재 표는 각 그룹 합계가 100이지만,
        // 추후 값이 변경되어도 안전하게 동작하도록 totalWeight 기준으로 뽑는다.
        int roll = UnityEngine.Random.Range(0, totalWeight);
        int accumulated = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            EncounterEntry entry = candidates[i];

            accumulated += entry.possibility;

            if (roll < accumulated)
            {
                enemyIds = entry.GetEnemyIds();

                Debug.Log(
                    "[EncounterManager] 인카운터 선택" +
                    " / 그룹: " + encounterGroupId +
                    " / 좌표 조합 적 수: " + enemyIds.Length +
                    " / Enemy IDs: " + string.Join(", ", enemyIds)
                );

                return enemyIds.Length > 0;
            }
        }

        // 정수 누적 방식상 정상적으로는 도달하지 않는다.
        return false;
    }

    // =========================================================
    // Validation
    // =========================================================

    private void ValidateEncounterData()
    {
        int[] groupIds =
        {
            7000,
            7001,
            7002,
            7003,
            7004,
            7005
        };

        for (int g = 0; g < groupIds.Length; g++)
        {
            int groupId = groupIds[g];
            int total = 0;
            int count = 0;

            for (int i = 0; i < encounterEntries.Count; i++)
            {
                EncounterEntry entry = encounterEntries[i];

                if (entry == null ||
                    entry.groupId != groupId)
                {
                    continue;
                }

                count++;
                total += entry.possibility;

                int enemyCount = entry.GetEnemyIds().Length;

                if (enemyCount <= 0 || enemyCount > 3)
                {
                    Debug.LogError(
                        "[EncounterManager] 잘못된 몬스터 수" +
                        " / 그룹: " + groupId +
                        " / 적 수: " + enemyCount
                    );
                }
            }

            if (count == 0)
            {
                Debug.LogError(
                    "[EncounterManager] 그룹 " +
                    groupId +
                    "의 인카운터가 없습니다."
                );
            }

            if (total != 100)
            {
                Debug.LogWarning(
                    "[EncounterManager] 그룹 " +
                    groupId +
                    "의 확률 합계가 100이 아닙니다. 현재: " +
                    total
                );
            }
        }
    }
}
