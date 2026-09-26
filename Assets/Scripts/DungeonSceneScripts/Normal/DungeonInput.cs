using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonInput : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField]
    private string lobbySceneName = "LobbyScene";

    [Header("Rules")]
    [Tooltip("체크하면 전투 중 ESC로 로비에 나갈 수 없습니다.")]
    [SerializeField]
    private bool blockExitDuringBattle = true;

    private bool isLeaving;

    private void Update()
    {
        if (isLeaving)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitToLobby();
        }
    }

    private void ExitToLobby()
    {
        if (isLeaving)
            return;

        if (blockExitDuringBattle &&
            BattleManager.Instance != null &&
            BattleManager.Instance.IsBattleRunning())
        {
            Debug.Log(
                "[DungeonInput] 전투 중에는 로비로 이동할 수 없습니다."
            );

            return;
        }

        isLeaving = true;

        SaveManager saveManager =
            ResolveSaveManager();

        if (saveManager == null)
        {
            Debug.LogError(
                "[DungeonInput] SaveManager를 생성하거나 찾지 못했습니다."
            );

            isLeaving = false;
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[DungeonInput] InventoryManager.Instance가 없습니다."
            );

            isLeaving = false;
            return;
        }

        if (EquipmentManager.Instance == null)
        {
            Debug.LogError(
                "[DungeonInput] EquipmentManager.Instance가 없습니다."
            );

            isLeaving = false;
            return;
        }

        Debug.Log(
            "[DungeonInput] 로비 이동 전 저장 시작\n" +
            $"인벤토리 아이템: {InventoryManager.Instance.items.Count}\n" +
            $"주 무기: {GetItemName(EquipmentManager.Instance.MainWeapon)}\n" +
            $"보조 무기: {GetItemName(EquipmentManager.Instance.SubWeapon)}\n" +
            $"갑옷: {GetItemName(EquipmentManager.Instance.Armor)}"
        );

        // 인벤토리, 장비, 내구도를 PlayerPrefs에 기록
        saveManager.SaveGameplayData();

        // DungeonManager와 PlayerResourceManager는
        // 값이 바뀔 때 자체적으로 PlayerPrefs에 저장한다.
        PlayerPrefs.Save();

        string savedJson =
            PlayerPrefs.GetString(
                "GameplaySaveData",
                ""
            );

        if (string.IsNullOrEmpty(savedJson))
        {
            Debug.LogError(
                "[DungeonInput] GameplaySaveData가 비어 있습니다. " +
                "로비 이동을 중단합니다."
            );

            isLeaving = false;
            return;
        }

        Debug.Log(
            "[DungeonInput] 저장 완료. JSON 길이: " +
            savedJson.Length
        );

        SceneManager.LoadScene(
            lobbySceneName
        );
    }

    private SaveManager ResolveSaveManager()
    {
        if (SaveManager.Instance != null)
        {
            return SaveManager.Instance;
        }

        // 씬이나 DontDestroyOnLoad 영역에 있지만
        // 정적 Instance만 놓친 경우를 대비한다.
        SaveManager existingManager =
            FindFirstObjectByType<SaveManager>();

        if (existingManager != null)
        {
            Debug.Log(
                "[DungeonInput] 씬에서 SaveManager를 찾았습니다."
            );

            return existingManager;
        }

        /*
         * DungeonScene을 에디터에서 직접 실행했을 때를 위한 안전장치.
         * SaveManager.Awake()에서 Instance 설정과
         * DontDestroyOnLoad 처리가 실행된다.
         */
        GameObject saveManagerObject =
            new GameObject("SaveManager");

        SaveManager createdManager =
            saveManagerObject.AddComponent<SaveManager>();

        if (createdManager != null)
        {
            Debug.LogWarning(
                "[DungeonInput] SaveManager가 없어 자동 생성했습니다. " +
                "실제 게임 테스트는 StartScene부터 실행하는 것이 안전합니다."
            );
        }

        return createdManager;
    }

    private string GetItemName(
        InventoryItem item)
    {
        if (item == null ||
            item.data == null)
        {
            return "없음";
        }

        return item.data.itemName;
    }
}