using System;
using UnityEngine;

[Serializable]
public class ItemEffectData
{
    public int itemId;
    public int effectId;
    public int target;
    public int duration;
    public int effectType;
    public float effectValue;
    public int effectTiming;
    public int useCondition;
    public bool stackable;
    public int statusId;
    public int order;

    public ItemEffectData(
        int itemId,
        int effectType,
        float effectValue,
        int statusId,
        int order,
        int effectId = 0,
        int target = 0,
        int duration = 0,
        int effectTiming = 0,
        int useCondition = 0,
        bool stackable = false)
    {
        this.itemId = itemId;
        this.effectId = effectId;
        this.target = target;
        this.duration = duration;
        this.effectType = effectType;
        this.effectValue = effectValue;
        this.effectTiming = effectTiming;
        this.useCondition = useCondition;
        this.stackable = stackable;
        this.statusId = statusId;
        this.order = order;
    }

    public bool IsStatusEffect => effectType == 17 && statusId > 0;
}
