using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 하단 장비 슬롯 5개와 장비 교체 팝업을 관리합니다.
///
/// 현재 하이라이키 기준:
///
/// MainWeaponSlot        ← 정리용 빈 부모, 이 스크립트 부착
/// ├ MainWeapon
/// ├ SubWeapon
/// ├ SwapWeaponButton
/// ├ Head
/// ├ Shoes
/// └ Armor
/// </summary>
public class EquipmentPanelUI : MonoBehaviour
{
    public static EquipmentPanelUI Instance;

    [Header("Equipment Slots")]
    [SerializeField]
    private EquipmentSlotUI headSlot;

    [SerializeField]
    private EquipmentSlotUI armorSlot;

    [SerializeField]
    private EquipmentSlotUI shoesSlot;

    [SerializeField]
    private EquipmentSlotUI mainWeaponSlot;

    [SerializeField]
    private EquipmentSlotUI subWeaponSlot;

    [Header("Popup")]
    [Tooltip("장비 교체 팝업 전체 오브젝트입니다.")]
    [SerializeField]
    private GameObject popupRoot;

    [SerializeField]
    private TextMeshProUGUI popupTitleText;

    [SerializeField]
    private TextMeshProUGUI currentEquipmentText;

    [Tooltip("교체 가능한 장비 버튼들이 생성될 Content입니다.")]
    [SerializeField]
    private RectTransform candidateRoot;

    [Tooltip("Project 창에 저장된 장비 후보 버튼 프리팹입니다.")]
    [SerializeField]
    private Button candidateButtonPrefab;

    [SerializeField]
    private Button unequipButton;

    [SerializeField]
    private Button closeButton;

    private EquipmentSlotType selectedSlot;

    private readonly List<GameObject> candidateObjects =
        new List<GameObject>();

    private EquipmentManager subscribedManager;

    private Coroutine subscribeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "[EquipmentPanelUI] 중복 인스턴스를 제거합니다.",
                gameObject
            );

            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        BindPopupButtons();

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }

        RefreshAll();

        subscribeRoutine =
            StartCoroutine(WaitAndSubscribeEquipmentManager());
    }

    private void OnDestroy()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        UnsubscribeEquipmentManager();

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(OnClickUnequip);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void BindPopupButtons()
    {
        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(OnClickUnequip);
            unequipButton.onClick.AddListener(OnClickUnequip);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    private IEnumerator WaitAndSubscribeEquipmentManager()
    {
        while (EquipmentManager.Instance == null)
        {
            yield return null;
        }

        SubscribeEquipmentManager(
            EquipmentManager.Instance
        );

        RefreshAll();
        subscribeRoutine = null;
    }

    private void SubscribeEquipmentManager(
        EquipmentManager manager)
    {
        if (manager == null)
            return;

        if (subscribedManager == manager)
            return;

        UnsubscribeEquipmentManager();

        subscribedManager = manager;
        subscribedManager.OnEquipmentChanged += RefreshAll;
    }

    private void UnsubscribeEquipmentManager()
    {
        if (subscribedManager == null)
            return;

        subscribedManager.OnEquipmentChanged -= RefreshAll;
        subscribedManager = null;
    }

    /// <summary>
    /// 현재 장착 상태를 화면 하단의 5개 아이콘에 반영합니다.
    /// </summary>
    public void RefreshAll()
    {
        EquipmentManager manager =
            EquipmentManager.Instance;

        if (manager == null)
        {
            RefreshSlot(headSlot, null);
            RefreshSlot(armorSlot, null);
            RefreshSlot(shoesSlot, null);
            RefreshSlot(mainWeaponSlot, null);
            RefreshSlot(subWeaponSlot, null);
            return;
        }

        if (subscribedManager != manager)
        {
            SubscribeEquipmentManager(manager);
        }

        RefreshSlot(headSlot, manager.Head);
        RefreshSlot(armorSlot, manager.Armor);
        RefreshSlot(shoesSlot, manager.Shoes);
        RefreshSlot(mainWeaponSlot, manager.MainWeapon);
        RefreshSlot(subWeaponSlot, manager.SubWeapon);

        if (popupRoot != null &&
            popupRoot.activeSelf)
        {
            RefreshPopup();
        }
    }

    private void RefreshSlot(
        EquipmentSlotUI slotUI,
        InventoryItem item)
    {
        if (slotUI != null)
        {
            slotUI.Refresh(item);
        }
    }

    /// <summary>
    /// 장비 슬롯 클릭 시 호출됩니다.
    /// 팝업이 아직 연결되지 않았다면 오류 없이 경고만 표시합니다.
    /// </summary>
    public void OpenSlotPopup(
        EquipmentSlotType slot)
    {
        selectedSlot = slot;

        if (popupRoot == null)
        {
            Debug.LogWarning(
                "[EquipmentPanelUI] EquipmentChangePopup이 " +
                "아직 연결되지 않았습니다.",
                this
            );

            return;
        }

        popupRoot.SetActive(true);
        RefreshPopup();
    }

    public void ClosePopup()
    {
        ClearCandidates();

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 가운데 스왑 버튼에서 호출합니다.
    /// </summary>
    public void OnClickSwapWeapons()
    {
        EquipmentManager manager =
            EquipmentManager.Instance;

        if (manager == null)
        {
            ShowNotice(
                "EquipmentManager를 찾을 수 없습니다."
            );

            return;
        }

        bool swapped =
            manager.SwapWeapons();

        if (!swapped)
        {
            ShowNotice(
                "현재는 무기를 스왑할 수 없습니다."
            );

            return;
        }

        RefreshAll();
    }

    private void RefreshPopup()
    {
        EquipmentManager manager =
            EquipmentManager.Instance;

        if (manager == null)
            return;

        InventoryItem currentItem =
            manager.GetEquippedItem(selectedSlot);

        RefreshPopupTitle();
        RefreshCurrentEquipment(currentItem);
        RefreshUnequipButton(currentItem);
        RebuildCandidates();
    }

    private void RefreshPopupTitle()
    {
        if (popupTitleText == null)
            return;

        popupTitleText.text =
            GetSlotDisplayName(selectedSlot);
    }

    private void RefreshCurrentEquipment(
        InventoryItem currentItem)
    {
        if (currentEquipmentText == null)
            return;

        if (currentItem == null ||
            currentItem.data == null)
        {
            currentEquipmentText.text =
                "현재 장비\n없음";

            return;
        }

        currentEquipmentText.text =
            BuildCurrentEquipmentDescription(
                currentItem
            );
    }

    private string BuildCurrentEquipmentDescription(
        InventoryItem item)
    {
        ItemData data = item.data;

        string stats =
            BuildStatDescription(
                data.statModifier
            );

        string result =
            "현재 장비\n" +
            data.itemName +
            "\n\n" +
            "내구도: " +
            item.currentDurability +
            " / " +
            data.SafeMaxDurability +
            "\n" +
            "능력치: " +
            stats;

        if (data.IsWeapon &&
            !string.IsNullOrWhiteSpace(
                data.weaponSkillDescription))
        {
            result +=
                "\n무기 스킬: " +
                data.weaponSkillDescription;
        }

        return result;
    }

    private void RefreshUnequipButton(
        InventoryItem currentItem)
    {
        if (unequipButton != null)
        {
            unequipButton.interactable =
                currentItem != null;
        }
    }

    private void RebuildCandidates()
    {
        ClearCandidates();

        if (candidateRoot == null ||
            candidateButtonPrefab == null)
        {
            return;
        }

        if (InventoryManager.Instance == null ||
            EquipmentManager.Instance == null)
        {
            return;
        }

        EquipmentType equipmentType =
            EquipmentManager.Instance
                .GetEquipmentTypeForSlot(
                    selectedSlot
                );

        List<InventoryItem> candidates =
            InventoryManager.Instance
                .FindEquipmentByType(
                    equipmentType
                );

        foreach (InventoryItem candidate in candidates)
        {
            CreateCandidateButton(candidate);
        }
    }

    private void CreateCandidateButton(
        InventoryItem candidate)
    {
        if (candidate == null ||
            candidate.data == null)
        {
            return;
        }

        Button button =
            Instantiate(
                candidateButtonPrefab,
                candidateRoot
            );

        candidateObjects.Add(
            button.gameObject
        );

        TextMeshProUGUI buttonText =
            button.GetComponentInChildren
                <TextMeshProUGUI>(true);

        if (buttonText != null)
        {
            buttonText.text =
                candidate.data.itemName +
                "\n내구도: " +
                candidate.currentDurability +
                " / " +
                candidate.data.SafeMaxDurability;
        }

        InventoryItem capturedItem =
            candidate;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            () => OnClickCandidate(capturedItem)
        );
    }

    private void OnClickCandidate(
        InventoryItem item)
    {
        EquipmentManager manager =
            EquipmentManager.Instance;

        if (manager == null)
            return;

        bool equipped =
            manager.EquipToSlot(
                item,
                selectedSlot
            );

        if (!equipped)
        {
            ShowNotice(
                "해당 장비로 교체할 수 없습니다.\n" +
                "전투 중이거나 인벤토리 공간이 부족할 수 있습니다."
            );

            return;
        }

        RefreshAll();
    }

    private void OnClickUnequip()
    {
        EquipmentManager manager =
            EquipmentManager.Instance;

        if (manager == null)
            return;

        InventoryItem currentItem =
            manager.GetEquippedItem(
                selectedSlot
            );

        if (currentItem == null)
            return;

        bool unequipped =
            manager.Unequip(
                selectedSlot
            );

        if (!unequipped)
        {
            ShowNotice(
                "장비를 해제할 수 없습니다.\n" +
                "인벤토리 공간을 확인해주세요."
            );

            return;
        }

        RefreshAll();
    }

    private void ClearCandidates()
    {
        foreach (GameObject candidateObject
                 in candidateObjects)
        {
            if (candidateObject != null)
            {
                Destroy(candidateObject);
            }
        }

        candidateObjects.Clear();
    }

    private string BuildStatDescription(
        EquipmentStatModifier modifier)
    {
        if (modifier == null)
            return "없음";

        List<string> descriptions =
            new List<string>();

        AddStat(
            descriptions,
            "STR",
            modifier.str
        );

        AddStat(
            descriptions,
            "DEX",
            modifier.dex
        );

        AddStat(
            descriptions,
            "CON",
            modifier.con
        );

        AddStat(
            descriptions,
            "INT",
            modifier.intelligence
        );

        AddStat(
            descriptions,
            "공격력",
            modifier.attackPower
        );

        AddStat(
            descriptions,
            "명중률",
            modifier.accuracyBonus,
            "%"
        );

        return descriptions.Count == 0
            ? "없음"
            : string.Join(", ", descriptions);
    }

    private void AddStat(
        List<string> descriptions,
        string statName,
        int value,
        string suffix = "")
    {
        if (value == 0)
            return;

        string sign =
            value > 0 ? "+" : string.Empty;

        descriptions.Add(
            statName +
            " " +
            sign +
            value +
            suffix
        );
    }

    private string GetSlotDisplayName(
        EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head:
                return "머리 장비";

            case EquipmentSlotType.Armor:
                return "갑옷 장비";

            case EquipmentSlotType.Shoes:
                return "신발 장비";

            case EquipmentSlotType.MainWeapon:
                return "주 무기";

            case EquipmentSlotType.SubWeapon:
                return "보조 무기";

            default:
                return "장비";
        }
    }

    private void ShowNotice(
        string message)
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance
                .ShowNotice(message);
        }
        else
        {
            Debug.Log(message);
        }
    }
}