using System.Collections.Generic;
using UnityEngine;

public class LockedDoorTestHelper : MonoBehaviour
{
    // =========================================================
    // TEST 1
    // LockedDoor 앞 이동
    // =========================================================

    [ContextMenu(
        "TEST 1 - LockedDoor 앞으로 이동"
    )]
    public void MoveToLockedDoorTest()
    {
        DungeonManager dungeon =
            DungeonManager.Instance;


        if (dungeon == null)
        {
            Debug.LogError(
                "[LockedDoorTest] " +
                "DungeonManager가 없습니다."
            );

            return;
        }


        /*
         * 첫 테스트 문:
         *
         * (6,33) <-> (6,32)
         *
         * 현재 위치는 (6,33)으로 강제.
         */


        Vector2Int destination =
            new Vector2Int(
                6,
                33
            );


        List<string> visited =
            dungeon
                .GetVisitedRoomsForSave();


        dungeon.RestoreDungeonState(
            destination,
            dungeon.CurrentTurn,
            dungeon.CurrentEnvironment,
            visited
        );


        dungeon.LogCurrentTile();


        Debug.Log(
            "[LockedDoorTest] " +
            "LockedDoor 테스트 위치 이동 완료\n" +
            "현재 위치: (6,33)\n" +
            "이제 Space를 누르세요."
        );


        PrintCurrentDirections();
    }


    // =========================================================
    // TEST 2
    // K1 지급
    // =========================================================

    [ContextMenu(
        "TEST 2 - K1 열쇠 지급"
    )]
    public void GiveK1()
    {
        InventoryManager inventory =
            InventoryManager.Instance;


        if (inventory == null)
        {
            Debug.LogError(
                "[LockedDoorTest] " +
                "InventoryManager가 없습니다."
            );

            return;
        }


        bool added =
            inventory.AddItem(
                3001
            );


        if (!added)
        {
            Debug.LogWarning(
                "[LockedDoorTest] " +
                "K_1 지급 실패.\n" +
                "인벤토리 공간 또는 ItemDatabase를 확인하세요."
            );

            return;
        }


        Debug.Log(
            "[LockedDoorTest] K_1 지급 성공\n" +
            "Item ID: 3001"
        );


        if (SaveManager.Instance != null)
        {
            SaveManager.Instance
                .SaveGameplayData();
        }
    }


    // =========================================================
    // TEST 3
    // LockedDoor Reset
    // =========================================================

    [ContextMenu(
        "TEST 3 - 열린 LockedDoor 초기화"
    )]
    public void ResetLockedDoors()
    {
        if (
            LockedDoorManager.Instance ==
            null
        )
        {
            Debug.LogError(
                "[LockedDoorTest] " +
                "LockedDoorManager가 없습니다."
            );

            return;
        }


        LockedDoorManager.Instance
            .ClearOpenedDoors();


        Debug.Log(
            "[LockedDoorTest] " +
            "LockedDoor 상태 초기화 완료"
        );
    }


    // =========================================================
    // TEST 4
    // K1 개수 확인
    // =========================================================

    [ContextMenu(
        "TEST 4 - K1 보유 확인"
    )]
    public void CheckK1()
    {
        InventoryManager inventory =
            InventoryManager.Instance;


        if (
            inventory == null ||
            inventory.items == null
        )
        {
            Debug.LogError(
                "[LockedDoorTest] " +
                "InventoryManager가 없습니다."
            );

            return;
        }


        int count =
            0;


        foreach (
            InventoryItem item
            in inventory.items
        )
        {
            if (
                item == null ||
                item.data == null
            )
            {
                continue;
            }


            if (item.data.id == 3001)
            {
                count++;
            }
        }


        Debug.Log(
            "[LockedDoorTest] " +
            $"현재 K_1 보유 개수: {count}"
        );
    }


    // =========================================================
    // DEBUG Direction
    // =========================================================

    [ContextMenu(
        "DEBUG - 현재 위치 이동 데이터 출력"
    )]
    public void PrintCurrentDirections()
    {
        DungeonManager dungeon =
            DungeonManager.Instance;


        if (dungeon == null)
        {
            return;
        }


        Debug.Log(
            "========== CURRENT MOVE DATA ==========\n" +
            $"현재 위치: {dungeon.CurrentRoom}"
        );


        PrintDirection(
            dungeon,
            MoveDirection.Up
        );


        PrintDirection(
            dungeon,
            MoveDirection.Down
        );


        PrintDirection(
            dungeon,
            MoveDirection.Left
        );


        PrintDirection(
            dungeon,
            MoveDirection.Right
        );
    }


    private void PrintDirection(
        DungeonManager dungeon,
        MoveDirection direction)
    {
        MoveData data =
            dungeon.GetMoveData(
                direction
            );


        if (data == null)
        {
            Debug.Log(
                $"{direction}: 데이터 없음"
            );

            return;
        }


        Vector2Int destination =
            MoveDataLoader.Instance != null
                ? MoveDataLoader.Instance
                    .GetDestination(
                        dungeon.CurrentRoom,
                        direction
                    )
                : dungeon.CurrentRoom;


        Debug.Log(
            $"{direction}\n" +
            $"Type: {data.PathType}\n" +
            $"Passable: {data.Passable}\n" +
            $"Destination: {destination}"
        );
    }
}