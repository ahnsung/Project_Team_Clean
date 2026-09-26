using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;


    // =========================================================
    // Keys
    // =========================================================

    private const string GameplaySaveKey =
        "GameplaySaveData";


    // =========================================================
    // Debug
    // =========================================================

    [Header("Debug")]
    [SerializeField]
    private bool printSaveLog = true;


    // =========================================================
    // State
    // =========================================================

    private bool isLoading;

    public bool IsLoading
    {
        get
        {
            return isLoading;
        }
    }


    // =========================================================
    // Save Data
    // =========================================================

    [Serializable]
    public class InventoryItemSaveData
    {
        public string uniqueId;

        public int itemId;

        public int positionX;
        public int positionY;

        public int rotation;

        public int remainUseCount;
        public int currentDurability;

        public bool isEquipped;

        public EquipmentSlotType equippedSlot;
    }


    [Serializable]
    public class GameplaySaveData
    {
        // =====================================================
        // Inventory / Equipment
        // =====================================================

        public List<InventoryItemSaveData> items =
            new List<InventoryItemSaveData>();


        // =====================================================
        // Chest
        // =====================================================

        public List<string> usedChestTiles =
            new List<string>();


        // =====================================================
        // Key
        // =====================================================

        public List<string> usedKeyTiles =
            new List<string>();


        // =====================================================
        // Locked Door
        // =====================================================

        public List<string> openedLockedDoors =
            new List<string>();
    }


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        DontDestroyOnLoad(
            gameObject
        );


        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }


    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }


        SceneManager.sceneLoaded -=
            OnSceneLoaded;


        Instance = null;
    }


    private void OnApplicationQuit()
    {
        if (!HasSave())
        {
            return;
        }


        SaveGameplayData();
    }


    private void OnApplicationPause(
        bool pauseStatus)
    {
        if (!pauseStatus)
        {
            return;
        }


        if (!HasSave())
        {
            return;
        }


        SaveGameplayData();
    }


    // =========================================================
    // Scene Load
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (!HasSave())
        {
            return;
        }


        if (!HasGameplaySave())
        {
            return;
        }


        StartCoroutine(
            LoadGameplayDataAfterSceneReady()
        );
    }


    private IEnumerator
        LoadGameplayDataAfterSceneReady()
    {
        /*
         * DungeonScene 오브젝트들이
         * Awake / Start를 끝낼 시간을 준다.
         */

        yield return null;
        yield return null;


        const float timeout =
            5f;


        float elapsed =
            0f;


        while (elapsed < timeout)
        {
            bool inventoryReady =
                InventoryManager.Instance != null;


            bool equipmentReady =
                EquipmentManager.Instance != null;


            bool itemDatabaseReady =
                ItemDatabase.Instance != null;


            /*
             * DungeonScene이 아니라면
             * 위 Manager들이 없을 수 있다.
             *
             * 그 경우 timeout까지 기다리기보다
             * 다음 프레임을 계속 확인한다.
             */

            if (
                inventoryReady &&
                equipmentReady &&
                itemDatabaseReady
            )
            {
                LoadGameplayData();

                yield break;
            }


            elapsed +=
                Time.unscaledDeltaTime;


            yield return null;
        }
    }


    // =========================================================
    // New Save
    // =========================================================

    public void CreateNewSave(
        int characterID,
        string playerName)
    {
        PlayerPrefs.SetInt(
            "HasSave",
            1
        );


        PlayerPrefs.SetInt(
            "CutscenePlayed",
            1
        );


        PlayerPrefs.SetInt(
            "SelectedCharacter",
            characterID
        );


        PlayerPrefs.SetString(
            "PlayerName",
            playerName ?? ""
        );


        // 기존 게임플레이 저장 제거
        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );


        PlayerPrefs.Save();


        // =====================================================
        // 현재 살아있는 런타임 상태 초기화
        // =====================================================

        DungeonTileEventManager tileEventManager =
            DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            tileEventManager
                .ClearUsedChestTiles();


            tileEventManager
                .ClearUsedKeyTiles();


            tileEventManager
                .ClearUsedFarmingTiles();
        }


        if (
            LockedDoorManager.Instance != null
        )
        {
            LockedDoorManager.Instance
                .ClearOpenedDoors();
        }


        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 새 게임 저장 생성\n" +
                $"캐릭터: {characterID}\n" +
                $"이름: {playerName}"
            );
        }
    }


    // =========================================================
    // Basic
    // =========================================================

    public bool HasSave()
    {
        return
            PlayerPrefs.GetInt(
                "HasSave",
                0
            ) == 1;
    }


    public bool HasPlayedCutscene()
    {
        return
            PlayerPrefs.GetInt(
                "CutscenePlayed",
                0
            ) == 1;
    }


    public int GetSelectedCharacter()
    {
        return
            PlayerPrefs.GetInt(
                "SelectedCharacter",
                -1
            );
    }


    public string GetPlayerName()
    {
        return
            PlayerPrefs.GetString(
                "PlayerName",
                ""
            );
    }


    public bool HasGameplaySave()
    {
        return
            PlayerPrefs.HasKey(
                GameplaySaveKey
            );
    }


    // =========================================================
    // SAVE
    // =========================================================

    public void SaveGameplayData()
    {
        if (isLoading)
        {
            return;
        }


        InventoryManager inventory =
            InventoryManager.Instance;


        EquipmentManager equipment =
            EquipmentManager.Instance;


        /*
         * DungeonScene 밖에서는
         * Inventory / Equipment가 없을 수 있다.
         *
         * 기존 저장을 빈 데이터로 덮어쓰지 않게
         * 바로 return.
         */

        if (
            inventory == null ||
            equipment == null
        )
        {
            return;
        }


        GameplaySaveData saveData =
            new GameplaySaveData();


        HashSet<string> savedUniqueIds =
            new HashSet<string>();


        // =====================================================
        // Inventory
        // =====================================================

        if (inventory.items != null)
        {
            foreach (
                InventoryItem item
                in inventory.items
            )
            {
                AddItemToSaveData(
                    saveData,
                    savedUniqueIds,
                    item,
                    false,
                    EquipmentSlotType.MainWeapon
                );
            }
        }


        // =====================================================
        // Equipment
        // =====================================================

        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.Head,
            true,
            EquipmentSlotType.Head
        );


        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.Armor,
            true,
            EquipmentSlotType.Armor
        );


        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.Shoes,
            true,
            EquipmentSlotType.Shoes
        );


        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.MainWeapon,
            true,
            EquipmentSlotType.MainWeapon
        );


        AddItemToSaveData(
            saveData,
            savedUniqueIds,
            equipment.SubWeapon,
            true,
            EquipmentSlotType.SubWeapon
        );


        // =====================================================
        // Chest / Key
        // =====================================================

        DungeonTileEventManager tileEventManager =
            DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            saveData.usedChestTiles =
                tileEventManager
                    .GetUsedChestTilesForSave();


            saveData.usedKeyTiles =
                tileEventManager
                    .GetUsedKeyTilesForSave();
        }


        // =====================================================
        // Locked Door
        // =====================================================

        LockedDoorManager lockedDoorManager =
            LockedDoorManager.Instance;


        if (lockedDoorManager != null)
        {
            saveData.openedLockedDoors =
                lockedDoorManager
                    .GetOpenedDoorsForSave();
        }


        // =====================================================
        // JSON
        // =====================================================

        string json =
            JsonUtility.ToJson(
                saveData
            );


        PlayerPrefs.SetString(
            GameplaySaveKey,
            json
        );


        PlayerPrefs.Save();


        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 저장 완료\n" +
                $"아이템: {saveData.items.Count}개\n" +
                $"사용한 상자: " +
                $"{saveData.usedChestTiles.Count}개\n" +
                $"획득한 Key 타일: " +
                $"{saveData.usedKeyTiles.Count}개\n" +
                $"열린 LockedDoor: " +
                $"{saveData.openedLockedDoors.Count}개"
            );
        }
    }


    // =========================================================
    // Item Save
    // =========================================================

    private void AddItemToSaveData(
        GameplaySaveData saveData,
        HashSet<string> savedUniqueIds,
        InventoryItem item,
        bool isEquipped,
        EquipmentSlotType equippedSlot)
    {
        if (
            item == null ||
            item.data == null
        )
        {
            return;
        }


        if (item.data.id <= 0)
        {
            return;
        }


        if (
            string.IsNullOrEmpty(
                item.uniqueId
            )
        )
        {
            item.uniqueId =
                Guid.NewGuid()
                    .ToString();
        }


        if (
            !savedUniqueIds.Add(
                item.uniqueId
            )
        )
        {
            Debug.LogWarning(
                "[SaveManager] 중복 아이템 저장 방지\n" +
                $"UniqueId: {item.uniqueId}"
            );

            return;
        }


        InventoryItemSaveData itemSave =
            new InventoryItemSaveData
            {
                uniqueId =
                    item.uniqueId,

                itemId =
                    item.data.id,

                positionX =
                    item.position.x,

                positionY =
                    item.position.y,

                rotation =
                    item.rotation,

                remainUseCount =
                    item.remainUseCount,

                currentDurability =
                    item.currentDurability,

                isEquipped =
                    isEquipped,

                equippedSlot =
                    equippedSlot
            };


        saveData.items.Add(
            itemSave
        );
    }


    // =========================================================
    // LOAD
    // =========================================================

    public void LoadGameplayData()
    {
        if (
            isLoading ||
            !HasGameplaySave()
        )
        {
            return;
        }


        InventoryManager inventory =
            InventoryManager.Instance;


        EquipmentManager equipment =
            EquipmentManager.Instance;


        ItemDatabase itemDatabase =
            ItemDatabase.Instance;


        if (
            inventory == null ||
            equipment == null ||
            itemDatabase == null
        )
        {
            Debug.LogWarning(
                "[SaveManager] " +
                "저장 데이터를 불러올 준비가 되지 않았습니다."
            );

            return;
        }


        string json =
            PlayerPrefs.GetString(
                GameplaySaveKey,
                ""
            );


        if (
            string.IsNullOrWhiteSpace(
                json
            )
        )
        {
            return;
        }


        GameplaySaveData saveData;


        try
        {
            saveData =
                JsonUtility
                    .FromJson<GameplaySaveData>(
                        json
                    );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[SaveManager] 저장 데이터 해석 실패\n" +
                exception
            );

            return;
        }


        if (saveData == null)
        {
            return;
        }


        // =====================================================
        // 구버전 Save 호환
        // =====================================================

        if (saveData.items == null)
        {
            saveData.items =
                new List<InventoryItemSaveData>();
        }


        if (saveData.usedChestTiles == null)
        {
            saveData.usedChestTiles =
                new List<string>();
        }


        if (saveData.usedKeyTiles == null)
        {
            saveData.usedKeyTiles =
                new List<string>();
        }


        if (saveData.openedLockedDoors == null)
        {
            saveData.openedLockedDoors =
                new List<string>();
        }


        isLoading =
            true;


        try
        {
            // =================================================
            // Inventory 초기화
            // =================================================

            inventory.ClearForLoad();


            equipment
                .ClearEquipmentForLoad();


            Dictionary<string, InventoryItem>
                restoredItems =
                    new Dictionary<
                        string,
                        InventoryItem
                    >();


            // =================================================
            // Item 객체 생성
            // =================================================

            foreach (
                InventoryItemSaveData savedItem
                in saveData.items
            )
            {
                if (savedItem == null)
                {
                    continue;
                }


                if (savedItem.itemId <= 0)
                {
                    continue;
                }


                ItemData itemData =
                    itemDatabase.GetItem(
                        savedItem.itemId
                    );


                if (itemData == null)
                {
                    Debug.LogWarning(
                        "[SaveManager] " +
                        "ItemDatabase에서 아이템을 찾지 못함\n" +
                        $"Item ID: {savedItem.itemId}"
                    );

                    continue;
                }


                InventoryItem restoredItem =
                    new InventoryItem(
                        itemData
                    );


                string uniqueId =
                    string.IsNullOrEmpty(
                        savedItem.uniqueId
                    )
                    ? Guid.NewGuid()
                        .ToString()
                    : savedItem.uniqueId;


                restoredItem.uniqueId =
                    uniqueId;


                restoredItem.position =
                    new Vector2Int(
                        savedItem.positionX,
                        savedItem.positionY
                    );


                restoredItem.SetRotation(
                    savedItem.rotation
                );


                restoredItem.remainUseCount =
                    Mathf.Max(
                        0,
                        savedItem.remainUseCount
                    );


                if (itemData.IsEquipment)
                {
                    restoredItem.currentDurability =
                        Mathf.Clamp(
                            savedItem.currentDurability,
                            0,
                            itemData.SafeMaxDurability
                        );
                }
                else
                {
                    restoredItem.currentDurability =
                        0;
                }


                restoredItems[
                    uniqueId
                ] =
                    restoredItem;
            }


            // =================================================
            // Inventory 복구
            // =================================================

            foreach (
                InventoryItemSaveData savedItem
                in saveData.items
            )
            {
                if (
                    savedItem == null ||
                    savedItem.isEquipped
                )
                {
                    continue;
                }


                if (
                    string.IsNullOrEmpty(
                        savedItem.uniqueId
                    )
                )
                {
                    continue;
                }


                if (
                    !restoredItems
                        .TryGetValue(
                            savedItem.uniqueId,
                            out InventoryItem restoredItem
                        )
                )
                {
                    continue;
                }


                if (
                    !inventory.AddRestoredItem(
                        restoredItem
                    )
                )
                {
                    inventory
                        .AddRestoredItemToEmptySpace(
                            restoredItem
                        );
                }
            }


            // =================================================
            // Equipment 복구
            // =================================================

            foreach (
                InventoryItemSaveData savedItem
                in saveData.items
            )
            {
                if (
                    savedItem == null ||
                    !savedItem.isEquipped
                )
                {
                    continue;
                }


                if (
                    string.IsNullOrEmpty(
                        savedItem.uniqueId
                    )
                )
                {
                    continue;
                }


                if (
                    !restoredItems
                        .TryGetValue(
                            savedItem.uniqueId,
                            out InventoryItem restoredItem
                        )
                )
                {
                    continue;
                }


                bool restored =
                    equipment
                        .RestoreEquipmentForLoad(
                            savedItem.equippedSlot,
                            restoredItem
                        );


                if (!restored)
                {
                    inventory
                        .AddRestoredItemToEmptySpace(
                            restoredItem
                        );
                }
            }


            inventory.FinishLoad();


            equipment
                .FinishEquipmentLoad();


            // =================================================
            // Chest / Key 복구
            // =================================================

            DungeonTileEventManager tileEventManager =
                DungeonTileEventManager.Instance;


            if (tileEventManager != null)
            {
                tileEventManager
                    .RestoreUsedChestTiles(
                        saveData.usedChestTiles
                    );


                tileEventManager
                    .RestoreUsedKeyTiles(
                        saveData.usedKeyTiles
                    );
            }


            // =================================================
            // Locked Door 복구
            // =================================================

            LockedDoorManager lockedDoorManager =
                LockedDoorManager.Instance;


            if (lockedDoorManager != null)
            {
                lockedDoorManager
                    .RestoreOpenedDoors(
                        saveData.openedLockedDoors
                    );
            }


            // =================================================
            // UI Refresh
            // =================================================

            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance
                    .RefreshAll();
            }


            // =================================================
            // Log
            // =================================================

            if (printSaveLog)
            {
                Debug.Log(
                    "[SaveManager] 불러오기 완료\n" +
                    $"아이템: {saveData.items.Count}개\n" +
                    $"사용한 상자: " +
                    $"{saveData.usedChestTiles.Count}개\n" +
                    $"획득한 Key 타일: " +
                    $"{saveData.usedKeyTiles.Count}개\n" +
                    $"열린 LockedDoor: " +
                    $"{saveData.openedLockedDoors.Count}개"
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[SaveManager] 복구 중 오류\n" +
                exception
            );
        }
        finally
        {
            isLoading =
                false;
        }
    }


    // =========================================================
    // Delete Gameplay
    // =========================================================

    public void DeleteGameplaySave()
    {
        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );


        PlayerPrefs.Save();


        DungeonTileEventManager tileEventManager =
            DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            tileEventManager
                .ClearUsedChestTiles();


            tileEventManager
                .ClearUsedKeyTiles();


            tileEventManager
                .ClearUsedFarmingTiles();
        }


        if (
            LockedDoorManager.Instance != null
        )
        {
            LockedDoorManager.Instance
                .ClearOpenedDoors();
        }


        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 게임플레이 저장 삭제"
            );
        }
    }


    // =========================================================
    // Delete All
    // =========================================================

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(
            "HasSave"
        );


        PlayerPrefs.DeleteKey(
            "CutscenePlayed"
        );


        PlayerPrefs.DeleteKey(
            "SelectedCharacter"
        );


        PlayerPrefs.DeleteKey(
            "PlayerName"
        );


        PlayerPrefs.DeleteKey(
            GameplaySaveKey
        );


        PlayerPrefs.Save();


        DungeonTileEventManager tileEventManager =
            DungeonTileEventManager.Instance;


        if (tileEventManager != null)
        {
            tileEventManager
                .ClearUsedChestTiles();


            tileEventManager
                .ClearUsedKeyTiles();


            tileEventManager
                .ClearUsedFarmingTiles();
        }


        if (
            LockedDoorManager.Instance != null
        )
        {
            LockedDoorManager.Instance
                .ClearOpenedDoors();
        }


        if (printSaveLog)
        {
            Debug.Log(
                "[SaveManager] 전체 저장 삭제"
            );
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu(
        "TEST - Save Gameplay"
    )]
    private void TestSaveGameplay()
    {
        SaveGameplayData();
    }


    [ContextMenu(
        "TEST - Load Gameplay"
    )]
    private void TestLoadGameplay()
    {
        LoadGameplayData();
    }
}