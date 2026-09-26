using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// SkillButton에 붙여서 사용하는 무기 스킬 툴팁 UI.
/// 마우스를 올리면 현재 메인 무기의 스킬 정보를 표시하고,
/// 마우스가 빠져나가면 툴팁을 닫는다.
/// </summary>
public class WeaponSkillTooltipUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Tooltip Root")]
    [Tooltip("BattleRoot 아래에 만든 WeaponSkillTooltip 오브젝트")]
    [SerializeField]
    private GameObject tooltipRoot;

    [Header("Tooltip Text")]
    [SerializeField]
    private TMP_Text skillNameText;

    [SerializeField]
    private TMP_Text durabilityText;

    [SerializeField]
    private TMP_Text descriptionText;

    private void Awake()
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipRoot == null)
        {
            Debug.LogWarning(
                "[WeaponSkillTooltipUI] Tooltip Root가 연결되어 있지 않습니다."
            );
            return;
        }

        InventoryItem mainWeapon = null;

        if (EquipmentManager.Instance != null)
        {
            mainWeapon =
                EquipmentManager.Instance.MainWeapon;
        }

        // 메인 무기 미장착
        if (mainWeapon == null ||
            mainWeapon.data == null)
        {
            SetNoWeaponText();
            tooltipRoot.SetActive(true);
            return;
        }

        int skillId =
            mainWeapon.data.weaponSkillId;

        // 무기는 있지만 스킬이 없는 경우
        if (skillId <= 0)
        {
            SetNoSkillText();
            tooltipRoot.SetActive(true);
            return;
        }

        if (WeaponSkillDatabase.Instance == null)
        {
            SetDatabaseErrorText();
            tooltipRoot.SetActive(true);

            Debug.LogWarning(
                "[WeaponSkillTooltipUI] WeaponSkillDatabase가 없습니다."
            );
            return;
        }

        WeaponSkillData skill =
            WeaponSkillDatabase.Instance
                .GetSkill(skillId);

        if (skill == null)
        {
            SetDatabaseErrorText();
            tooltipRoot.SetActive(true);

            Debug.LogWarning(
                "[WeaponSkillTooltipUI] WeaponSkillData를 찾을 수 없습니다. Skill ID: " +
                skillId
            );
            return;
        }

        if (skillNameText != null)
        {
            skillNameText.text =
                skill.skillName;
        }

        if (durabilityText != null)
        {
            durabilityText.text =
                "내구도 소모 : " +
                skill.useDurability;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                skill.description;
        }

        tooltipRoot.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    private void SetNoWeaponText()
    {
        if (skillNameText != null)
        {
            skillNameText.text =
                "무기 스킬";
        }

        if (durabilityText != null)
        {
            durabilityText.text =
                string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                "무기가 장착되어 있지 않습니다!";
        }
    }

    private void SetNoSkillText()
    {
        if (skillNameText != null)
        {
            skillNameText.text =
                "무기 스킬";
        }

        if (durabilityText != null)
        {
            durabilityText.text =
                string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                "장착된 무기에 사용할 수 있는 무기 스킬이 없습니다.";
        }
    }

    private void SetDatabaseErrorText()
    {
        if (skillNameText != null)
        {
            skillNameText.text =
                "무기 스킬";
        }

        if (durabilityText != null)
        {
            durabilityText.text =
                string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                "무기 스킬 정보를 불러올 수 없습니다.";
        }
    }
}
