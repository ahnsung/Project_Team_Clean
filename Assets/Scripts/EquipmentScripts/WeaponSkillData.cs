using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponSkillData
{
    [Header("Skill")]
    public int skillId;

    public string skillName;

    [TextArea(2, 5)]
    public string description;

    [Min(0)]
    public int useDurability;

    [Header("Effects")]
    public List<WeaponSkillEffectData> effects =
        new List<WeaponSkillEffectData>();


    public bool HasEffects
    {
        get
        {
            return effects != null &&
                   effects.Count > 0;
        }
    }


    public bool RequiresEnemyTarget
    {
        get
        {
            if (effects == null)
                return false;

            foreach (WeaponSkillEffectData effect in effects)
            {
                if (effect == null)
                    continue;

                if (effect.target ==
                    WeaponSkillTarget.Enemy)
                {
                    return true;
                }
            }

            return false;
        }
    }


    public List<WeaponSkillEffectData>
        GetOrderedEffects()
    {
        List<WeaponSkillEffectData> result =
            new List<WeaponSkillEffectData>();

        if (effects == null)
            return result;

        foreach (WeaponSkillEffectData effect in effects)
        {
            if (effect != null)
            {
                result.Add(effect);
            }
        }

        result.Sort(
            (a, b) =>
                a.order.CompareTo(b.order)
        );

        return result;
    }
}


[Serializable]
public class WeaponSkillEffectData
{
    [Header("Identity")]
    public int effectId;

    [Header("Target")]
    public WeaponSkillTarget target;

    [Header("Effect")]
    public WeaponSkillEffectType effectType;

    public int effectValue;

    [Tooltip(
        "EffectType이 StatusEffect일 때 적용할 " +
        "상태이상 ID입니다."
    )]
    public int statusId;

    [Header("Execution")]
    [Tooltip(
        "하나의 스킬에 여러 효과가 있을 경우 " +
        "낮은 Order부터 실행합니다."
    )]
    public int order;
}


public enum WeaponSkillTarget
{
    Self = 0,
    Enemy = 1
}


public enum WeaponSkillEffectType
{
    /*
     * 무기 스킬 효과 테이블 기준.
     *
     * 0 = 직접 피해
     * 17 = 상태이상 적용
     *
     * 현재 기획서에서 무기 스킬에 실제로
     * 사용되는 타입만 우선 구현한다.
     */

    Damage = 0,

    StatusEffect = 17
}