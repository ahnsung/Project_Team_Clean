using UnityEngine;

public class RoomDirectionVisualController : MonoBehaviour
{
    // =========================================================
    // Direction Visual Set
    // =========================================================

    [System.Serializable]
    public class DirectionVisualSet
    {
        [Header("Normal / Open")]
        [SerializeField]
        private GameObject normalVisual;

        [Header("Door")]
        [SerializeField]
        private GameObject doorVisual;

        [Header("One Way")]
        [SerializeField]
        private GameObject oneWayVisual;

        [Header("Locked Door")]
        [SerializeField]
        private GameObject lockedDoorVisual;

        [Header("Gimmick Door")]
        [SerializeField]
        private GameObject gimmickDoorVisual;


        public GameObject NormalVisual => normalVisual;
        public GameObject DoorVisual => doorVisual;
        public GameObject OneWayVisual => oneWayVisual;
        public GameObject LockedDoorVisual => lockedDoorVisual;
        public GameObject GimmickDoorVisual => gimmickDoorVisual;
    }


    // =========================================================
    // References
    // =========================================================

    [Header("References")]

    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private MoveDataLoader moveDataLoader;


    // =========================================================
    // Directions
    // =========================================================

    [Header("Up")]
    [SerializeField]
    private DirectionVisualSet up;

    [Header("Down")]
    [SerializeField]
    private DirectionVisualSet down;

    [Header("Left")]
    [SerializeField]
    private DirectionVisualSet left;

    [Header("Right")]
    [SerializeField]
    private DirectionVisualSet right;


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

    private bool initialized;

    /*
     * 직전 프레임의 전투 상태.
     *
     * 전투 시작 / 종료 순간을 감지해서
     * 방향 이미지를 갱신한다.
     */
    private bool wasBattleRunning;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();


        wasBattleRunning =
            IsBattleRunning();


        if (wasBattleRunning)
        {
            HideAll();
        }
        else
        {
            RefreshVisuals();
        }
    }


    private void Update()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            HideAll();

            return;
        }


        // =====================================================
        // Battle State
        // =====================================================

        bool battleRunning =
            IsBattleRunning();


        // -----------------------------------------------------
        // 전투 시작
        // -----------------------------------------------------

        if (
            battleRunning &&
            !wasBattleRunning
        )
        {
            HideAll();


            Print(
                "전투 시작 -> 이동 방향 이미지 전체 OFF"
            );
        }


        // -----------------------------------------------------
        // 전투 중
        // -----------------------------------------------------

        if (battleRunning)
        {
            /*
             * 다른 Refresh 호출이 들어오더라도
             * 전투 중에는 이동 방향 이미지가
             * 다시 나타나지 않도록 보장한다.
             */
            HideAll();


            wasBattleRunning =
                true;


            return;
        }


        // -----------------------------------------------------
        // 전투 종료
        // -----------------------------------------------------

        if (
            !battleRunning &&
            wasBattleRunning
        )
        {
            wasBattleRunning =
                false;


            RefreshVisuals();


            Print(
                "전투 종료 -> 현재 방 방향 이미지 복구"
            );


            return;
        }


        wasBattleRunning =
            false;


        // =====================================================
        // Room Change
        // =====================================================

        Vector2Int currentRoom =
            dungeonManager.CurrentRoom;


        if (
            !initialized ||
            currentRoom != lastRoom
        )
        {
            RefreshVisuals();
        }
    }


    // =========================================================
    // Battle
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


        if (moveDataLoader == null)
        {
            moveDataLoader =
                MoveDataLoader.Instance;
        }
    }


    // =========================================================
    // Refresh All
    // =========================================================

    public void RefreshVisuals()
    {
        ResolveReferences();


        // =====================================================
        // Battle
        // =====================================================

        /*
         * 외부에서 RefreshVisuals()를 직접 호출해도
         * 전투 중이면 절대 방향 이미지를 띄우지 않는다.
         */
        if (IsBattleRunning())
        {
            HideAll();

            return;
        }


        if (dungeonManager == null)
        {
            Debug.LogWarning(
                "[RoomDirectionVisual] DungeonManager가 없습니다."
            );


            HideAll();

            return;
        }


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


        lastRoom =
            dungeonManager.CurrentRoom;


        initialized =
            true;


        Print(
            "갱신 완료 / 현재 위치: " +
            dungeonManager.CurrentRoom
        );
    }


    // =========================================================
    // Direction
    // =========================================================

    private void RefreshDirection(
        MoveDirection direction,
        DirectionVisualSet visualSet)
    {
        if (visualSet == null)
        {
            return;
        }


        // =====================================================
        // Reset
        // =====================================================

        /*
         * 해당 방향에 이전 방 이미지가 남지 않도록
         * 일단 전부 끈 뒤 현재 MoveData에 맞는
         * 이미지 하나만 켠다.
         */
        HideDirection(
            visualSet
        );


        MoveData data =
            dungeonManager.GetMoveData(
                direction
            );


        if (data == null)
        {
            Print(
                direction +
                " / MoveData 없음 -> OFF"
            );


            return;
        }


        // =====================================================
        // WALL
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Wall
        )
        {
            Print(
                direction +
                " / Wall -> OFF"
            );


            return;
        }


        // =====================================================
        // OPEN
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Open
        )
        {
            /*
             * Open은 실제로 WASD 이동 가능한 경우에만
             * *_open 화살표를 보여준다.
             */
            if (
                !dungeonManager.CanMoveOpen(
                    direction
                )
            )
            {
                Print(
                    direction +
                    " / Open / 이동 불가 -> OFF"
                );


                return;
            }


            SetVisual(
                visualSet.NormalVisual,
                true
            );


            Print(
                direction +
                " / Open -> Open 화살표 ON"
            );


            return;
        }


        // =====================================================
        // DOOR
        // =====================================================

        if (
            data.PathType ==
            MovePathType.Door
        )
        {
            /*
             * Door는 "문이 존재한다"는 것을
             * 월드에서 보여주는 것이 목적이다.
             *
             * 현재 이동 가능 여부와
             * 월드 이미지 표시는 분리한다.
             */
            ShowWithFallback(
                visualSet.DoorVisual,
                visualSet.NormalVisual
            );


            Print(
                direction +
                " / Door -> Door ON"
            );


            return;
        }


        // =====================================================
        // ONE WAY
        // =====================================================

        if (
            data.PathType ==
            MovePathType.OneWay
        )
        {
            /*
             * 중요:
             *
             * OneWay O / X 모두
             * OneWay 통로 자체는 존재한다.
             *
             * 따라서 Passable 값과 관계없이
             * 월드에서는 OneWayVisual을 보여준다.
             *
             * Passable O/X에 따른 이동 가능 여부는
             * Space 방향 UI에서
             * 파랑 / 빨강으로 구분한다.
             */
            if (
                visualSet.OneWayVisual != null
            )
            {
                SetVisual(
                    visualSet.OneWayVisual,
                    true
                );
            }
            else if (
                visualSet.DoorVisual != null
            )
            {
                SetVisual(
                    visualSet.DoorVisual,
                    true
                );
            }
            else
            {
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / OneWay / Passable=" +
                data.Passable +
                " -> OneWay ON"
            );


            return;
        }


        // =====================================================
        // LOCKED DOOR
        // =====================================================

        if (
            data.PathType ==
            MovePathType.LockedDoor
        )
        {
            /*
             * 열쇠가 없어도 잠긴 문 자체는
             * 현재 방에 존재한다.
             *
             * 따라서 CanUseSpecialPath()를
             * 여기서는 검사하지 않는다.
             */
            if (
                visualSet.LockedDoorVisual != null
            )
            {
                SetVisual(
                    visualSet.LockedDoorVisual,
                    true
                );
            }
            else if (
                visualSet.DoorVisual != null
            )
            {
                SetVisual(
                    visualSet.DoorVisual,
                    true
                );
            }
            else
            {
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / LockedDoor -> LockedDoor ON"
            );


            return;
        }


        // =====================================================
        // GIMMICK DOOR
        // =====================================================

        if (
            data.PathType ==
            MovePathType.GimmickDoor
        )
        {
            /*
             * 10턴이 아니어도
             * 기믹문 자체는 방에 존재한다.
             *
             * 따라서 월드 이미지에는 항상 표시.
             *
             * 현재 통과 가능 여부는
             * Space UI의 파랑 / 빨강이 담당한다.
             */
            if (
                visualSet.GimmickDoorVisual != null
            )
            {
                SetVisual(
                    visualSet.GimmickDoorVisual,
                    true
                );
            }
            else if (
                visualSet.DoorVisual != null
            )
            {
                SetVisual(
                    visualSet.DoorVisual,
                    true
                );
            }
            else
            {
                SetVisual(
                    visualSet.NormalVisual,
                    true
                );
            }


            Print(
                direction +
                " / GimmickDoor -> GimmickDoor ON"
            );


            return;
        }


        // =====================================================
        // UNKNOWN
        // =====================================================

        Print(
            direction +
            " / 알 수 없는 PathType: " +
            data.PathType +
            " -> OFF"
        );
    }


    // =========================================================
    // Fallback
    // =========================================================

    private void ShowWithFallback(
        GameObject primary,
        GameObject fallback)
    {
        if (primary != null)
        {
            SetVisual(
                primary,
                true
            );


            return;
        }


        SetVisual(
            fallback,
            true
        );
    }


    // =========================================================
    // Hide Direction
    // =========================================================

    private void HideDirection(
        DirectionVisualSet visualSet)
    {
        if (visualSet == null)
        {
            return;
        }


        SetVisual(
            visualSet.NormalVisual,
            false
        );


        SetVisual(
            visualSet.DoorVisual,
            false
        );


        SetVisual(
            visualSet.OneWayVisual,
            false
        );


        SetVisual(
            visualSet.LockedDoorVisual,
            false
        );


        SetVisual(
            visualSet.GimmickDoorVisual,
            false
        );
    }


    // =========================================================
    // Hide All Direction Visuals
    // =========================================================

    private void HideAll()
    {
        /*
         * DirectionVisualRoot 자체는 끄지 않는다.
         *
         * 같은 Root 아래의
         * Farming / Rest / Teleport / Trap / Hint
         * 등에 영향을 주지 않기 위해
         * Up / Down / Left / Right의
         * 이동 관련 이미지만 끈다.
         */

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
    }


    // =========================================================
    // Set Active
    // =========================================================

    private void SetVisual(
        GameObject target,
        bool active)
    {
        if (target == null)
        {
            return;
        }


        /*
         * 위치 / 회전 / Scale은 절대 수정하지 않는다.
         *
         * Inspector에서 맞춰둔 Transform을 그대로 두고
         * 활성화 여부만 변경한다.
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
    // Debug
    // =========================================================

    private void Print(
        string message)
    {
        if (!printLog)
        {
            return;
        }


        Debug.Log(
            "[RoomDirectionVisual] " +
            message
        );
    }
}