using System.Collections.Generic;
using UnityEngine;

public class RestTileManager : MonoBehaviour
{
    public static RestTileManager Instance { get; private set; }

    /*
     * 이번 던전 입장 동안 사용한 Rest 좌표.
     *
     * 기획서:
     * 같은 Rest는 던전을 나갔다 다시 들어와야 재사용 가능.
     *
     * 따라서 영구 세이브 데이터가 아니라
     * 현재 DungeonScene 입장 동안만 유지한다.
     */
    private readonly HashSet<Vector2Int> usedRestTiles =
        new HashSet<Vector2Int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanRest(Vector2Int position)
    {
        return !usedRestTiles.Contains(position);
    }

    public bool Rest(Vector2Int position)
    {
        if (usedRestTiles.Contains(position))
        {
            Debug.Log(
                $"[RestTileManager] 이미 휴식한 장소입니다: {position}"
            );

            return false;
        }

        PlayerResourceManager resource =
            PlayerResourceManager.Instance;

        if (resource == null)
        {
            Debug.LogError(
                "[RestTileManager] PlayerResourceManager가 없습니다."
            );

            return false;
        }

        /*
         * 기획서 기준:
         *
         * 체력    최대치의 30%
         * 정신력  최대치의 20%
         * 배고픔  최대치의 20%
         */
        int healthRecovery =
            Mathf.RoundToInt(
                resource.MaxHealth * 0.30f
            );

        int mentalRecovery =
            Mathf.RoundToInt(
                resource.MaxMental * 0.20f
            );

        int hungerRecovery =
            Mathf.RoundToInt(
                resource.MaxHunger * 0.20f
            );

        resource.ChangeHealth(
            healthRecovery,
            "휴식"
        );

        resource.ChangeMental(
            mentalRecovery,
            "휴식"
        );

        resource.ChangeHunger(
            hungerRecovery,
            "휴식"
        );

        usedRestTiles.Add(position);

        Debug.Log(
            "[RestTileManager] 휴식 완료\n" +
            $"좌표: {position}\n" +
            $"체력 회복 요청: +{healthRecovery}\n" +
            $"정신력 회복 요청: +{mentalRecovery}\n" +
            $"배고픔 회복 요청: +{hungerRecovery}"
        );

        return true;
    }

    public void ResetRun()
    {
        usedRestTiles.Clear();

        Debug.Log(
            "[RestTileManager] 던전 입장 단위 Rest 기록 초기화"
        );
    }
}