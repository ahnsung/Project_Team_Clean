using UnityEngine;

public class DungeonMoveDebugTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private DungeonManager dungeonManager;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
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
    }


    // =========================================================
    // 현재 위치
    // =========================================================

    [ContextMenu("DEBUG/현재 위치 출력")]
    private void DebugCurrentPosition()
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonMoveDebugTester] " +
                "DungeonManager를 찾을 수 없습니다."
            );

            return;
        }

        Debug.Log(
            "============================\n" +
            "[이동 테스트]\n" +
            "현재 위치: " +
            dungeonManager.CurrentRoom +
            "\n============================"
        );
    }


    // =========================================================
    // 현재 방 이동 데이터 전체 확인
    // =========================================================

    [ContextMenu("DEBUG/현재 방 상하좌우 통로 확인")]
    private void DebugCurrentRoomPaths()
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonMoveDebugTester] " +
                "DungeonManager를 찾을 수 없습니다."
            );

            return;
        }

        Debug.Log(
            "====================================\n" +
            "[현재 방 통로 검사]\n" +
            "현재 위치: " +
            dungeonManager.CurrentRoom +
            "\n===================================="
        );

        PrintDirection(
            MoveDirection.Up
        );

        PrintDirection(
            MoveDirection.Down
        );

        PrintDirection(
            MoveDirection.Left
        );

        PrintDirection(
            MoveDirection.Right
        );
    }


    private void PrintDirection(
        MoveDirection direction)
    {
        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );

        if (data == null)
        {
            Debug.Log(
                "[통로] " +
                direction +
                " → 데이터 없음"
            );

            return;
        }

        bool canMove =
            dungeonManager.CanMove(
                direction
            );

        bool canSpecial =
            dungeonManager
                .CanUseSpecialPath(
                    direction
                );

        string openedText =
            "-";

        if (
            data.PathType ==
            MovePathType.LockedDoor)
        {
            if (
                LockedDoorManager.Instance ==
                null)
            {
                openedText =
                    "LockedDoorManager 없음";
            }
            else
            {
                bool opened =
                    LockedDoorManager.Instance
                        .IsOpened(
                            dungeonManager.CurrentRoom,
                            direction
                        );

                openedText =
                    opened
                        ? "OPEN"
                        : "LOCKED";
            }
        }

        Debug.Log(
            "[통로]\n" +
            "방향: " +
            direction +
            "\n" +
            "Type: " +
            data.PathType +
            "\n" +
            "Passable: " +
            data.Passable +
            "\n" +
            "CanMove: " +
            canMove +
            "\n" +
            "CanUseSpecialPath: " +
            canSpecial +
            "\n" +
            "LockedDoor 상태: " +
            openedText
        );
    }


    // =========================================================
    // LockedDoor 찾기
    // =========================================================

    [ContextMenu("DEBUG/현재 방 LockedDoor 찾기")]
    private void DebugFindLockedDoor()
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[DungeonMoveDebugTester] " +
                "DungeonManager가 없습니다."
            );

            return;
        }

        bool found = false;

        found |=
            CheckLockedDoor(
                MoveDirection.Up
            );

        found |=
            CheckLockedDoor(
                MoveDirection.Down
            );

        found |=
            CheckLockedDoor(
                MoveDirection.Left
            );

        found |=
            CheckLockedDoor(
                MoveDirection.Right
            );

        if (!found)
        {
            Debug.Log(
                "[LockedDoor 테스트]\n" +
                "현재 위치 " +
                dungeonManager.CurrentRoom +
                " 주변에는 LockedDoor가 없습니다."
            );
        }
    }


    private bool CheckLockedDoor(
        MoveDirection direction)
    {
        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );

        if (data == null)
            return false;

        if (
            data.PathType !=
            MovePathType.LockedDoor)
        {
            return false;
        }

        bool opened = false;

        if (
            LockedDoorManager.Instance !=
            null)
        {
            opened =
                LockedDoorManager.Instance
                    .IsOpened(
                        dungeonManager.CurrentRoom,
                        direction
                    );
        }

        Debug.Log(
            "============================\n" +
            "[LockedDoor 발견]\n" +
            "현재 위치: " +
            dungeonManager.CurrentRoom +
            "\n" +
            "방향: " +
            direction +
            "\n" +
            "Passable: " +
            data.Passable +
            "\n" +
            "현재 상태: " +
            (
                opened
                    ? "OPEN"
                    : "LOCKED"
            ) +
            "\n============================"
        );

        return true;
    }


    // =========================================================
    // LockedDoor 직접 열기 테스트
    //
    // 실제 열쇠 소비 로직까지 그대로 사용한다.
    // 강제로 문을 열어버리는 테스트가 아님.
    // =========================================================

    [ContextMenu("DEBUG/LockedDoor UP 열기 시도")]
    private void DebugOpenLockedDoorUp()
    {
        TryOpenLockedDoor(
            MoveDirection.Up
        );
    }


    [ContextMenu("DEBUG/LockedDoor DOWN 열기 시도")]
    private void DebugOpenLockedDoorDown()
    {
        TryOpenLockedDoor(
            MoveDirection.Down
        );
    }


    [ContextMenu("DEBUG/LockedDoor LEFT 열기 시도")]
    private void DebugOpenLockedDoorLeft()
    {
        TryOpenLockedDoor(
            MoveDirection.Left
        );
    }


    [ContextMenu("DEBUG/LockedDoor RIGHT 열기 시도")]
    private void DebugOpenLockedDoorRight()
    {
        TryOpenLockedDoor(
            MoveDirection.Right
        );
    }


    private void TryOpenLockedDoor(
        MoveDirection direction)
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[LockedDoor 테스트] " +
                "DungeonManager가 없습니다."
            );

            return;
        }

        if (
            LockedDoorManager.Instance ==
            null)
        {
            Debug.LogError(
                "[LockedDoor 테스트] " +
                "LockedDoorManager가 없습니다."
            );

            return;
        }

        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );

        if (data == null)
        {
            Debug.Log(
                "[LockedDoor 테스트]\n" +
                direction +
                " 방향에 MoveData가 없습니다."
            );

            return;
        }

        if (
            data.PathType !=
            MovePathType.LockedDoor)
        {
            Debug.Log(
                "[LockedDoor 테스트]\n" +
                direction +
                " 방향은 LockedDoor가 아닙니다.\n" +
                "현재 Type: " +
                data.PathType
            );

            return;
        }

        bool alreadyOpened =
            LockedDoorManager.Instance
                .IsOpened(
                    dungeonManager.CurrentRoom,
                    direction
                );

        if (alreadyOpened)
        {
            Debug.Log(
                "[LockedDoor 테스트]\n" +
                "이미 열린 문입니다.\n" +
                "현재 위치: " +
                dungeonManager.CurrentRoom +
                "\n방향: " +
                direction
            );

            return;
        }

        Debug.Log(
            "[LockedDoor 테스트]\n" +
            "문 열기 시도\n" +
            "현재 위치: " +
            dungeonManager.CurrentRoom +
            "\n방향: " +
            direction
        );

        bool success =
            LockedDoorManager.Instance
                .TryOpenDoor(
                    dungeonManager.CurrentRoom,
                    direction
                );

        Debug.Log(
            "============================\n" +
            "[LockedDoor 결과]\n" +
            "성공 여부: " +
            success +
            "\n" +
            "현재 상태: " +
            (
                LockedDoorManager.Instance
                    .IsOpened(
                        dungeonManager.CurrentRoom,
                        direction
                    )
                    ? "OPEN"
                    : "LOCKED"
            ) +
            "\n============================"
        );

        dungeonManager.RefreshAll();
    }


    // =========================================================
    // 열린 LockedDoor 이동 테스트
    // =========================================================

    [ContextMenu("DEBUG/이동 UP")]
    private void DebugMoveUp()
    {
        TryMove(
            MoveDirection.Up
        );
    }


    [ContextMenu("DEBUG/이동 DOWN")]
    private void DebugMoveDown()
    {
        TryMove(
            MoveDirection.Down
        );
    }


    [ContextMenu("DEBUG/이동 LEFT")]
    private void DebugMoveLeft()
    {
        TryMove(
            MoveDirection.Left
        );
    }


    [ContextMenu("DEBUG/이동 RIGHT")]
    private void DebugMoveRight()
    {
        TryMove(
            MoveDirection.Right
        );
    }


    private void TryMove(
        MoveDirection direction)
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[이동 테스트] " +
                "DungeonManager가 없습니다."
            );

            return;
        }

        Vector2Int before =
            dungeonManager.CurrentRoom;

        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );

        if (data == null)
        {
            Debug.Log(
                "[이동 테스트]\n" +
                direction +
                " 방향 MoveData 없음"
            );

            return;
        }

        Debug.Log(
            "[이동 테스트 시작]\n" +
            "현재 위치: " +
            before +
            "\n방향: " +
            direction +
            "\nType: " +
            data.PathType +
            "\nPassable: " +
            data.Passable
        );

        bool moved =
            dungeonManager.MoveToNextRoom(
                direction
            );

        Debug.Log(
            "============================\n" +
            "[이동 테스트 결과]\n" +
            "성공: " +
            moved +
            "\n" +
            "이전: " +
            before +
            "\n" +
            "현재: " +
            dungeonManager.CurrentRoom +
            "\n============================"
        );
    }


    // =========================================================
    // Teleport 테스트
    //
    // 목적지를 직접 입력해서 DungeonManager의
    // TeleportToRoom 자체가 정상인지 검사.
    //
    // 실제 Teleport 데이터 연결 테스트는
    // 실제 Teleport 타일을 밟아서 별도로 확인.
    // =========================================================

    [Header("Teleport Debug")]
    [SerializeField]
    private Vector2Int teleportTestDestination =
        new Vector2Int(5, 5);


    [ContextMenu("DEBUG/Teleport 목적지로 이동")]
    private void DebugTeleport()
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogError(
                "[Teleport 테스트] " +
                "DungeonManager가 없습니다."
            );

            return;
        }

        Vector2Int before =
            dungeonManager.CurrentRoom;

        Debug.Log(
            "[Teleport 테스트 시작]\n" +
            "출발: " +
            before +
            "\n" +
            "테스트 목적지: " +
            teleportTestDestination
        );

        bool moved =
            dungeonManager.TeleportToRoom(
                teleportTestDestination
            );

        Debug.Log(
            "============================\n" +
            "[Teleport 테스트 결과]\n" +
            "성공: " +
            moved +
            "\n" +
            "출발: " +
            before +
            "\n" +
            "현재 위치: " +
            dungeonManager.CurrentRoom +
            "\n============================"
        );
    }
}