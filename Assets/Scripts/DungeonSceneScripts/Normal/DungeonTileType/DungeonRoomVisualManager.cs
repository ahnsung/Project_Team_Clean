using UnityEngine;

public class DungeonRoomVisualManager : MonoBehaviour
{
    // =========================================================
    // Direction Visual Set
    // =========================================================

    [System.Serializable]
    public class DirectionVisualSet
    {
        [Header("Root")]
        public GameObject root;

        [Header("Path Visuals")]
        public GameObject door;
        public GameObject oneWay;
        public GameObject lockedDoor;
        public GameObject gimmickDoor;
    }


    // =========================================================
    // Manager
    // =========================================================

    [Header("Manager")]
    [SerializeField]
    private DungeonManager dungeonManager;


    // =========================================================
    // Direction Visuals
    // =========================================================

    [Header("Directions")]

    [SerializeField]
    private DirectionVisualSet up;

    [SerializeField]
    private DirectionVisualSet down;

    [SerializeField]
    private DirectionVisualSet left;

    [SerializeField]
    private DirectionVisualSet right;


    // =========================================================
    // Farming
    // =========================================================

    [Header("Farming")]

    [SerializeField]
    private GameObject farmingRoot;

    [SerializeField]
    private GameObject farmingBefore;

    [SerializeField]
    private GameObject farmingAfter;


    // =========================================================
    // Rest
    // =========================================================

    [Header("Rest")]

    [SerializeField]
    private GameObject restRoot;

    [SerializeField]
    private GameObject restBefore;

    [SerializeField]
    private GameObject restAfter;


    // =========================================================
    // Teleport
    // =========================================================

    [Header("Teleport")]

    [SerializeField]
    private GameObject teleportRoot;

    [SerializeField]
    private GameObject teleportVisual;


    // =========================================================
    // Trap
    // =========================================================

    [Header("Trap")]

    [SerializeField]
    private GameObject trapRoot;

    [SerializeField]
    private GameObject trap01;

    [SerializeField]
    private GameObject trap02;


    // =========================================================
    // Hint
    // =========================================================

    [Header("Hint")]

    [SerializeField]
    private GameObject hintRoot;

    [SerializeField]
    private GameObject hintVisual;


    // =========================================================
    // Chest
    // =========================================================

    [Header("Chest")]

    [SerializeField]
    private GameObject chestRoot;

    [SerializeField]
    private GameObject chestBefore;

    [SerializeField]
    private GameObject chestAfter;


    // =========================================================
    // Key
    // =========================================================

    [Header("Key")]

    [SerializeField]
    private GameObject keyRoot;

    [SerializeField]
    private GameObject keyBefore;

    [SerializeField]
    private GameObject keyAfter;


    // =========================================================
    // Puzzle / Letter
    // =========================================================

    [Header("Puzzle / Letter")]

    [SerializeField]
    private GameObject puzzleRoot;

    [SerializeField]
    private GameObject puzzleVisual;


    // =========================================================
    // Boss
    // =========================================================

    [Header("Boss")]

    [SerializeField]
    private GameObject bossRoot;

    [SerializeField]
    private GameObject bossVisual;


    // =========================================================
    // Runtime
    // =========================================================

    private Vector2Int lastRoom;
    private bool initialized;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();

        if (dungeonManager != null)
        {
            lastRoom =
                dungeonManager.CurrentRoom;

            initialized =
                true;
        }

        RefreshAllVisuals();
    }


    private void Update()
    {
        ResolveReferences();

        if (dungeonManager == null)
            return;


        Vector2Int currentRoom =
            dungeonManager.CurrentRoom;


        /*
         * 방 좌표가 변경된 경우에만 갱신.
         */
        if (
            !initialized ||
            currentRoom != lastRoom
        )
        {
            initialized =
                true;

            lastRoom =
                currentRoom;

            RefreshAllVisuals();
        }
    }


    // =========================================================
    // References
    // =========================================================

    private void ResolveReferences()
    {
        if (dungeonManager == null)
        {
            dungeonManager =
                DungeonManager.Instance;
        }

        if (dungeonManager == null)
        {
            dungeonManager =
                FindFirstObjectByType<DungeonManager>();
        }
    }


    // =========================================================
    // All Refresh
    // =========================================================

    public void RefreshAllVisuals()
    {
        ResolveReferences();

        if (dungeonManager == null)
        {
            Debug.LogWarning(
                "[DungeonRoomVisualManager] DungeonManager가 없습니다."
            );

            HideEverything();

            return;
        }


        RefreshDirections();

        RefreshCurrentTileVisual();


        Debug.Log(
            "[DungeonRoomVisualManager] 갱신 완료\n" +
            $"현재 위치: {dungeonManager.CurrentRoom}\n" +
            $"현재 타일: {dungeonManager.GetCurrentTileType()}"
        );
    }


    // =========================================================
    // Direction Refresh
    // =========================================================

    private void RefreshDirections()
    {
        RefreshDirection(
            MoveDirection.Up,
            up
        );

        RefreshDirection(
            MoveDirection.Down,
            down
        );

        RefreshDirection(
            MoveDirection.Left,
            left
        );

        RefreshDirection(
            MoveDirection.Right,
            right
        );
    }


    private void RefreshDirection(
        MoveDirection direction,
        DirectionVisualSet visual)
    {
        if (visual == null)
            return;


        HideDirection(
            visual
        );


        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );


        if (data == null)
        {
            Debug.Log(
                $"[RoomDirectionVisual] {direction} / MoveData 없음 -> OFF"
            );

            return;
        }


        // =====================================================
        // Open
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Open
        )
        {
            if (
                !dungeonManager.CanMove(
                    direction
                )
            )
            {
                Debug.Log(
                    $"[RoomDirectionVisual] {direction} / Open / 이동 불가 -> OFF"
                );

                return;
            }


            /*
             * 일반 이동 가능한 방향.
             *
             * 현재 별도의 Open 이미지가 없으므로
             * Door 이미지를 기본 이동 방향 표시로 사용한다.
             */
            SetActive(
                visual.root,
                true
            );

            SetActive(
                visual.door,
                true
            );


            Debug.Log(
                $"[RoomDirectionVisual] {direction} / Open / Passable -> NORMAL ON"
            );

            return;
        }


        // =====================================================
        // Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Door
        )
        {
            if (
                !dungeonManager.CanMove(
                    direction
                )
            )
            {
                Debug.Log(
                    $"[RoomDirectionVisual] {direction} / Door / 이동 불가 -> OFF"
                );

                return;
            }


            SetActive(
                visual.root,
                true
            );

            SetActive(
                visual.door,
                true
            );


            Debug.Log(
                $"[RoomDirectionVisual] {direction} / Door -> DOOR ON"
            );

            return;
        }


        // =====================================================
        // OneWay
        // =====================================================

        if (
            data.PathType ==
            MovePathType.OneWay
        )
        {
            /*
             * OneWay는 현재 방향에서 실제로
             * 통과 가능한 경우에만 표시한다.
             */
            if (
                !dungeonManager.CanMove(
                    direction
                )
            )
            {
                Debug.Log(
                    $"[RoomDirectionVisual] {direction} / OneWay / 역방향 또는 이동불가 -> OFF"
                );

                return;
            }


            SetActive(
                visual.root,
                true
            );

            /*
             * 해당 방향에 OneWay 전용 이미지가 있으면 사용.
             *
             * 아직 없는 방향은 Door 이미지로 임시 표시.
             */
            if (visual.oneWay != null)
            {
                SetActive(
                    visual.oneWay,
                    true
                );
            }
            else
            {
                SetActive(
                    visual.door,
                    true
                );
            }


            Debug.Log(
                $"[RoomDirectionVisual] {direction} / OneWay -> ONEWAY ON"
            );

            return;
        }


        // =====================================================
        // Locked Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.LockedDoor
        )
        {
            /*
             * LockedDoor는 잠겨 있어도
             * 문 자체는 화면에 보여야 한다.
             */
            SetActive(
                visual.root,
                true
            );


            if (visual.lockedDoor != null)
            {
                SetActive(
                    visual.lockedDoor,
                    true
                );
            }
            else
            {
                SetActive(
                    visual.door,
                    true
                );
            }


            Debug.Log(
                $"[RoomDirectionVisual] {direction} / LockedDoor -> ON"
            );

            return;
        }


        // =====================================================
        // Gimmick Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.GimmickDoor
        )
        {
            SetActive(
                visual.root,
                true
            );


            if (visual.gimmickDoor != null)
            {
                SetActive(
                    visual.gimmickDoor,
                    true
                );
            }
            else
            {
                SetActive(
                    visual.door,
                    true
                );
            }


            Debug.Log(
                $"[RoomDirectionVisual] {direction} / GimmickDoor -> ON"
            );

            return;
        }


        // =====================================================
        // Wall / Unknown
        // =====================================================

        Debug.Log(
            $"[RoomDirectionVisual] {direction} / {data.PathType} -> OFF"
        );
    }


    // =========================================================
    // Current Tile Visual
    // =========================================================

    private void RefreshCurrentTileVisual()
    {
        /*
         * 이전 방 이벤트 이미지 제거
         */
        HideTileVisuals();


        DungeonTileType tileType =
            dungeonManager.GetCurrentTileType();


        // =====================================================
        // Farming
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Farming
        )
        {
            SetActive(
                farmingRoot,
                true
            );

            SetActive(
                farmingBefore,
                true
            );

            SetActive(
                farmingAfter,
                false
            );


            Debug.Log(
                "[RoomTileVisual] Farming ON"
            );

            return;
        }


        // =====================================================
        // Rest
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Rest
        )
        {
            SetActive(
                restRoot,
                true
            );

            SetActive(
                restBefore,
                true
            );

            SetActive(
                restAfter,
                false
            );


            Debug.Log(
                "[RoomTileVisual] Rest ON"
            );

            return;
        }


        // =====================================================
        // Teleport
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Teleport
        )
        {
            SetActive(
                teleportRoot,
                true
            );

            SetActive(
                teleportVisual,
                true
            );


            Debug.Log(
                "[RoomTileVisual] Teleport ON"
            );

            return;
        }


        // =====================================================
        // Trap
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Trap
        )
        {
            SetActive(
                trapRoot,
                true
            );

            /*
             * 현재는 Trap_Data 세부 타입을
             * 아직 연결하지 않았기 때문에
             * 첫 번째 Trap 이미지를 기본 표시.
             */
            if (trap01 != null)
            {
                SetActive(
                    trap01,
                    true
                );
            }
            else
            {
                SetActive(
                    trap02,
                    true
                );
            }


            Debug.Log(
                "[RoomTileVisual] Trap ON"
            );

            return;
        }


        // =====================================================
        // Event / Hint
        // =====================================================

        if (
            tileType ==
            DungeonTileType.EventHint
        )
        {
            SetActive(
                hintRoot,
                true
            );

            SetActive(
                hintVisual,
                true
            );


            Debug.Log(
                "[RoomTileVisual] EventHint ON"
            );

            return;
        }


        // =====================================================
        // Chest
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Chest
        )
        {
            SetActive(
                chestRoot,
                true
            );

            SetActive(
                chestBefore,
                true
            );

            SetActive(
                chestAfter,
                false
            );


            Debug.Log(
                "[RoomTileVisual] Chest ON"
            );

            return;
        }


        // =====================================================
        // Key
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Key
        )
        {
            SetActive(
                keyRoot,
                true
            );

            SetActive(
                keyBefore,
                true
            );

            SetActive(
                keyAfter,
                false
            );


            Debug.Log(
                "[RoomTileVisual] Key ON"
            );

            return;
        }


        // =====================================================
        // Puzzle / Letter
        // =====================================================

        if (
            tileType ==
            DungeonTileType.PuzzleLetter
        )
        {
            SetActive(
                puzzleRoot,
                true
            );

            SetActive(
                puzzleVisual,
                true
            );


            Debug.Log(
                "[RoomTileVisual] PuzzleLetter ON"
            );

            return;
        }


        // =====================================================
        // Boss
        // =====================================================

        if (
            tileType ==
            DungeonTileType.Boss
        )
        {
            SetActive(
                bossRoot,
                true
            );

            SetActive(
                bossVisual,
                true
            );


            Debug.Log(
                "[RoomTileVisual] Boss ON"
            );

            return;
        }


        Debug.Log(
            $"[RoomTileVisual] {tileType} / 별도 이벤트 이미지 없음"
        );
    }


    // =========================================================
    // Hide Direction
    // =========================================================

    private void HideDirection(
        DirectionVisualSet visual)
    {
        if (visual == null)
            return;


        SetActive(
            visual.door,
            false
        );

        SetActive(
            visual.oneWay,
            false
        );

        SetActive(
            visual.lockedDoor,
            false
        );

        SetActive(
            visual.gimmickDoor,
            false
        );

        SetActive(
            visual.root,
            false
        );
    }


    // =========================================================
    // Hide Tile Visuals
    // =========================================================

    private void HideTileVisuals()
    {
        // Farming
        SetActive(
            farmingBefore,
            false
        );

        SetActive(
            farmingAfter,
            false
        );

        SetActive(
            farmingRoot,
            false
        );


        // Rest
        SetActive(
            restBefore,
            false
        );

        SetActive(
            restAfter,
            false
        );

        SetActive(
            restRoot,
            false
        );


        // Teleport
        SetActive(
            teleportVisual,
            false
        );

        SetActive(
            teleportRoot,
            false
        );


        // Trap
        SetActive(
            trap01,
            false
        );

        SetActive(
            trap02,
            false
        );

        SetActive(
            trapRoot,
            false
        );


        // Hint
        SetActive(
            hintVisual,
            false
        );

        SetActive(
            hintRoot,
            false
        );


        // Chest
        SetActive(
            chestBefore,
            false
        );

        SetActive(
            chestAfter,
            false
        );

        SetActive(
            chestRoot,
            false
        );


        // Key
        SetActive(
            keyBefore,
            false
        );

        SetActive(
            keyAfter,
            false
        );

        SetActive(
            keyRoot,
            false
        );


        // Puzzle
        SetActive(
            puzzleVisual,
            false
        );

        SetActive(
            puzzleRoot,
            false
        );


        // Boss
        SetActive(
            bossVisual,
            false
        );

        SetActive(
            bossRoot,
            false
        );
    }


    // =========================================================
    // Hide Everything
    // =========================================================

    private void HideEverything()
    {
        HideDirection(
            up
        );

        HideDirection(
            down
        );

        HideDirection(
            left
        );

        HideDirection(
            right
        );


        HideTileVisuals();
    }


    // =========================================================
    // Helper
    // =========================================================

    private void SetActive(
        GameObject target,
        bool active)
    {
        if (target == null)
            return;


        if (
            target.activeSelf !=
            active
        )
        {
            target.SetActive(
                active
            );
        }
    }
}