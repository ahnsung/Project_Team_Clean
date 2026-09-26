using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUIManager : MonoBehaviour
{
    // =========================================================
    // Panels
    // =========================================================

    [Header("Panels")]
    [SerializeField]
    private GameObject directionPanel;

    [SerializeField]
    private GameObject minimapRoot;


    // =========================================================
    // Direction Buttons
    // =========================================================

    [Header("Direction Buttons")]

    [SerializeField]
    private Button upButton;

    [SerializeField]
    private Button downButton;

    [SerializeField]
    private Button leftButton;

    [SerializeField]
    private Button rightButton;


    // =========================================================
    // Special Direction Colors
    // =========================================================

    [Header("Special Direction Colors")]

    [Tooltip("현재 이동 가능한 특수 통로 방향 색상")]
    [SerializeField]
    private Color availableColor =
        new Color(
            0.20f,
            0.55f,
            1.00f,
            1.00f
        );

    [Tooltip("현재 이동 불가능한 특수 통로 방향 색상")]
    [SerializeField]
    private Color unavailableColor =
        new Color(
            1.00f,
            0.20f,
            0.20f,
            1.00f
        );


    // =========================================================
    // Reference
    // =========================================================

    [Header("Reference")]

    [SerializeField]
    private RoomTraversalController roomTraversalController;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveReferences();


        HideDirectionPanel();


        if (minimapRoot != null)
        {
            minimapRoot.SetActive(
                true
            );
        }
    }


    // =========================================================
    // References
    // =========================================================

    private void ResolveReferences()
    {
        if (roomTraversalController == null)
        {
            roomTraversalController =
                FindFirstObjectByType<
                    RoomTraversalController
                >();
        }
    }


    // =========================================================
    // Generic Button Refresh
    // =========================================================

    /*
     * DungeonManager.RefreshAll() 등에서
     * 기존 호환을 위해 유지한다.
     *
     * 일반 방향 갱신에서는
     * 버튼 GameObject를 숨기지 않고
     * interactable만 변경한다.
     */
    public void RefreshDirectionButtons(
        Dictionary<
            MoveDirection,
            bool
        > availableDirections)
    {
        SetButtonInteractable(
            upButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Up
            )
        );


        SetButtonInteractable(
            downButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Down
            )
        );


        SetButtonInteractable(
            leftButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Left
            )
        );


        SetButtonInteractable(
            rightButton,
            IsAvailable(
                availableDirections,
                MoveDirection.Right
            )
        );
    }


    // =========================================================
    // Special Direction Panel
    // =========================================================

    /*
     * 기존 함수 이름을 유지한다.
     *
     * DungeonManager에서
     *
     * existence:
     * 특수통로 자체가 존재하는가?
     *
     * available:
     * 현재 실제로 이동 가능한가?
     *
     * 두 정보를 받아서
     *
     * 없음
     * → 숨김
     *
     * 있음 + 가능
     * → 파랑
     *
     * 있음 + 불가능
     * → 빨강
     *
     * 으로 표시한다.
     */
    public void ShowSpecialDirectionPanel(
        Dictionary<
            MoveDirection,
            bool
        > availableDirections)
    {
        ResolveReferences();


        DungeonManager dungeonManager =
            DungeonManager.Instance;


        if (dungeonManager == null)
        {
            Debug.LogWarning(
                "[DungeonUIManager] " +
                "DungeonManager가 없습니다."
            );

            return;
        }


        Dictionary<
            MoveDirection,
            bool
        > existenceDirections =
            dungeonManager
                .GetSpecialPathExistence();


        // =====================================================
        // Up
        // =====================================================

        SetupSpecialButton(
            upButton,
            HasDirection(
                existenceDirections,
                MoveDirection.Up
            ),
            IsAvailable(
                availableDirections,
                MoveDirection.Up
            )
        );


        // =====================================================
        // Down
        // =====================================================

        SetupSpecialButton(
            downButton,
            HasDirection(
                existenceDirections,
                MoveDirection.Down
            ),
            IsAvailable(
                availableDirections,
                MoveDirection.Down
            )
        );


        // =====================================================
        // Left
        // =====================================================

        SetupSpecialButton(
            leftButton,
            HasDirection(
                existenceDirections,
                MoveDirection.Left
            ),
            IsAvailable(
                availableDirections,
                MoveDirection.Left
            )
        );


        // =====================================================
        // Right
        // =====================================================

        SetupSpecialButton(
            rightButton,
            HasDirection(
                existenceDirections,
                MoveDirection.Right
            ),
            IsAvailable(
                availableDirections,
                MoveDirection.Right
            )
        );


        if (directionPanel != null)
        {
            directionPanel.SetActive(
                true
            );
        }


        Debug.Log(
            "[DungeonUIManager] " +
            "특수 이동 방향 패널 표시\n" +
            "파랑 = 이동 가능 / " +
            "빨강 = 이동 불가"
        );
    }


    // =========================================================
    // Special Button Setup
    // =========================================================

    private void SetupSpecialButton(
        Button button,
        bool exists,
        bool available)
    {
        if (button == null)
        {
            return;
        }


        // =====================================================
        // No Special Path
        // =====================================================

        if (!exists)
        {
            button.gameObject.SetActive(
                false
            );

            return;
        }


        // =====================================================
        // Special Path Exists
        // =====================================================

        button.gameObject.SetActive(
            true
        );


        /*
         * 빨간 버튼도 화면에는 존재하지만
         * 실제 클릭 이벤트가 실행되면 안 된다.
         *
         * 따라서 이동 가능한 경우에만
         * interactable=true.
         */
        button.interactable =
            available;


        // =====================================================
        // Color
        // =====================================================

        Color targetColor =
            available
                ? availableColor
                : unavailableColor;


        ApplyButtonColor(
            button,
            targetColor
        );
    }


    // =========================================================
    // Button Color
    // =========================================================

    /*
     * 현재 방향 버튼은
     *
     * Button
     * Image
     *
     * 가 같은 GameObject에 붙어 있고
     * Button의 Target Graphic도 자기 Image다.
     *
     * Button Transition이 Color Tint이므로
     * 단순 Image.color만 바꾸면
     * Hover / Pressed 상태에서 색이 달라질 수 있다.
     *
     * 그래서 Button.colors도 같이 변경한다.
     */
    private void ApplyButtonColor(
        Button button,
        Color targetColor)
    {
        if (button == null)
        {
            return;
        }


        Image image =
            button.GetComponent<Image>();


        if (image != null)
        {
            image.color =
                targetColor;
        }


        ColorBlock colors =
            button.colors;


        colors.normalColor =
            targetColor;


        /*
         * Hover 시에도 기본 상태를 유지한다.
         */
        colors.highlightedColor =
            targetColor;


        /*
         * 클릭 중에는 살짝 어둡게 보이도록 한다.
         */
        colors.pressedColor =
            new Color(
                targetColor.r * 0.8f,
                targetColor.g * 0.8f,
                targetColor.b * 0.8f,
                targetColor.a
            );


        colors.selectedColor =
            targetColor;


        /*
         * 빨간 방향은 interactable=false가 되므로
         * Disabled Color도 빨간색 그대로 유지해야 한다.
         */
        colors.disabledColor =
            targetColor;


        colors.colorMultiplier =
            1f;


        button.colors =
            colors;
    }


    // =========================================================
    // Show Normal
    // =========================================================

    public void ShowDirectionPanel()
    {
        /*
         * 기존 코드 호환용.
         *
         * 일반 Show를 호출하면
         * 버튼들을 모두 다시 보이게 한다.
         */

        ShowAllDirectionButtons();


        if (directionPanel != null)
        {
            directionPanel.SetActive(
                true
            );
        }
    }


    // =========================================================
    // Hide
    // =========================================================

    public void HideDirectionPanel()
    {
        if (directionPanel != null)
        {
            directionPanel.SetActive(
                false
            );
        }
    }


    // =========================================================
    // Button Click
    // =========================================================

    public void OnClickMoveUp()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Up
                );
        }
    }


    public void OnClickMoveDown()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Down
                );
        }
    }


    public void OnClickMoveLeft()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Left
                );
        }
    }


    public void OnClickMoveRight()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .SelectNextRoom(
                    MoveDirection.Right
                );
        }
    }


    // =========================================================
    // Close Button
    // =========================================================

    public void OnClickCloseDirectionPanel()
    {
        ResolveReferences();


        if (roomTraversalController != null)
        {
            roomTraversalController
                .CloseDirectionPanel();
        }
        else
        {
            HideDirectionPanel();
        }
    }


    // =========================================================
    // Helpers
    // =========================================================

    private bool IsAvailable(
        Dictionary<
            MoveDirection,
            bool
        > directions,
        MoveDirection direction)
    {
        if (directions == null)
        {
            return false;
        }


        if (
            !directions.TryGetValue(
                direction,
                out bool available
            )
        )
        {
            return false;
        }


        return
            available;
    }


    private bool HasDirection(
        Dictionary<
            MoveDirection,
            bool
        > directions,
        MoveDirection direction)
    {
        if (directions == null)
        {
            return false;
        }


        if (
            !directions.TryGetValue(
                direction,
                out bool exists
            )
        )
        {
            return false;
        }


        return
            exists;
    }


    private void SetButtonInteractable(
        Button button,
        bool value)
    {
        if (button == null)
        {
            return;
        }


        button.interactable =
            value;
    }


    private void ShowAllDirectionButtons()
    {
        ShowButton(
            upButton
        );


        ShowButton(
            downButton
        );


        ShowButton(
            leftButton
        );


        ShowButton(
            rightButton
        );
    }


    private void ShowButton(
        Button button)
    {
        if (button == null)
        {
            return;
        }


        button.gameObject.SetActive(
            true
        );
    }
}