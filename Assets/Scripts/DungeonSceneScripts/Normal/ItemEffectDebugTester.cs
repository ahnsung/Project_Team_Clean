using UnityEngine;

public class ItemEffectDebugTester : MonoBehaviour
{
    private const string TAG = "[ITEM DEBUG]";

    private BattleManager Battle => BattleManager.Instance;

    private StatusEffectController GetPlayerStatus()
    {
        if (Battle != null && Battle.playerUnit != null)
        {
            StatusEffectController c =
                Battle.playerUnit.GetComponent<StatusEffectController>();

            if (c == null)
                c = Battle.playerUnit.GetComponentInChildren<StatusEffectController>(true);

            if (c == null)
                c = Battle.playerUnit.GetComponentInParent<StatusEffectController>();

            if (c != null)
                return c;
        }

        if (PlayerResourceManager.Instance != null)
        {
            StatusEffectController c =
                PlayerResourceManager.Instance.GetComponent<StatusEffectController>();

            if (c == null)
                c = PlayerResourceManager.Instance
                    .GetComponentInChildren<StatusEffectController>(true);

            if (c == null)
                c = PlayerResourceManager.Instance
                    .GetComponentInParent<StatusEffectController>();

            return c;
        }

        return null;
    }

    private void Header(int itemId, string name)
    {
        Debug.Log(
            "\n========================================\n" +
            TAG + " TEST " + itemId + " - " + name + "\n" +
            "========================================"
        );
    }

    [ContextMenu("TEST 1011 - Flashlight")]
    public void Test1011()
    {
        Header(1011, "Flashlight");

        ItemRuntimeEffectManager runtime =
            ItemRuntimeEffectManager.EnsureExists();

        runtime.AddEncounterEffect(1.5f, 10, true);
        runtime.AddFarmingMultiplier(1.5f, 10, true);

        Debug.Log(
            TAG + " 1011 ACTIVATED\n" +
            "Encounter multiplier = x" +
            runtime.GetEncounterChanceMultiplier() + "\n" +
            "Farming multiplier = x" +
            runtime.GetFarmingQuantityMultiplier() + "\n" +
            "Duration = 10 dungeon turns\n" +
            "PASS if values above are x1.5 / x1.5."
        );
    }

    [ContextMenu("TEST 1012 - Tent Context")]
    public void Test1012()
    {
        Header(1012, "Tent");

        if (DungeonManager.Instance == null)
        {
            Debug.LogWarning(TAG + " FAIL: DungeonManager.Instance is null.");
            return;
        }

        DungeonTileType tile =
            DungeonManager.Instance.GetCurrentTileType();

        bool isRest = tile == DungeonTileType.Rest;
        bool canRest = false;

        if (RestTileManager.Instance != null)
        {
            canRest = RestTileManager.Instance.CanRest(
                DungeonManager.Instance.CurrentRoom
            );
        }

        Debug.Log(
            TAG + " 1012 CONTEXT CHECK\n" +
            "Current tile = " + tile + "\n" +
            "Is Rest tile = " + isRest + "\n" +
            "RestTileManager exists = " +
            (RestTileManager.Instance != null) + "\n" +
            "CanRest = " + canRest + "\n" +
            "Expected: item can be used only when Is Rest tile=True and CanRest=True."
        );
    }

    [ContextMenu("TEST 1013 - Brick 10 Damage")]
    public void Test1013()
    {
        Header(1013, "Brick");

        if (Battle == null || !Battle.IsBattleRunning())
        {
            Debug.LogWarning(TAG + " 1013 FAIL: Start a battle first.");
            return;
        }

        bool started = Battle.BeginTargetDamageItem(1013, 10);

        Debug.Log(
            TAG + " 1013 target mode requested.\n" +
            "Damage = 10\n" +
            "BeginTargetDamageItem result = " + started + "\n" +
            "Select one enemy to finish the test."
        );
    }

    [ContextMenu("TEST 1014 - Molotov 7 AOE Damage")]
    public void Test1014()
    {
        Header(1014, "Molotov");

        if (Battle == null || !Battle.IsBattleRunning())
        {
            Debug.LogWarning(TAG + " 1014 FAIL: Start a battle first.");
            return;
        }

        bool result = Battle.UseAllEnemyDamageItem(7);

        Debug.Log(
            TAG + " 1014 executed.\n" +
            "AOE damage = 7\n" +
            "Result = " + result + "\n" +
            "PASS if every living enemy lost 7 HP."
        );
    }

    [ContextMenu("TEST 1015 - One Hit Shield")]
    public void Test1015()
    {
        Header(1015, "One Hit Shield");

        if (Battle == null || !Battle.IsBattleRunning())
        {
            Debug.LogWarning(TAG + " 1015 FAIL: Start a battle first.");
            return;
        }

        Battle.ActivateIgnoreNextEnemyHit();

        Debug.Log(
            TAG + " 1015 ACTIVATED\n" +
            "Next successful enemy hit should become BLOCK.\n" +
            "BattleManager should log: energy shield blocked enemy attack.\n" +
            "After one blocked hit, later hits should work normally."
        );
    }

    [ContextMenu("TEST 1016 - Full Enemy Turn Shield")]
    public void Test1016()
    {
        Header(1016, "Full Enemy Turn Shield");

        if (Battle == null || !Battle.IsBattleRunning())
        {
            Debug.LogWarning(TAG + " 1016 FAIL: Start a battle first.");
            return;
        }

        Battle.ActivateIgnoreAllEnemyHitsThisTurn();

        Debug.Log(
            TAG + " 1016 ACTIVATED\n" +
            "Every successful enemy hit in the next enemy turn should become BLOCK.\n" +
            "The flag is reset when that enemy turn ends."
        );
    }

    [ContextMenu("TEST 1017 - Guaranteed Hit")]
    public void Test1017()
    {
        Header(1017, "Magnet / Guaranteed Hit");

        StatusEffectController playerStatus = GetPlayerStatus();

        if (playerStatus == null)
        {
            Debug.LogWarning(TAG + " 1017 FAIL: Player StatusEffectController not found.");
            return;
        }

        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogWarning(TAG + " 1017 FAIL: StatusEffectDatabase.Instance is null.");
            return;
        }

        StatusEffectData data =
            StatusEffectDatabase.Instance.GetStatusEffect(2125);

        if (data == null)
        {
            Debug.LogWarning(TAG + " 1017 FAIL: Status 2125 not found.");
            return;
        }

        bool added = playerStatus.AddStatusEffect(data);

        Debug.Log(
            TAG + " 1017 ACTIVATED\n" +
            "Status 2125 added = " + added + "\n" +
            "Has 2125 = " + playerStatus.HasStatusEffectById(2125) + "\n" +
            "Next normal attack should log guaranteed hit and consume one 2125 stack."
        );
    }

    [ContextMenu("TEST 1018 - Double Next Normal Attack")]
    public void Test1018()
    {
        Header(1018, "Stimulant");

        if (Battle == null || !Battle.IsBattleRunning())
        {
            Debug.LogWarning(TAG + " 1018 FAIL: Start a battle first.");
            return;
        }

        Battle.ActivateDoubleNextNormalAttackDamage();

        Debug.Log(
            TAG + " 1018 ACTIVATED\n" +
            "Next NORMAL attack damage should be x2.\n" +
            "BattleManager should log: stimulant -> normal attack damage x2.\n" +
            "The effect is consumed by the next normal attack attempt."
        );
    }

    [ContextMenu("TEST 1019 - Add STR +1")]
    public void Test1019STR()
    {
        Header(1019, "Modification Device / STR");

        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning(TAG + " 1019 FAIL: PlayerStats.Instance is null.");
            return;
        }

        int before = PlayerStats.Instance.TotalSTR;
        PlayerStats.Instance.AddSTR();
        int after = PlayerStats.Instance.TotalSTR;

        Debug.Log(
            TAG + " 1019 STR TEST\n" +
            "Before TotalSTR = " + before + "\n" +
            "After TotalSTR = " + after + "\n" +
            "Difference = " + (after - before) + "\n" +
            "Expected difference = +1."
        );
    }

    [ContextMenu("TEST 1019 - Add DEX +1")]
    public void Test1019DEX()
    {
        Header(1019, "Modification Device / DEX");

        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning(TAG + " 1019 FAIL: PlayerStats.Instance is null.");
            return;
        }

        int before = PlayerStats.Instance.TotalDEX;
        PlayerStats.Instance.AddDEX();
        int after = PlayerStats.Instance.TotalDEX;

        Debug.Log(
            TAG + " 1019 DEX TEST\n" +
            "Before TotalDEX = " + before + "\n" +
            "After TotalDEX = " + after + "\n" +
            "Difference = " + (after - before) + "\n" +
            "Expected difference = +1."
        );
    }

    [ContextMenu("TEST 1019 - Add CON +1")]
    public void Test1019CON()
    {
        Header(1019, "Modification Device / CON");

        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning(TAG + " 1019 FAIL: PlayerStats.Instance is null.");
            return;
        }

        int before = PlayerStats.Instance.TotalCON;
        PlayerStats.Instance.AddCON();
        int after = PlayerStats.Instance.TotalCON;

        Debug.Log(
            TAG + " 1019 CON TEST\n" +
            "Before TotalCON = " + before + "\n" +
            "After TotalCON = " + after + "\n" +
            "Difference = " + (after - before) + "\n" +
            "Expected difference = +1."
        );
    }

    [ContextMenu("TEST 1019 - Add INT +1")]
    public void Test1019INT()
    {
        Header(1019, "Modification Device / INT");

        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning(TAG + " 1019 FAIL: PlayerStats.Instance is null.");
            return;
        }

        int before = PlayerStats.Instance.TotalINT;
        PlayerStats.Instance.AddINT();
        int after = PlayerStats.Instance.TotalINT;

        Debug.Log(
            TAG + " 1019 INT TEST\n" +
            "Before TotalINT = " + before + "\n" +
            "After TotalINT = " + after + "\n" +
            "Difference = " + (after - before) + "\n" +
            "Expected difference = +1."
        );
    }

    [ContextMenu("TEST 1020 - Solastone")]
    public void Test1020()
    {
        Header(1020, "Solastone");

        Debug.Log(
            TAG + " 1020 INFO\n" +
            "Trading currency item.\n" +
            "No direct-use effect is implemented yet because Shop/Trade system is deferred."
        );
    }

    [ContextMenu("TEST 1021 - Key")]
    public void Test1021()
    {
        Header(1021, "Key");

        Debug.Log(
            TAG + " 1021 INFO\n" +
            "Direct inventory use is intentionally disabled.\n" +
            "Current LockedDoorManager still uses legacy key IDs 3001~3004.\n" +
            "Generic item 1021 migration is NOT applied yet."
        );
    }

    [ContextMenu("TEST 1022 - Smoke Bomb Accuracy -30")]
    public void Test1022()
    {
        Header(1022, "Smoke Bomb");

        if (Battle == null || !Battle.IsBattleRunning())
        {
            Debug.LogWarning(TAG + " 1022 FAIL: Start a battle first.");
            return;
        }

        Battle.SetEnemyAccuracyModifierForNextEnemyTurn(-30);

        Debug.Log(
            TAG + " 1022 ACTIVATED\n" +
            "Enemy accuracy modifier = -30\n" +
            "On the next enemy turn, BattleManager's final enemy accuracy log should be 30 lower than normal.\n" +
            "Modifier resets when that enemy turn ends."
        );
    }

    [ContextMenu("TEST 1023 - Invisibility Device")]
    public void Test1023()
    {
        Header(1023, "Invisibility Device");

        ItemRuntimeEffectManager runtime =
            ItemRuntimeEffectManager.EnsureExists();

        runtime.AddEncounterEffect(-25f, 10, true);

        Debug.Log(
            TAG + " 1023 ACTIVATED\n" +
            "Encounter flat modifier = " +
            runtime.GetEncounterChanceFlatModifier() + "\n" +
            "Duration = 10 dungeon turns\n" +
            "Expected flat modifier = -25."
        );
    }

    [ContextMenu("PRINT Runtime Item Effects")]
    public void PrintRuntimeEffects()
    {
        Header(0, "Runtime Effect Snapshot");

        ItemRuntimeEffectManager runtime =
            ItemRuntimeEffectManager.EnsureExists();

        Debug.Log(
            TAG + " RUNTIME SNAPSHOT\n" +
            "Encounter multiplier = x" +
            runtime.GetEncounterChanceMultiplier() + "\n" +
            "Encounter flat modifier = " +
            runtime.GetEncounterChanceFlatModifier() + "\n" +
            "Farming multiplier = x" +
            runtime.GetFarmingQuantityMultiplier()
        );
    }
}
