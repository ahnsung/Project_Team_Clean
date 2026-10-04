using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // Map
    // =========================================================

    [Header("Map Size")]
    [SerializeField]
    private int mapWidth = 44;

    [SerializeField]
    private int mapHeight = 43;


    // =========================================================
    // Start / Base Camp
    // =========================================================

    [Header("Start / Base Camp")]
    [SerializeField]
    private Vector2Int startRoom =
        new Vector2Int(15, 29);


    // =========================================================
    // State
    // =========================================================

    [Header("Dungeon State")]
    [SerializeField]
    private int currentTurn = 0;

    [SerializeField]
    private string currentEnvironment =
        "지하";


    public event Action<int> OnTurnChanged;


    // =========================================================
    // References
    // =========================================================

    [Header("Refs")]

    [SerializeField]
    private DungeonUIManager uiManager;

    [SerializeField]
    private MinimapUIManager minimapUI;

    [SerializeField]
    private DungeonMapDatabase mapDatabase;

    [SerializeField]
    private MoveDataLoader moveDataLoader;


    // =========================================================
    // Runtime
    // =========================================================

    private Vector2Int currentRoom;


    private readonly HashSet<string>
        visited =
            new HashSet<string>();


    private bool freshDungeonEntry;


    // =========================================================
    // PlayerPrefs
    // =========================================================

    private const string XKEY =
        "ROOM_X";

    private const string YKEY =
        "ROOM_Y";

    private const string VISITED =
        "VISITED";

    private const string TURN_KEY =
        "DUNGEON_TURN";

    private const string ENVIRONMENT_KEY =
        "DUNGEON_ENVIRONMENT";


    private const string FreshDungeonEntryKey =
        "DUNGEON_FRESH_ENTRY";


    // =========================================================
    // Properties
    // =========================================================

    public int MapWidth =>
        mapWidth;


    public int MapHeight =>
        mapHeight;


    public Vector2Int CurrentRoom =>
        currentRoom;


    public int CurrentTurn =>
        currentTurn;


    public string CurrentEnvironment =>
        currentEnvironment;


    public Vector2Int StartRoom =>
        startRoom;


    public bool IsFreshDungeonEntry =>
        freshDungeonEntry;


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


        Instance =
            this;


        // =====================================================
        // 기존 던전 상태 불러오기
        // =====================================================

        Load();


        // =====================================================
        // Lobby에서 새 Run으로 들어온 것인지 확인
        // =====================================================

        freshDungeonEntry =
            PlayerPrefs.GetInt(
                FreshDungeonEntryKey,
                0
            ) == 1;


        /*
         * 중요:
         * DUNGEON_FRESH_ENTRY가 남아 있더라도
         * 실제 저장된 던전 좌표가 존재한다면 Continue로 판단한다.
         *
         * 예전 코드처럼 freshDungeonEntry만 보고 startRoom으로
         * 강제 이동시키면 게임 재실행 시 마지막 위치가 사라진다.
         */
        bool hasSavedDungeonPosition =
            PlayerPrefs.HasKey(
                XKEY
            ) &&
            PlayerPrefs.HasKey(
                YKEY
            );


        if (
            freshDungeonEntry &&
            hasSavedDungeonPosition
        )
        {
            freshDungeonEntry =
                false;


            PlayerPrefs.DeleteKey(
                FreshDungeonEntryKey
            );


            PlayerPrefs.Save();


            Debug.Log(
                "[DungeonManager] " +
                "저장된 던전 위치가 있으므로 Continue로 처리\n" +
                $"복구 위치: {currentRoom}"
            );
        }
        else if (freshDungeonEntry)
        {
            /*
             * 정말 저장된 던전 위치가 없는 경우에만
             * 새로운 Run으로 보고 Base Camp에서 시작한다.
             */

            currentRoom =
                startRoom;


            PlayerPrefs.DeleteKey(
                FreshDungeonEntryKey
            );


            PlayerPrefs.Save();


            Debug.Log(
                "[DungeonManager] " +
                "새 던전 Run 입장\n" +
                $"Base Camp 시작: {currentRoom}"
            );
        }


        MarkVisited(
            currentRoom
        );
    }


    private void Start()
    {
        ResolveReferences();


        ValidateCurrentRoom();


        /*
         * 새 Run인 경우 DungeonScene 안의
         * Run 단위 상태들을 초기화한다.
         */
        if (freshDungeonEntry)
        {
            ResetRunState();
        }


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[던전] 현재 턴: " +
            currentTurn
        );


        Debug.Log(
            "[던전] 현재 장소: " +
            currentEnvironment
        );


        OnTurnChanged?.Invoke(
            currentTurn
        );
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }
    }


    // =========================================================
    // New Run Reset
    // =========================================================

    private void ResetRunState()
    {
        DungeonTileEventManager
            tileEventManager =
                DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            /*
             * Farming:
             * 한 번 던전을 나가면 다시 사용 가능.
             */
            tileEventManager
                .ClearUsedFarmingTiles();


            /*
             * General:
             * 새로운 Run에서는 다시 10%부터.
             */
            tileEventManager
                .ResetGeneralBattleChance();
        }


        /*
         * RestTileManager는 DungeonScene 오브젝트이므로
         * Scene 재진입 과정에서 새 인스턴스로 생성된다.
         * 따라서 사용한 Rest HashSet도 자연스럽게 초기화된다.
         *
         * Chest / Key:
         * 절대 초기화하지 않는다.
         *
         * LockedDoor:
         * SaveManager가 열린 문 상태를 복구하므로
         * 절대 초기화하지 않는다.
         */


        Debug.Log(
            "[DungeonManager] 새 Run 상태 초기화\n" +
            "Farming = 초기화\n" +
            "General = 10% 초기화\n" +
            "Rest = 새 Scene 인스턴스로 초기화\n" +
            "Chest = 유지\n" +
            "Key = 유지\n" +
            "LockedDoor = 유지"
        );
    }


    // =========================================================
    // References
    // =========================================================

    private void ResolveReferences()
    {
        if (mapDatabase == null)
        {
            mapDatabase =
                DungeonMapDatabase.Instance;
        }


        if (moveDataLoader == null)
        {
            moveDataLoader =
                MoveDataLoader.Instance;
        }


        if (uiManager == null)
        {
            uiManager =
                FindFirstObjectByType<
                    DungeonUIManager
                >();
        }


        if (minimapUI == null)
        {
            minimapUI =
                FindFirstObjectByType<
                    MinimapUIManager
                >();
        }
    }


    // =========================================================
    // Move Data
    // =========================================================

    public MoveData GetMoveData(
        MoveDirection direction)
    {
        ResolveReferences();


        if (moveDataLoader == null)
        {
            return null;
        }


        return
            moveDataLoader.GetMoveData(
                currentRoom,
                direction
            );
    }


    // =========================================================
    // Open Move
    // =========================================================

    public bool CanMoveOpen(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        if (
            data.PathType !=
            MovePathType.Open
        )
        {
            return false;
        }


        if (!data.Passable)
        {
            return false;
        }


        return
            IsDestinationValid(
                direction
            );
    }


    // =========================================================
    // Special Path - Exists
    // =========================================================

    /*
     * 해당 방향에 Space로 선택하는
     * "특수 통로 자체가 존재하는지" 확인한다.
     *
     * 여기서는 현재 통과 가능한지는 검사하지 않는다.
     *
     * Door / OneWay / LockedDoor / GimmickDoor
     * → true
     *
     * Open / Wall
     * → false
     */
    public bool HasSpecialPath(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        switch (data.PathType)
        {
            case MovePathType.Door:
            case MovePathType.OneWay:
            case MovePathType.LockedDoor:
            case MovePathType.GimmickDoor:

                return true;


            default:

                return false;
        }
    }


    // =========================================================
    // Special Path - Can Use Now
    // =========================================================

    /*
     * 특수 통로가 "현재 실제로 사용 가능한지" 검사한다.
     *
     * Door
     * → 일반 문이므로 이동 가능
     *
     * OneWay
     * → Passable=true인 방향만 가능
     *
     * LockedDoor
     * → 이미 열렸거나 필요한 열쇠가 있으면 가능
     *
     * GimmickDoor
     * → 10, 20, 30... 턴에만 가능
     *
     * 목적지 자체가 유효하지 않으면
     * 어떤 경우든 이동할 수 없다.
     */
    public bool CanUseSpecialPath(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        if (
            !HasSpecialPath(
                direction
            )
        )
        {
            return false;
        }


        if (
            !IsDestinationValid(
                direction
            )
        )
        {
            return false;
        }


        // =====================================================
        // Door
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Door
        )
        {
            /*
             * 일반 Door는 Space로 선택하면 이동 가능.
             *
             * Open과 Door의 차이는
             * "이동 가능 여부"가 아니라
             * "입력 방식"이다.
             *
             * Open = WASD
             * Door = Space
             */
            return true;
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
             * Passable=true
             * → 현재 쪽에서 통과 가능
             *
             * Passable=false
             * → 반대 방향에서만 통과 가능
             */
            return
                data.Passable;
        }


        // =====================================================
        // LockedDoor
        // =====================================================

        if (
            data.PathType ==
            MovePathType.LockedDoor
        )
        {
            LockedDoorManager
                lockedDoorManager =
                    LockedDoorManager.Instance;


            if (lockedDoorManager == null)
            {
                return false;
            }


            /*
             * 이미 열린 문이면
             * 열쇠가 없어도 통과 가능.
             */
            if (
                lockedDoorManager.IsOpened(
                    currentRoom,
                    direction
                )
            )
            {
                return true;
            }


            /*
             * 아직 잠겨 있다면
             * 실제 열쇠를 소비하지 않고
             * 현재 열 수 있는지만 확인.
             */
            return
                lockedDoorManager.CanOpenDoor(
                    currentRoom,
                    direction
                );
        }


        // =====================================================
        // GimmickDoor
        // =====================================================

        if (
            data.PathType ==
            MovePathType.GimmickDoor
        )
        {
            /*
             * 0턴은 제외.
             *
             * 10 / 20 / 30 / 40 ...
             * 턴에만 열린다.
             */
            return
                currentTurn > 0 &&
                currentTurn % 10 == 0;
        }


        return false;
    }


    // =========================================================
    // Any Special Path
    // =========================================================

    /*
     * "사용 가능한 특수통로가 있는가?"가 아니라
     * "특수통로 자체가 하나라도 존재하는가?"를 확인한다.
     *
     * 따라서 열쇠 없는 LockedDoor,
     * 역방향 OneWay,
     * 닫힌 GimmickDoor만 있어도
     * Space UI는 열릴 수 있다.
     */
    public bool HasAnySpecialPath()
    {
        return
            HasSpecialPath(
                MoveDirection.Up
            ) ||

            HasSpecialPath(
                MoveDirection.Down
            ) ||

            HasSpecialPath(
                MoveDirection.Left
            ) ||

            HasSpecialPath(
                MoveDirection.Right
            );
    }


    // =========================================================
    // General Move
    // =========================================================

    public bool CanMove(
        MoveDirection direction)
    {
        MoveData data =
            GetMoveData(
                direction
            );


        if (data == null)
        {
            return false;
        }


        // =====================================================
        // Open
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Open
        )
        {
            return
                CanMoveOpen(
                    direction
                );
        }


        // =====================================================
        // Special
        // =====================================================

        if (
            HasSpecialPath(
                direction
            )
        )
        {
            return
                CanUseSpecialPath(
                    direction
                );
        }


        return false;
    }


    // =========================================================
    // Move Room
    // =========================================================

    public bool MoveToNextRoom(
        MoveDirection direction)
    {
        ResolveReferences();


        MoveData moveData =
            GetMoveData(
                direction
            );


        if (moveData == null)
        {
            Debug.LogWarning(
                "[DungeonManager] Move Data가 없습니다.\n" +
                $"현재 위치: {currentRoom}\n" +
                $"방향: {direction}"
            );

            return false;
        }


        // =====================================================
        // Final Movement Validation
        // =====================================================

        bool canPass =
            false;


        if (
            moveData.PathType ==
            MovePathType.Open
        )
        {
            canPass =
                CanMoveOpen(
                    direction
                );
        }
        else if (
            HasSpecialPath(
                direction
            )
        )
        {
            canPass =
                CanUseSpecialPath(
                    direction
                );
        }


        if (!canPass)
        {
            Debug.Log(
                "[DungeonManager] 이동 불가\n" +
                $"현재 위치: {currentRoom}\n" +
                $"방향: {direction}\n" +
                $"Type: {moveData.PathType}\n" +
                $"Passable: {moveData.Passable}"
            );

            return false;
        }


        if (moveDataLoader == null)
        {
            return false;
        }


        Vector2Int destination =
            moveDataLoader
                .GetDestination(
                    currentRoom,
                    direction
                );


        if (
            !CanMoveTo(
                destination
            )
        )
        {
            Debug.LogWarning(
                "[DungeonManager] 목적지 타일이 " +
                "유효하지 않습니다.\n" +
                $"목적지: {destination}"
            );

            return false;
        }


        Vector2Int previous =
            currentRoom;


        currentRoom =
            destination;


        MarkVisited(
            currentRoom
        );


        AddTurn(
            "방 이동"
        );


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[DungeonManager] 이동 완료\n" +
            $"{previous} -> {currentRoom}\n" +
            $"방향: {direction}\n" +
            $"Type: {moveData.PathType}"
        );


        return true;
    }


    // =========================================================
    // Teleport
    // =========================================================

    public bool TeleportToRoom(
        Vector2Int destination)
    {
        ResolveReferences();


        if (mapDatabase == null)
        {
            return false;
        }


        if (
            !mapDatabase.IsValidTile(
                destination
            )
        )
        {
            Debug.LogError(
                "[DungeonManager] 텔레포트 목적지가 " +
                "유효하지 않습니다.\n" +
                $"목적지: {destination}"
            );

            return false;
        }


        Vector2Int previous =
            currentRoom;


        currentRoom =
            destination;


        MarkVisited(
            currentRoom
        );


        /*
         * Teleport 자체는 추가 턴 없음.
         */


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[DungeonManager] 텔레포트 이동 완료\n" +
            $"출발: {previous}\n" +
            $"도착: {currentRoom}"
        );


        return true;
    }


    // =========================================================
    // Destination
    // =========================================================

    private bool IsDestinationValid(
        MoveDirection direction)
    {
        ResolveReferences();


        if (moveDataLoader == null)
        {
            return false;
        }


        Vector2Int destination =
            moveDataLoader
                .GetDestination(
                    currentRoom,
                    direction
                );


        return
            CanMoveTo(
                destination
            );
    }


    public bool CanMoveTo(
        Vector2Int position)
    {
        ResolveReferences();


        // =====================================================
        // Base Camp
        // =====================================================

        /*
         * Base Camp는 Tile_Data에 존재하지 않는
         * 특수 좌표다.
         *
         * 따라서 DungeonMapDatabase에 없어도
         * 이동 가능한 유효 좌표로 취급한다.
         */
        if (position == startRoom)
        {
            return true;
        }


        // =====================================================
        // Normal Dungeon Tile
        // =====================================================

        if (mapDatabase == null)
        {
            return false;
        }


        return
            mapDatabase.IsValidTile(
                position
            );
    }


    // =========================================================
    // Directions
    // =========================================================

    /*
     * 기존 코드 호환용.
     *
     * 각 방향으로 현재 실제 이동 가능한지 반환한다.
     */
    public Dictionary<
        MoveDirection,
        bool
    > GetDirections()
    {
        return
            new Dictionary<
                MoveDirection,
                bool
            >
            {
                {
                    MoveDirection.Up,
                    CanMove(
                        MoveDirection.Up
                    )
                },

                {
                    MoveDirection.Down,
                    CanMove(
                        MoveDirection.Down
                    )
                },

                {
                    MoveDirection.Left,
                    CanMove(
                        MoveDirection.Left
                    )
                },

                {
                    MoveDirection.Right,
                    CanMove(
                        MoveDirection.Right
                    )
                }
            };
    }


    /*
     * 기존 함수 이름을 유지한다.
     *
     * 값의 의미:
     *
     * true
     * → 특수통로 존재 + 현재 이동 가능
     *
     * false
     * → 특수통로가 없거나 현재 이동 불가능
     *
     * 실제 "존재 여부"는
     * HasSpecialPath(direction)으로 별도 확인한다.
     */
    public Dictionary<
        MoveDirection,
        bool
    > GetSpecialDirections()
    {
        return
            new Dictionary<
                MoveDirection,
                bool
            >
            {
                {
                    MoveDirection.Up,
                    CanUseSpecialPath(
                        MoveDirection.Up
                    )
                },

                {
                    MoveDirection.Down,
                    CanUseSpecialPath(
                        MoveDirection.Down
                    )
                },

                {
                    MoveDirection.Left,
                    CanUseSpecialPath(
                        MoveDirection.Left
                    )
                },

                {
                    MoveDirection.Right,
                    CanUseSpecialPath(
                        MoveDirection.Right
                    )
                }
            };
    }


    /*
     * Space UI가
     *
     * 숨김 / 빨강 / 파랑
     *
     * 을 구분하기 위해 사용하는
     * "특수통로 존재 여부" 데이터.
     */
    public Dictionary<
        MoveDirection,
        bool
    > GetSpecialPathExistence()
    {
        return
            new Dictionary<
                MoveDirection,
                bool
            >
            {
                {
                    MoveDirection.Up,
                    HasSpecialPath(
                        MoveDirection.Up
                    )
                },

                {
                    MoveDirection.Down,
                    HasSpecialPath(
                        MoveDirection.Down
                    )
                },

                {
                    MoveDirection.Left,
                    HasSpecialPath(
                        MoveDirection.Left
                    )
                },

                {
                    MoveDirection.Right,
                    HasSpecialPath(
                        MoveDirection.Right
                    )
                }
            };
    }


    // =========================================================
    // Tile
    // =========================================================

    public DungeonTileData GetCurrentTile()
    {
        ResolveReferences();


        if (mapDatabase == null)
        {
            return null;
        }


        return
            mapDatabase.GetTile(
                currentRoom
            );
    }


    public DungeonTileType
        GetCurrentTileType()
    {
        DungeonTileData tile =
            GetCurrentTile();


        if (tile == null)
        {
            return
                DungeonTileType.None;
        }


        return
            tile.TileType;
    }


    // =========================================================
    // Validation
    // =========================================================

    private void ValidateCurrentRoom()
    {
        ResolveReferences();


        // =====================================================
        // Base Camp
        // =====================================================

        /*
         * Base Camp는 Tile_Data에 없는 특수 좌표이므로
         * 여기서는 정상 좌표로 인정하고 종료.
         */
        if (currentRoom == startRoom)
        {
            Debug.Log(
                "[DungeonManager] 현재 위치: Base Camp\n" +
                $"좌표: {currentRoom}"
            );

            return;
        }


        // =====================================================
        // Database
        // =====================================================

        if (mapDatabase == null)
        {
            Debug.LogError(
                "[DungeonManager] " +
                "DungeonMapDatabase가 없습니다."
            );

            return;
        }


        // =====================================================
        // Normal Tile
        // =====================================================

        if (
            mapDatabase.IsValidTile(
                currentRoom
            )
        )
        {
            return;
        }


        // =====================================================
        // Invalid Saved Position
        // =====================================================

        Debug.LogWarning(
            "[DungeonManager] " +
            "유효하지 않은 현재 좌표입니다.\n" +
            $"현재: {currentRoom}\n" +
            "Base Camp으로 복구합니다."
        );


        currentRoom =
            startRoom;


        MarkVisited(
            currentRoom
        );


        Debug.Log(
            "[DungeonManager] " +
            "Base Camp으로 복구 완료\n" +
            $"좌표: {currentRoom}"
        );
    }


    // =========================================================
    // Log
    // =========================================================

    public void LogCurrentTile()
    {
        // =====================================================
        // Base Camp
        // =====================================================

        if (currentRoom == startRoom)
        {
            Debug.Log(
                "[DungeonManager] 현재 위치: Base Camp\n" +
                $"좌표: {currentRoom}"
            );

            return;
        }


        // =====================================================
        // Normal Tile
        // =====================================================

        DungeonTileData tile =
            GetCurrentTile();


        if (tile == null)
        {
            Debug.LogWarning(
                "[DungeonManager] 현재 타일 데이터 없음\n" +
                $"좌표: {currentRoom}"
            );

            return;
        }


        Debug.Log(
            "[DungeonManager] 현재 타일: (" +
            tile.X +
            ", " +
            tile.Y +
            ") / " +
            tile.TileType
        );
    }


    // =========================================================
    // Visited
    // =========================================================

    public bool IsVisited(
        int x,
        int y)
    {
        return
            visited.Contains(
                GetVisitedKey(
                    x,
                    y
                )
            );
    }


    public bool IsVisited(
        Vector2Int position)
    {
        return
            IsVisited(
                position.x,
                position.y
            );
    }


    private void MarkVisited(
        Vector2Int position)
    {
        visited.Add(
            GetVisitedKey(
                position.x,
                position.y
            )
        );
    }


    private string GetVisitedKey(
        int x,
        int y)
    {
        return
            x + "," + y;
    }


    // =========================================================
    // Turn
    // =========================================================

    public void AddTurn(
        string reason = "")
    {
        currentTurn++;


        PlayerPrefs.SetInt(
            TURN_KEY,
            currentTurn
        );


        PlayerPrefs.Save();


        Debug.Log(
            "[던전 턴] 현재 턴: " +
            currentTurn +
            (
                string.IsNullOrEmpty(
                    reason
                )
                ? ""
                : " / 행동: " + reason
            )
        );


        // =====================================================
        // 플레이어 상태이상 턴 종료 처리
        // =====================================================

        ProcessPlayerStatusEffectsOnTurnEnd();


        // =====================================================
        // 기존 턴 변경 이벤트
        // =====================================================

        OnTurnChanged?.Invoke(
            currentTurn
        );
    }


    // =========================================================
    // Player Status Effect Turn End
    // =========================================================

    private void ProcessPlayerStatusEffectsOnTurnEnd()
    {
        StatusEffectController controller =
            null;


        // -----------------------------------------------------
        // 1순위: BattleManager에 연결된 Player
        // -----------------------------------------------------

        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null
        )
        {
            controller =
                BattleManager.Instance
                    .playerUnit
                    .GetComponent<
                        StatusEffectController
                    >();


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInParent<
                            StatusEffectController
                        >();
            }


            if (controller == null)
            {
                controller =
                    BattleManager.Instance
                        .playerUnit
                        .GetComponentInChildren<
                            StatusEffectController
                        >();
            }
        }


        // -----------------------------------------------------
        // 2순위: Scene 내 플레이어 StatusEffectController
        // -----------------------------------------------------

        if (controller == null)
        {
            StatusEffectController[] controllers =
                FindObjectsByType<
                    StatusEffectController
                >(
                    FindObjectsSortMode.None
                );


            foreach (
                StatusEffectController candidate
                in controllers
            )
            {
                if (candidate == null)
                {
                    continue;
                }


                BattleUnit unit =
                    candidate.GetComponent<
                        BattleUnit
                    >();


                if (
                    BattleManager.Instance != null &&
                    BattleManager.Instance.playerUnit != null &&
                    unit ==
                    BattleManager.Instance.playerUnit
                )
                {
                    controller =
                        candidate;

                    break;
                }
            }
        }


        if (controller == null)
        {
            Debug.LogWarning(
                "[DungeonManager] " +
                "플레이어 StatusEffectController를 찾지 못했습니다."
            );

            return;
        }


        controller.ProcessTiming(
            StatusEffectTiming.TurnEnd
        );


        Debug.Log(
            "[DungeonManager] " +
            "플레이어 상태이상 TurnEnd 처리 완료"
        );
    }


    // =========================================================
    // UI
    // =========================================================

    public void RefreshAll()
    {
        ResolveReferences();


        if (uiManager != null)
        {
            uiManager
                .RefreshDirectionButtons(
                    GetDirections()
                );
        }


        if (minimapUI != null)
        {
            minimapUI
                .RefreshMinimap();
        }
    }


    // =========================================================
    // New Game
    // =========================================================

    public void ResetForNewGame(
        bool saveData = true)
    {
        currentRoom =
            startRoom;


        currentTurn =
            0;


        currentEnvironment =
            "지하";


        freshDungeonEntry =
            true;


        visited.Clear();


        MarkVisited(
            currentRoom
        );


        if (
            LockedDoorManager.Instance != null
        )
        {
            /*
             * 진짜 New Game에서만 LockedDoor 초기화.
             */
            LockedDoorManager.Instance
                .ClearOpenedDoors();
        }


        ResetRunState();


        if (saveData)
        {
            Save();
        }


        RefreshAll();


        OnTurnChanged?.Invoke(
            currentTurn
        );
    }


    // =========================================================
    // Save
    // =========================================================

    private void Save()
    {
        PlayerPrefs.SetInt(
            XKEY,
            currentRoom.x
        );


        PlayerPrefs.SetInt(
            YKEY,
            currentRoom.y
        );


        PlayerPrefs.SetString(
            VISITED,
            string.Join(
                "|",
                visited
            )
        );


        PlayerPrefs.SetInt(
            TURN_KEY,
            currentTurn
        );


        PlayerPrefs.SetString(
            ENVIRONMENT_KEY,
            currentEnvironment
        );


        PlayerPrefs.Save();
    }


    private void Load()
    {
        if (
            PlayerPrefs.HasKey(
                XKEY
            ) &&
            PlayerPrefs.HasKey(
                YKEY
            )
        )
        {
            currentRoom =
                new Vector2Int(
                    PlayerPrefs.GetInt(
                        XKEY
                    ),
                    PlayerPrefs.GetInt(
                        YKEY
                    )
                );
        }
        else
        {
            currentRoom =
                startRoom;
        }


        currentTurn =
            PlayerPrefs.GetInt(
                TURN_KEY,
                0
            );


        currentEnvironment =
            PlayerPrefs.GetString(
                ENVIRONMENT_KEY,
                "지하"
            );


        visited.Clear();


        string visitedData =
            PlayerPrefs.GetString(
                VISITED,
                ""
            );


        if (
            string.IsNullOrWhiteSpace(
                visitedData
            )
        )
        {
            return;
        }


        string[] values =
            visitedData.Split('|');


        foreach (
            string value
            in values
        )
        {
            if (
                !string.IsNullOrWhiteSpace(
                    value
                )
            )
            {
                visited.Add(
                    value
                );
            }
        }
    }


    // =========================================================
    // SaveManager API
    // =========================================================

    public List<string>
        GetVisitedRoomsForSave()
    {
        return
            new List<string>(
                visited
            );
    }


    public void RestoreDungeonState(
        Vector2Int room,
        int turn,
        string environment,
        List<string> visitedRooms)
    {
        /*
         * Lobby에서 새 Run으로 들어왔다면
         * SaveManager가 과거 좌표를 복원해
         * Base Camp 위치를 덮어쓰면 안 된다.
         */

        if (freshDungeonEntry)
        {
            room =
                startRoom;
        }


        currentRoom =
            room;


        currentTurn =
            Mathf.Max(
                0,
                turn
            );


        currentEnvironment =
            string.IsNullOrEmpty(
                environment
            )
            ? "지하"
            : environment;


        visited.Clear();


        if (visitedRooms != null)
        {
            foreach (
                string roomKey
                in visitedRooms
            )
            {
                if (
                    !string.IsNullOrWhiteSpace(
                        roomKey
                    )
                )
                {
                    visited.Add(
                        roomKey
                    );
                }
            }
        }


        MarkVisited(
            currentRoom
        );


        Save();


        RefreshAll();


        OnTurnChanged?.Invoke(
            currentTurn
        );


        Debug.Log(
            "[DungeonManager] 던전 상태 복구\n" +
            $"현재 위치: {currentRoom}\n" +
            $"새 Run: {freshDungeonEntry}"
        );
    }


    // =========================================================
    // Tests
    // =========================================================

    [ContextMenu(
        "TEST - Move To Base Camp"
    )]
    public void TestMoveToBaseCamp()
    {
        TestMoveTo(
            startRoom,
            "Base Camp"
        );
    }


    [ContextMenu(
        "TEST - Move To Door Test"
    )]
    public void TestMoveToDoor()
    {
        TestMoveTo(
            new Vector2Int(
                6,
                24
            ),
            "Door Test"
        );
    }


    [ContextMenu(
        "TEST - Move To Farming"
    )]
    public void TestMoveToFarming()
    {
        TestMoveTo(
            new Vector2Int(
                8,
                23
            ),
            "Farming"
        );
    }


    [ContextMenu(
        "TEST - Move To Teleport"
    )]
    public void TestMoveToTeleport()
    {
        TestMoveTo(
            new Vector2Int(
                3,
                33
            ),
            "Teleport"
        );
    }


    private void TestMoveTo(
        Vector2Int position,
        string label)
    {
        currentRoom =
            position;


        MarkVisited(
            currentRoom
        );


        Save();


        RefreshAll();


        LogCurrentTile();


        Debug.Log(
            "[DungeonManager] " +
            label +
            " 테스트 위치 이동: " +
            currentRoom
        );
    }
}