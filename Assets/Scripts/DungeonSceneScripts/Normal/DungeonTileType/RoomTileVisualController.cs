using UnityEngine;

public class RoomTileVisualController : MonoBehaviour
{
    // =========================================================
    // References
    // =========================================================

    [Header("References")]

    [SerializeField]
    private DungeonManager dungeonManager;


    // =========================================================
    // Farming
    // =========================================================

    [Header("Farming")]

    [SerializeField]
    private GameObject farmingBefore;

    [SerializeField]
    private GameObject farmingAfter;


    // =========================================================
    // Rest
    // =========================================================

    [Header("Rest")]

    [SerializeField]
    private GameObject restBefore;

    [SerializeField]
    private GameObject restAfter;


    // =========================================================
    // Teleport
    // =========================================================

    [Header("Teleport")]

    [SerializeField]
    private GameObject teleportVisual;


    // =========================================================
    // Trap
    // =========================================================

    [Header("Trap")]

    [Tooltip("TrapType 1")]
    [SerializeField]
    private GameObject trap01Visual;

    [Tooltip("TrapType 2")]
    [SerializeField]
    private GameObject trap02Visual;


    // =========================================================
    // Hint
    // =========================================================

    [Header("Event / Hint")]

    [SerializeField]
    private GameObject hintVisual;


    // =========================================================
    // Chest
    // =========================================================

    [Header("Chest")]

    [SerializeField]
    private GameObject chestBefore;

    [SerializeField]
    private GameObject chestAfter;


    // =========================================================
    // Key
    // =========================================================

    [Header("Key")]

    [SerializeField]
    private GameObject keyBefore;

    [SerializeField]
    private GameObject keyAfter;


    // =========================================================
    // Puzzle / Letter
    // =========================================================

    [Header("Puzzle / Letter")]

    [SerializeField]
    private GameObject puzzleLetterVisual;


    // =========================================================
    // Boss
    // =========================================================

    [Header("Boss")]

    [SerializeField]
    private GameObject bossVisual;


    // =========================================================
    // Debug
    // =========================================================

    [Header("Debug")]

    [SerializeField]
    private bool printLog = true;


    // =========================================================
    // Runtime
    // =========================================================

    private Vector2Int lastRoom;

    private DungeonTileType lastTileType;

    private bool lastUsedState;

    private int lastTrapType = -1;

    private bool initialized;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();

        RefreshVisual();
    }


    private void Update()
    {
        ResolveReferences();


        if (dungeonManager == null)
            return;


        Vector2Int room =
            dungeonManager.CurrentRoom;


        DungeonTileType tileType =
            dungeonManager.GetCurrentTileType();


        bool usedState =
            GetCurrentUsedState(
                room,
                tileType
            );


        int trapType =
            GetCurrentTrapType(
                room,
                tileType
            );


        /*
         * 방이 바뀌었거나
         * 파밍/상자/열쇠/휴식 상태가 바뀌었거나
         * TrapType이 달라졌을 때만 갱신.
         */
        if (
            !initialized ||
            room != lastRoom ||
            tileType != lastTileType ||
            usedState != lastUsedState ||
            trapType != lastTrapType
        )
        {
            RefreshVisual();
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
    }


    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshVisual()
    {
        ResolveReferences();


        HideAll();


        if (dungeonManager == null)
        {
            Debug.LogWarning(
                "[RoomTileVisual] " +
                "DungeonManager가 없습니다."
            );

            return;
        }


        Vector2Int room =
            dungeonManager.CurrentRoom;


        DungeonTileData tile =
            dungeonManager.GetCurrentTile();


        /*
         * BaseCamp처럼 Tile_Data에 없는 좌표
         */
        if (tile == null)
        {
            SaveCurrentState(
                room,
                DungeonTileType.None,
                false,
                -1
            );

            Print(
                $"현재 위치 {room} / Tile 없음"
            );

            return;
        }


        bool usedState =
            GetCurrentUsedState(
                room,
                tile.TileType
            );


        int trapType =
            GetCurrentTrapType(
                room,
                tile.TileType
            );


        // =====================================================
        // Tile Type
        // =====================================================

        switch (tile.TileType)
        {
            // -------------------------------------------------
            // Farming
            // -------------------------------------------------

            case DungeonTileType.Farming:

                if (usedState)
                {
                    SetVisual(
                        farmingAfter,
                        true
                    );

                    Print(
                        $"Farming AFTER ON / {room}"
                    );
                }
                else
                {
                    SetVisual(
                        farmingBefore,
                        true
                    );

                    Print(
                        $"Farming BEFORE ON / {room}"
                    );
                }

                break;


            // -------------------------------------------------
            // Rest
            // -------------------------------------------------

            case DungeonTileType.Rest:

                if (usedState)
                {
                    SetVisual(
                        restAfter,
                        true
                    );

                    Print(
                        $"Rest AFTER ON / {room}"
                    );
                }
                else
                {
                    SetVisual(
                        restBefore,
                        true
                    );

                    Print(
                        $"Rest BEFORE ON / {room}"
                    );
                }

                break;


            // -------------------------------------------------
            // Teleport
            // -------------------------------------------------

            case DungeonTileType.Teleport:

                SetVisual(
                    teleportVisual,
                    true
                );

                Print(
                    $"Teleport ON / {room}"
                );

                break;


            // -------------------------------------------------
            // Trap
            // -------------------------------------------------

            case DungeonTileType.Trap:

                RefreshTrap(
                    room,
                    trapType
                );

                break;


            // -------------------------------------------------
            // Event / Hint
            // -------------------------------------------------

            case DungeonTileType.EventHint:

                SetVisual(
                    hintVisual,
                    true
                );

                Print(
                    $"Hint ON / {room}"
                );

                break;


            // -------------------------------------------------
            // Chest
            // -------------------------------------------------

            case DungeonTileType.Chest:

                if (usedState)
                {
                    SetVisual(
                        chestAfter,
                        true
                    );

                    Print(
                        $"Chest AFTER ON / {room}"
                    );
                }
                else
                {
                    SetVisual(
                        chestBefore,
                        true
                    );

                    Print(
                        $"Chest BEFORE ON / {room}"
                    );
                }

                break;


            // -------------------------------------------------
            // Key
            // -------------------------------------------------

            case DungeonTileType.Key:

                if (usedState)
                {
                    SetVisual(
                        keyAfter,
                        true
                    );

                    Print(
                        $"Key AFTER ON / {room}"
                    );
                }
                else
                {
                    SetVisual(
                        keyBefore,
                        true
                    );

                    Print(
                        $"Key BEFORE ON / {room}"
                    );
                }

                break;


            // -------------------------------------------------
            // Puzzle / Letter
            // -------------------------------------------------

            case DungeonTileType.PuzzleLetter:

                SetVisual(
                    puzzleLetterVisual,
                    true
                );

                Print(
                    $"PuzzleLetter ON / {room}"
                );

                break;


            // -------------------------------------------------
            // Boss
            // -------------------------------------------------

            case DungeonTileType.Boss:

                SetVisual(
                    bossVisual,
                    true
                );

                Print(
                    $"Boss ON / {room}"
                );

                break;


            // -------------------------------------------------
            // General / None
            // -------------------------------------------------

            default:

                Print(
                    $"{tile.TileType} / 이벤트 이미지 없음 / {room}"
                );

                break;
        }


        SaveCurrentState(
            room,
            tile.TileType,
            usedState,
            trapType
        );
    }


    // =========================================================
    // Trap
    // =========================================================

    private void RefreshTrap(
        Vector2Int position,
        int trapType)
    {
        /*
         * 현재 리소스:
         *
         * trap_01_0
         * trap_02_0
         *
         * 따라서 일단 TrapType 1/2를 각각 연결.
         */


        if (trapType == 1)
        {
            SetVisual(
                trap01Visual,
                true
            );

            Print(
                $"Trap 01 ON / {position}"
            );

            return;
        }


        if (trapType == 2)
        {
            SetVisual(
                trap02Visual,
                true
            );

            Print(
                $"Trap 02 ON / {position}"
            );

            return;
        }


        /*
         * Trap_Data가 없거나
         * 아직 이미지가 없는 타입이면
         * 우선 01 이미지를 기본값으로 보여준다.
         */
        SetVisual(
            trap01Visual,
            true
        );


        Print(
            $"TrapType {trapType} / " +
            $"기본 Trap 01 표시 / {position}"
        );
    }


    private int GetCurrentTrapType(
        Vector2Int position,
        DungeonTileType tileType)
    {
        if (
            tileType !=
            DungeonTileType.Trap
        )
        {
            return -1;
        }


        if (
            TrapDataLoader.Instance ==
            null
        )
        {
            return -1;
        }


        TrapTileData data =
            TrapDataLoader.Instance
                .GetData(
                    position.x,
                    position.y
                );


        if (data == null)
        {
            return -1;
        }


        return
            data.trapType;
    }


    // =========================================================
    // Used State
    // =========================================================

    private bool GetCurrentUsedState(
        Vector2Int position,
        DungeonTileType tileType)
    {
        // Farming
        if (
            tileType ==
            DungeonTileType.Farming
        )
        {
            if (
                DungeonTileEventManager.Instance ==
                null
            )
            {
                return false;
            }


            return
                DungeonTileEventManager.Instance
                    .IsFarmingTileUsed(
                        position
                    );
        }


        // Chest
        if (
            tileType ==
            DungeonTileType.Chest
        )
        {
            if (
                DungeonTileEventManager.Instance ==
                null
            )
            {
                return false;
            }


            return
                DungeonTileEventManager.Instance
                    .IsChestTileUsed(
                        position
                    );
        }


        // Key
        if (
            tileType ==
            DungeonTileType.Key
        )
        {
            if (
                DungeonTileEventManager.Instance ==
                null
            )
            {
                return false;
            }


            return
                DungeonTileEventManager.Instance
                    .IsKeyTileUsed(
                        position
                    );
        }


        // Rest
        if (
            tileType ==
            DungeonTileType.Rest
        )
        {
            if (
                RestTileManager.Instance ==
                null
            )
            {
                return false;
            }


            /*
             * CanRest == false
             * → 이미 이번 Run에서 사용 완료.
             */
            return
                !RestTileManager.Instance
                    .CanRest(
                        position
                    );
        }


        return false;
    }


    // =========================================================
    // Hide
    // =========================================================

    private void HideAll()
    {
        // Farming
        SetVisual(
            farmingBefore,
            false
        );

        SetVisual(
            farmingAfter,
            false
        );


        // Rest
        SetVisual(
            restBefore,
            false
        );

        SetVisual(
            restAfter,
            false
        );


        // Teleport
        SetVisual(
            teleportVisual,
            false
        );


        // Trap
        SetVisual(
            trap01Visual,
            false
        );

        SetVisual(
            trap02Visual,
            false
        );


        // Hint
        SetVisual(
            hintVisual,
            false
        );


        // Chest
        SetVisual(
            chestBefore,
            false
        );

        SetVisual(
            chestAfter,
            false
        );


        // Key
        SetVisual(
            keyBefore,
            false
        );

        SetVisual(
            keyAfter,
            false
        );


        // Puzzle
        SetVisual(
            puzzleLetterVisual,
            false
        );


        // Boss
        SetVisual(
            bossVisual,
            false
        );
    }


    // =========================================================
    // Active Only
    // =========================================================

    private void SetVisual(
        GameObject target,
        bool active)
    {
        if (target == null)
            return;


        /*
         * 중요:
         *
         * 위치 / 회전 / 크기 절대 수정하지 않는다.
         *
         * transform.position
         * transform.localPosition
         * transform.rotation
         * transform.localRotation
         * transform.localScale
         *
         * 전혀 사용하지 않음.
         */

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


    // =========================================================
    // State
    // =========================================================

    private void SaveCurrentState(
        Vector2Int room,
        DungeonTileType tileType,
        bool usedState,
        int trapType)
    {
        lastRoom =
            room;

        lastTileType =
            tileType;

        lastUsedState =
            usedState;

        lastTrapType =
            trapType;

        initialized =
            true;
    }


    // =========================================================
    // Debug
    // =========================================================

    private void Print(
        string message)
    {
        if (!printLog)
            return;


        Debug.Log(
            "[RoomTileVisual] " +
            message
        );
    }
}