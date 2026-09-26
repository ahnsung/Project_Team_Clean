public enum StatusEffectTiming
{
    // 기존 타이밍 값 - 숫자 변경 금지
    SelfTurnStart = 0,
    SelfTurnEnd = 1,
    PlayerTeamStart = 2,
    PlayerTeamEnd = 3,
    EnemyTeamStart = 4,
    EnemyTeamEnd = 5,
    TurnStart = 6,
    TurnEnd = 7,
    None = 8,

    // 상태이상 원본 테이블 when__buff_effect 전용
    // 기존 값 뒤에 추가하여 기존 코드 호환성을 유지한다.
    Continuous = 9,
    AttackHit = 10,
    AttackTaken = 11
}
