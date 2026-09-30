using System;
using UnityEngine;

[Serializable]
public class EnemyPatternData
{
    [Header("기본 정보")]
    [Tooltip("이 패턴을 사용하는 몬스터 ID")]
    public int enemyId;

    [Tooltip("패턴 고유 ID")]
    public int patternId;

    [Tooltip("인게임에 표시할 패턴 이름")]
    public string patternName;

    [Header("패턴 선택")]
    [Tooltip("일반 패턴 선택 확률")]
    [Range(0, 100)]
    public int patternPossibility;

    [Tooltip(
        "0 = 일반 랜덤 패턴\n" +
        "1 = 이전 패턴 연계\n" +
        "2 = HP 조건 (현재 보류)"
    )]
    public int condition;

    [Tooltip("condition 2에서 사용할 HP 조건. 현재는 사용하지 않음")]
    public int hp;

    [Tooltip("연계 조건이 되는 이전 Pattern ID")]
    public int beforePatternId;

    [Header("효과")]
    [Tooltip("패턴 효과 종류")]
    public int effectType;

    [Tooltip("패턴 효과 수치")]
    public float effectPower;

    [Tooltip("적용할 상태이상 ID. 없으면 0")]
    public int statusId;

    [Tooltip("같은 패턴 안에서 효과 실행 순서")]
    public int order;

    [Tooltip("효과 대상")]
    public int target;

    public EnemyPatternData()
    {
    }

    public EnemyPatternData(
        int enemyId,
        int patternId,
        string patternName,
        int patternPossibility,
        int condition,
        int hp,
        int beforePatternId,
        int effectType,
        float effectPower,
        int statusId,
        int order,
        int target)
    {
        this.enemyId = enemyId;
        this.patternId = patternId;
        this.patternName = patternName;
        this.patternPossibility = patternPossibility;
        this.condition = condition;
        this.hp = hp;
        this.beforePatternId = beforePatternId;
        this.effectType = effectType;
        this.effectPower = effectPower;
        this.statusId = statusId;
        this.order = order;
        this.target = target;
    }

    // 일반 랜덤 패턴인지
    public bool IsNormalPattern
    {
        get
        {
            return condition == 0;
        }
    }

    // 이전 패턴과 이어지는 연계 패턴인지
    public bool IsChainPattern
    {
        get
        {
            return condition == 1;
        }
    }

    // HP 조건 패턴인지
    // 데이터 보존용이며 현재 시스템에서는 사용하지 않는다.
    public bool IsHpConditionPattern
    {
        get
        {
            return condition == 2;
        }
    }

    // 상태이상 효과가 연결되어 있는지
    public bool HasStatusEffect
    {
        get
        {
            return statusId > 0;
        }
    }
}