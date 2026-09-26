using System.Collections;
using UnityEngine;

public class RoomTraversalController : MonoBehaviour
{
    private enum RoomState
    {
        EventRunning,
        WaitingForInput,
        DirectionChoosing,
        Transition
    }


    [Header("Points")]
    [SerializeField]
    private Transform playerCenterPoint;


    [Header("Managers")]
    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private DungeonUIManager uiManager;

    [SerializeField]
    private FadeController fadeController;

    [SerializeField]
    private DungeonTileEventManager tileEventManager;

    [SerializeField]
    private CameraRoomTransition cameraRoomTransition;


    private RoomState state;

    private bool isTransitioning;
    private bool isInteracting;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();


        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        if (uiManager != null)
        {
            uiManager.HideDirectionPanel();
        }


        StartCoroutine(
            RoomStartRoutine()
        );
    }


    private void Update()
    {
        // =====================================================
        // 전투 중 던전 이동 / 상호작용 완전 차단
        // =====================================================

        if (IsBattleRunning())
        {
            /*
             * 전투가 시작되기 전에
             * 방향 선택창이 열려 있었다면 닫는다.
             */
            if (
                state ==
                RoomState.DirectionChoosing
            )
            {
                if (uiManager != null)
                {
                    uiManager.HideDirectionPanel();
                }


                state =
                    RoomState.WaitingForInput;
            }


            return;
        }


        // =====================================================
        // 일반 입력 가능 상태
        // =====================================================

        if (
            state !=
            RoomState.WaitingForInput
        )
        {
            return;
        }


        if (
            isTransitioning ||
            isInteracting
        )
        {
            return;
        }


        // =====================================================
        // E - Interaction
        // =====================================================

        if (
            Input.GetKeyDown(
                KeyCode.E
            )
        )
        {
            TryInteract();

            return;
        }


        // =====================================================
        // Space - Special Path
        // =====================================================

        if (
            Input.GetKeyDown(
                KeyCode.Space
            )
        )
        {
            OpenDirectionPanel();

            return;
        }


        // =====================================================
        // WASD / Arrow - Open Path
        // =====================================================

        if (
            Input.GetKeyDown(
                KeyCode.W
            ) ||
            Input.GetKeyDown(
                KeyCode.UpArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Up
            );

            return;
        }


        if (
            Input.GetKeyDown(
                KeyCode.S
            ) ||
            Input.GetKeyDown(
                KeyCode.DownArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Down
            );

            return;
        }


        if (
            Input.GetKeyDown(
                KeyCode.A
            ) ||
            Input.GetKeyDown(
                KeyCode.LeftArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Left
            );

            return;
        }


        if (
            Input.GetKeyDown(
                KeyCode.D
            ) ||
            Input.GetKeyDown(
                KeyCode.RightArrow
            )
        )
        {
            TryOpenMove(
                MoveDirection.Right
            );

            return;
        }
    }


    // =========================================================
    // Battle Check
    // =========================================================

    private bool IsBattleRunning()
    {
        if (BattleManager.Instance == null)
        {
            return false;
        }


        return
            BattleManager.Instance
                .IsBattleRunning();
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


        if (tileEventManager == null)
        {
            tileEventManager =
                DungeonTileEventManager.Instance;
        }


        if (uiManager == null)
        {
            uiManager =
                FindFirstObjectByType<
                    DungeonUIManager
                >();
        }


        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<
                    FadeController
                >();
        }


        if (
            cameraRoomTransition == null
        )
        {
            cameraRoomTransition =
                FindFirstObjectByType<
                    CameraRoomTransition
                >();
        }
    }


    // =========================================================
    // Start
    // =========================================================

    private IEnumerator RoomStartRoutine()
    {
        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }


    // =========================================================
    // Enter Event
    // =========================================================

    private IEnumerator RunRoomEnterEvent()
    {
        state =
            RoomState.EventRunning;


        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        ResolveReferences();


        if (tileEventManager != null)
        {
            yield return StartCoroutine(
                tileEventManager
                    .ExecuteEnterEvent()
            );
        }


        /*
         * ExecuteEnterEvent 안에서 전투가 시작되어도
         * 상태 자체는 WaitingForInput으로 돌려놓는다.
         *
         * 실제 입력 차단은 Update의
         * IsBattleRunning()에서 담당한다.
         */
        state =
            RoomState.WaitingForInput;
    }


    // =========================================================
    // Interaction
    // =========================================================

    private void TryInteract()
    {
        // =====================================================
        // Battle
        // =====================================================

        if (IsBattleRunning())
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "전투 중에는 상호작용할 수 없습니다."
            );

            return;
        }


        if (
            isInteracting ||
            isTransitioning
        )
        {
            return;
        }


        ResolveReferences();


        // =====================================================
        // Base Camp Exit
        // =====================================================

        if (
            BaseCampExitManager.Instance != null &&
            BaseCampExitManager.Instance
                .IsAtBaseCamp()
        )
        {
            bool opened =
                BaseCampExitManager.Instance
                    .TryOpenExitConfirm();


            if (opened)
            {
                return;
            }
        }


        // =====================================================
        // Normal Tile Interaction
        // =====================================================

        if (tileEventManager == null)
        {
            return;
        }


        if (
            !tileEventManager
                .CanInteractCurrentTile()
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "현재 타일에는 상호작용할 것이 없습니다."
            );

            return;
        }


        StartCoroutine(
            InteractionRoutine()
        );
    }


    private IEnumerator InteractionRoutine()
    {
        isInteracting =
            true;


        state =
            RoomState.EventRunning;


        yield return StartCoroutine(
            tileEventManager
                .ExecuteInteraction()
        );


        state =
            RoomState.WaitingForInput;


        isInteracting =
            false;
    }


    // =========================================================
    // WASD / Arrow - Open
    // =========================================================

    private void TryOpenMove(
        MoveDirection direction)
    {
        // =====================================================
        // Battle
        // =====================================================

        if (IsBattleRunning())
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "전투 중에는 방을 이동할 수 없습니다."
            );

            return;
        }


        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        MoveData moveData =
            dungeonManager.GetMoveData(
                direction
            );


        if (moveData == null)
        {
            Debug.Log(
                "[이동] Move Data가 없습니다."
            );

            return;
        }


        Debug.Log(
            "[이동 입력]\n" +
            $"현재 위치: {dungeonManager.CurrentRoom}\n" +
            $"방향: {direction}\n" +
            $"Type: {moveData.PathType}\n" +
            $"Passable: {moveData.Passable}"
        );


        // =====================================================
        // Only Open
        // =====================================================

        if (
            moveData.PathType !=
            MovePathType.Open
        )
        {
            Debug.Log(
                "[이동] 이 방향은 Open이 아닙니다.\n" +
                $"Type: {moveData.PathType}"
            );

            return;
        }


        if (!moveData.Passable)
        {
            Debug.Log(
                "[이동] 이동할 수 없는 Open 방향입니다."
            );

            return;
        }


        if (
            !dungeonManager.CanMoveOpen(
                direction
            )
        )
        {
            return;
        }


        StartCoroutine(
            ChangeRoom(
                direction,
                false
            )
        );
    }


    // =========================================================
    // Space - Open Direction Panel
    // =========================================================

    private void OpenDirectionPanel()
    {
        // =====================================================
        // Battle
        // =====================================================

        if (IsBattleRunning())
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "전투 중에는 특수 통로를 사용할 수 없습니다."
            );

            return;
        }


        if (
            isTransitioning ||
            isInteracting
        )
        {
            return;
        }


        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        /*
         * 여기서 중요한 점:
         *
         * "사용 가능한 특수통로"가 아니라
         * "존재하는 특수통로"를 검사한다.
         *
         * 따라서
         * - 열쇠 없는 LockedDoor
         * - 역방향 OneWay
         * - 현재 닫힌 GimmickDoor
         *
         * 만 있어도 Space UI가 열린다.
         */
        if (
            !dungeonManager.HasAnySpecialPath()
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "현재 위치에는 Space로 사용할 " +
                "Door / OneWay / LockedDoor / " +
                "GimmickDoor 통로가 없습니다."
            );

            return;
        }


        if (uiManager != null)
        {
            uiManager
                .ShowSpecialDirectionPanel(
                    dungeonManager
                        .GetSpecialDirections()
                );
        }


        state =
            RoomState.DirectionChoosing;


        Debug.Log(
            "[RoomTraversalController] " +
            "특수 통로 방향 선택 패널 열기"
        );
    }


    // =========================================================
    // Close Direction Panel
    // =========================================================

    public void CloseDirectionPanel()
    {
        if (isTransitioning)
        {
            return;
        }


        if (uiManager != null)
        {
            uiManager
                .HideDirectionPanel();
        }


        state =
            RoomState.WaitingForInput;


        Debug.Log(
            "[RoomTraversalController] " +
            "방향 선택 취소"
        );
    }


    // =========================================================
    // Select Special Path
    // =========================================================

    public void SelectNextRoom(
        MoveDirection direction)
    {
        // =====================================================
        // Battle
        // =====================================================

        if (IsBattleRunning())
        {
            if (uiManager != null)
            {
                uiManager.HideDirectionPanel();
            }


            state =
                RoomState.WaitingForInput;


            Debug.Log(
                "[RoomTraversalController] " +
                "전투 중에는 특수 통로를 사용할 수 없습니다."
            );

            return;
        }


        // =====================================================
        // State
        // =====================================================

        if (
            state !=
            RoomState.DirectionChoosing
        )
        {
            return;
        }


        if (isTransitioning)
        {
            return;
        }


        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


        MoveData moveData =
            dungeonManager.GetMoveData(
                direction
            );


        if (moveData == null)
        {
            return;
        }


        // =====================================================
        // Must Be Special Path
        // =====================================================

        if (
            !dungeonManager.HasSpecialPath(
                direction
            )
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "선택한 방향에는 특수 통로가 없습니다."
            );

            return;
        }


        // =====================================================
        // Current Availability Check
        // =====================================================

        /*
         * Door
         * → true
         *
         * OneWay
         * → Passable에 따라 true / false
         *
         * LockedDoor
         * → 이미 열렸거나 열쇠 보유 시 true
         *
         * GimmickDoor
         * → 10 / 20 / 30 ... 턴에 true
         */
        if (
            !dungeonManager
                .CanUseSpecialPath(
                    direction
                )
        )
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "현재 사용할 수 없는 특수 통로입니다.\n" +
                $"방향: {direction}\n" +
                $"Type: {moveData.PathType}\n" +
                $"현재 턴: {dungeonManager.CurrentTurn}"
            );

            return;
        }


        // =====================================================
        // Locked Door
        // =====================================================

        /*
         * CanUseSpecialPath()에서는
         * 열쇠를 소비하지 않고 보유 여부만 검사했다.
         *
         * 실제 버튼을 선택한 지금 시점에서만
         * TryOpenDoor()를 실행하여 열쇠를 소비한다.
         */
        if (
            moveData.PathType ==
            MovePathType.LockedDoor
        )
        {
            LockedDoorManager
                lockedDoorManager =
                    LockedDoorManager.Instance;


            if (lockedDoorManager == null)
            {
                Debug.LogError(
                    "[RoomTraversalController] " +
                    "LockedDoorManager가 없습니다."
                );

                return;
            }


            bool opened =
                lockedDoorManager.IsOpened(
                    dungeonManager.CurrentRoom,
                    direction
                );


            if (!opened)
            {
                opened =
                    lockedDoorManager.TryOpenDoor(
                        dungeonManager.CurrentRoom,
                        direction
                    );
            }


            if (!opened)
            {
                Debug.Log(
                    "[RoomTraversalController] " +
                    "잠긴 문을 열 수 없습니다."
                );

                return;
            }
        }


        // =====================================================
        // Gimmick Door
        // =====================================================

        /*
         * 별도의 문 개방 상태를 저장하지 않는다.
         *
         * DungeonManager.CanUseSpecialPath()가
         * 현재 턴을 기준으로 이미 판정했다.
         *
         * 10 / 20 / 30 ... 턴
         * → 여기까지 도달하여 이동
         *
         * 그 외
         * → 위의 CanUseSpecialPath에서 차단
         */
        if (
            moveData.PathType ==
            MovePathType.GimmickDoor
        )
        {
            Debug.Log(
                "[GimmickDoor] 개방된 턴에 이동합니다.\n" +
                $"현재 턴: {dungeonManager.CurrentTurn}\n" +
                $"방향: {direction}"
            );
        }


        // =====================================================
        // Start Move
        // =====================================================

        Debug.Log(
            "[특수 이동 선택]\n" +
            $"현재 위치: {dungeonManager.CurrentRoom}\n" +
            $"방향: {direction}\n" +
            $"Type: {moveData.PathType}"
        );


        StartCoroutine(
            ChangeRoom(
                direction,
                true
            )
        );
    }


    // =========================================================
    // Change Room
    // =========================================================

    private IEnumerator ChangeRoom(
        MoveDirection direction,
        bool specialMove)
    {
        /*
         * 실제 방 변경 직전에도 전투를 다시 검사한다.
         *
         * 입력 직후 전투가 시작되는 등의
         * 타이밍 문제까지 방지한다.
         */
        if (IsBattleRunning())
        {
            Debug.Log(
                "[RoomTraversalController] " +
                "전투가 진행 중이므로 방 이동을 취소합니다."
            );

            yield break;
        }


        isTransitioning =
            true;


        state =
            RoomState.Transition;


        if (uiManager != null)
        {
            uiManager
                .HideDirectionPanel();
        }


        // =====================================================
        // Camera Movement
        // =====================================================

        if (
            cameraRoomTransition != null
        )
        {
            yield return
                cameraRoomTransition
                    .PlayRoomMove(
                        direction
                    );
        }


        /*
         * 카메라 연출 중 전투가 시작되는
         * 특수 상황도 방어한다.
         */
        if (IsBattleRunning())
        {
            if (cameraRoomTransition != null)
            {
                cameraRoomTransition
                    .ResetCameraPosition();
            }


            isTransitioning =
                false;


            state =
                RoomState.WaitingForInput;


            yield break;
        }


        // =====================================================
        // Fade Out
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController
                    .FadeOut();
        }


        /*
         * 실제 DungeonManager 이동 직전에
         * 마지막으로 전투 상태를 검사한다.
         */
        if (IsBattleRunning())
        {
            if (fadeController != null)
            {
                yield return
                    fadeController
                        .FadeIn();
            }


            if (cameraRoomTransition != null)
            {
                cameraRoomTransition
                    .ResetCameraPosition();
            }


            isTransitioning =
                false;


            state =
                RoomState.WaitingForInput;


            yield break;
        }


        // =====================================================
        // Move
        // =====================================================

        bool moved =
            false;


        if (dungeonManager != null)
        {
            moved =
                dungeonManager
                    .MoveToNextRoom(
                        direction
                    );
        }


        // =====================================================
        // Player Position Reset
        // =====================================================

        if (playerCenterPoint != null)
        {
            transform.position =
                playerCenterPoint.position;
        }


        // =====================================================
        // Camera Reset
        // =====================================================

        if (
            cameraRoomTransition != null
        )
        {
            cameraRoomTransition
                .ResetCameraPosition();
        }


        // =====================================================
        // Fade In
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController
                    .FadeIn();
        }


        isTransitioning =
            false;


        // =====================================================
        // Failed
        // =====================================================

        if (!moved)
        {
            state =
                RoomState.WaitingForInput;


            Debug.Log(
                "[RoomTraversalController] " +
                "방 이동에 실패했습니다."
            );


            yield break;
        }


        // =====================================================
        // Success
        // =====================================================

        Debug.Log(
            "[RoomTraversalController] 이동 완료\n" +
            "이동 방식: " +
            (
                specialMove
                    ? "특수 통로"
                    : "Open"
            ) +
            "\n현재 위치: " +
            dungeonManager.CurrentRoom
        );


        // =====================================================
        // New Room Event
        // =====================================================

        yield return StartCoroutine(
            RunRoomEnterEvent()
        );
    }
}