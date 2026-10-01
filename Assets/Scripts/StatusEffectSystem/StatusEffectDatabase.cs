using System.Collections.Generic;
using UnityEngine;

public class StatusEffectDatabase : MonoBehaviour
{
    public static StatusEffectDatabase Instance;

    private readonly Dictionary<int, StatusEffectData> database =
        new Dictionary<int, StatusEffectData>();


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateStatusEffects();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // 상태이상 생성
    //
    // "상태이상 테이블 (1).xlsx"의 실제 데이터 행을
    // 그대로 등록한다.
    //
    // 등록 행:
    // 2001, 2101, 2002~2024, 2124, 2130
    //
    // 추가로 기존 무기 스킬용:
    // 2125, 2126, 2127
    // =========================================================

    private void CreateStatusEffects()
    {
        database.Clear();

        CreateTableStatusEffects();

        // 기존 무기 스킬 전용 상태이상
        CreateGuaranteedHit();
        CreatePowerStrike();
        CreateEndure();

        Debug.Log(
            "[StatusEffectDatabase] 상태이상 등록 완료: " +
            database.Count +
            "개"
        );
    }


    // =========================================================
    // 원본 상태이상 테이블
    //
    // RegisterTableRow 파라미터 순서:
    //
    // ID
    // buff_name
    // tendency
    // effect_type
    // effect_power
    // buff_duration
    // when_decrease_duration
    // when__buff_effect
    // can_stack
    // when_remove
    // description
    // =========================================================

    private void CreateTableStatusEffects()
    {
        // -----------------------------------------------------
        // 2001 / 2101 - 기절
        // -----------------------------------------------------

        RegisterTableRow(
            2001,
            "기절",
            0,
            0,
            0,
            1,
            1,
            6,
            1,
            0,
            "지속 시간 동안 행동할 수 없다. " +
            "(이동, 전투 시의 행동, 아이템 사용 불가) " +
            "전투 시에는 플레이어의 턴은 스킵되고, " +
            "전투 외에는 턴이 그냥 지나간다."
        );

        RegisterTableRow(
            2101,
            "기절",
            0,
            0,
            0,
            5,
            1,
            6,
            1,
            0,
            "지속 시간 동안 행동할 수 없다. " +
            "(이동, 전투 시의 행동, 아이템 사용 불가) " +
            "전투 시에는 플레이어의 턴은 스킵되고, " +
            "전투 외에는 턴이 그냥 지나간다."
        );


        // -----------------------------------------------------
        // 2002~2009 - 기본 부정 상태이상
        // -----------------------------------------------------

        RegisterTableRow(
            2002,
            "부상",
            0,
            1,
            0,
            1,
            1,
            6,
            1,
            0,
            "지속 시간 동안 체력 회복량이 50% 감소한다."
        );

        RegisterTableRow(
            2003,
            "피로",
            0,
            2,
            0,
            1,
            1,
            6,
            1,
            0,
            "지속 시간 동안 배고픔 회복량이 50% 감소한다."
        );

        RegisterTableRow(
            2004,
            "좌절",
            0,
            3,
            0,
            1,
            1,
            6,
            1,
            0,
            "지속 시간 동안 정신력 회복량이 50% 감소한다."
        );

        RegisterTableRow(
            2005,
            "중독",
            0,
            4,
            10,
            1,
            7,
            10,
            0,
            0,
            "지속 시간 동안 매 턴 최대 체력의 일정 %만큼 피해를 입는다."
        );

        RegisterTableRow(
            2006,
            "취약",
            0,
            5,
            100,
            1,
            0,
            6,
            0,
            0,
            "지속 시간 동안 적으로부터 받는 피해가 x% 증가한다."
        );

        RegisterTableRow(
            2007,
            "침묵",
            0,
            6,
            0,
            1,
            1,
            6,
            1,
            0,
            "지속 시간 동안 무기 스킬을 사용할 수 없다."
        );

        RegisterTableRow(
            2008,
            "혼란",
            0,
            7,
            0,
            1,
            1,
            6,
            1,
            0,
            "지속 시간 동안 아이템을 사용할 수 없다."
        );

        RegisterTableRow(
            2009,
            "부식",
            0,
            8,
            0,
            1,
            0,
            6,
            1,
            0,
            "지속 시간 동안 장비의 내구도가 2배로 감소한다."
        );


        // -----------------------------------------------------
        // 2010 - 방어
        // -----------------------------------------------------

        RegisterTableRow(
            2010,
            "방어",
            1,
            9,
            0,
            1,
            5,
            6,
            0,
            0,
            "지속 시간 동안 받는 데미지가 50% 감소하고 " +
            "방어구의 내구도 소모율이 2배가 되며 " +
            "공격한 적을 1턴 기절 시킨다."
        );


        // -----------------------------------------------------
        // 2011~2016 - 배고픔 / 정신력 조건 상태
        //
        // 중요:
        // 아래 값은 엑셀 원본을 수정하지 않고 그대로 등록한다.
        //
        // 특히 2013~2016은 effect_type 이름과 description의
        // 실제 기능 사이에 별도 구현이 필요한 부분이 있으므로
        // 여기서는 데이터를 임의로 바꾸지 않는다.
        // -----------------------------------------------------

        RegisterTableRow(
            2011,
            "배고픔 70%",
            0,
            1,
            0,
            0,
            8,
            6,
            1,
            2,
            "지속 시간 동안 체력 회복량이 50% 감소한다."
        );

        RegisterTableRow(
            2012,
            "배고픔 50%",
            0,
            3,
            0,
            0,
            8,
            6,
            0,
            3,
            "지속 시간 동안 정신력 회복량이 50% 감소한다."
        );

        RegisterTableRow(
            2013,
            "배고픔 25%",
            0,
            12,
            0,
            0,
            8,
            6,
            0,
            4,
            "공격 시 데미지 50% 감소"
        );

        RegisterTableRow(
            2014,
            "정신력 75%",
            0,
            13,
            0,
            0,
            8,
            6,
            0,
            5,
            "공격 명중률 감소 " +
            "(잃은 정신력 10%당 명중률 5% 감소)"
        );

        RegisterTableRow(
            2015,
            "정신력 50%",
            0,
            14,
            0,
            0,
            8,
            6,
            0,
            6,
            "이벤트 성공률 25% 감소"
        );

        RegisterTableRow(
            2016,
            "정신력 25%",
            0,
            15,
            0,
            0,
            8,
            6,
            0,
            7,
            "모든 자원의 감소량 50% 증가"
        );


        // -----------------------------------------------------
        // 2017~2020 - 능력치 증가
        // -----------------------------------------------------

        RegisterTableRow(
            2017,
            "STR 증가",
            1,
            16,
            1,
            1,
            1,
            6,
            0,
            0,
            null
        );

        RegisterTableRow(
            2018,
            "DEX 증가",
            1,
            17,
            1,
            1,
            1,
            6,
            0,
            0,
            null
        );

        RegisterTableRow(
            2019,
            "INT 증가",
            1,
            18,
            1,
            1,
            1,
            6,
            0,
            0,
            null
        );

        RegisterTableRow(
            2020,
            "CON 증가",
            1,
            19,
            1,
            1,
            1,
            6,
            0,
            0,
            null
        );


        // -----------------------------------------------------
        // 2021~2024 - 능력치 감소
        // -----------------------------------------------------

        RegisterTableRow(
            2021,
            "STR 감소",
            0,
            20,
            -1,
            1,
            1,
            6,
            0,
            0,
            null
        );

        RegisterTableRow(
            2022,
            "DEX 감소",
            0,
            21,
            -1,
            1,
            1,
            6,
            0,
            0,
            null
        );

        RegisterTableRow(
            2023,
            "INT 감소",
            0,
            22,
            -1,
            1,
            1,
            6,
            0,
            0,
            null
        );

        RegisterTableRow(
            2024,
            "CON 감소",
            0,
            23,
            -1,
            1,
            1,
            6,
            0,
            0,
            null
        );


        // -----------------------------------------------------
        // 2124 - CON 감소 강화형
        // -----------------------------------------------------

        RegisterTableRow(
            2124,
            "CON 감소",
            0,
            23,
            -2,
            2,
            1,
            6,
            0,
            0,
            null
        );

        // -----------------------------------------------------
        // 2130 - 아이템 획득 증가
        // 파밍 시 아이템을 추가로 1개 획득
        // -----------------------------------------------------

        RegisterTableRow(
            2130,
            "아이템 획득 증가",
            1,
            25,
            1,
            10,
            7,
            6,
            1,
            0,
            "파밍 시 아이템을 추가로 1개 획득한다."
        );

        // -----------------------------------------------------
        // 2132 - 명중률 감소
        // 패턴 5002 "위액 뿌리기"에서 참조
        // -----------------------------------------------------

        RegisterTableRow(
            2132,
            "명중률 감소",
            0,
            26,
            -20,
            3,
            3,
            3,
            1,
            0,
            "명중률을 20% 감소시킨다."
        );

    }


    // =========================================================
    // 원본 테이블 행 등록
    // =========================================================

    private void RegisterTableRow(
        int statusId,
        string statusName,
        int tendencyValue,
        int tableEffectType,
        int effectPower,
        int duration,
        int decreaseTimingValue,
        int buffEffectTimingValue,
        int canStackValue,
        int removeTypeValue,
        string description)
    {
        if (!StatusEffectTypeMapper.IsSupported(
                tableEffectType))
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "지원하지 않는 effect_type: " +
                tableEffectType +
                " / ID: " +
                statusId
            );

            return;
        }

        StatusEffectType effectType =
            StatusEffectTypeMapper
                .FromTableEffectType(
                    tableEffectType
                );

        if (effectType ==
            StatusEffectType.None)
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "effect_type 변환 실패" +
                " / ID: " +
                statusId +
                " / effect_type: " +
                tableEffectType
            );

            return;
        }

        StatusEffectData data =
            new StatusEffectData
            {
                id = statusId,

                buffName =
                    string.IsNullOrWhiteSpace(
                        statusName)
                        ? "상태이상 " + statusId
                        : statusName,

                description =
                    description ?? string.Empty,

                tendency =
                    tendencyValue == 1
                        ? StatusEffectTendency.Positive
                        : StatusEffectTendency.Negative,

                effectType =
                    effectType,

                effectPower =
                    effectPower,

                buffDuration =
                    Mathf.Max(0, duration),

                whenDecreaseDuration =
                    StatusEffectTypeMapper
                        .FromTableDecreaseTiming(
                            decreaseTimingValue
                        ),

                whenBuffEffect =
                    StatusEffectTypeMapper
                        .FromTableBuffEffectTiming(
                            buffEffectTimingValue
                        ),

                canStack =
                    canStackValue == 1,

                whenRemove =
                    StatusEffectTypeMapper
                        .FromTableRemoveType(
                            removeTypeValue
                        )
            };

        Register(data);
    }


    // =========================================================
    // 2125 - 확정 명중
    //
    // 기존 무기 스킬용 특수 상태.
    // 다음 공격 완료 후 BattleManager에서 직접 1회 소모.
    // =========================================================

    private void CreateGuaranteedHit()
    {
        Register(
            new StatusEffectData
            {
                id = 2125,

                buffName =
                    "확정 명중",

                description =
                    "다음 공격이 반드시 명중합니다.",

                tendency =
                    StatusEffectTendency.Positive,

                effectType =
                    StatusEffectTypeMapper
                        .FromTableEffectType(26),

                effectPower = 0,

                buffDuration = 1,

                whenDecreaseDuration =
                    StatusEffectTiming.None,

                whenBuffEffect =
                    StatusEffectTiming.Continuous,

                canStack = true,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            }
        );
    }


    // =========================================================
    // 2126 - 강타
    //
    // 기존 무기 스킬용 특수 상태.
    // 실제 2배 피해는 BattleManager에서 ID 2126으로 처리.
    // =========================================================

    private void CreatePowerStrike()
    {
        Register(
            new StatusEffectData
            {
                id = 2126,

                buffName =
                    "강타",

                description =
                    "다음 공격의 피해가 2배가 됩니다.",

                tendency =
                    StatusEffectTendency.Positive,

                effectType =
                    StatusEffectTypeMapper
                        .FromTableEffectType(27),

                effectPower = 0,

                buffDuration = 1,

                whenDecreaseDuration =
                    StatusEffectTiming.None,

                whenBuffEffect =
                    StatusEffectTiming.Continuous,

                canStack = true,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            }
        );
    }


    // =========================================================
    // 2127 - 견디기
    //
    // 기존 무기 스킬용 특수 상태.
    // 받는 피해 50% 감소 / 전체 턴 종료 시 감소.
    // =========================================================

    private void CreateEndure()
    {
        Register(
            new StatusEffectData
            {
                id = 2127,

                buffName =
                    "견디기",

                description =
                    "받는 피해가 50% 감소합니다.",

                tendency =
                    StatusEffectTendency.Positive,

                effectType =
                    StatusEffectTypeMapper
                        .FromTableEffectType(28),

                effectPower = 50,

                buffDuration = 1,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.Continuous,

                canStack = true,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            }
        );
    }


    // =========================================================
    // 외부 데이터 등록용
    //
    // 이후 CSV/DataLoader를 붙일 때 사용할 수 있다.
    // =========================================================

    public bool RegisterFromTable(
        int statusId,
        string statusName,
        string description,
        StatusEffectTendency tendency,
        int tableEffectType,
        int effectPower,
        int duration,
        int decreaseTimingValue,
        int buffEffectTimingValue,
        bool canStack,
        int removeTypeValue)
    {
        if (!StatusEffectTypeMapper.IsSupported(
                tableEffectType))
        {
            return false;
        }

        StatusEffectType mappedType =
            StatusEffectTypeMapper
                .FromTableEffectType(
                    tableEffectType
                );

        if (mappedType ==
            StatusEffectType.None)
        {
            return false;
        }

        StatusEffectData data =
            new StatusEffectData
            {
                id = statusId,

                buffName =
                    string.IsNullOrWhiteSpace(
                        statusName)
                        ? "상태이상 " + statusId
                        : statusName,

                description =
                    description ?? string.Empty,

                tendency =
                    tendency,

                effectType =
                    mappedType,

                effectPower =
                    effectPower,

                buffDuration =
                    Mathf.Max(0, duration),

                whenDecreaseDuration =
                    StatusEffectTypeMapper
                        .FromTableDecreaseTiming(
                            decreaseTimingValue
                        ),

                whenBuffEffect =
                    StatusEffectTypeMapper
                        .FromTableBuffEffectTiming(
                            buffEffectTimingValue
                        ),

                canStack =
                    canStack,

                whenRemove =
                    StatusEffectTypeMapper
                        .FromTableRemoveType(
                            removeTypeValue
                        )
            };

        return Register(data);
    }


    // =========================================================
    // 등록
    // =========================================================

    private bool Register(
        StatusEffectData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "null 상태이상은 등록할 수 없습니다."
            );

            return false;
        }

        if (data.id <= 0)
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "잘못된 상태이상 ID: " +
                data.id
            );

            return false;
        }

        if (data.effectType ==
            StatusEffectType.None)
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "EffectType이 None인 상태이상은 " +
                "등록할 수 없습니다." +
                " / ID: " +
                data.id
            );

            return false;
        }

        if (database.ContainsKey(
                data.id))
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "중복 상태이상 ID: " +
                data.id
            );

            return false;
        }

        database.Add(
            data.id,
            data
        );

        return true;
    }


    // =========================================================
    // 조회
    // =========================================================

    public bool HasStatusEffect(
        int statusId)
    {
        return database.ContainsKey(
            statusId
        );
    }


    public StatusEffectData GetStatusEffect(
        int statusId)
    {
        if (!database.TryGetValue(
                statusId,
                out StatusEffectData data))
        {
            Debug.LogWarning(
                "[StatusEffectDatabase] " +
                "등록되지 않은 상태이상 ID: " +
                statusId
            );

            return null;
        }

        return data.Clone();
    }


    public int GetRegisteredCount()
    {
        return database.Count;
    }
}
