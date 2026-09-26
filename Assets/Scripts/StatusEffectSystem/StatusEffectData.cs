using System;
using UnityEngine;

[Serializable]
public class StatusEffectData
{
    [Header("Identity")]
    public int id;

    public string buffName;

    [TextArea(2, 5)]
    public string description;

    [Header("UI")]
    public Sprite icon;

    [Header("Type")]
    public StatusEffectTendency tendency;

    public StatusEffectType effectType;

    [Header("Effect")]
    [Tooltip("상태이상 효과의 수치입니다. 예: 공격력 +20%라면 20")]
    public int effectPower;

    [Header("Duration")]
    [Tooltip("0이면 무한 지속")]
    public int buffDuration;

    [Header("Timing")]
    public StatusEffectTiming whenDecreaseDuration;

    public StatusEffectTiming whenBuffEffect;

    [Header("Stack")]
    public bool canStack;

    [Header("Remove Rule")]
    public StatusEffectRemoveType whenRemove =
        StatusEffectRemoveType.DurationEnded;

    public bool IsInfiniteDuration
    {
        get
        {
            return buffDuration <= 0;
        }
    }

    public StatusEffectData Clone()
    {
        return new StatusEffectData
        {
            id = id,
            buffName = buffName,
            description = description,

            icon = icon,

            tendency = tendency,
            effectType = effectType,
            effectPower = effectPower,

            buffDuration = buffDuration,

            whenDecreaseDuration =
                whenDecreaseDuration,

            whenBuffEffect =
                whenBuffEffect,

            canStack = canStack,
            whenRemove = whenRemove
        };
    }
}

public enum StatusEffectRemoveType
{
    DurationEnded = 0,

    EffectTriggered = 1,

    HungerAbove70 = 2,
    HungerAbove50 = 3,
    HungerAbove25 = 4,

    MentalAbove75 = 5,
    MentalAbove50 = 6,
    MentalAbove25 = 7
}