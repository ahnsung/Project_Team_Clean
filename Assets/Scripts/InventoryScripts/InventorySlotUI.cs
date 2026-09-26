using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler
{
    public Vector2Int cellPosition;

    [SerializeField] private Image background;

    public void Init(Vector2Int pos)
    {
        cellPosition = pos;

        if (background == null)
            background = GetComponent<Image>();
    }

    public void SetLocked(bool locked)
    {
        if (background == null)
            background = GetComponent<Image>();

        if (background == null) return;

        background.sprite = null;
        background.raycastTarget = true;

        background.color = locked
            ? new Color(0f, 0f, 0f, 0.25f)
            : new Color(0f, 0f, 0f, 0f);

        Outline outline = GetComponent<Outline>();
        if (outline != null)
            Destroy(outline);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.SetHoverCell(cellPosition);
    }
}