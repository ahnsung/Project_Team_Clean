using System.Collections.Generic;
using UnityEngine;

public class ItemRuntimeEffectManager : MonoBehaviour
{
    public static ItemRuntimeEffectManager Instance
    {
        get;
        private set;
    }

    private class TimedEffect
    {
        public float value;
        public int endTurn;

        public TimedEffect(
            float value,
            int endTurn)
        {
            this.value = value;
            this.endTurn = endTurn;
        }
    }

    private readonly List<TimedEffect>
        encounterEffects =
            new List<TimedEffect>();

    private readonly List<TimedEffect>
        farmingEffects =
            new List<TimedEffect>();

    public static ItemRuntimeEffectManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameObject obj =
            new GameObject(
                "ItemRuntimeEffectManager"
            );

        return obj.AddComponent<
            ItemRuntimeEffectManager>();
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        RemoveExpiredEffects();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddEncounterEffect(
        float value,
        int duration,
        bool stackable)
    {
        AddTimedEffect(
            encounterEffects,
            value,
            duration,
            stackable
        );

        Debug.Log(
            "[ItemRuntimeEffectManager] " +
            "전투 발생률 효과 추가 / Value: " +
            value +
            " / Duration: " +
            duration
        );
    }

    public void AddFarmingMultiplier(
        float multiplier,
        int duration,
        bool stackable)
    {
        AddTimedEffect(
            farmingEffects,
            multiplier,
            duration,
            stackable
        );

        Debug.Log(
            "[ItemRuntimeEffectManager] " +
            "파밍 획득량 배율 추가 / x" +
            multiplier +
            " / Duration: " +
            duration
        );
    }

    private void AddTimedEffect(
        List<TimedEffect> list,
        float value,
        int duration,
        bool stackable)
    {
        RemoveExpiredEffects();

        if (!stackable)
            list.Clear();

        int currentTurn =
            DungeonManager.Instance != null
                ? DungeonManager.Instance.CurrentTurn
                : 0;

        int endTurn =
            currentTurn +
            Mathf.Max(
                1,
                duration
            );

        list.Add(
            new TimedEffect(
                value,
                endTurn
            )
        );
    }

    public float GetEncounterChanceMultiplier()
    {
        RemoveExpiredEffects();

        float multiplier = 1f;

        foreach (TimedEffect effect in encounterEffects)
        {
            // 원본 표에서 *1.5처럼 1보다 큰 값은 배율.
            if (effect.value > 1f)
                multiplier *= effect.value;
        }

        return multiplier;
    }

    public float GetEncounterChanceFlatModifier()
    {
        RemoveExpiredEffects();

        float modifier = 0f;

        foreach (TimedEffect effect in encounterEffects)
        {
            // 원본 표의 -25처럼 음수 값은 %p 가감값으로 처리.
            if (effect.value <= 1f)
                modifier += effect.value;
        }

        return modifier;
    }

    public float GetFarmingQuantityMultiplier()
    {
        RemoveExpiredEffects();

        float multiplier = 1f;

        foreach (TimedEffect effect in farmingEffects)
        {
            if (effect.value > 0f)
                multiplier *= effect.value;
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }

    private void RemoveExpiredEffects()
    {
        if (DungeonManager.Instance == null)
            return;

        int currentTurn =
            DungeonManager.Instance.CurrentTurn;

        encounterEffects.RemoveAll(
            effect =>
                effect == null ||
                currentTurn >= effect.endTurn
        );

        farmingEffects.RemoveAll(
            effect =>
                effect == null ||
                currentTurn >= effect.endTurn
        );
    }

    [ContextMenu("DEBUG - Print Runtime Item Effects")]
    private void DebugPrintEffects()
    {
        RemoveExpiredEffects();

        Debug.Log(
            "[ItemRuntimeEffectManager]\n" +
            "Encounter Multiplier: " +
            GetEncounterChanceMultiplier() +
            "\nEncounter Flat: " +
            GetEncounterChanceFlatModifier() +
            "\nFarming Multiplier: " +
            GetFarmingQuantityMultiplier()
        );
    }
}
