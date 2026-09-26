using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 하단의 장비 슬롯 하나를 담당합니다.
///
/// 현재 프로젝트 UI 구조:
/// 슬롯 부모
/// └ EquippedIcon
///
/// 슬롯 부모에는 Image, Button, EquipmentSlotUI가 있고,
/// EquippedIcon에는 실제 장착된 장비 아이콘이 표시됩니다.
/// </summary>
public class EquipmentSlotUI : MonoBehaviour
{
    [Header("Slot")]
    [SerializeField]
    private EquipmentSlotType slotType;

    [Tooltip("실제 장착 장비 아이콘을 표시할 자식 Image입니다.")]
    [SerializeField]
    private Image equippedIcon;

    [Tooltip("이 슬롯을 클릭하는 부모 Button입니다.")]
    [SerializeField]
    private Button slotButton;

    public EquipmentSlotType SlotType => slotType;

    private void Awake()
    {
        ResolveReferences();

        if (slotButton != null)
        {
            // Inspector에서 따로 OnClick을 연결했더라도
            // 이 메서드가 중복 실행되지 않게 자기 리스너만 정리합니다.
            slotButton.onClick.RemoveListener(OnClickSlot);
            slotButton.onClick.AddListener(OnClickSlot);
        }

        // 시작할 때 실제 장비 상태가 갱신되기 전까지
        // 장착 아이콘은 숨겨 둡니다.
        SetIconVisible(false);
    }

    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnClickSlot);
        }
    }

    private void ResolveReferences()
    {
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        // Inspector 연결을 빼먹었을 때
        // 자식 이름이 EquippedIcon이면 자동으로 찾습니다.
        if (equippedIcon == null)
        {
            Transform iconTransform =
                transform.Find("EquippedIcon");

            if (iconTransform != null)
            {
                equippedIcon =
                    iconTransform.GetComponent<Image>();
            }
        }
    }

    /// <summary>
    /// EquipmentPanelUI가 장비 변경 시 호출합니다.
    /// </summary>
    public void Refresh(InventoryItem item)
    {
        bool hasValidItem =
            item != null &&
            item.data != null &&
            !item.IsBroken &&
            item.data.icon != null;

        if (!hasValidItem)
        {
            ClearIcon();
            return;
        }

        if (equippedIcon == null)
        {
            Debug.LogWarning(
                $"[EquipmentSlotUI] {gameObject.name}의 " +
                "EquippedIcon이 연결되지 않았습니다.",
                this
            );

            return;
        }

        equippedIcon.sprite = item.data.icon;
        equippedIcon.color = Color.white;
        equippedIcon.preserveAspect = true;
        equippedIcon.raycastTarget = false;

        SetIconVisible(true);
    }

    /// <summary>
    /// 슬롯이 클릭되면 해당 슬롯의 장비 교체 팝업을 요청합니다.
    /// </summary>
    public void OnClickSlot()
    {
        if (EquipmentPanelUI.Instance == null)
        {
            Debug.LogWarning(
                "[EquipmentSlotUI] EquipmentPanelUI가 씬에 없습니다.",
                this
            );

            return;
        }

        EquipmentPanelUI.Instance.OpenSlotPopup(slotType);
    }

    private void ClearIcon()
    {
        if (equippedIcon == null)
            return;

        equippedIcon.sprite = null;
        SetIconVisible(false);
    }

    private void SetIconVisible(bool visible)
    {
        if (equippedIcon != null)
        {
            equippedIcon.enabled = visible;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ResolveReferences();

        if (equippedIcon != null)
        {
            equippedIcon.raycastTarget = false;
            equippedIcon.preserveAspect = true;
        }
    }
#endif
}