using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    // =========================================================
    // Basic
    // =========================================================

    [Header("Basic")]
    public int id;

    public string itemName;

    public ItemCategory category;


    // =========================================================
    // Shop
    // =========================================================

    [Header("Shop")]

    [Tooltip(
        "상점에서 이 아이템을 구매할 때 필요한 솔라스톤 가격입니다. " +
        "기획서의 아이템 구매 가격을 사용합니다."
    )]
    [Min(0)]
    public int buyPrice;

    [Tooltip(
        "플레이어가 이 아이템을 상점에 판매할 때 받는 솔라스톤 가격입니다. " +
        "0이면 상점에 판매할 수 없는 아이템입니다."
    )]
    [Min(0)]
    public int sellPrice;


    // =========================================================
    // Consumable
    // =========================================================

    [Header("Consumable")]

    [Min(0)]
    public int maxUseCount;

    public bool consumeTurnOnUse;

    public bool canDrop = true;

    [TextArea]
    public string effectDescription;

    public Sprite icon;


    // =========================================================
    // Item Effect
    // =========================================================

    [Header("Item Effect")]

    [Tooltip(
        "상태이상을 부여하는 아이템 효과가 참조할 " +
        "StatusEffectDatabase의 상태이상 ID입니다. " +
        "0이면 연결된 상태이상이 없습니다."
    )]
    [Min(0)]
    public int statusId;

    [Tooltip(
        "하나의 아이템에 여러 효과가 존재할 경우 " +
        "효과 실행 순서를 결정합니다. " +
        "낮은 숫자부터 먼저 실행됩니다."
    )]
    [Min(0)]
    public int order;


    // =========================================================
    // Inventory Shape
    // =========================================================

    [Header("Inventory Shape")]

    public List<Vector2Int> shape =
        new List<Vector2Int>();


    // =========================================================
    // Equipment
    // =========================================================

    [Header("Equipment")]

    public EquipmentType equipmentType =
        EquipmentType.None;

    [Min(0)]
    public int maxDurability;

    public EquipmentStatModifier statModifier =
        new EquipmentStatModifier();


    // =========================================================
    // Weapon Skill
    // =========================================================

    [Header("Weapon Skill")]

    [Tooltip(
        "이 무기가 사용하는 무기 스킬 ID입니다. " +
        "0이면 무기 스킬이 없습니다."
    )]
    [Min(0)]
    public int weaponSkillId;

    [TextArea]
    [Tooltip(
        "기존 UI 호환용 무기 스킬 설명입니다. " +
        "실제 스킬 정보는 WeaponSkillData에서 관리합니다."
    )]
    public string weaponSkillDescription;


    // =========================================================
    // Properties
    // =========================================================

    public bool IsEquipment
    {
        get
        {
            return category == ItemCategory.Equipment &&
                   equipmentType != EquipmentType.None;
        }
    }


    public bool IsWeapon
    {
        get
        {
            return IsEquipment &&
                   equipmentType == EquipmentType.Weapon;
        }
    }


    public bool HasWeaponSkill
    {
        get
        {
            return IsWeapon &&
                   weaponSkillId > 0;
        }
    }


    public bool HasStatusEffect
    {
        get
        {
            return statusId > 0;
        }
    }


    public int SafeMaxDurability
    {
        get
        {
            return IsEquipment
                ? Mathf.Max(1, maxDurability)
                : 0;
        }
    }


    // =========================================================
    // Shape
    // =========================================================

    public void EnsureValidShape()
    {
        if (shape == null)
        {
            shape =
                new List<Vector2Int>();
        }

        if (shape.Count == 0)
        {
            shape.Add(
                Vector2Int.zero
            );
        }
    }
}