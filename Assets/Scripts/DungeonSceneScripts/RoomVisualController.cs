using UnityEngine;

public class RoomVisualController : MonoBehaviour
{
    // =========================================================
    // References
    // =========================================================

    [Header("References")]
    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private MoveDataLoader moveDataLoader;


    // =========================================================
    // UP
    // =========================================================

    [Header("Up")]
    [SerializeField]
    private GameObject upDoor;

    [SerializeField]
    private GameObject upOneWay;


    // =========================================================
    // DOWN
    // =========================================================

    [Header("Down")]
    [SerializeField]
    private GameObject downDoor;

    [SerializeField]
    private GameObject downOneWay;


    // =========================================================
    // LEFT
    // =========================================================

    [Header("Left")]
    [SerializeField]
    private GameObject leftDoor;

    [SerializeField]
    private GameObject leftOneWay;


    // =========================================================
    // RIGHT
    // =========================================================

    [Header("Right")]
    [SerializeField]
    private GameObject rightDoor;

    [SerializeField]
    private GameObject rightOneWay;


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


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();

        HideAll();

        RefreshVisuals();
    }


    private void Update()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            return;
        }


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
    // Refresh
    // =========================================================

    public void RefreshVisuals()
    {
        ResolveReferences();


        if (
            dungeonManager == null ||
            moveDataLoader == null
        )
        {
            return;
        }


        HideAll();


        Vector2Int room =
            dungeonManager.CurrentRoom;


        ApplyDirection(
            room,
            MoveDirection.Up,
            upDoor,
            upOneWay
        );


        ApplyDirection(
            room,
            MoveDirection.Down,
            downDoor,
            downOneWay
        );


        ApplyDirection(
            room,
            MoveDirection.Left,
            leftDoor,
            leftOneWay
        );


        ApplyDirection(
            room,
            MoveDirection.Right,
            rightDoor,
            rightOneWay
        );


        lastRoom =
            room;


        initialized =
            true;


        if (printLog)
        {
            Debug.Log(
                "[RoomVisualController] " +
                "방 방향 이미지 갱신\n" +
                $"현재 위치: {room}"
            );
        }
    }


    // =========================================================
    // Direction
    // =========================================================

    private void ApplyDirection(
        Vector2Int room,
        MoveDirection direction,
        GameObject doorObject,
        GameObject oneWayObject)
    {
        MoveData data =
            moveDataLoader.GetMoveData(
                room,
                direction
            );


        if (data == null)
        {
            PrintLog(
                direction,
                "No Data"
            );

            return;
        }


        switch (data.PathType)
        {
            // =================================================
            // Open
            // =================================================

            case MovePathType.Open:

                /*
                 * 현재 Open 전용 이미지가 없으므로
                 * 아무것도 표시하지 않는다.
                 */

                PrintLog(
                    direction,
                    "Open"
                );

                break;


            // =================================================
            // Wall
            // =================================================

            case MovePathType.Wall:

                /*
                 * 벽 역시 현재 별도 오버레이 없음.
                 */

                PrintLog(
                    direction,
                    "Wall"
                );

                break;


            // =================================================
            // Door
            // =================================================

            case MovePathType.Door:

                SetActive(
                    doorObject,
                    true
                );


                PrintLog(
                    direction,
                    "Door ON"
                );

                break;


            // =================================================
            // OneWay
            // =================================================

            case MovePathType.OneWay:

                SetActive(
                    oneWayObject,
                    true
                );


                PrintLog(
                    direction,
                    "OneWay ON"
                );

                break;


            // =================================================
            // LockedDoor
            // =================================================

            case MovePathType.LockedDoor:

                /*
                 * 현재 LockedDoor 전용 월드 이미지가
                 * 없으므로 Door 이미지 사용.
                 *
                 * 나중에 전용 리소스가 오면
                 * LockedDoor 슬롯을 따로 추가하면 된다.
                 */

                SetActive(
                    doorObject,
                    true
                );


                PrintLog(
                    direction,
                    "LockedDoor -> Door ON"
                );

                break;


            // =================================================
            // GimmickDoor
            // =================================================

            case MovePathType.GimmickDoor:

                /*
                 * 전용 리소스가 아직 없으므로
                 * 일단 Door 이미지 사용.
                 */

                SetActive(
                    doorObject,
                    true
                );


                PrintLog(
                    direction,
                    "GimmickDoor -> Door ON"
                );

                break;
        }
    }


    // =========================================================
    // Hide
    // =========================================================

    private void HideAll()
    {
        SetActive(
            upDoor,
            false
        );

        SetActive(
            upOneWay,
            false
        );


        SetActive(
            downDoor,
            false
        );

        SetActive(
            downOneWay,
            false
        );


        SetActive(
            leftDoor,
            false
        );

        SetActive(
            leftOneWay,
            false
        );


        SetActive(
            rightDoor,
            false
        );

        SetActive(
            rightOneWay,
            false
        );
    }


    // =========================================================
    // Utility
    // =========================================================

    private void SetActive(
        GameObject target,
        bool value)
    {
        if (target == null)
        {
            return;
        }


        target.SetActive(
            value
        );
    }


    // =========================================================
    // Debug
    // =========================================================

    private void PrintLog(
        MoveDirection direction,
        string result)
    {
        if (!printLog)
        {
            return;
        }


        Debug.Log(
            "[RoomVisual] " +
            $"{direction}: {result}"
        );
    }


    [ContextMenu(
        "DEBUG - 방향 이미지 강제 갱신"
    )]
    private void DebugRefresh()
    {
        initialized =
            false;


        RefreshVisuals();
    }
}