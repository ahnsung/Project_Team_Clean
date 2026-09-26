using UnityEngine;

/// <summary>
/// 기획서/엑셀의 상태이상 테이블 숫자 값을
/// 현재 프로젝트 enum으로 변환한다.
///
/// 중요:
/// 1) 상태이상 테이블 effect_type과
///    무기/아이템 테이블 effect_type은 서로 다른 컬럼이다.
/// 2) when_decrease_duration과 when__buff_effect도
///    숫자 체계가 서로 다르므로 반드시 별도로 변환한다.
/// </summary>
public static class StatusEffectTypeMapper
{
    // =========================================================
    // 상태이상 테이블 effect_type
    // =========================================================

    public static StatusEffectType FromTableEffectType(
        int tableEffectType)
    {
        switch (tableEffectType)
        {
            case 0:
                return StatusEffectType.Stun;

            case 1:
                return StatusEffectType.HealthHealingDown;

            case 2:
                return StatusEffectType.HungerHealingDown;

            case 3:
                return StatusEffectType.MentalHealingDown;

            case 4:
                return StatusEffectType.Poison;

            case 5:
                return StatusEffectType.DamageTakenUp;

            case 6:
                return StatusEffectType.Silence;

            case 7:
                return StatusEffectType.Confusion;

            case 8:
                return StatusEffectType.Corrosion;

            case 9:
                return StatusEffectType.Guard;

            /*
             * 10~15는 원본 엑셀 내부에서
             * effect_type 설명과 실제 행 description이
             * 서로 일치하지 않는 부분이 있다.
             *
             * 예:
             * effect_type 12의 표 설명 = "배고픔 25%"
             * ID 2013의 description = "공격 시 데미지 50% 감소"
             *
             * 따라서 이 구간은 임의로 다른 전투 효과에
             * 연결하지 않는다.
             *
             * 현재 2011~2016의 정확한 실제 기능은
             * 별도 구현 단계에서 ID/description 기준으로
             * 처리한다.
             */
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
                return StatusEffectType.StatIncrease;

            case 16:
                return StatusEffectType.StrengthUp;

            case 17:
                return StatusEffectType.DexterityUp;

            case 18:
                return StatusEffectType.IntelligenceUp;

            case 19:
                return StatusEffectType.ConstitutionUp;

            case 20:
                return StatusEffectType.StrengthDown;

            case 21:
                return StatusEffectType.DexterityDown;

            case 22:
                return StatusEffectType.IntelligenceDown;

            case 23:
                return StatusEffectType.ConstitutionDown;

            case 24:
                return StatusEffectType.BattleEncounterRateUp;

            case 25:
                return StatusEffectType.ItemAcquisitionUp;

            case 26:
                return StatusEffectType.AccuracyUp;

            case 27:
                return StatusEffectType.AttackPowerUp;

            case 28:
                return StatusEffectType.DamageTakenDown;

            default:
                Debug.LogWarning(
                    "[StatusEffectTypeMapper] " +
                    "알 수 없는 상태이상 effect_type: " +
                    tableEffectType
                );

                return StatusEffectType.None;
        }
    }


    public static bool IsSupported(
        int tableEffectType)
    {
        return
            tableEffectType >= 0 &&
            tableEffectType <= 28;
    }


    // =========================================================
    // when_decrease_duration
    //
    // 원본 엑셀:
    // 0 자신 순서 시작
    // 1 자신 순서 끝
    // 2 플레이어 순서 시작
    // 3 플레이어 순서 끝
    // 4 적 순서 시작
    // 5 적 순서 끝
    // 6 턴 시작 시
    // 7 턴 종료 시
    // 8 감소 없음
    // =========================================================

    public static StatusEffectTiming
        FromTableDecreaseTiming(
            int tableValue)
    {
        switch (tableValue)
        {
            case 0:
                return StatusEffectTiming.SelfTurnStart;

            case 1:
                return StatusEffectTiming.SelfTurnEnd;

            case 2:
                return StatusEffectTiming.PlayerTeamStart;

            case 3:
                return StatusEffectTiming.PlayerTeamEnd;

            case 4:
                return StatusEffectTiming.EnemyTeamStart;

            case 5:
                return StatusEffectTiming.EnemyTeamEnd;

            case 6:
                return StatusEffectTiming.TurnStart;

            case 7:
                return StatusEffectTiming.TurnEnd;

            case 8:
                return StatusEffectTiming.None;

            default:
                Debug.LogWarning(
                    "[StatusEffectTypeMapper] " +
                    "알 수 없는 when_decrease_duration: " +
                    tableValue
                );

                return StatusEffectTiming.None;
        }
    }


    // =========================================================
    // when__buff_effect
    //
    // 원본 엑셀:
    // 0 자신 순서 시작
    // 1 자신 순서 끝
    // 2 플레이어 순서 시작
    // 3 플레이어 순서 끝
    // 4 적 순서 시작
    // 5 적 순서 끝
    // 6 상태이상 중에는 계속 지속
    // 7 공격 적중 시
    // 8 공격 피격 시
    // 9 턴 시작 시
    // 10 턴 종료 시
    //
    // decrease_duration과 6 이후의 의미가 완전히 다르다.
    // =========================================================

    public static StatusEffectTiming
        FromTableBuffEffectTiming(
            int tableValue)
    {
        switch (tableValue)
        {
            case 0:
                return StatusEffectTiming.SelfTurnStart;

            case 1:
                return StatusEffectTiming.SelfTurnEnd;

            case 2:
                return StatusEffectTiming.PlayerTeamStart;

            case 3:
                return StatusEffectTiming.PlayerTeamEnd;

            case 4:
                return StatusEffectTiming.EnemyTeamStart;

            case 5:
                return StatusEffectTiming.EnemyTeamEnd;

            case 6:
                return StatusEffectTiming.Continuous;

            case 7:
                return StatusEffectTiming.AttackHit;

            case 8:
                return StatusEffectTiming.AttackTaken;

            case 9:
                return StatusEffectTiming.TurnStart;

            case 10:
                return StatusEffectTiming.TurnEnd;

            default:
                Debug.LogWarning(
                    "[StatusEffectTypeMapper] " +
                    "알 수 없는 when__buff_effect: " +
                    tableValue
                );

                return StatusEffectTiming.None;
        }
    }


    // =========================================================
    // when_remove
    //
    // 원본 엑셀 값을 그대로 변환한다.
    // =========================================================

    public static StatusEffectRemoveType
        FromTableRemoveType(
            int tableValue)
    {
        switch (tableValue)
        {
            case 0:
                return StatusEffectRemoveType.DurationEnded;

            case 1:
                return StatusEffectRemoveType.EffectTriggered;

            case 2:
                return StatusEffectRemoveType.HungerAbove70;

            case 3:
                return StatusEffectRemoveType.HungerAbove50;

            case 4:
                return StatusEffectRemoveType.HungerAbove25;

            case 5:
                return StatusEffectRemoveType.MentalAbove75;

            case 6:
                return StatusEffectRemoveType.MentalAbove50;

            case 7:
                return StatusEffectRemoveType.MentalAbove25;

            default:
                Debug.LogWarning(
                    "[StatusEffectTypeMapper] " +
                    "알 수 없는 when_remove: " +
                    tableValue
                );

                return StatusEffectRemoveType.DurationEnded;
        }
    }


    // =========================================================
    // 기존 호환 함수
    //
    // 이전 StatusEffectDatabase에서 호출하고 있으므로
    // 삭제하지 않는다.
    //
    // 단, 실제 원본 테이블을 읽을 때는
    // 반드시 FromTableRemoveType()을 사용해야 한다.
    // =========================================================

    public static StatusEffectRemoveType
        GetRemoveType(
            int tableEffectType)
    {
        switch (tableEffectType)
        {
            case 10:
                return StatusEffectRemoveType.HungerAbove70;

            case 11:
                return StatusEffectRemoveType.HungerAbove50;

            case 12:
                return StatusEffectRemoveType.HungerAbove25;

            case 13:
                return StatusEffectRemoveType.MentalAbove75;

            case 14:
                return StatusEffectRemoveType.MentalAbove50;

            case 15:
                return StatusEffectRemoveType.MentalAbove25;

            default:
                return StatusEffectRemoveType.DurationEnded;
        }
    }


    public static bool IsResourceConditionEffect(
        int tableEffectType)
    {
        return
            tableEffectType >= 10 &&
            tableEffectType <= 15;
    }


    // =========================================================
    // 디버그용 이름
    // =========================================================

    public static string GetTableEffectName(
        int tableEffectType)
    {
        switch (tableEffectType)
        {
            case 0: return "기절";
            case 1: return "부상";
            case 2: return "피로";
            case 3: return "좌절";
            case 4: return "중독";
            case 5: return "취약";
            case 6: return "침묵";
            case 7: return "혼란";
            case 8: return "부식";
            case 9: return "방어";
            case 10: return "배고픔 70%";
            case 11: return "배고픔 50%";
            case 12: return "배고픔 25%";
            case 13: return "정신력 75%";
            case 14: return "정신력 50%";
            case 15: return "정신력 25%";
            case 16: return "STR 증가";
            case 17: return "DEX 증가";
            case 18: return "INT 증가";
            case 19: return "CON 증가";
            case 20: return "STR 감소";
            case 21: return "DEX 감소";
            case 22: return "INT 감소";
            case 23: return "CON 감소";
            case 24: return "전투 발생률 증가";
            case 25: return "아이템 획득 증가";
            case 26: return "명중률";
            case 27: return "공격력 증가";
            case 28: return "받는 데미지 감소";

            default:
                return "알 수 없음";
        }
    }
}
