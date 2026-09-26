using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUIManager : MonoBehaviour
{
    // =========================================================
    // References
    // =========================================================

    [Header("References")]

    [SerializeField]
    private DungeonManager dungeonManager;

    [SerializeField]
    private DungeonMapDatabase mapDatabase;

    [SerializeField]
    private MoveDataLoader moveDataLoader;

    [SerializeField]
    private RectTransform gridRoot;

    [SerializeField]
    private GameObject cellPrefab;


    // =========================================================
    // MapTile Sprites
    // =========================================================

    [Header("MapTile Sprites")]

    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite farmingSprite;

    [SerializeField]
    private Sprite trapSprite;

    [SerializeField]
    private Sprite keySprite;

    [SerializeField]
    private Sprite teleportSprite;

    [SerializeField]
    private Sprite chestSprite;

    [SerializeField]
    private Sprite eventSprite;

    [SerializeField]
    private Sprite puzzleLetterSprite;

    [SerializeField]
    private Sprite restSprite;

    [SerializeField]
    private Sprite bossSprite;


    // =========================================================
    // Path Sprites
    // =========================================================

    [Header("Path Sprites")]

    [SerializeField]
    private Sprite wallSprite;

    [SerializeField]
    private Sprite doorSprite;

    [SerializeField]
    private Sprite lockedDoorSprite;

    [SerializeField]
    private Sprite oneWayDoorSprite;


    // =========================================================
    // Path Size
    // =========================================================

    [Header("Path Size")]

    [Tooltip("Wall 두께")]
    [SerializeField]
    [Range(0.05f, 0.50f)]
    private float wallThicknessRatio = 0.22f;

    [Tooltip("Door 폭")]
    [SerializeField]
    [Range(0.10f, 0.60f)]
    private float doorThicknessRatio = 0.32f;

    [Tooltip("Door 길이")]
    [SerializeField]
    [Range(0.30f, 1.20f)]
    private float doorLengthRatio = 0.90f;

    [Tooltip(
        "Wall / Door를 타일 바깥쪽으로 얼마나 밀어낼지 결정합니다."
    )]
    [SerializeField]
    [Range(0f, 0.30f)]
    private float pathOutwardOffsetRatio = 0.08f;


    // =========================================================
    // Current Position
    // =========================================================

    [Header("Current Position")]

    [SerializeField]
    private Color currentBorderColor =
        new Color(
            0.15f,
            1f,
            0.15f,
            1f
        );

    [SerializeField]
    [Range(1f, 8f)]
    private float currentBorderThickness = 2f;


    // =========================================================
    // Minimap View
    // =========================================================

    [Header("Minimap View")]

    [SerializeField]
    [Range(3, 15)]
    private int viewSize = 9;


    // =========================================================
    // Runtime Cell
    // =========================================================

    private class MinimapCell
    {
        public RectTransform rootRect;

        public Image tileImage;

        public Image upPathImage;
        public Image rightPathImage;
        public Image downPathImage;
        public Image leftPathImage;

        public GameObject currentBorder;
    }


    private readonly List<MinimapCell> cells =
        new List<MinimapCell>();


    private GridLayoutGroup gridLayout;

    // 실제 표시 범위
    // 세로는 기존 viewSize를 유지하고,
    // 가로는 MinimapRoot의 실제 폭에 맞춰 자동 확장한다.
    private int runtimeViewWidth;
    private int runtimeViewHeight;


    // =========================================================
    // Unity
    // =========================================================

    private IEnumerator Start()
    {
        ResolveReferences();


        if (dungeonManager == null)
        {
            Debug.LogError(
                "[MinimapUIManager] DungeonManager를 찾을 수 없습니다."
            );

            yield break;
        }


        if (mapDatabase == null)
        {
            Debug.LogError(
                "[MinimapUIManager] DungeonMapDatabase를 찾을 수 없습니다."
            );

            yield break;
        }


        if (gridRoot == null)
        {
            Debug.LogError(
                "[MinimapUIManager] Grid Root가 연결되지 않았습니다."
            );

            yield break;
        }


        if (cellPrefab == null)
        {
            Debug.LogError(
                "[MinimapUIManager] Cell Prefab이 연결되지 않았습니다."
            );

            yield break;
        }


        gridLayout =
            gridRoot.GetComponent<GridLayoutGroup>();


        if (gridLayout == null)
        {
            Debug.LogError(
                "[MinimapUIManager] GridRoot에 GridLayoutGroup이 없습니다."
            );

            yield break;
        }


        float timeout = 5f;
        float elapsed = 0f;


        while (
            mapDatabase.MapData == null ||
            mapDatabase.MapData.Tiles.Count == 0
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            if (elapsed >= timeout)
            {
                Debug.LogError(
                    "[MinimapUIManager] 맵 데이터 로드 대기 시간 초과"
                );

                yield break;
            }


            yield return null;
        }


        if (viewSize % 2 == 0)
        {
            viewSize++;
        }


        yield return null;


        ConfigureGrid();

        BuildGrid();


        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            gridRoot
        );

        Canvas.ForceUpdateCanvases();


        yield return null;


        RefreshMinimap();


        Debug.Log(
            "[MinimapUIManager] 초기 미니맵 표시 완료"
        );
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
    }


    // =========================================================
    // Grid
    // =========================================================

    private void ConfigureGrid()
    {
        Canvas.ForceUpdateCanvases();


        Rect rect =
            gridRoot.rect;


        float availableWidth =
            rect.width
            - gridLayout.padding.left
            - gridLayout.padding.right;


        float availableHeight =
            rect.height
            - gridLayout.padding.top
            - gridLayout.padding.bottom;


        // 기존 viewSize는 세로 표시 칸 수로 유지한다.
        runtimeViewHeight =
            Mathf.Max(
                1,
                viewSize
            );


        if (runtimeViewHeight % 2 == 0)
        {
            runtimeViewHeight++;
        }


        // 세로 9칸에 맞춰 기존과 같은 정사각형 Cell 크기를 유지한다.
        float cellSize =
            availableHeight /
            runtimeViewHeight;


        // 같은 Cell 크기로 가로 패널에 들어갈 수 있는 만큼 확장한다.
        runtimeViewWidth =
            Mathf.FloorToInt(
                availableWidth /
                cellSize
            );


        runtimeViewWidth =
            Mathf.Max(
                1,
                runtimeViewWidth
            );


        // 플레이어가 정확히 중앙 칸에 오도록 홀수 칸으로 맞춘다.
        // 패널을 넘지 않게 짝수면 한 칸 줄인다.
        if (runtimeViewWidth % 2 == 0)
        {
            runtimeViewWidth--;
        }


        gridLayout.spacing =
            Vector2.zero;


        gridLayout.cellSize =
            new Vector2(
                cellSize,
                cellSize
            );


        gridLayout.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;


        gridLayout.constraintCount =
            runtimeViewWidth;


        gridLayout.startCorner =
            GridLayoutGroup.Corner.UpperLeft;


        gridLayout.startAxis =
            GridLayoutGroup.Axis.Horizontal;


        gridLayout.childAlignment =
            TextAnchor.MiddleCenter;


        Debug.Log(
            "[MinimapUIManager] " +
            $"미니맵 {runtimeViewWidth}x{runtimeViewHeight} 설정 완료 / " +
            $"Cell Size: {cellSize}"
        );
    }


    // =========================================================
    // Build Grid
    // =========================================================

    private void BuildGrid()
    {
        for (
            int i = gridRoot.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                gridRoot.GetChild(i).gameObject
            );
        }


        cells.Clear();


        int cellCount =
            runtimeViewWidth *
            runtimeViewHeight;


        for (
            int i = 0;
            i < cellCount;
            i++
        )
        {
            GameObject obj =
                Instantiate(
                    cellPrefab,
                    gridRoot
                );


            RectTransform rootRect =
                obj.GetComponent<RectTransform>();


            Image tileImage =
                obj.GetComponent<Image>();


            if (
                rootRect == null ||
                tileImage == null
            )
            {
                Debug.LogError(
                    "[MinimapUIManager] " +
                    "CellPrefab에는 RectTransform과 Image가 필요합니다."
                );

                Destroy(obj);

                continue;
            }


            tileImage.enabled =
                false;


            tileImage.preserveAspect =
                true;


            tileImage.raycastTarget =
                false;


            Image upPath =
                CreatePathImage(
                    obj.transform,
                    "Path_Up"
                );


            Image rightPath =
                CreatePathImage(
                    obj.transform,
                    "Path_Right"
                );


            Image downPath =
                CreatePathImage(
                    obj.transform,
                    "Path_Down"
                );


            Image leftPath =
                CreatePathImage(
                    obj.transform,
                    "Path_Left"
                );


            GameObject border =
                CreateCurrentBorder(
                    obj.transform
                );


            border.SetActive(
                false
            );


            MinimapCell cell =
                new MinimapCell
                {
                    rootRect =
                        rootRect,

                    tileImage =
                        tileImage,

                    upPathImage =
                        upPath,

                    rightPathImage =
                        rightPath,

                    downPathImage =
                        downPath,

                    leftPathImage =
                        leftPath,

                    currentBorder =
                        border
                };


            cells.Add(
                cell
            );
        }


        Debug.Log(
            "[MinimapUIManager] 미니맵 Cell 생성 완료: " +
            cells.Count
        );
    }


    // =========================================================
    // Path Image
    // =========================================================

    private Image CreatePathImage(
        Transform parent,
        string objectName)
    {
        GameObject obj =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );


        obj.transform.SetParent(
            parent,
            false
        );


        RectTransform rect =
            obj.GetComponent<RectTransform>();


        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        rect.anchoredPosition =
            Vector2.zero;


        rect.sizeDelta =
            Vector2.zero;


        rect.localRotation =
            Quaternion.identity;


        Image image =
            obj.GetComponent<Image>();


        image.enabled =
            false;


        // 원본 Sprite 비율을 유지
        image.preserveAspect =
            true;


        image.raycastTarget =
            false;


        return image;
    }


    // =========================================================
    // Current Border
    // =========================================================

    private GameObject CreateCurrentBorder(
        Transform parent)
    {
        GameObject borderRoot =
            new GameObject(
                "CurrentBorder",
                typeof(RectTransform)
            );


        borderRoot.transform.SetParent(
            parent,
            false
        );


        RectTransform rootRect =
            borderRoot.GetComponent<RectTransform>();


        rootRect.anchorMin =
            Vector2.zero;


        rootRect.anchorMax =
            Vector2.one;


        rootRect.offsetMin =
            Vector2.zero;


        rootRect.offsetMax =
            Vector2.zero;


        CreateBorderLine(
            borderRoot.transform,
            "Top",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(
                0f,
                -currentBorderThickness
            ),
            Vector2.zero
        );


        CreateBorderLine(
            borderRoot.transform,
            "Bottom",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            Vector2.zero,
            new Vector2(
                0f,
                currentBorderThickness
            )
        );


        CreateBorderLine(
            borderRoot.transform,
            "Left",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            Vector2.zero,
            new Vector2(
                currentBorderThickness,
                0f
            )
        );


        CreateBorderLine(
            borderRoot.transform,
            "Right",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(
                -currentBorderThickness,
                0f
            ),
            Vector2.zero
        );


        return borderRoot;
    }


    private void CreateBorderLine(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject lineObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );


        lineObject.transform.SetParent(
            parent,
            false
        );


        RectTransform rect =
            lineObject.GetComponent<RectTransform>();


        rect.anchorMin =
            anchorMin;


        rect.anchorMax =
            anchorMax;


        rect.offsetMin =
            offsetMin;


        rect.offsetMax =
            offsetMax;


        Image image =
            lineObject.GetComponent<Image>();


        image.color =
            currentBorderColor;


        image.raycastTarget =
            false;
    }


    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshMinimap()
    {
        ResolveReferences();


        if (
            dungeonManager == null ||
            mapDatabase == null
        )
        {
            return;
        }


        if (cells.Count == 0)
        {
            return;
        }


        Vector2Int center =
            dungeonManager.CurrentRoom;


        int radiusX =
            runtimeViewWidth / 2;

        int radiusY =
            runtimeViewHeight / 2;


        int index = 0;


        /*
         * 화면 위   = North = Y - 1
         * 화면 아래 = South = Y + 1
         */

        for (
            int row = 0;
            row < runtimeViewHeight;
            row++
        )
        {
            int offsetY =
                row - radiusY;


            for (
                int column = 0;
                column < runtimeViewWidth;
                column++
            )
            {
                if (index >= cells.Count)
                {
                    return;
                }


                int offsetX =
                    column - radiusX;


                Vector2Int worldPosition =
                    new Vector2Int(
                        center.x + offsetX,
                        center.y + offsetY
                    );


                MinimapCell cell =
                    cells[index];


                index++;


                // =================================================
                // Base Camp
                // =================================================

                /*
                 * Base Camp는 DungeonManager.StartRoom으로 관리되며
                 * Tile_Data에는 존재하지 않는 특수 좌표다.
                 *
                 * 따라서 일반 타일 조회 결과가 null이어도
                 * Base Camp 좌표라면 미니맵의 정상적인 한 칸으로
                 * 표시한다.
                 */
                bool isBaseCamp =
                    worldPosition ==
                    dungeonManager.StartRoom;


                DungeonTileData tile =
                    mapDatabase.GetTile(
                        worldPosition
                    );


                if (
                    !isBaseCamp &&
                    (
                        tile == null ||
                        tile.TileType ==
                        DungeonTileType.None
                    )
                )
                {
                    HideCell(
                        cell
                    );

                    continue;
                }


                bool isCurrent =
                    worldPosition ==
                    center;


                bool isVisited =
                    dungeonManager.IsVisited(
                        worldPosition
                    );


                if (
                    !isCurrent &&
                    !isVisited
                )
                {
                    HideCell(
                        cell
                    );

                    continue;
                }


                if (isBaseCamp)
                {
                    /*
                     * Base Camp 전용 Sprite는 아직 사용하지 않으므로
                     * 일반 방 Sprite로 표시한다.
                     */
                    ShowTile(
                        cell,
                        DungeonTileType.General
                    );
                }
                else
                {
                    ShowTile(
                        cell,
                        tile.TileType
                    );
                }


                /*
                 * 방 사이 경로 표시(Path_Up / Path_Right /
                 * Path_Down / Path_Left)는 사용하지 않는다.
                 *
                 * 실제 MoveData / 이동 판정은 전혀 건드리지 않고,
                 * 미니맵에서 시각적인 연결선만 표시하지 않는다.
                 *
                 * ShowTile()에서 각 Path Image를 OFF하므로
                 * 여기서는 RefreshPathImages()를 호출하지 않는다.
                 */


                if (cell.currentBorder != null)
                {
                    cell.currentBorder.SetActive(
                        isCurrent
                    );
                }
            }
        }
    }


    // =========================================================
    // Tile
    // =========================================================

    private void ShowTile(
        MinimapCell cell,
        DungeonTileType tileType)
    {
        if (
            cell == null ||
            cell.tileImage == null
        )
        {
            return;
        }


        Sprite sprite =
            GetTileSprite(
                tileType
            );


        if (sprite == null)
        {
            cell.tileImage.enabled =
                false;

            return;
        }


        cell.tileImage.sprite =
            sprite;


        cell.tileImage.enabled =
            true;
    }


    private Sprite GetTileSprite(
        DungeonTileType tileType)
    {
        switch (tileType)
        {
            case DungeonTileType.General:

                return normalSprite;


            case DungeonTileType.Farming:

                return farmingSprite;


            case DungeonTileType.Trap:

                return trapSprite;


            case DungeonTileType.Key:

                return keySprite;


            case DungeonTileType.Teleport:

                return teleportSprite;


            case DungeonTileType.Chest:

                return chestSprite;


            case DungeonTileType.EventHint:

                return eventSprite;


            case DungeonTileType.PuzzleLetter:

                return
                    puzzleLetterSprite != null
                        ? puzzleLetterSprite
                        : normalSprite;


            case DungeonTileType.Rest:

                return
                    restSprite != null
                        ? restSprite
                        : normalSprite;


            case DungeonTileType.Boss:

                return
                    bossSprite != null
                        ? bossSprite
                        : normalSprite;


            default:

                return normalSprite;
        }
    }


    // =========================================================
    // Path Refresh
    // =========================================================

    private void RefreshPathImages(
        MinimapCell cell,
        Vector2Int position)
    {
        HideAllPathImages(
            cell
        );


        if (
            cell == null ||
            cell.rootRect == null
        )
        {
            return;
        }


        if (
            moveDataLoader == null ||
            !moveDataLoader.IsLoaded
        )
        {
            return;
        }


        RefreshSinglePath(
            cell,
            cell.upPathImage,
            position,
            MoveDirection.Up
        );


        RefreshSinglePath(
            cell,
            cell.rightPathImage,
            position,
            MoveDirection.Right
        );


        RefreshSinglePath(
            cell,
            cell.downPathImage,
            position,
            MoveDirection.Down
        );


        RefreshSinglePath(
            cell,
            cell.leftPathImage,
            position,
            MoveDirection.Left
        );
    }


    private void RefreshSinglePath(
        MinimapCell cell,
        Image image,
        Vector2Int position,
        MoveDirection direction)
    {
        if (
            cell == null ||
            image == null
        )
        {
            return;
        }


        MoveData moveData =
            moveDataLoader.GetMoveData(
                position,
                direction
            );


        if (moveData == null)
        {
            image.enabled =
                false;

            return;
        }


        Sprite sprite =
            GetPathSprite(
                moveData.PathType
            );


        /*
         * 중요:
         *
         * Passable이 false여도
         * OneWay / LockedDoor / Wall 등
         * Path 자체는 표시한다.
         */

        if (sprite == null)
        {
            image.enabled =
                false;

            return;
        }


        image.sprite =
            sprite;


        LayoutPathImage(
            cell,
            image,
            direction,
            moveData.PathType
        );


        image.enabled =
            true;
    }


    // =========================================================
    // Path Layout
    // =========================================================

    private void LayoutPathImage(
        MinimapCell cell,
        Image image,
        MoveDirection direction,
        MovePathType pathType)
    {
        if (
            cell == null ||
            cell.rootRect == null ||
            image == null
        )
        {
            return;
        }


        float cellWidth =
            cell.rootRect.rect.width;


        float cellHeight =
            cell.rootRect.rect.height;


        if (cellWidth <= 0.01f)
        {
            cellWidth =
                gridLayout.cellSize.x;
        }


        if (cellHeight <= 0.01f)
        {
            cellHeight =
                gridLayout.cellSize.y;
        }


        RectTransform pathRect =
            image.rectTransform;


        if (
            pathType ==
            MovePathType.Wall
        )
        {
            LayoutWall(
                pathRect,
                direction,
                cellWidth,
                cellHeight
            );

            return;
        }


        LayoutDoor(
            pathRect,
            direction,
            cellWidth,
            cellHeight
        );
    }


    // =========================================================
    // Wall
    // =========================================================

    private void LayoutWall(
        RectTransform rect,
        MoveDirection direction,
        float cellWidth,
        float cellHeight)
    {
        float shortSide =
            Mathf.Min(
                cellWidth,
                cellHeight
            );


        float outwardOffset =
            shortSide *
            pathOutwardOffsetRatio;


        float horizontalThickness =
            Mathf.Max(
                1f,
                shortSide *
                wallThicknessRatio
            );


        float verticalThickness =
            horizontalThickness;


        switch (direction)
        {
            case MoveDirection.Up:

                rect.localRotation =
                    Quaternion.identity;


                rect.sizeDelta =
                    new Vector2(
                        cellWidth * 1.05f,
                        horizontalThickness
                    );


                rect.anchoredPosition =
                    new Vector2(
                        0f,
                        (cellHeight * 0.5f)
                        +
                        outwardOffset
                    );

                break;


            case MoveDirection.Down:

                rect.localRotation =
                    Quaternion.identity;


                rect.sizeDelta =
                    new Vector2(
                        cellWidth * 1.05f,
                        horizontalThickness
                    );


                rect.anchoredPosition =
                    new Vector2(
                        0f,
                        -(cellHeight * 0.5f)
                        -
                        outwardOffset
                    );

                break;


            case MoveDirection.Left:

                rect.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f
                    );


                rect.sizeDelta =
                    new Vector2(
                        cellHeight * 1.05f,
                        verticalThickness
                    );


                rect.anchoredPosition =
                    new Vector2(
                        -(cellWidth * 0.5f)
                        -
                        outwardOffset,
                        0f
                    );

                break;


            case MoveDirection.Right:

                rect.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f
                    );


                rect.sizeDelta =
                    new Vector2(
                        cellHeight * 1.05f,
                        verticalThickness
                    );


                rect.anchoredPosition =
                    new Vector2(
                        (cellWidth * 0.5f)
                        +
                        outwardOffset,
                        0f
                    );

                break;
        }
    }


    // =========================================================
    // Door
    // =========================================================

    private void LayoutDoor(
        RectTransform rect,
        MoveDirection direction,
        float cellWidth,
        float cellHeight)
    {
        float shortSide =
            Mathf.Min(
                cellWidth,
                cellHeight
            );


        float outwardOffset =
            shortSide *
            pathOutwardOffsetRatio;


        float thickness =
            Mathf.Max(
                1f,
                shortSide *
                doorThicknessRatio
            );


        float horizontalLength =
            cellWidth *
            doorLengthRatio;


        float verticalLength =
            cellHeight *
            doorLengthRatio;


        switch (direction)
        {
            // ---------------------------------------------
            // North
            // ---------------------------------------------

            case MoveDirection.Up:

                rect.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f
                    );


                rect.sizeDelta =
                    new Vector2(
                        thickness,
                        horizontalLength
                    );


                rect.anchoredPosition =
                    new Vector2(
                        0f,
                        (cellHeight * 0.5f)
                        +
                        outwardOffset
                    );

                break;


            // ---------------------------------------------
            // South
            // ---------------------------------------------

            case MoveDirection.Down:

                rect.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f
                    );


                rect.sizeDelta =
                    new Vector2(
                        thickness,
                        horizontalLength
                    );


                rect.anchoredPosition =
                    new Vector2(
                        0f,
                        -(cellHeight * 0.5f)
                        -
                        outwardOffset
                    );

                break;


            // ---------------------------------------------
            // West
            // ---------------------------------------------

            case MoveDirection.Left:

                rect.localRotation =
                    Quaternion.identity;


                rect.sizeDelta =
                    new Vector2(
                        thickness,
                        verticalLength
                    );


                rect.anchoredPosition =
                    new Vector2(
                        -(cellWidth * 0.5f)
                        -
                        outwardOffset,
                        0f
                    );

                break;


            // ---------------------------------------------
            // East
            // ---------------------------------------------

            case MoveDirection.Right:

                rect.localRotation =
                    Quaternion.identity;


                rect.sizeDelta =
                    new Vector2(
                        thickness,
                        verticalLength
                    );


                rect.anchoredPosition =
                    new Vector2(
                        (cellWidth * 0.5f)
                        +
                        outwardOffset,
                        0f
                    );

                break;
        }
    }


    // =========================================================
    // Path Sprite
    // =========================================================

    private Sprite GetPathSprite(
        MovePathType pathType)
    {
        switch (pathType)
        {
            case MovePathType.Open:

                return null;


            case MovePathType.Wall:

                return wallSprite;


            case MovePathType.Door:

                return doorSprite;


            case MovePathType.OneWay:

                return oneWayDoorSprite;


            case MovePathType.LockedDoor:

                return lockedDoorSprite;


            case MovePathType.GimmickDoor:

                return doorSprite;


            default:

                return null;
        }
    }


    // =========================================================
    // Hide
    // =========================================================

    private void HideAllPathImages(
        MinimapCell cell)
    {
        if (cell == null)
        {
            return;
        }


        SetImageEnabled(
            cell.upPathImage,
            false
        );


        SetImageEnabled(
            cell.rightPathImage,
            false
        );


        SetImageEnabled(
            cell.downPathImage,
            false
        );


        SetImageEnabled(
            cell.leftPathImage,
            false
        );
    }


    private void HideCell(
        MinimapCell cell)
    {
        if (cell == null)
        {
            return;
        }


        SetImageEnabled(
            cell.tileImage,
            false
        );


        HideAllPathImages(
            cell
        );


        if (cell.currentBorder != null)
        {
            cell.currentBorder.SetActive(
                false
            );
        }
    }


    private void SetImageEnabled(
        Image image,
        bool value)
    {
        if (image != null)
        {
            image.enabled =
                value;
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("DEBUG - Current Move Data")]
    private void DebugCurrentMoveData()
    {
        ResolveReferences();


        if (
            dungeonManager == null ||
            moveDataLoader == null
        )
        {
            Debug.LogWarning(
                "[MinimapUIManager] Debug에 필요한 참조가 없습니다."
            );

            return;
        }


        Vector2Int position =
            dungeonManager.CurrentRoom;


        Debug.Log(
            "[MinimapUIManager] 현재 위치 Move Data 확인: " +
            position
        );


        DebugDirection(
            position,
            MoveDirection.Up
        );


        DebugDirection(
            position,
            MoveDirection.Right
        );


        DebugDirection(
            position,
            MoveDirection.Down
        );


        DebugDirection(
            position,
            MoveDirection.Left
        );
    }


    private void DebugDirection(
        Vector2Int position,
        MoveDirection direction)
    {
        MoveData data =
            moveDataLoader.GetMoveData(
                position,
                direction
            );


        if (data == null)
        {
            Debug.Log(
                $"{direction}: 데이터 없음"
            );

            return;
        }


        Debug.Log(
            $"{direction}: " +
            $"Type={data.PathType} / " +
            $"Passable={data.Passable}"
        );
    }
}