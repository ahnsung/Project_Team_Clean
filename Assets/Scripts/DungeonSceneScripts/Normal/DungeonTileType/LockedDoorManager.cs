using System.Collections.Generic;
using UnityEngine;

public class LockedDoorManager : MonoBehaviour
{
    public static LockedDoorManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Temporary Door Definition
    // =========================================================

    private class LockedDoorDefinition
    {
        public Vector2Int roomA;
        public Vector2Int roomB;

        public string keyID;
        public int requiredItemID;


        public LockedDoorDefinition(
            Vector2Int roomA,
            Vector2Int roomB,
            string keyID,
            int requiredItemID)
        {
            this.roomA = roomA;
            this.roomB = roomB;
            this.keyID = keyID;
            this.requiredItemID = requiredItemID;
        }
    }


    private readonly List<LockedDoorDefinition>
        doorDefinitions =
            new List<LockedDoorDefinition>();


    private readonly HashSet<string>
        openedDoors =
            new HashSet<string>();


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


        BuildTemporaryDefinitions();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // Temporary Mapping
    // =========================================================

    private void BuildTemporaryDefinitions()
    {
        doorDefinitions.Clear();


        /*
         * 현재 Move Data에서 발견된
         * LockedDoor 5쌍.
         *
         * Key 매핑은 기획 데이터가 나오기 전
         * TEST용 임시 설정.
         */


        AddDefinition(
            new Vector2Int(6, 33),
            new Vector2Int(6, 32),
            "K_1",
            3001
        );


        AddDefinition(
            new Vector2Int(14, 30),
            new Vector2Int(14, 31),
            "K_2",
            3002
        );


        AddDefinition(
            new Vector2Int(16, 29),
            new Vector2Int(17, 29),
            "K_3",
            3003
        );


        AddDefinition(
            new Vector2Int(22, 23),
            new Vector2Int(23, 23),
            "K_4",
            3004
        );


        /*
         * 열쇠가 현재 4종뿐이라
         * 마지막 문은 테스트용으로 K_1 재사용.
         */
        AddDefinition(
            new Vector2Int(30, 29),
            new Vector2Int(31, 29),
            "K_1",
            3001
        );


        Debug.Log(
            "[LockedDoorManager] " +
            "테스트 LockedDoor 정의 완료: " +
            doorDefinitions.Count +
            "쌍"
        );
    }


    private void AddDefinition(
        Vector2Int roomA,
        Vector2Int roomB,
        string keyID,
        int requiredItemID)
    {
        doorDefinitions.Add(
            new LockedDoorDefinition(
                roomA,
                roomB,
                keyID,
                requiredItemID
            )
        );
    }


    // =========================================================
    // Is Open
    // =========================================================

    public bool IsOpened(
        Vector2Int currentRoom,
        MoveDirection direction)
    {
        Vector2Int destination =
            GetDestination(
                currentRoom,
                direction
            );


        return IsDoorOpened(
            currentRoom,
            destination
        );
    }


    public bool IsDoorOpened(
        Vector2Int roomA,
        Vector2Int roomB)
    {
        string doorKey =
            MakeDoorKey(
                roomA,
                roomB
            );


        return
            openedDoors.Contains(
                doorKey
            );
    }


    // =========================================================
    // Can Open
    // =========================================================

    /*
     * 잠긴 문을 "현재 열 수 있는지" 확인만 한다.
     *
     * 중요:
     * 이 함수에서는 열쇠를 소비하지 않는다.
     *
     * Space 방향 선택 UI에서
     * 파랑 / 빨강 상태를 결정할 때 사용한다.
     *
     * 이미 열린 문
     * → true
     *
     * 닫힌 문 + 필요한 열쇠 보유
     * → true
     *
     * 닫힌 문 + 열쇠 없음
     * → false
     */
    public bool CanOpenDoor(
        Vector2Int currentRoom,
        MoveDirection direction)
    {
        Vector2Int destination =
            GetDestination(
                currentRoom,
                direction
            );


        LockedDoorDefinition definition =
            FindDefinition(
                currentRoom,
                destination
            );


        // =====================================================
        // Definition Check
        // =====================================================

        if (definition == null)
        {
            Debug.LogWarning(
                "[LockedDoor] 등록되지 않은 잠금문입니다.\n" +
                $"현재 위치: {currentRoom}\n" +
                $"방향: {direction}\n" +
                $"계산된 목적지: {destination}"
            );

            return false;
        }


        string doorKey =
            MakeDoorKey(
                definition.roomA,
                definition.roomB
            );


        // =====================================================
        // Already Open
        // =====================================================

        if (
            openedDoors.Contains(
                doorKey
            )
        )
        {
            return true;
        }


        // =====================================================
        // Inventory
        // =====================================================

        InventoryManager inventory =
            InventoryManager.Instance;


        if (inventory == null)
        {
            return false;
        }


        // =====================================================
        // Key Check Only
        // =====================================================

        InventoryItem keyItem =
            FindKeyItem(
                inventory,
                definition.requiredItemID
            );


        return
            keyItem != null;
    }


    // =========================================================
    // Try Open
    // =========================================================

    public bool TryOpenDoor(
        Vector2Int currentRoom,
        MoveDirection direction)
    {
        Vector2Int destination =
            GetDestination(
                currentRoom,
                direction
            );


        LockedDoorDefinition definition =
            FindDefinition(
                currentRoom,
                destination
            );


        if (definition == null)
        {
            Debug.LogWarning(
                "[LockedDoor] 등록되지 않은 잠금문입니다.\n" +
                $"현재 위치: {currentRoom}\n" +
                $"방향: {direction}\n" +
                $"계산된 목적지: {destination}"
            );

            return false;
        }


        string doorKey =
            MakeDoorKey(
                definition.roomA,
                definition.roomB
            );


        // =====================================================
        // Already Open
        // =====================================================

        if (
            openedDoors.Contains(
                doorKey
            )
        )
        {
            Debug.Log(
                "[LockedDoor] 이미 열린 문입니다.\n" +
                $"{definition.roomA} <-> " +
                $"{definition.roomB}"
            );

            return true;
        }


        // =====================================================
        // Inventory
        // =====================================================

        InventoryManager inventory =
            InventoryManager.Instance;


        if (inventory == null)
        {
            Debug.LogError(
                "[LockedDoor] " +
                "InventoryManager가 없습니다."
            );

            return false;
        }


        InventoryItem keyItem =
            FindKeyItem(
                inventory,
                definition.requiredItemID
            );


        // =====================================================
        // No Key
        // =====================================================

        if (keyItem == null)
        {
            Debug.Log(
                "[LockedDoor] 문이 잠겨 있습니다.\n" +
                $"필요 열쇠: {definition.keyID}\n" +
                $"Item ID: {definition.requiredItemID}"
            );

            return false;
        }


        // =====================================================
        // Consume Key
        // =====================================================

        inventory.RemoveItem(
            keyItem
        );


        // =====================================================
        // Permanently Open
        // =====================================================

        openedDoors.Add(
            doorKey
        );


        Debug.Log(
            "[LockedDoor] 잠긴 문 개방 성공!\n" +
            $"문: {definition.roomA} <-> " +
            $"{definition.roomB}\n" +
            $"소비한 열쇠: {definition.keyID}\n" +
            $"Item ID: {definition.requiredItemID}"
        );


        if (SaveManager.Instance != null)
        {
            SaveManager.Instance
                .SaveGameplayData();
        }


        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .RefreshAll();
        }


        return true;
    }


    // =========================================================
    // Find Key
    // =========================================================

    private InventoryItem FindKeyItem(
        InventoryManager inventory,
        int requiredItemID)
    {
        if (
            inventory == null ||
            inventory.items == null
        )
        {
            return null;
        }


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


            if (
                item.data.id ==
                requiredItemID
            )
            {
                return item;
            }
        }


        return null;
    }


    // =========================================================
    // Definition
    // =========================================================

    private LockedDoorDefinition FindDefinition(
        Vector2Int roomA,
        Vector2Int roomB)
    {
        foreach (
            LockedDoorDefinition definition
            in doorDefinitions
        )
        {
            bool forward =
                definition.roomA == roomA &&
                definition.roomB == roomB;


            bool backward =
                definition.roomA == roomB &&
                definition.roomB == roomA;


            if (
                forward ||
                backward
            )
            {
                return definition;
            }
        }


        return null;
    }


    // =========================================================
    // Destination
    // =========================================================

    private Vector2Int GetDestination(
        Vector2Int currentRoom,
        MoveDirection direction)
    {
        /*
         * 현재 프로젝트 좌표계:
         *
         * Up    = Y - 1
         * Down  = Y + 1
         * Left  = X - 1
         * Right = X + 1
         */

        if (MoveDataLoader.Instance != null)
        {
            return
                MoveDataLoader.Instance
                    .GetDestination(
                        currentRoom,
                        direction
                    );
        }


        // Loader가 없을 때만 fallback.
        switch (direction)
        {
            case MoveDirection.Up:

                return new Vector2Int(
                    currentRoom.x,
                    currentRoom.y - 1
                );


            case MoveDirection.Down:

                return new Vector2Int(
                    currentRoom.x,
                    currentRoom.y + 1
                );


            case MoveDirection.Left:

                return new Vector2Int(
                    currentRoom.x - 1,
                    currentRoom.y
                );


            case MoveDirection.Right:

                return new Vector2Int(
                    currentRoom.x + 1,
                    currentRoom.y
                );
        }


        return currentRoom;
    }


    // =========================================================
    // Door Key
    // =========================================================

    private string MakeDoorKey(
        Vector2Int roomA,
        Vector2Int roomB)
    {
        bool firstA =
            roomA.x < roomB.x ||
            (
                roomA.x == roomB.x &&
                roomA.y <= roomB.y
            );


        Vector2Int first =
            firstA
                ? roomA
                : roomB;


        Vector2Int second =
            firstA
                ? roomB
                : roomA;


        return
            first.x +
            "," +
            first.y +
            "|" +
            second.x +
            "," +
            second.y;
    }


    // =========================================================
    // Save API
    // =========================================================

    public List<string>
        GetOpenedDoorsForSave()
    {
        return
            new List<string>(
                openedDoors
            );
    }


    public void RestoreOpenedDoors(
        List<string> savedDoors)
    {
        openedDoors.Clear();


        if (savedDoors == null)
        {
            return;
        }


        foreach (
            string value
            in savedDoors
        )
        {
            if (
                !string.IsNullOrWhiteSpace(
                    value
                )
            )
            {
                openedDoors.Add(
                    value
                );
            }
        }


        Debug.Log(
            "[LockedDoorManager] 열린 문 복원: " +
            openedDoors.Count +
            "개"
        );
    }


    public void ClearOpenedDoors()
    {
        openedDoors.Clear();


        Debug.Log(
            "[LockedDoorManager] " +
            "열린 문 상태 초기화"
        );
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu(
        "DEBUG - 열린 LockedDoor 초기화"
    )]
    private void DebugClearDoors()
    {
        ClearOpenedDoors();
    }


    [ContextMenu(
        "DEBUG - 테스트 문 목록 출력"
    )]
    private void DebugDefinitions()
    {
        Debug.Log(
            "========== LOCKED DOOR TEST =========="
        );


        foreach (
            LockedDoorDefinition definition
            in doorDefinitions
        )
        {
            Debug.Log(
                $"{definition.roomA} <-> " +
                $"{definition.roomB} / " +
                $"{definition.keyID} / " +
                $"ItemID {definition.requiredItemID}"
            );
        }
    }
}