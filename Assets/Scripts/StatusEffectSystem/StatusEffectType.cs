public enum StatusEffectType
{
    None = 0,

    Stun = 1,
    Poison = 2,
    StatIncrease = 3,

    Guard = 9,

    AttackPowerUp = 10,
    AttackPowerDown = 11,

    DefenseUp = 12,
    DefenseDown = 13,

    AccuracyUp = 14,
    AccuracyDown = 15,

    EvasionUp = 16,
    EvasionDown = 17,

    DamageTakenUp = 18,
    DamageTakenDown = 19,

    HealingUp = 20,
    HealingDown = 21,

    StrengthUp = 30,
    StrengthDown = 31,

    DexterityUp = 32,
    DexterityDown = 33,

    ConstitutionUp = 34,
    ConstitutionDown = 35,

    IntelligenceUp = 36,
    IntelligenceDown = 37,

    // 부상
    HealthHealingDown = 40,

    // 피로
    HungerHealingDown = 41,

    // 좌절
    MentalHealingDown = 42,

    // 침묵
    Silence = 43,

    // 혼란
    Confusion = 44,

    // 부식
    Corrosion = 45,

    // General 전투 발생률
    BattleEncounterRateUp = 46,

    // 아이템 획득량/확률
    ItemAcquisitionUp = 47
}