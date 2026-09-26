using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonRewardItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;

    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;

    private int itemID;

    private bool hasDecision;
    private bool selected;
    private bool acquired;

    public int ItemID => itemID;

    public bool HasDecision =>
        hasDecision;

    public bool IsSelected =>
        hasDecision &&
        selected &&
        !acquired;

    public bool IsAcquired =>
        acquired;

    public void Setup(int newItemID)
    {
        itemID = newItemID;

        hasDecision = false;
        selected = false;
        acquired = false;

        ItemData data = null;

        if (ItemDatabase.Instance != null)
        {
            data =
                ItemDatabase.Instance
                    .GetItem(itemID);
        }

        if (data != null)
        {
            if (itemNameText != null)
            {
                itemNameText.text =
                    data.itemName;
            }

            if (itemIcon != null)
            {
                itemIcon.sprite =
                    data.icon;

                itemIcon.enabled =
                    data.icon != null;
            }
        }
        else
        {
            if (itemNameText != null)
            {
                itemNameText.text =
                    $"Unknown ({itemID})";
            }

            if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }
        }

        if (amountText != null)
        {
            amountText.text = "";
        }

        if (acceptButton != null)
        {
            acceptButton.onClick
                .RemoveAllListeners();

            acceptButton.onClick
                .AddListener(OnAccept);

            acceptButton.interactable = true;
        }

        if (rejectButton != null)
        {
            rejectButton.onClick
                .RemoveAllListeners();

            rejectButton.onClick
                .AddListener(OnReject);

            rejectButton.interactable = true;
        }
    }

    private void OnAccept()
    {
        if (acquired)
            return;

        hasDecision = true;
        selected = true;

        if (amountText != null)
        {
            amountText.text =
                "선택";
        }

        if (acceptButton != null)
        {
            acceptButton.interactable =
                false;
        }

        if (rejectButton != null)
        {
            rejectButton.interactable =
                true;
        }
    }

    private void OnReject()
    {
        if (acquired)
            return;

        hasDecision = true;
        selected = false;

        if (amountText != null)
        {
            amountText.text =
                "제외";
        }

        if (acceptButton != null)
        {
            acceptButton.interactable =
                true;
        }

        if (rejectButton != null)
        {
            rejectButton.interactable =
                false;
        }
    }

    public void MarkAcquired()
    {
        acquired = true;
        hasDecision = true;
        selected = false;

        if (amountText != null)
        {
            amountText.text =
                "획득";
        }

        DisableButtons();
    }

    public void MarkInventoryFull()
    {
        if (amountText != null)
        {
            amountText.text =
                "공간 부족";
        }

        hasDecision = true;
        selected = true;

        if (acceptButton != null)
        {
            acceptButton.interactable =
                false;
        }

        if (rejectButton != null)
        {
            rejectButton.interactable =
                true;
        }
    }

    private void DisableButtons()
    {
        if (acceptButton != null)
        {
            acceptButton.interactable =
                false;
        }

        if (rejectButton != null)
        {
            rejectButton.interactable =
                false;
        }
    }
}