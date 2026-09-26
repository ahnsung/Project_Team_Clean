using UnityEngine;

public static class NewGameResetManager
{
    /// <summary>
    /// 완전한 새 게임용 초기화.
    ///
    /// PlayerPrefs에 남아 있는 이전 플레이 기록을
    /// 전부 삭제한다.
    ///
    /// 초기화 대상 예:
    /// - 캐릭터
    /// - 플레이어 이름
    /// - 체력
    /// - 정신력
    /// - 배고픔
    /// - 던전 위치
    /// - 던전 턴
    /// - 방문한 방
    /// - 미니맵 방문 기록
    /// - 인벤토리
    /// - 장비
    /// - 상자
    /// - 열쇠
    /// - 파밍
    /// - 열린 LockedDoor
    /// - 기타 PlayerPrefs 기반 진행 데이터
    /// </summary>
    public static void ResetEverything()
    {
        Debug.Log(
            "[NewGameReset] 새 게임 전체 초기화를 시작합니다."
        );

        // =====================================================
        // 1. 런타임 Dungeon 초기화
        // =====================================================

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance
                .ResetForNewGame(false);

            Debug.Log(
                "[NewGameReset] DungeonManager 초기화 완료"
            );
        }


        // =====================================================
        // 2. 런타임 Player Resource 초기화
        // =====================================================

        if (PlayerResourceManager.Instance != null)
        {
            PlayerResourceManager.Instance
                .ResetForNewGame(false);

            Debug.Log(
                "[NewGameReset] PlayerResourceManager 초기화 완료"
            );
        }


        // =====================================================
        // 3. 런타임 Tile Event 초기화
        // =====================================================

        if (DungeonTileEventManager.Instance != null)
        {
            DungeonTileEventManager.Instance
                .ClearUsedChestTiles();

            DungeonTileEventManager.Instance
                .ClearUsedKeyTiles();

            DungeonTileEventManager.Instance
                .ClearUsedFarmingTiles();

            DungeonTileEventManager.Instance
                .ResetGeneralBattleChance();

            Debug.Log(
                "[NewGameReset] DungeonTileEventManager 초기화 완료"
            );
        }


        // =====================================================
        // 4. Locked Door 초기화
        // =====================================================

        if (LockedDoorManager.Instance != null)
        {
            LockedDoorManager.Instance
                .ClearOpenedDoors();

            Debug.Log(
                "[NewGameReset] LockedDoorManager 초기화 완료"
            );
        }


        // =====================================================
        // 5. 장비 초기화
        // =====================================================

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance
                .ClearEquipmentForLoad();

            EquipmentManager.Instance
                .FinishEquipmentLoad();

            Debug.Log(
                "[NewGameReset] Equipment 초기화 완료"
            );
        }


        // =====================================================
        // 6. 인벤토리 초기화
        // =====================================================

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .ClearForLoad();

            InventoryManager.Instance
                .FinishLoad();

            Debug.Log(
                "[NewGameReset] Inventory 초기화 완료"
            );
        }


        // =====================================================
        // 7. 모든 PlayerPrefs 삭제
        // =====================================================
        //
        // 여기서 핵심적으로
        //
        // ROOM_X
        // ROOM_Y
        // VISITED
        // DUNGEON_TURN
        // DUNGEON_ENVIRONMENT
        //
        // PLAYER_HEALTH
        // PLAYER_MENTAL
        // PLAYER_HUNGER
        // PLAYER_LAST_PROCESSED_TURN
        //
        // GameplaySaveData
        //
        // HasSave
        // CutscenePlayed
        // SelectedCharacter
        // PlayerName
        //
        // 기타 진행 관련 키
        //
        // 를 전부 삭제한다.
        // =====================================================

        PlayerPrefs.DeleteAll();

        PlayerPrefs.Save();


        Debug.Log(
            "[NewGameReset] ==================================\n" +
            "새 게임 전체 초기화 완료\n" +
            "체력/정신력/배고픔 = 초기값\n" +
            "던전 위치 = 시작 위치\n" +
            "던전 턴 = 0\n" +
            "방문 기록 = 초기화\n" +
            "미니맵 = 초기화\n" +
            "인벤토리/장비 = 초기화\n" +
            "Chest = 초기화\n" +
            "Key = 초기화\n" +
            "Farming = 초기화\n" +
            "LockedDoor = 초기화\n" +
            "=================================="
        );
    }
}