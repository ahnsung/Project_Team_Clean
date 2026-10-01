using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    [Header("Runtime Status Effects")]
    [SerializeField]
    private List<ActiveStatusEffect> activeEffects =
        new List<ActiveStatusEffect>();

    public event Action OnStatusEffectsChanged;

    public IReadOnlyList<ActiveStatusEffect> ActiveEffects
        => activeEffects;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (activeEffects == null)
        {
            activeEffects =
                new List<ActiveStatusEffect>();
        }

        RemoveInvalidEffects();
    }


    // =========================================================
    // 상태이상 추가
    // =========================================================

    public bool AddStatusEffect(
        StatusEffectData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                "[StatusEffectController] " +
                "추가하려는 StatusEffectData가 null입니다."
            );

            return false;
        }

        if (data.effectType ==
            StatusEffectType.None)
        {
            Debug.LogWarning(
                "[StatusEffectController] " +
                "EffectType이 None인 상태이상은 추가하지 않습니다."
            );

            return false;
        }

        ActiveStatusEffect sameEffect =
            FindSameEffect(data);


        // =====================================================
        // 동일 ID 상태이상은 항상 하나만 유지
        //
        // canStack 값과 관계없이 같은 ID가 이미 존재하면
        // 새 인스턴스를 만들지 않고 지속시간만 갱신한다.
        //
        // 예:
        // 기존 2턴 + 새 3턴 -> 3턴
        // 기존 3턴 + 새 2턴 -> 3턴
        // =====================================================

        // =====================================================
        // 기존 동일 ID가 있으면 갱신
        //
        // 하나만 유지하고 더 긴 지속시간으로 갱신
        // =====================================================

        if (sameEffect != null)
        {
            if (!sameEffect.IsInfinite &&
                data.buffDuration > 0)
            {
                int refreshedDuration =
                    Mathf.Max(
                        sameEffect.RemainingDuration,
                        data.buffDuration
                    );

                sameEffect.SetRemainingDuration(
                    refreshedDuration
                );
            }

            Debug.Log(
                "[StatusEffectController] " +
                "동일 ID 상태이상 갱신: " +
                data.buffName +
                " / 남은 지속시간: " +
                (
                    sameEffect.IsInfinite
                        ? "무한"
                        : sameEffect
                            .RemainingDuration
                            .ToString()
                )
            );

            NotifyChanged();

            return true;
        }


        // =====================================================
        // 최초 추가
        // =====================================================

        ActiveStatusEffect newEffect =
            new ActiveStatusEffect(data);

        activeEffects.Add(
            newEffect
        );

        Debug.Log(
            "[StatusEffectController] 상태이상 추가: " +
            data.buffName +
            " / 지속시간: " +
            (
                newEffect.IsInfinite
                    ? "무한"
                    : newEffect
                        .RemainingDuration
                        .ToString()
            )
        );

        NotifyChanged();

        return true;
    }


    // =========================================================
    // 상태이상 조회
    // =========================================================

    public bool HasStatusEffect(
        StatusEffectType type)
    {
        return GetStatusEffect(type) != null;
    }


    public ActiveStatusEffect GetStatusEffect(
        StatusEffectType type)
    {
        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.effectType ==
                type)
            {
                return effect;
            }
        }

        return null;
    }


    public List<ActiveStatusEffect>
        GetStatusEffects(
            StatusEffectType type)
    {
        List<ActiveStatusEffect> result =
            new List<ActiveStatusEffect>();

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.effectType ==
                type)
            {
                result.Add(
                    effect
                );
            }
        }

        return result;
    }


    public bool HasStatusEffectById(
        int id)
    {
        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.id == id)
            {
                return true;
            }
        }

        return false;
    }


    // =========================================================
    // ID 기준 상태이상 스택 조회
    // =========================================================

    public int GetStatusEffectCountById(
        int id)
    {
        int count = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.id == id)
            {
                count++;
            }
        }

        return count;
    }


    // =========================================================
    // 공격 후 소비되는 상태이상
    //
    // 2125 - 확정 명중
    // 2126 - 강타
    //
    // 두 상태 모두:
    // - 지속시간 1
    // - 중첩 가능
    // - 공격이 끝난 뒤 가장 먼저 들어온 스택 하나만 1 감소
    // - 0이 되면 해당 스택만 제거
    //
    // StatusEffectTiming에는 "공격 후"가 없으므로
    // BattleManager가 실제 공격 종료 시 이 함수를 호출한다.
    // =========================================================

    public bool ConsumeAttackTriggeredStatusById(
        int statusId)
    {
        ActiveStatusEffect targetEffect = null;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.id == statusId)
            {
                targetEffect = effect;
                break;
            }
        }

        if (targetEffect == null)
        {
            return false;
        }

        if (targetEffect.IsInfinite)
        {
            Debug.LogWarning(
                "[StatusEffectController] " +
                "공격 후 소비하려는 상태이상이 무한 지속입니다." +
                " / ID: " +
                statusId
            );

            return false;
        }

        int before =
            targetEffect.RemainingDuration;

        targetEffect.DecreaseDuration();

        Debug.Log(
            "[StatusEffectController] 공격 후 상태이상 감소: " +
            targetEffect.Data.buffName +
            " / ID: " +
            statusId +
            " / " +
            before +
            " -> " +
            targetEffect.RemainingDuration
        );

        if (targetEffect.IsExpired)
        {
            activeEffects.Remove(
                targetEffect
            );

            Debug.Log(
                "[StatusEffectController] 공격 후 상태이상 제거: " +
                targetEffect.Data.buffName +
                " / ID: " +
                statusId
            );
        }

        NotifyChanged();

        return true;
    }


    public void ConsumeWeaponAttackStatuses()
    {
        ConsumeAttackTriggeredStatusById(
            2125
        );

        ConsumeAttackTriggeredStatusById(
            2126
        );
    }


    // =========================================================
    // 공격력
    // =========================================================

    public float GetAttackPowerMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            // 2013 - 배고픔 25%: 공격 시 데미지 50% 감소
            if (effect.Data.id == 2013)
            {
                multiplier *= 0.5f;
                continue;
            }

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.AttackPowerUp:

                    multiplier +=
                        power / 100f;

                    break;


                case StatusEffectType.AttackPowerDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 방어력
    // =========================================================

    public float GetDefenseMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.DefenseUp:

                    multiplier +=
                        power / 100f;

                    break;


                case StatusEffectType.DefenseDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 명중
    // =========================================================

    public int GetAccuracyBonus()
    {
        int result = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.AccuracyUp:

                    result +=
                        effect.Data.effectPower;

                    break;


                case StatusEffectType.AccuracyDown:

                    result -=
                        effect.Data.effectPower;

                    break;
            }
        }

        // 2014 - 정신력 75%
        // 잃은 정신력 10%당 명중률 5 감소
        if (HasStatusEffectById(2014) &&
            PlayerResourceManager.Instance != null &&
            PlayerResourceManager.Instance.MaxMental > 0)
        {
            float mentalRatio =
                Mathf.Clamp01(
                    (float)PlayerResourceManager.Instance.CurrentMental /
                    PlayerResourceManager.Instance.MaxMental
                );

            float lostRatio = 1f - mentalRatio;

            int lostTenPercentSteps =
                Mathf.FloorToInt(
                    (lostRatio + 0.0001f) / 0.10f
                );

            result -= lostTenPercentSteps * 5;
        }

        return result;
    }


    // =========================================================
    // 회피
    // =========================================================

    public int GetEvasionBonus()
    {
        int result = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.EvasionUp:

                    result +=
                        effect.Data.effectPower;

                    break;


                case StatusEffectType.EvasionDown:

                    result -=
                        effect.Data.effectPower;

                    break;
            }
        }

        return result;
    }


    // =========================================================
    // 받는 피해
    // =========================================================

    public float GetDamageTakenMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.DamageTakenUp:

                    multiplier +=
                        power / 100f;

                    break;


                case StatusEffectType.DamageTakenDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        /*
         * Guard는 BattleManager에서 별도로 처리 중.
         * 여기서 Guard까지 적용하면 피해 감소가
         * 두 번 들어가므로 적용하지 않는다.
         */

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 범용 회복량
    // =========================================================

    public float GetHealingMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            int power =
                effect.Data.effectPower;

            switch (
                effect.Data.effectType)
            {
                case StatusEffectType.HealingUp:

                    multiplier +=
                        power / 100f;

                    break;


                case StatusEffectType.HealingDown:

                    multiplier -=
                        power / 100f;

                    break;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 체력 회복량
    // 부상
    // =========================================================

    public float GetHealthHealingMultiplier()
    {
        float multiplier =
            GetHealingMultiplier();

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.id == 2011)
            {
                // 배고픔 70%: 체력 회복량 50% 감소
                multiplier *= 0.5f;
                continue;
            }

            if (effect.Data.id == 2002 &&
                effect.Data.effectPower == 0)
            {
                // 원본 상태이상 테이블의 부상(2002)은
                // effect_power가 0이지만 설명상 체력 회복량 50% 감소.
                multiplier *= 0.5f;
                continue;
            }

            if (
                effect.Data.effectType ==
                StatusEffectType.HealthHealingDown)
            {
                multiplier -=
                    effect.Data.effectPower /
                    100f;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 배고픔 회복량
    // 피로
    // =========================================================

    public float GetHungerHealingMultiplier()
    {
        float multiplier =
            GetHealingMultiplier();

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.id == 2003 &&
                effect.Data.effectPower == 0)
            {
                // 원본 상태이상 테이블의 피로(2003)는
                // effect_power가 0이지만 설명상 배고픔 회복량 50% 감소.
                multiplier *= 0.5f;
                continue;
            }

            if (
                effect.Data.effectType ==
                StatusEffectType.HungerHealingDown)
            {
                multiplier -=
                    effect.Data.effectPower /
                    100f;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 정신력 회복량
    // 좌절
    // =========================================================

    public float GetMentalHealingMultiplier()
    {
        float multiplier =
            GetHealingMultiplier();

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (effect.Data.id == 2012)
            {
                // 배고픔 50%: 정신력 회복량 50% 감소
                multiplier *= 0.5f;
                continue;
            }

            if (effect.Data.id == 2004 &&
                effect.Data.effectPower == 0)
            {
                // 원본 상태이상 테이블의 좌절(2004)은
                // effect_power가 0이지만 설명상 정신력 회복량 50% 감소.
                multiplier *= 0.5f;
                continue;
            }

            if (
                effect.Data.effectType ==
                StatusEffectType.MentalHealingDown)
            {
                multiplier -=
                    effect.Data.effectPower /
                    100f;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // 행동 가능 여부
    // =========================================================

    public bool CanAct()
    {
        return !HasStatusEffect(
            StatusEffectType.Stun
        );
    }


    public bool CanAttack()
    {
        return CanAct();
    }


    // =========================================================
    // 침묵
    // =========================================================

    public bool CanUseSkill()
    {
        return
            CanAct() &&
            !HasStatusEffect(
                StatusEffectType.Silence
            );
    }


    // =========================================================
    // 혼란
    // =========================================================

    public bool CanUseItem()
    {
        return
            CanAct() &&
            !HasStatusEffect(
                StatusEffectType.Confusion
            );
    }


    // =========================================================
    // 부식
    // =========================================================

    public float GetDurabilityCostMultiplier()
    {
        float multiplier = 1f;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (
                effect.Data.effectType ==
                StatusEffectType.Corrosion)
            {
                if (effect.Data.id == 2009 &&
                    effect.Data.effectPower == 0)
                {
                    // 원본 상태이상 테이블의 부식(2009)은
                    // effect_power가 0이지만 설명상 내구도 감소량 2배.
                    multiplier *= 2f;
                    continue;
                }

                multiplier +=
                    effect.Data.effectPower /
                    100f;
            }
        }

        return Mathf.Max(
            0f,
            multiplier
        );
    }


    // =========================================================
    // STR / DEX / CON / INT
    // =========================================================

    public int GetStrengthBonus()
    {
        return GetStatBonus(
            StatusEffectType.StrengthUp,
            StatusEffectType.StrengthDown
        );
    }


    public int GetDexterityBonus()
    {
        return GetStatBonus(
            StatusEffectType.DexterityUp,
            StatusEffectType.DexterityDown
        );
    }


    public int GetConstitutionBonus()
    {
        return GetStatBonus(
            StatusEffectType.ConstitutionUp,
            StatusEffectType.ConstitutionDown
        );
    }


    public int GetIntelligenceBonus()
    {
        return GetStatBonus(
            StatusEffectType.IntelligenceUp,
            StatusEffectType.IntelligenceDown
        );
    }


    private int GetStatBonus(
        StatusEffectType upType,
        StatusEffectType downType)
    {
        int result = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (!IsValidEffect(effect))
                continue;

            if (
                effect.Data.effectType ==
                upType)
            {
                /*
                 * 증가 데이터:
                 * +1 같은 양수값
                 */
                result +=
                    effect.Data.effectPower;
            }
            else if (
                effect.Data.effectType ==
                downType)
            {
                /*
                 * 감소 데이터:
                 * 이미 -1 같은 음수값.
                 *
                 * 따라서 다시 빼면 안 된다.
                 */
                result +=
                    effect.Data.effectPower;
            }
        }

        return result;
    }


    // =========================================================
    // 2015 - 정신력 50%
    // 이벤트 성공률 25% 감소
    // 이벤트 성공률 계산부에서 이 값을 곱해서 사용한다.
    // =========================================================

    public float GetEventSuccessRateMultiplier()
    {
        return HasStatusEffectById(2015)
            ? 0.75f
            : 1f;
    }


    // =========================================================
    // 2016 - 정신력 25%
    // 모든 자원의 감소량 50% 증가
    // PlayerResourceManager의 '감소' 계산에서 사용한다.
    // =========================================================

    public float GetResourceDecreaseMultiplier()
    {
        return HasStatusEffectById(2016)
            ? 1.5f
            : 1f;
    }


    // =========================================================
    // 배고픔 / 정신력 조건 상태 즉시 재검사
    // 자원 수치가 바뀐 직후 외부에서 호출 가능.
    // =========================================================

    public void RefreshResourceConditionEffects()
    {
        RemoveBySpecialConditions();
        NotifyChanged();
    }


    // =========================================================
    // 특정 타이밍 처리
    // =========================================================

    public void ProcessTiming(
        StatusEffectTiming timing)
    {
        if (activeEffects == null ||
            activeEffects.Count == 0)
        {
            return;
        }

        List<ActiveStatusEffect> snapshot =
            new List<ActiveStatusEffect>(
                activeEffects
            );

        foreach (
            ActiveStatusEffect effect
            in snapshot)
        {
            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }


            // =================================================
            // 효과 발동
            // =================================================

            if (
                effect.Data.whenBuffEffect ==
                timing)
            {
                TriggerStatusEffect(
                    effect,
                    timing
                );
            }


            /*
             * 효과 발동으로 제거되었을 수 있음.
             */
            if (!activeEffects.Contains(
                    effect))
            {
                continue;
            }


            // =================================================
            // 지속시간 감소
            // =================================================

            if (
                effect.Data.whenDecreaseDuration ==
                timing)
            {
                DecreaseDuration(
                    effect
                );
            }
        }

        RemoveExpiredEffects();
        RemoveBySpecialConditions();

        NotifyChanged();
    }


    // =========================================================
    // 상태이상 효과 발동
    // =========================================================

    private void TriggerStatusEffect(
        ActiveStatusEffect effect,
        StatusEffectTiming timing)
    {
        if (effect == null ||
            effect.Data == null)
        {
            return;
        }

        StatusEffectData data =
            effect.Data;

        switch (
            data.effectType)
        {
            // =================================================
            // 즉발형
            // =================================================

            case StatusEffectType.Stun:

                /*
                 * 실제 행동 차단은
                 * CanAct() / BattleManager 쪽에서 처리.
                 */
                break;


            case StatusEffectType.Poison:

                ApplyPoisonDamage(
                    data.effectPower
                );

                break;


            // =================================================
            // 기존 호환용
            // =================================================

            case StatusEffectType.StatIncrease:

                break;


            case StatusEffectType.Guard:

                /*
                 * Guard 피해 감소와
                 * 내구도 처리는 BattleManager 담당.
                 */
                break;


            // =================================================
            // 패시브 조회형
            //
            // 여기서 즉시 뭔가 하지 않고
            // 전투/자원 계산 시 GetXXX()로 조회한다.
            // =================================================

            case StatusEffectType.AttackPowerUp:
            case StatusEffectType.AttackPowerDown:

            case StatusEffectType.DefenseUp:
            case StatusEffectType.DefenseDown:

            case StatusEffectType.AccuracyUp:
            case StatusEffectType.AccuracyDown:

            case StatusEffectType.EvasionUp:
            case StatusEffectType.EvasionDown:

            case StatusEffectType.DamageTakenUp:
            case StatusEffectType.DamageTakenDown:

            case StatusEffectType.HealingUp:
            case StatusEffectType.HealingDown:

            case StatusEffectType.StrengthUp:
            case StatusEffectType.StrengthDown:

            case StatusEffectType.DexterityUp:
            case StatusEffectType.DexterityDown:

            case StatusEffectType.ConstitutionUp:
            case StatusEffectType.ConstitutionDown:

            case StatusEffectType.IntelligenceUp:
            case StatusEffectType.IntelligenceDown:

            case StatusEffectType.HealthHealingDown:
            case StatusEffectType.HungerHealingDown:
            case StatusEffectType.MentalHealingDown:

            case StatusEffectType.Silence:
            case StatusEffectType.Confusion:
            case StatusEffectType.Corrosion:

            case StatusEffectType.BattleEncounterRateUp:
            case StatusEffectType.ItemAcquisitionUp:
                break;
        }


        // =====================================================
        // 효과 발동 후 제거
        // =====================================================

        if (
            data.whenRemove ==
            StatusEffectRemoveType.EffectTriggered)
        {
            RemoveStatusEffect(
                effect
            );
        }
    }


    // =========================================================
    // 중독
    //
    // effectPower = 최대 체력 대비 %
    //
    // 예:
    // 최대 HP 100 / power 10
    // -> 10 피해
    //
    // 최대 HP 150 / power 10
    // -> 15 피해
    // =========================================================

    private void ApplyPoisonDamage(
        int percent)
    {
        percent =
            Mathf.Max(
                0,
                percent
            );

        if (percent <= 0)
            return;


        BattleUnit unit =
            GetComponent<BattleUnit>();

        bool isPlayer =
            IsPlayerController(
                unit
            );


        // =====================================================
        // 플레이어
        // =====================================================

        if (isPlayer)
        {
            int maxHealth = 1;

            if (
                PlayerResourceManager.Instance !=
                null)
            {
                maxHealth =
                    Mathf.Max(
                        1,
                        PlayerResourceManager.Instance
                            .MaxHealth
                    );
            }


            int damage =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        maxHealth *
                        (percent / 100f)
                    )
                );


            /*
             * BattleUnit의 전투 HP와
             * PlayerResourceManager의 던전 자원을
             * 기존 BattleManager와 동일하게
             * 함께 갱신한다.
             */

            if (
                unit != null &&
                !unit.IsDead)
            {
                unit.TakeDamage(
                    damage
                );
            }


            if (
                PlayerResourceManager.Instance !=
                null)
            {
                PlayerResourceManager.Instance
                    .ChangeHealth(
                        -damage,
                        "중독 피해"
                    );
            }


            Debug.Log(
                "[StatusEffectController] " +
                "플레이어 중독 피해: " +
                damage +
                " / 최대 체력: " +
                maxHealth +
                " / 비율: " +
                percent +
                "%"
            );

            return;
        }


        // =====================================================
        // 적
        //
        // 현재 BattleUnit에서 최대 HP를
        // StatusEffectController가 안전하게 가져오는
        // 공개 API가 확인되지 않았기 때문에,
        // 적에게 사용되는 경우에는 기존 수치 피해 방식 유지.
        //
        // 현재 함정 중독은 플레이어에게 적용되므로
        // 플레이어 함정 기능에는 영향 없음.
        // =====================================================

        int enemyDamage =
            Mathf.Max(
                1,
                percent
            );


        if (
            unit != null &&
            !unit.IsDead)
        {
            unit.TakeDamage(
                enemyDamage
            );

            Debug.Log(
                "[StatusEffectController] " +
                unit.unitName +
                " 중독 피해: " +
                enemyDamage
            );
        }
    }


    // =========================================================
    // 플레이어 판정
    // =========================================================

    private bool IsPlayerController(
        BattleUnit unit)
    {
        /*
         * 가장 확실한 판정:
         * BattleManager의 playerUnit
         */

        if (
            BattleManager.Instance != null &&
            BattleManager.Instance.playerUnit != null &&
            unit ==
            BattleManager.Instance.playerUnit)
        {
            return true;
        }


        /*
         * 비전투 상태에서도 PlayerStats가 같은
         * GameObject에 붙어 있으면 플레이어로 판단.
         */

        PlayerStats stats =
            GetComponent<PlayerStats>();

        if (
            stats != null &&
            PlayerStats.Instance ==
            stats)
        {
            return true;
        }


        return false;
    }


    // =========================================================
    // 지속시간 감소
    // =========================================================

    private void DecreaseDuration(
        ActiveStatusEffect effect)
    {
        if (
            effect == null ||
            effect.Data == null ||
            effect.IsInfinite)
        {
            return;
        }

        int before =
            effect.RemainingDuration;

        effect.DecreaseDuration();

        Debug.Log(
            "[StatusEffectController] 지속시간 감소: " +
            effect.Data.buffName +
            " / " +
            before +
            " -> " +
            effect.RemainingDuration
        );
    }


    // =========================================================
    // 개별 제거
    // =========================================================

    public bool RemoveStatusEffect(
        ActiveStatusEffect effect)
    {
        if (effect == null)
            return false;

        bool removed =
            activeEffects.Remove(
                effect
            );

        if (!removed)
            return false;

        if (effect.Data != null)
        {
            Debug.Log(
                "[StatusEffectController] 상태이상 제거: " +
                effect.Data.buffName
            );
        }

        NotifyChanged();

        return true;
    }


    // =========================================================
    // 타입 전체 제거
    // =========================================================

    public int RemoveStatusEffect(
        StatusEffectType type)
    {
        int removedCount =
            activeEffects.RemoveAll(
                effect =>
                    effect != null &&
                    effect.Data != null &&
                    effect.Data.effectType ==
                    type
            );

        if (removedCount > 0)
        {
            Debug.Log(
                "[StatusEffectController] 상태이상 제거: " +
                type +
                " / " +
                removedCount +
                "개"
            );

            NotifyChanged();
        }

        return removedCount;
    }


    // =========================================================
    // 전체 제거
    // =========================================================

    public void ClearAllStatusEffects()
    {
        if (
            activeEffects == null ||
            activeEffects.Count == 0)
        {
            return;
        }

        activeEffects.Clear();

        Debug.Log(
            "[StatusEffectController] " +
            "모든 상태이상을 제거했습니다."
        );

        NotifyChanged();
    }


    // =========================================================
    // 지속시간 종료 제거
    // =========================================================

    private void RemoveExpiredEffects()
    {
        int removedCount =
            activeEffects.RemoveAll(
                effect =>
                    effect == null ||
                    effect.Data == null ||
                    (
                        effect.Data.whenRemove ==
                        StatusEffectRemoveType.DurationEnded &&
                        effect.IsExpired
                    )
            );

        if (removedCount > 0)
        {
            Debug.Log(
                "[StatusEffectController] " +
                "지속시간이 끝난 상태이상 " +
                removedCount +
                "개 제거"
            );
        }
    }


    // =========================================================
    // 배고픔 / 정신력 조건 제거
    // =========================================================

    private void RemoveBySpecialConditions()
    {
        BattleUnit unit =
            GetComponent<BattleUnit>();

        bool isPlayer =
            IsPlayerController(
                unit
            );

        if (
            !isPlayer ||
            PlayerResourceManager.Instance ==
            null)
        {
            return;
        }


        float hungerRatio = 0f;
        float mentalRatio = 0f;


        if (
            PlayerResourceManager.Instance
                .MaxHunger > 0)
        {
            hungerRatio =
                (float)
                PlayerResourceManager.Instance
                    .CurrentHunger /
                PlayerResourceManager.Instance
                    .MaxHunger;
        }


        if (
            PlayerResourceManager.Instance
                .MaxMental > 0)
        {
            mentalRatio =
                (float)
                PlayerResourceManager.Instance
                    .CurrentMental /
                PlayerResourceManager.Instance
                    .MaxMental;
        }


        activeEffects.RemoveAll(
            effect =>
            {
                if (
                    effect == null ||
                    effect.Data == null)
                {
                    return true;
                }

                switch (
                    effect.Data.whenRemove)
                {
                    case StatusEffectRemoveType.HungerAbove70:

                        return
                            hungerRatio >
                            0.70f;


                    case StatusEffectRemoveType.HungerAbove50:

                        return
                            hungerRatio >
                            0.50f;


                    case StatusEffectRemoveType.HungerAbove25:

                        return
                            hungerRatio >
                            0.25f;


                    case StatusEffectRemoveType.MentalAbove75:

                        return
                            mentalRatio >
                            0.75f;


                    case StatusEffectRemoveType.MentalAbove50:

                        return
                            mentalRatio >
                            0.50f;


                    case StatusEffectRemoveType.MentalAbove25:

                        return
                            mentalRatio >
                            0.25f;
                }

                return false;
            }
        );
    }


    // =========================================================
    // 동일 ID 개수
    // =========================================================

    private int GetSameEffectCount(
        int id)
    {
        int count = 0;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (
                effect == null ||
                effect.Data == null)
            {
                continue;
            }

            if (
                effect.Data.id ==
                id)
            {
                count++;
            }
        }

        return count;
    }


    // =========================================================
    // 같은 효과 검색
    // =========================================================

    private ActiveStatusEffect FindSameEffect(
        StatusEffectData data)
    {
        if (data == null)
            return null;

        foreach (
            ActiveStatusEffect effect
            in activeEffects)
        {
            if (
                effect == null ||
                effect.Data == null)
            {
                continue;
            }

            /*
             * ID 기준.
             *
             * 같은 이름이라도 ID가 다르면
             * 서로 다른 상태이상으로 취급.
             */
            if (
                effect.Data.id ==
                data.id)
            {
                return effect;
            }
        }

        return null;
    }


    // =========================================================
    // 유효성 검사
    // =========================================================

    private bool IsValidEffect(
        ActiveStatusEffect effect)
    {
        return
            effect != null &&
            effect.Data != null &&
            !effect.IsExpired;
    }


    private void RemoveInvalidEffects()
    {
        if (activeEffects == null)
        {
            activeEffects =
                new List<ActiveStatusEffect>();

            return;
        }

        activeEffects.RemoveAll(
            effect =>
                effect == null ||
                effect.Data == null
        );
    }


    // =========================================================
    // UI 갱신 알림
    // =========================================================

    private void NotifyChanged()
    {
        OnStatusEffectsChanged?.Invoke();
    }


    // =========================================================
    // DEBUG - 원본 상태이상 테이블 2011 ~ 2016
    //
    // Unity:
    // StatusEffectController 컴포넌트 우클릭
    // -> DEBUG STATUS/...
    //
    // StatusEffectDatabase에 등록된 실제 데이터를 가져와
    // 적용하므로 테스트용 복제 데이터를 만들지 않는다.
    // =========================================================

    private void DebugAddDatabaseStatusEffect(
        int statusId)
    {
        if (StatusEffectDatabase.Instance == null)
        {
            Debug.LogError(
                "[DEBUG STATUS] StatusEffectDatabase.Instance가 null입니다."
            );
            return;
        }

        StatusEffectData data =
            StatusEffectDatabase.Instance.GetStatusEffect(
                statusId
            );

        if (data == null)
        {
            Debug.LogError(
                "[DEBUG STATUS] 상태이상 데이터를 찾지 못했습니다." +
                " / ID: " +
                statusId
            );
            return;
        }

        bool added =
            AddStatusEffect(
                data
            );

        Debug.Log(
            "[DEBUG STATUS] " +
            data.buffName +
            " 적용 " +
            (added ? "성공" : "실패") +
            " / ID: " +
            statusId +
            " / EffectType: " +
            data.effectType +
            " / Power: " +
            data.effectPower +
            " / Duration: " +
            data.buffDuration +
            " / Remove: " +
            data.whenRemove
        );
    }


    [ContextMenu("DEBUG STATUS/2130 - 아이템 획득 증가")]
    private void DebugStatus2130ItemAcquisitionUp()
    {
        DebugAddDatabaseStatusEffect(
            2130
        );
    }


    [ContextMenu("DEBUG STATUS/24 - 전투 발생률 증가 (테스트용)")]
    private void DebugStatusBattleEncounterRateUp()
    {
        StatusEffectData debugData =
            new StatusEffectData
            {
                id = 990024,
                buffName = "전투 발생률 증가 DEBUG",
                description =
                    "effect_type 24 전투 발생률 증가 연결 확인용 테스트 상태이상",
                tendency =
                    StatusEffectTendency.Negative,
                effectType =
                    StatusEffectType.BattleEncounterRateUp,
                effectPower = 0,
                buffDuration = 0,
                whenDecreaseDuration =
                    StatusEffectTiming.None,
                whenBuffEffect =
                    StatusEffectTiming.Continuous,
                canStack = false,
                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        bool added =
            AddStatusEffect(
                debugData
            );

        Debug.Log(
            "[DEBUG STATUS 24] " +
            (added
                ? "전투 발생률 증가 상태 적용 완료"
                : "전투 발생률 증가 상태 적용 실패/이미 적용 중")
        );
    }


    [ContextMenu("DEBUG STATUS/24 - 전투 발생률 증가 제거")]
    private void DebugRemoveBattleEncounterRateUp()
    {
        int removedCount =
            activeEffects.RemoveAll(
                effect =>
                    effect != null &&
                    effect.Data != null &&
                    effect.Data.id == 990024
            );

        if (removedCount > 0)
        {
            NotifyChanged();
        }

        Debug.Log(
            "[DEBUG STATUS 24] " +
            (removedCount > 0
                ? "전투 발생률 증가 테스트 상태 제거 완료"
                : "제거할 전투 발생률 증가 테스트 상태가 없음")
        );
    }


    [ContextMenu("DEBUG STATUS/2010 - 방어")]
    private void DebugStatus2010()
    {
        DebugAddDatabaseStatusEffect(
            2010
        );
    }


    [ContextMenu("DEBUG STATUS/2011 - 배고픔 70%")]
    private void DebugStatus2011()
    {
        DebugAddDatabaseStatusEffect(
            2011
        );
    }


    [ContextMenu("DEBUG STATUS/2012 - 배고픔 50%")]
    private void DebugStatus2012()
    {
        DebugAddDatabaseStatusEffect(
            2012
        );
    }


    [ContextMenu("DEBUG STATUS/2013 - 배고픔 25%")]
    private void DebugStatus2013()
    {
        DebugAddDatabaseStatusEffect(
            2013
        );
    }


    [ContextMenu("DEBUG STATUS/2014 - 정신력 75%")]
    private void DebugStatus2014()
    {
        DebugAddDatabaseStatusEffect(
            2014
        );
    }


    [ContextMenu("DEBUG STATUS/2015 - 정신력 50%")]
    private void DebugStatus2015()
    {
        DebugAddDatabaseStatusEffect(
            2015
        );
    }


    [ContextMenu("DEBUG STATUS/2016 - 정신력 25%")]
    private void DebugStatus2016()
    {
        DebugAddDatabaseStatusEffect(
            2016
        );
    }


    [ContextMenu("DEBUG STATUS/2011~2016 전부 적용")]
    private void DebugStatus2011To2016()
    {
        for (int statusId = 2011;
             statusId <= 2016;
             statusId++)
        {
            DebugAddDatabaseStatusEffect(
                statusId
            );
        }
    }


    [ContextMenu("DEBUG STATUS/2011~2016 전부 제거")]
    private void DebugRemoveStatus2011To2016()
    {
        int removedCount = 0;

        for (int i = activeEffects.Count - 1;
             i >= 0;
             i--)
        {
            ActiveStatusEffect effect =
                activeEffects[i];

            if (effect == null ||
                effect.Data == null)
            {
                continue;
            }

            int id =
                effect.Data.id;

            if (id < 2011 ||
                id > 2016)
            {
                continue;
            }

            activeEffects.RemoveAt(
                i
            );

            removedCount++;
        }

        Debug.Log(
            "[DEBUG STATUS] 2011~2016 제거 완료" +
            " / 제거 개수: " +
            removedCount
        );

        NotifyChanged();
    }


    [ContextMenu("DEBUG STATUS/현재 2011~2016 상태 출력")]
    private void DebugPrintStatus2011To2016()
    {
        Debug.Log(
            "[DEBUG STATUS] ===== 2011~2016 현재 상태 ====="
        );

        for (int statusId = 2011;
             statusId <= 2016;
             statusId++)
        {
            int count =
                GetStatusEffectCountById(
                    statusId
                );

            Debug.Log(
                "[DEBUG STATUS] ID " +
                statusId +
                " / 적용 개수: " +
                count
            );
        }

        Debug.Log(
            "[DEBUG STATUS] 공격력 배율: " +
            GetAttackPowerMultiplier()
        );

        Debug.Log(
            "[DEBUG STATUS] 명중 보정: " +
            GetAccuracyBonus()
        );

        Debug.Log(
            "[DEBUG STATUS] 체력 회복 배율: " +
            GetHealthHealingMultiplier()
        );

        Debug.Log(
            "[DEBUG STATUS] 정신력 회복 배율: " +
            GetMentalHealingMultiplier()
        );

        Debug.Log(
            "[DEBUG STATUS] 이벤트 성공률 배율: " +
            GetEventSuccessRateMultiplier()
        );

        Debug.Log(
            "[DEBUG STATUS] 자원 감소 배율: " +
            GetResourceDecreaseMultiplier()
        );
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("DEBUG - 중독 3턴 / 최대 체력 10%")]
    private void DebugAddPoison()
    {
        StatusEffectData poison =
            new StatusEffectData
            {
                id = 9101,

                buffName =
                    "중독",

                description =
                    "턴 종료마다 최대 체력의 10% 피해를 받습니다.",

                tendency =
                    StatusEffectTendency.Negative,

                effectType =
                    StatusEffectType.Poison,

                effectPower =
                    10,

                buffDuration =
                    3,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.TurnEnd,

                /*
                 * 실제 함정 중독과 동일하게
                 * 중첩 불가.
                 */
                canStack =
                    false,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        AddStatusEffect(
            poison
        );
    }


    [ContextMenu("DEBUG - 공격력 +25% / 3턴")]
    private void DebugAddAttackPowerUp()
    {
        StatusEffectData buff =
            new StatusEffectData
            {
                id = 9201,

                buffName =
                    "공격력 증가",

                description =
                    "공격력이 25% 증가합니다.",

                tendency =
                    StatusEffectTendency.Positive,

                effectType =
                    StatusEffectType.AttackPowerUp,

                effectPower =
                    25,

                buffDuration =
                    3,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.None,

                canStack =
                    false,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        AddStatusEffect(
            buff
        );

        Debug.Log(
            "[DEBUG] 현재 공격력 배율: " +
            GetAttackPowerMultiplier()
        );
    }


    [ContextMenu("DEBUG - 명중률 -15 / 2턴")]
    private void DebugAddAccuracyDown()
    {
        StatusEffectData debuff =
            new StatusEffectData
            {
                id = 9202,

                buffName =
                    "명중률 감소",

                description =
                    "명중률이 15 감소합니다.",

                tendency =
                    StatusEffectTendency.Negative,

                effectType =
                    StatusEffectType.AccuracyDown,

                effectPower =
                    15,

                buffDuration =
                    2,

                whenDecreaseDuration =
                    StatusEffectTiming.TurnEnd,

                whenBuffEffect =
                    StatusEffectTiming.None,

                canStack =
                    false,

                whenRemove =
                    StatusEffectRemoveType.DurationEnded
            };

        AddStatusEffect(
            debuff
        );

        Debug.Log(
            "[DEBUG] 현재 명중 보정: " +
            GetAccuracyBonus()
        );
    }


    // =========================================================
    // 함정 DEBUG 공통
    // =========================================================

    private void DebugApplyTrapStatus(
        int trapType,
        int duration)
    {
        StatusEffectData data =
            TrapStatusEffectFactory.Create(
                trapType,
                duration
            );

        if (data == null)
        {
            Debug.LogError(
                "[DEBUG] 함정 상태이상 생성 실패" +
                " / TrapType: " +
                trapType
            );

            return;
        }

        AddStatusEffect(
            data
        );

        Debug.Log(
            "[DEBUG] 함정 상태이상 적용 완료" +
            " / TrapType: " +
            trapType +
            " / 이름: " +
            data.buffName +
            " / 지속시간: " +
            duration
        );
    }


    // =========================================================
    // Trap 1 ~ 8
    // =========================================================

    [ContextMenu("DEBUG TRAP/01 - 부상")]
    private void DebugTrap01_Injury()
    {
        DebugApplyTrapStatus(
            1,
            3
        );
    }


    [ContextMenu("DEBUG TRAP/02 - 피로")]
    private void DebugTrap02_Fatigue()
    {
        DebugApplyTrapStatus(
            2,
            4
        );
    }


    [ContextMenu("DEBUG TRAP/03 - 좌절")]
    private void DebugTrap03_Frustration()
    {
        DebugApplyTrapStatus(
            3,
            5
        );
    }


    [ContextMenu("DEBUG TRAP/04 - 중독")]
    private void DebugTrap04_Poison()
    {
        DebugApplyTrapStatus(
            4,
            3
        );
    }


    [ContextMenu("DEBUG TRAP/05 - 취약")]
    private void DebugTrap05_Vulnerable()
    {
        DebugApplyTrapStatus(
            5,
            4
        );
    }


    [ContextMenu("DEBUG TRAP/06 - 침묵")]
    private void DebugTrap06_Silence()
    {
        DebugApplyTrapStatus(
            6,
            5
        );
    }


    [ContextMenu("DEBUG TRAP/07 - 혼란")]
    private void DebugTrap07_Confusion()
    {
        DebugApplyTrapStatus(
            7,
            3
        );
    }


    [ContextMenu("DEBUG TRAP/08 - 부식")]
    private void DebugTrap08_Corrosion()
    {
        DebugApplyTrapStatus(
            8,
            3
        );
    }


    // =========================================================
    // Trap 20 ~ 23
    // =========================================================

    [ContextMenu("DEBUG TRAP/20 - STR 감소")]
    private void DebugTrap20_StrengthDown()
    {
        DebugApplyTrapStatus(
            20,
            3
        );
    }


    [ContextMenu("DEBUG TRAP/21 - DEX 감소")]
    private void DebugTrap21_DexterityDown()
    {
        DebugApplyTrapStatus(
            21,
            5
        );
    }


    [ContextMenu("DEBUG TRAP/22 - INT 감소")]
    private void DebugTrap22_IntelligenceDown()
    {
        DebugApplyTrapStatus(
            22,
            3
        );
    }


    [ContextMenu("DEBUG TRAP/23 - CON 감소")]
    private void DebugTrap23_ConstitutionDown()
    {
        DebugApplyTrapStatus(
            23,
            3
        );
    }
}