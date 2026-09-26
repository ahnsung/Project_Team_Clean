using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseCampExitManager : MonoBehaviour
{
    public static BaseCampExitManager Instance
    {
        get;
        private set;
    }


    [Header("Base Camp")]
    [SerializeField]
    private Vector2Int baseCampPosition =
        new Vector2Int(15, 29);


    [Header("Scene")]
    [SerializeField]
    private string lobbySceneName =
        "LobbyScene";


    [Header("Exit Confirm UI")]
    [SerializeField]
    private GameObject exitConfirmPanel;


    [Header("Fade")]
    [SerializeField]
    private FadeController fadeController;


    private bool isExitProcessing;


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


        if (exitConfirmPanel != null)
        {
            exitConfirmPanel.SetActive(
                false
            );
        }
    }


    private void Start()
    {
        if (fadeController == null)
        {
            fadeController =
                FindFirstObjectByType<
                    FadeController
                >();
        }
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // Base Camp Check
    // =========================================================

    public bool IsAtBaseCamp()
    {
        if (DungeonManager.Instance == null)
        {
            return false;
        }


        return
            DungeonManager.Instance
                .CurrentRoom ==
            baseCampPosition;
    }


    public Vector2Int GetBaseCampPosition()
    {
        return baseCampPosition;
    }


    // =========================================================
    // E Interaction
    // =========================================================

    public bool TryOpenExitConfirm()
    {
        if (isExitProcessing)
        {
            return false;
        }


        if (!IsAtBaseCamp())
        {
            return false;
        }


        if (exitConfirmPanel == null)
        {
            Debug.LogWarning(
                "[BaseCampExitManager] " +
                "Exit Confirm Panel이 없습니다."
            );

            return false;
        }


        exitConfirmPanel.SetActive(
            true
        );


        Debug.Log(
            "[BaseCamp] 던전 탈출 확인창 표시"
        );


        return true;
    }


    // =========================================================
    // UI Buttons
    // =========================================================

    public void OnClickConfirmExit()
    {
        if (isExitProcessing)
        {
            return;
        }


        StartCoroutine(
            ExitDungeonRoutine()
        );
    }


    public void OnClickCancelExit()
    {
        if (isExitProcessing)
        {
            return;
        }


        if (exitConfirmPanel != null)
        {
            exitConfirmPanel.SetActive(
                false
            );
        }


        Debug.Log(
            "[BaseCamp] 던전 탈출 취소"
        );
    }


    // =========================================================
    // Exit
    // =========================================================

    private IEnumerator ExitDungeonRoutine()
    {
        isExitProcessing =
            true;


        if (exitConfirmPanel != null)
        {
            exitConfirmPanel.SetActive(
                false
            );
        }


        // =====================================================
        // 현재 진행 저장
        // =====================================================

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance
                .SaveGameplayData();
        }


        // =====================================================
        // Run 단위 상태 초기화
        // =====================================================

        DungeonTileEventManager tileEventManager =
            DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            /*
             * Farming은 던전 1회 입장 동안만 사용 완료.
             * 정상적으로 던전을 나가면 초기화한다.
             */
            tileEventManager
                .ClearUsedFarmingTiles();


            /*
             * General 누적 확률도
             * 새로운 던전 입장에서는 다시 10%.
             */
            tileEventManager
                .ResetGeneralBattleChance();
        }


        /*
         * Chest / Key / LockedDoor는
         * 영구 상태이므로 절대 초기화하지 않는다.
         */


        Debug.Log(
            "[BaseCamp] 던전 퇴장 처리\n" +
            "Farming 초기화\n" +
            "General 확률 초기화\n" +
            "Chest / Key / LockedDoor 유지"
        );


        // =====================================================
        // Fade Out
        // =====================================================

        if (fadeController != null)
        {
            yield return
                fadeController.FadeOut();
        }


        // =====================================================
        // Lobby
        // =====================================================

        SceneManager.LoadScene(
            lobbySceneName
        );
    }


    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu(
        "DEBUG - Base Camp 여부 확인"
    )]
    private void DebugBaseCamp()
    {
        Debug.Log(
            "[BaseCamp] 현재 위치: " +
            (
                DungeonManager.Instance != null
                ? DungeonManager.Instance
                    .CurrentRoom.ToString()
                : "DungeonManager 없음"
            ) +
            "\nBase Camp: " +
            baseCampPosition +
            "\n현재 Base Camp 여부: " +
            IsAtBaseCamp()
        );
    }
}