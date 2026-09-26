using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Root")]
    public GameObject inventoryRoot;
    public Canvas canvas;

    [Header("Window Buttons")]
    public Button closeButton;

    [Header("Grid")]
    public RectTransform gridRoot;
    public GameObject slotPrefab;
    public GameObject itemPrefab;

    [Header("Grid Calibration")]
    public float cellSize = 48f;
    public float cellGap = 0f;
    public Vector2 gridOffset = Vector2.zero;

    [Header("Info Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemInfoText;

    [Tooltip("소비 아이템은 사용, 장비는 장착 버튼으로 사용됩니다.")]
    public Button useButton;

    public TextMeshProUGUI useButtonText;
    public Button dropButton;
    public Button closeInfoButton;

    [Header("Drop Popup")]
    public GameObject dropPopup;
    public TextMeshProUGUI dropPopupText;
    public Button confirmDropButton;
    public Button cancelDropButton;

    [Header("Notice Popup")]
    public GameObject noticePopup;
    public TextMeshProUGUI noticeText;
    public Button noticeConfirmButton;

    private InventoryState state = InventoryState.Closed;

    private InventoryItem selectedItem;
    private InventoryItem holdingItem;

    private readonly Dictionary<InventoryItem, InventoryItemUI> itemUIs =
        new Dictionary<InventoryItem, InventoryItemUI>();

    public Vector2Int CurrentHoverCell { get; private set; }

    private float Step
    {
        get
        {
            return cellSize + cellGap;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (dropPopup != null)
            dropPopup.SetActive(false);

        if (noticePopup != null)
            noticePopup.SetActive(false);

        BindButtons();
        BuildSlots();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BindButtons()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnClickUseOrEquip);
        }

        if (dropButton != null)
        {
            dropButton.onClick.RemoveAllListeners();
            dropButton.onClick.AddListener(OnClickDrop);
        }

        if (closeInfoButton != null)
        {
            closeInfoButton.onClick.RemoveAllListeners();
            closeInfoButton.onClick.AddListener(CloseInfoPanel);
        }

        if (confirmDropButton != null)
        {
            confirmDropButton.onClick.RemoveAllListeners();
            confirmDropButton.onClick.AddListener(ConfirmDrop);
        }

        if (cancelDropButton != null)
        {
            cancelDropButton.onClick.RemoveAllListeners();
            cancelDropButton.onClick.AddListener(CancelDrop);
        }

        if (noticeConfirmButton != null)
        {
            noticeConfirmButton.onClick.RemoveAllListeners();
            noticeConfirmButton.onClick.AddListener(CloseNotice);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickCloseButton);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();

        if (InventoryManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.B))
            InventoryManager.Instance.AddTestBandage();

        if (Input.GetKeyDown(KeyCode.N))
            InventoryManager.Instance.AddTestLongBandage();

        if (Input.GetKeyDown(KeyCode.M))
            InventoryManager.Instance.AddTestMedKit();

        if (Input.GetKeyDown(KeyCode.G))
            InventoryManager.Instance.AddTestScrap();

        if (Input.GetKeyDown(KeyCode.Alpha8))
            InventoryManager.Instance.AddTestWeapon();

        if (Input.GetKeyDown(KeyCode.Alpha9))
            InventoryManager.Instance.AddTestArmor();
    }

    public void ToggleInventory()
    {
        if (inventoryRoot == null)
            return;

        if (inventoryRoot.activeSelf)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OnClickInventoryButton()
    {
        ToggleInventory();
    }

    public void OnClickCloseButton()
    {
        CloseInventory();
    }

    public void OpenInventory()
    {
        if (inventoryRoot == null)
            return;

        inventoryRoot.SetActive(true);
        state = InventoryState.Default;

        RefreshUI();
    }

    public void CloseInventory()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        CloseInfoPanel();
        CloseDropPopup();
        CloseNotice();

        selectedItem = null;
        holdingItem = null;

        state = InventoryState.Closed;
    }

    public void RebuildAndRefresh()
    {
        BuildSlots();
        RefreshUI();
    }

    private void BuildSlots()
    {
        if (gridRoot == null || slotPrefab == null)
            return;

        if (InventoryManager.Instance == null)
            return;

        for (int i = gridRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(gridRoot.GetChild(i).gameObject);
        }

        itemUIs.Clear();

        int width = InventoryManager.Instance.width;
        int height = InventoryManager.Instance.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject slotObject =
                    Instantiate(slotPrefab, gridRoot);

                slotObject.name =
                    "Slot_" + x + "_" + y;

                RectTransform rectTransform =
                    slotObject.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    rectTransform.anchorMin =
                        new Vector2(0.5f, 0.5f);

                    rectTransform.anchorMax =
                        new Vector2(0.5f, 0.5f);

                    rectTransform.pivot =
                        new Vector2(0.5f, 0.5f);

                    rectTransform.sizeDelta =
                        new Vector2(cellSize, cellSize);

                    rectTransform.anchoredPosition =
                        CellToLocalPosition(
                            new Vector2Int(x, y)
                        );
                }

                Image image =
                    slotObject.GetComponent<Image>();

                if (image != null)
                {
                    Color color = image.color;
                    color.a = 0f;

                    image.color = color;
                    image.raycastTarget = true;
                }

                Outline outline =
                    slotObject.GetComponent<Outline>();

                if (outline != null)
                    Destroy(outline);

                InventorySlotUI slot =
                    slotObject.GetComponent<InventorySlotUI>();

                if (slot != null)
                {
                    slot.Init(
                        new Vector2Int(x, y)
                    );

                    bool locked =
                        x >= InventoryManager.Instance.unlockedWidth ||
                        y >= InventoryManager.Instance.unlockedHeight;

                    slot.SetLocked(locked);
                }
            }
        }
    }

    public void RefreshUI()
    {
        foreach (
            KeyValuePair<InventoryItem, InventoryItemUI> pair
            in itemUIs)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        itemUIs.Clear();

        if (InventoryManager.Instance == null)
            return;

        foreach (
            InventoryItem item
            in InventoryManager.Instance.items)
        {
            if (item == null || item.data == null)
                continue;

            CreateItemUI(item);
        }

        bool selectedItemStillExists =
            selectedItem != null &&
            InventoryManager.Instance.items.Contains(selectedItem);

        if (selectedItemStillExists)
        {
            UpdateInfoPanel();
        }
        else
        {
            selectedItem = null;

            if (infoPanel != null)
                infoPanel.SetActive(false);
        }
    }

    private void CreateItemUI(
        InventoryItem item)
    {
        if (itemPrefab == null || gridRoot == null)
            return;

        GameObject itemObject =
            Instantiate(itemPrefab, gridRoot);

        itemObject.name =
            "Item_" + item.data.itemName;

        RectTransform rectTransform =
            itemObject.GetComponent<RectTransform>();

        Vector2Int shapeSize =
            item.GetShapeSize();

        if (rectTransform != null)
        {
            rectTransform.anchorMin =
                new Vector2(0.5f, 0.5f);

            rectTransform.anchorMax =
                new Vector2(0.5f, 0.5f);

            rectTransform.pivot =
                new Vector2(0.5f, 0.5f);

            rectTransform.sizeDelta =
                new Vector2(
                    shapeSize.x * cellSize +
                    (shapeSize.x - 1) * cellGap,

                    shapeSize.y * cellSize +
                    (shapeSize.y - 1) * cellGap
                );

            Vector2 baseCellPosition =
                CellToLocalPosition(item.position);

            rectTransform.anchoredPosition =
                new Vector2(
                    baseCellPosition.x +
                    ((shapeSize.x - 1) * Step / 2f),

                    baseCellPosition.y -
                    ((shapeSize.y - 1) * Step / 2f)
                );
        }

        InventoryItemUI itemUI =
            itemObject.GetComponent<InventoryItemUI>();

        if (itemUI != null)
        {
            itemUI.Init(item, canvas);
            itemUIs[item] = itemUI;
        }
    }

    private Vector2 CellToLocalPosition(
        Vector2Int cell)
    {
        if (gridRoot == null)
            return Vector2.zero;

        Rect rect = gridRoot.rect;

        float x =
            rect.xMin +
            gridOffset.x +
            cellSize / 2f +
            cell.x * Step;

        float y =
            rect.yMax +
            gridOffset.y -
            cellSize / 2f -
            cell.y * Step;

        return new Vector2(x, y);
    }

    public Vector2Int ScreenToCell(
        Vector2 screenPosition)
    {
        if (gridRoot == null)
            return new Vector2Int(-1, -1);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRoot,
            screenPosition,
            canvas != null ? canvas.worldCamera : null,
            out Vector2 localPoint
        );

        if (InventoryManager.Instance == null)
            return new Vector2Int(-1, -1);

        Rect rect = gridRoot.rect;

        float startX =
            rect.xMin + gridOffset.x;

        float startY =
            rect.yMax + gridOffset.y;

        int x =
            Mathf.FloorToInt(
                (localPoint.x - startX) / Step
            );

        int y =
            Mathf.FloorToInt(
                (startY - localPoint.y) / Step
            );

        return new Vector2Int(x, y);
    }

    public void SelectItem(
        InventoryItem item)
    {
        if (state == InventoryState.ItemHolding)
            return;

        if (item == null || item.data == null)
            return;

        selectedItem = item;
        state = InventoryState.ItemSelected;

        foreach (
            KeyValuePair<InventoryItem, InventoryItemUI> pair
            in itemUIs)
        {
            if (pair.Value != null)
            {
                pair.Value.SetHighlight(
                    pair.Key == item
                );
            }
        }

        UpdateInfoPanel();

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    private void UpdateInfoPanel()
    {
        if (selectedItem == null ||
            selectedItem.data == null)
        {
            return;
        }

        if (itemNameText != null)
        {
            itemNameText.text =
                selectedItem.data.itemName;
        }

        if (selectedItem.data.IsEquipment)
        {
            UpdateEquipmentInfoPanel();
        }
        else
        {
            UpdateConsumableInfoPanel();
        }

        UpdatePrimaryButton();
    }

    private void UpdateConsumableInfoPanel()
    {
        if (itemInfoText == null ||
            selectedItem == null ||
            selectedItem.data == null)
        {
            return;
        }

        itemInfoText.text =
            "남은 사용 횟수: " +
            selectedItem.remainUseCount +
            "\n" +
            "[" +
            selectedItem.data.category +
            "] 아이템" +
            "\n" +
            "버리기 가능: " +
            (selectedItem.data.canDrop ? "O" : "X") +
            "\n" +
            "사용 시 턴 소모: " +
            (selectedItem.data.consumeTurnOnUse ? "O" : "X") +
            "\n" +
            "회전: 드래그 중 R 키" +
            "\n" +
            "효과: " +
            selectedItem.data.effectDescription;
    }

    private void UpdateEquipmentInfoPanel()
    {
        if (itemInfoText == null ||
            selectedItem == null ||
            selectedItem.data == null)
        {
            return;
        }

        ItemData data =
            selectedItem.data;

        itemInfoText.text =
            "남은 내구도: " +
            selectedItem.currentDurability +
            "\n" +
            "[" +
            GetEquipmentTypeName(data.equipmentType) +
            "] 장비" +
            "\n" +
            "능력치: " +
            GetEquipmentStatDescription(data.statModifier) +
            "\n" +
            "버리기 가능: " +
            (data.canDrop ? "O" : "X") +
            "\n" +
            "무기 스킬: " +
            GetWeaponSkillDescription(data) +
            "\n" +
            "회전: 드래그 중 R 키";
    }

    private void UpdatePrimaryButton()
    {
        if (useButton == null)
            return;

        bool isEquipment =
            selectedItem != null &&
            selectedItem.data != null &&
            selectedItem.data.IsEquipment;

        useButton.interactable =
            selectedItem != null;

        string buttonLabel =
            isEquipment
                ? GetEquipmentButtonLabel()
                : "사용";

        if (useButtonText != null)
        {
            useButtonText.text =
                buttonLabel;
        }
        else
        {
            TextMeshProUGUI buttonText =
                useButton.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
                buttonText.text = buttonLabel;
        }
    }

    private string GetEquipmentButtonLabel()
    {
        if (selectedItem == null ||
            selectedItem.data == null)
        {
            return "장착";
        }

        if (EquipmentManager.Instance == null)
            return "장착";

        EquipmentSlotType targetSlot =
            GetDefaultEquipmentSlot(
                selectedItem.data.equipmentType
            );

        InventoryItem equipped =
            EquipmentManager.Instance
                .GetEquippedItem(targetSlot);

        return equipped == null
            ? "장착"
            : "교체";
    }

    private EquipmentSlotType GetDefaultEquipmentSlot(
        EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Head:
                return EquipmentSlotType.Head;

            case EquipmentType.Armor:
                return EquipmentSlotType.Armor;

            case EquipmentType.Shoes:
                return EquipmentSlotType.Shoes;

            case EquipmentType.Weapon:
                return EquipmentSlotType.MainWeapon;

            default:
                return EquipmentSlotType.MainWeapon;
        }
    }

    private string GetEquipmentTypeName(
        EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Head:
                return "머리";

            case EquipmentType.Armor:
                return "갑옷";

            case EquipmentType.Shoes:
                return "신발";

            case EquipmentType.Weapon:
                return "무기";

            default:
                return "장비";
        }
    }

    private string GetEquipmentStatDescription(
        EquipmentStatModifier modifier)
    {
        if (modifier == null)
            return "없음";

        List<string> descriptions =
            new List<string>();

        AddStatDescription(
            descriptions,
            "STR",
            modifier.str
        );

        AddStatDescription(
            descriptions,
            "DEX",
            modifier.dex
        );

        AddStatDescription(
            descriptions,
            "CON",
            modifier.con
        );

        AddStatDescription(
            descriptions,
            "INT",
            modifier.intelligence
        );

        AddStatDescription(
            descriptions,
            "공격력",
            modifier.attackPower
        );

        AddStatDescription(
            descriptions,
            "명중률",
            modifier.accuracyBonus,
            "%"
        );

        if (descriptions.Count == 0)
            return "없음";

        return string.Join(", ", descriptions);
    }

    private void AddStatDescription(
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

    private string GetWeaponSkillDescription(
        ItemData data)
    {
        if (data == null ||
            data.equipmentType != EquipmentType.Weapon)
        {
            return "없음";
        }

        if (string.IsNullOrWhiteSpace(
            data.weaponSkillDescription))
        {
            return "없음";
        }

        return data.weaponSkillDescription;
    }

    public void StartHoldItem(
        InventoryItem item,
        InventoryItemUI itemUI)
    {
        holdingItem = item;
        state = InventoryState.ItemHolding;

        CloseInfoPanel();
    }

    public void EndHoldItem()
    {
        holdingItem = null;
        state = InventoryState.Default;
    }

    public void SetHoverCell(
        Vector2Int cell)
    {
        CurrentHoverCell = cell;
    }

    private void OnClickUseOrEquip()
    {
        if (selectedItem == null ||
            selectedItem.data == null)
        {
            return;
        }

        if (selectedItem.data.IsEquipment)
        {
            OnClickEquip();
        }
        else
        {
            OnClickUseConsumable();
        }
    }

    private void OnClickUseConsumable()
    {
        if (selectedItem == null)
            return;

        if (BattleManager.Instance != null &&
            BattleManager.Instance.IsBattleRunning() &&
            !BattleManager.Instance.CanPlayerUseItem())
        {
            ShowNotice(
                "지금은 플레이어 턴이 아니라\n" +
                "아이템을 사용할 수 없습니다."
            );

            return;
        }

        InventoryItem usedItem =
            selectedItem;

        InventoryManager.Instance.UseItem(
            usedItem
        );

        selectedItem = null;

        CloseInfoPanel();
        RefreshUI();
    }

    private void OnClickEquip()
    {
        if (selectedItem == null ||
            selectedItem.data == null ||
            !selectedItem.data.IsEquipment)
        {
            return;
        }

        if (EquipmentManager.Instance == null)
        {
            ShowNotice(
                "EquipmentManager가\n" +
                "씬에 존재하지 않습니다."
            );

            return;
        }

        bool equipped =
            EquipmentManager.Instance
                .EquipFromInventory(
                    selectedItem
                );

        if (!equipped)
        {
            if (BattleManager.Instance != null &&
                BattleManager.Instance.IsBattleRunning())
            {
                ShowNotice(
                    "전투 중에는 장비를\n" +
                    "장착하거나 교체할 수 없습니다."
                );
            }
            else
            {
                ShowNotice(
                    "장비를 교체할 공간이 부족하거나\n" +
                    "현재 장착할 수 없습니다."
                );
            }

            return;
        }

        selectedItem = null;

        CloseInfoPanel();
        RefreshUI();
    }

    private void OnClickDrop()
    {
        if (selectedItem == null ||
            selectedItem.data == null)
        {
            return;
        }

        if (!selectedItem.data.canDrop)
        {
            ShowNotice(
                "이 아이템은\n" +
                "버릴 수 없습니다."
            );

            CloseInfoPanel();
            return;
        }

        state = InventoryState.DropPopup;

        if (dropPopup != null)
            dropPopup.SetActive(true);

        if (dropPopupText != null)
        {
            dropPopupText.text =
                "정말로 아이템을\n" +
                "버리시겠습니까?";
        }
    }

    private void ConfirmDrop()
    {
        if (selectedItem != null &&
            InventoryManager.Instance != null)
        {
            InventoryManager.Instance
                .RemoveItem(selectedItem);

            selectedItem = null;
        }

        CloseDropPopup();
        CloseInfoPanel();

        state = InventoryState.Default;
    }

    private void CancelDrop()
    {
        CloseDropPopup();
        CloseInfoPanel();

        selectedItem = null;
        state = InventoryState.Default;
    }

    public void CloseInfoPanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        foreach (
            KeyValuePair<InventoryItem, InventoryItemUI> pair
            in itemUIs)
        {
            if (pair.Value != null)
                pair.Value.SetHighlight(false);
        }

        if (state == InventoryState.ItemSelected)
            state = InventoryState.Default;
    }

    private void CloseDropPopup()
    {
        if (dropPopup != null)
            dropPopup.SetActive(false);
    }

    public void ShowNotice(
        string message)
    {
        if (noticePopup != null)
            noticePopup.SetActive(true);

        if (noticeText != null)
            noticeText.text = message;
    }

    public void CloseNotice()
    {
        if (noticePopup != null)
            noticePopup.SetActive(false);

        if (state != InventoryState.Closed)
            state = InventoryState.Default;
    }
}