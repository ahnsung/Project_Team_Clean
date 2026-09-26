using System.Collections.Generic;
using UnityEngine;

public class WeaponSkillDatabase : MonoBehaviour
{
    public static WeaponSkillDatabase Instance;

    private readonly Dictionary<int, WeaponSkillData> database =
        new Dictionary<int, WeaponSkillData>();


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateSkills();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // 스킬 생성
    // =========================================================

    private void CreateSkills()
    {
        database.Clear();

        CreateGuaranteedHitSkill();
        CreateAllAttackSkill();
        CreateStunSkill();
        CreatePowerStrikeSkill();
        CreateEndureSkill();

        Debug.Log(
            "[WeaponSkillDatabase] 무기 스킬 등록 완료: " +
            database.Count +
            "개"
        );
    }


    // =========================================================
    // 4001 - 확정 명중
    // =========================================================

    private void CreateGuaranteedHitSkill()
    {
        WeaponSkillData skill =
            new WeaponSkillData
            {
                skillId = 4001,

                skillName =
                    "확정 명중",

                description =
                    "다음 공격이 반드시 명중합니다.",

                useDurability =
                    15
            };

        skill.effects.Add(
            new WeaponSkillEffectData
            {
                effectId = 5001,

                target =
                    WeaponSkillTarget.Self,

                effectType =
                    WeaponSkillEffectType.StatusEffect,

                effectValue =
                    0,

                statusId =
                    2125,

                order =
                    0
            }
        );

        RegisterSkill(skill);
    }


    // =========================================================
    // 4002 - 전체 공격
    // =========================================================

    private void CreateAllAttackSkill()
    {
        WeaponSkillData skill =
            new WeaponSkillData
            {
                skillId = 4002,

                skillName =
                    "전체 공격",

                description =
                    "모든 적에게 10의 피해를 줍니다.",

                useDurability =
                    15
            };

        skill.effects.Add(
            new WeaponSkillEffectData
            {
                effectId = 5002,

                target =
                    WeaponSkillTarget.Enemy,

                effectType =
                    WeaponSkillEffectType.Damage,

                effectValue =
                    10,

                statusId =
                    0,

                order =
                    0
            }
        );

        RegisterSkill(skill);
    }


    // =========================================================
    // 4003 - 기절
    // =========================================================

    private void CreateStunSkill()
    {
        WeaponSkillData skill =
            new WeaponSkillData
            {
                skillId = 4003,

                skillName =
                    "기절",

                description =
                    "선택한 적을 1턴 동안 기절시킵니다.",

                useDurability =
                    15
            };

        skill.effects.Add(
            new WeaponSkillEffectData
            {
                effectId = 5003,

                target =
                    WeaponSkillTarget.Enemy,

                effectType =
                    WeaponSkillEffectType.StatusEffect,

                effectValue =
                    0,

                statusId =
                    2001,

                order =
                    0
            }
        );

        RegisterSkill(skill);
    }


    // =========================================================
    // 4004 - 강타
    //
    // Order 0:
    // 플레이어에게 2126 적용
    //
    // Order 1:
    // 선택한 적 공격
    // =========================================================

    private void CreatePowerStrikeSkill()
    {
        WeaponSkillData skill =
            new WeaponSkillData
            {
                skillId = 4004,

                skillName =
                    "강타",

                description =
                    "선택한 적에게 일반 공격의 2배 피해를 줍니다.",

                useDurability =
                    15
            };

        skill.effects.Add(
            new WeaponSkillEffectData
            {
                effectId = 5004,

                target =
                    WeaponSkillTarget.Self,

                effectType =
                    WeaponSkillEffectType.StatusEffect,

                effectValue =
                    0,

                statusId =
                    2126,

                order =
                    0
            }
        );

        skill.effects.Add(
            new WeaponSkillEffectData
            {
                effectId = 5005,

                target =
                    WeaponSkillTarget.Enemy,

                effectType =
                    WeaponSkillEffectType.Damage,

                /*
                 * 기획 데이터의 effect_value는 1.
                 *
                 * 실제 "일반 공격 2배" 계산은
                 * Status 2126 효과와 함께
                 * BattleManager에서 처리한다.
                 */
                effectValue =
                    1,

                statusId =
                    0,

                order =
                    1
            }
        );

        RegisterSkill(skill);
    }


    // =========================================================
    // 4005 - 견디기
    // =========================================================

    private void CreateEndureSkill()
    {
        WeaponSkillData skill =
            new WeaponSkillData
            {
                skillId = 4005,

                skillName =
                    "견디기",

                description =
                    "받는 피해가 절반으로 감소합니다.",

                useDurability =
                    15
            };

        skill.effects.Add(
            new WeaponSkillEffectData
            {
                effectId = 5006,

                target =
                    WeaponSkillTarget.Self,

                effectType =
                    WeaponSkillEffectType.StatusEffect,

                effectValue =
                    0,

                statusId =
                    2127,

                order =
                    0
            }
        );

        RegisterSkill(skill);
    }


    // =========================================================
    // 등록
    // =========================================================

    private void RegisterSkill(
        WeaponSkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning(
                "[WeaponSkillDatabase] " +
                "null 스킬은 등록할 수 없습니다."
            );

            return;
        }

        if (skill.skillId <= 0)
        {
            Debug.LogWarning(
                "[WeaponSkillDatabase] " +
                "잘못된 Skill ID: " +
                skill.skillId
            );

            return;
        }

        if (database.ContainsKey(
                skill.skillId))
        {
            Debug.LogWarning(
                "[WeaponSkillDatabase] " +
                "중복 Skill ID: " +
                skill.skillId
            );

            return;
        }

        database.Add(
            skill.skillId,
            skill
        );
    }


    // =========================================================
    // 조회
    // =========================================================

    public WeaponSkillData GetSkill(
        int skillId)
    {
        if (database.TryGetValue(
                skillId,
                out WeaponSkillData skill))
        {
            return skill;
        }

        Debug.LogWarning(
            "[WeaponSkillDatabase] " +
            "등록되지 않은 무기 스킬 ID: " +
            skillId
        );

        return null;
    }


    public bool HasSkill(
        int skillId)
    {
        return database.ContainsKey(
            skillId
        );
    }


    // =========================================================
    // 현재 장착 주무기의 스킬
    // =========================================================

    public WeaponSkillData GetCurrentMainWeaponSkill()
    {
        if (EquipmentManager.Instance == null)
        {
            return null;
        }

        InventoryItem mainWeapon =
            EquipmentManager.Instance.MainWeapon;

        if (mainWeapon == null ||
            mainWeapon.data == null)
        {
            return null;
        }

        if (!mainWeapon.data.IsWeapon ||
            !mainWeapon.data.HasWeaponSkill)
        {
            return null;
        }

        return GetSkill(
            mainWeapon.data.weaponSkillId
        );
    }
}