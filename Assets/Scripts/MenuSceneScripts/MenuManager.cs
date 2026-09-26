using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject continueButton;
    public GameObject warningPanel;


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        RefreshContinueButton();

        if (warningPanel != null)
        {
            warningPanel.SetActive(
                false
            );
        }
    }


    // =========================================================
    // Continue Button
    // =========================================================

    private void RefreshContinueButton()
    {
        if (continueButton == null)
        {
            return;
        }

        bool hasSave =
            SaveManager.Instance != null &&
            SaveManager.Instance.HasSave();

        continueButton.SetActive(
            hasSave
        );
    }


    // =========================================================
    // Continue
    // =========================================================

    public void ContinueGame()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[MenuManager] SaveManager가 없습니다."
            );

            return;
        }


        if (!SaveManager.Instance.HasSave())
        {
            Debug.Log(
                "[MenuManager] Continue할 저장 데이터가 없습니다."
            );

            RefreshContinueButton();

            return;
        }


        SceneManager.LoadScene(
            "LobbyScene"
        );
    }


    // =========================================================
    // New Game
    // =========================================================

    public void NewGame()
    {
        bool hasSave =
            SaveManager.Instance != null &&
            SaveManager.Instance.HasSave();


        // 기존 저장이 있으면 경고
        if (hasSave)
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(
                    true
                );
            }

            return;
        }


        // 저장 자체가 없다면
        // 바로 완전 초기화 후 새 게임
        StartNewGame();
    }


    // =========================================================
    // Confirm New Game
    // =========================================================

    public void ConfirmNewGame()
    {
        StartNewGame();
    }


    // =========================================================
    // Cancel
    // =========================================================

    public void CancelNewGame()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(
                false
            );
        }
    }


    // =========================================================
    // Actual New Game
    // =========================================================

    private void StartNewGame()
    {
        Debug.Log(
            "[MenuManager] 새 게임 시작"
        );


        // =====================================================
        // 이전 게임 완전 삭제
        // =====================================================

        NewGameResetManager
            .ResetEverything();


        // =====================================================
        // 컷씬 종료 후 CharacterSelect로 이동하기 위한
        // 1회용 플래그
        // =====================================================
        //
        // ResetEverything()에서 DeleteAll을 했기 때문에
        // 반드시 그 다음에 다시 만들어야 한다.
        // =====================================================

        PlayerPrefs.SetInt(
            "AfterCutsceneGoToCharacterSelect",
            1
        );

        PlayerPrefs.Save();


        if (warningPanel != null)
        {
            warningPanel.SetActive(
                false
            );
        }


        SceneManager.LoadScene(
            "StartScene"
        );
    }
}