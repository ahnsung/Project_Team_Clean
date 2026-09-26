using UnityEngine;

public static class TrapStatusEffectFactory
{
    public static StatusEffectData Create(
        int trapType,
        int duration)
    {
        int safeDuration =
            Mathf.Max(1, duration);

        switch (trapType)
        {
            // =============================================
            // 1 - 부상
            // 체력 회복량 50% 감소
            // =============================================

            case 1:
                return CreateDebuff(
                    2002,
                    "부상",
                    "체력 회복량이 50% 감소합니다.",
                    StatusEffectType.HealthHealingDown,
                    50,
                    safeDuration,
                    true
                );


            // =============================================
            // 2 - 피로
            // 배고픔 회복량 50% 감소
            // =============================================

            case 2:
                return CreateDebuff(
                    2003,
                    "피로",
                    "배고픔 회복량이 50% 감소합니다.",
                    StatusEffectType.HungerHealingDown,
                    50,
                    safeDuration,
                    true
                );


            // =============================================
            // 3 - 좌절
            // 정신력 회복량 50% 감소
            // =============================================

            case 3:
                return CreateDebuff(
                    2004,
                    "좌절",
                    "정신력 회복량이 50% 감소합니다.",
                    StatusEffectType.MentalHealingDown,
                    50,
                    safeDuration,
                    true
                );


            // =============================================
            // 4 - 중독
            // 최대 체력 10%
            // =============================================

            case 4:
                return new StatusEffectData
                {
                    id = 2005,

                    buffName = "중독",

                    description =
                        "턴 종료마다 최대 체력의 10% 피해를 받습니다.",

                    tendency =
                        StatusEffectTendency.Negative,

                    effectType =
                        StatusEffectType.Poison,

                    effectPower = 10,

                    buffDuration =
                        safeDuration,

                    whenDecreaseDuration =
                        StatusEffectTiming.TurnEnd,

                    whenBuffEffect =
                        StatusEffectTiming.TurnEnd,

                    canStack = false,

                    whenRemove =
                        StatusEffectRemoveType.DurationEnded
                };


            // =============================================
            // 5 - 취약
            // 받는 피해 +100%
            // =============================================

            case 5:
                return CreateDebuff(
                    2006,
                    "취약",
                    "받는 피해가 100% 증가합니다.",
                    StatusEffectType.DamageTakenUp,
                    100,
                    safeDuration,
                    false
                );


            // =============================================
            // 6 - 침묵
            // =============================================

            case 6:
                return CreateDebuff(
                    2007,
                    "침묵",
                    "스킬을 사용할 수 없습니다.",
                    StatusEffectType.Silence,
                    0,
                    safeDuration,
                    false
                );


            // =============================================
            // 7 - 혼란
            // =============================================

            case 7:
                return CreateDebuff(
                    2008,
                    "혼란",
                    "아이템을 사용할 수 없습니다.",
                    StatusEffectType.Confusion,
                    0,
                    safeDuration,
                    false
                );


            // =============================================
            // 8 - 부식
            //
            // 현재 실제 Trap CSV에는 없음.
            // 테스트/향후 데이터용.
            // =============================================

            case 8:
                return CreateDebuff(
                    2009,
                    "부식",
                    "장비 내구도 소모량이 증가합니다.",
                    StatusEffectType.Corrosion,
                    100,
                    safeDuration,
                    false
                );


            // =============================================
            // 20 - STR 감소
            // =============================================

            case 20:
                return CreateStatDown(
                    2021,
                    "STR 감소",
                    StatusEffectType.StrengthDown,
                    safeDuration
                );


            // =============================================
            // 21 - DEX 감소
            // =============================================

            case 21:
                return CreateStatDown(
                    2022,
                    "DEX 감소",
                    StatusEffectType.DexterityDown,
                    safeDuration
                );


            // =============================================
            // 22 - INT 감소
            // =============================================

            case 22:
                return CreateStatDown(
                    2023,
                    "INT 감소",
                    StatusEffectType.IntelligenceDown,
                    safeDuration
                );


            // =============================================
            // 23 - CON 감소
            // =============================================

            case 23:
                return CreateStatDown(
                    2024,
                    "CON 감소",
                    StatusEffectType.ConstitutionDown,
                    safeDuration
                );


            default:
                Debug.LogWarning(
                    "[TrapStatusEffectFactory] " +
                    "아직 실제 데이터가 연결되지 않은 TrapType: " +
                    trapType
                );

                return null;
        }
    }


    private static StatusEffectData CreateDebuff(
        int id,
        string name,
        string description,
        StatusEffectType type,
        int power,
        int duration,
        bool canStack)
    {
        return new StatusEffectData
        {
            id = id,

            buffName = name,

            description = description,

            tendency =
                StatusEffectTendency.Negative,

            effectType = type,

            effectPower = power,

            buffDuration =
                duration,

            whenDecreaseDuration =
                StatusEffectTiming.TurnEnd,

            whenBuffEffect =
                StatusEffectTiming.None,

            canStack =
                canStack,

            whenRemove =
                StatusEffectRemoveType.DurationEnded
        };
    }


    private static StatusEffectData CreateStatDown(
        int id,
        string name,
        StatusEffectType type,
        int duration)
    {
        return new StatusEffectData
        {
            id = id,

            buffName = name,

            description =
                name + " 상태입니다.",

            tendency =
                StatusEffectTendency.Negative,

            effectType = type,

            // 기획 데이터 자체가 음수
            effectPower = -1,

            buffDuration =
                duration,

            whenDecreaseDuration =
                StatusEffectTiming.TurnEnd,

            whenBuffEffect =
                StatusEffectTiming.None,

            /*
             * 매우 중요:
             *
             * 2턴짜리 DEX 감소
             * +
             * 5턴짜리 DEX 감소
             *
             * => 서로 독립적으로 존재
             */
            canStack = true,

            whenRemove =
                StatusEffectRemoveType.DurationEnded
        };
    }
}