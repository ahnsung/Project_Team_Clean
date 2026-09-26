using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    // =========================================================
    // Constants
    // =========================================================

    /*
     * Lobby에서 정상적으로 DungeonScene에 입장했음을 알리는 플래그.
     *
     * DungeonManager가 이 값을 확인해서
     * 저장된 마지막 좌표 대신 Base Camp에서 시작한다.
     */
    public const string FreshDungeonEntryKey =
        "DUNGEON_FRESH_ENTRY";


    // =========================================================
    // UI
    // =========================================================

    [Header("Dungeon")]
    [SerializeField]
    private GameObject confirmDungeonPanel;


    [Header("Scene")]
    [SerializeField]
    private string dungeonSceneName =
        "DungeonScene";

    [SerializeField]
    private string menuSceneName =
        "MenuScene";


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        if (confirmDungeonPanel != null)
        {
            confirmDungeonPanel.SetActive(
                false
            );
        }
    }


    // =========================================================
    // Status
    // =========================================================

    public void OpenStatus()
    {
        Debug.Log(
            "[Lobby] Status Window"
        );
    }


    // =========================================================
    // Inventory
    // =========================================================

    public void OpenInventory()
    {
        Debug.Log(
            "[Lobby] Inventory Window"
        );
    }


    // =========================================================
    // Quest
    // =========================================================

    public void OpenQuest()
    {
        Debug.Log(
            "[Lobby] Quest Window"
        );
    }


    // =========================================================
    // Rest
    // =========================================================

    public void Rest()
    {
        Debug.Log(
            "[Lobby] Player Rested"
        );
    }


    // =========================================================
    // Enter Dungeon
    // =========================================================

    public void EnterDungeon()
    {
        if (confirmDungeonPanel != null)
        {
            confirmDungeonPanel.SetActive(
                true
            );
        }


        Debug.Log(
            "[Lobby] 던전 입장 확인창 표시"
        );
    }


    public void ConfirmDungeon()
    {
        /*
         * 중요:
         *
         * Continue에서 DungeonScene으로 바로 들어가는 경우와
         * Lobby에서 새 던전 Run을 시작하는 경우를 구분한다.
         *
         * Lobby에서 들어가는 경우에는
         * 무조건 Base Camp에서 시작.
         */

        PlayerPrefs.SetInt(
            FreshDungeonEntryKey,
            1
        );


        PlayerPrefs.Save();


        Debug.Log(
            "[Lobby] 새 던전 Run 시작\n" +
            "Base Camp 시작 플래그 저장"
        );


        SceneManager.LoadScene(
            dungeonSceneName
        );
    }


    public void CancelDungeon()
    {
        if (confirmDungeonPanel != null)
        {
            confirmDungeonPanel.SetActive(
                false
            );
        }


        Debug.Log(
            "[Lobby] 던전 입장 취소"
        );
    }


    // =========================================================
    // Exit
    // =========================================================

    public void ExitGame()
    {
        /*
         * Lobby 상태에서 저장 가능한 데이터가 존재하면
         * PlayerPrefs도 함께 저장.
         */

        PlayerPrefs.Save();


        SceneManager.LoadScene(
            menuSceneName
        );
    }
}