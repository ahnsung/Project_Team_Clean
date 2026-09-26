using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler
{
    public Image iconImage;
    public Image highlightImage;

    private InventoryItem item;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 originalAnchoredPosition;
    private int originalRotation;
    private bool didDrag;

    private readonly List<GameObject> cellVisuals = new List<GameObject>();

    public void Init(InventoryItem item, Canvas canvas)
    {
        this.item = item;
        this.canvas = canvas;

        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconImage != null)
            iconImage.gameObject.SetActive(false);

        if (highlightImage != null)
            highlightImage.gameObject.SetActive(false);

        RebuildShapeVisual();
    }

    private void Update()
    {
        if (!didDrag) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            item.RotateClockwise();
            RebuildShapeVisual();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        didDrag = false;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalRotation = item.rotation;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        didDrag = true;

        InventoryUIManager.Instance.StartHoldItem(item, this);

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.75f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        didDrag = true;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Vector2Int targetCell = InventoryUIManager.Instance.ScreenToCell(eventData.position);

        bool success = InventoryManager.Instance.TryMoveItem(item, targetCell);

        if (!success)
        {
            item.SetRotation(originalRotation);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            RebuildShapeVisual();
        }

        InventoryUIManager.Instance.EndHoldItem();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (didDrag) return;

        InventoryUIManager.Instance.SelectItem(item);
    }

    public void RebuildShapeVisual()
    {
        foreach (GameObject obj in cellVisuals)
        {
            if (obj != null)
                Destroy(obj);
        }

        cellVisuals.Clear();

        if (item == null) return;

        float cellSize = InventoryUIManager.Instance.cellSize;
        List<Vector2Int> shape = item.GetRotatedShape();
        Vector2Int size = item.GetShapeSize();

        rectTransform.sizeDelta = new Vector2(size.x * cellSize, size.y * cellSize);

        float startX = -(size.x * cellSize) / 2f + cellSize / 2f;
        float startY = (size.y * cellSize) / 2f - cellSize / 2f;

        foreach (Vector2Int cell in shape)
        {
            GameObject cellObj = new GameObject("ItemCell_" + cell.x + "_" + cell.y);
            cellObj.transform.SetParent(transform, false);

            Image img = cellObj.AddComponent<Image>();
            img.sprite = item.data.icon;
            img.color = Color.white;
            img.raycastTarget = false;

            RectTransform rt = cellObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cellSize - 6f, cellSize - 6f);
            rt.anchoredPosition = new Vector2(
                startX + cell.x * cellSize,
                startY - cell.y * cellSize
            );

            Outline outline = cellObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            cellVisuals.Add(cellObj);
        }
    }

    public void SetHighlight(bool value)
    {
        foreach (GameObject obj in cellVisuals)
        {
            if (obj == null) continue;

            Image img = obj.GetComponent<Image>();
            if (img != null)
                img.color = value ? new Color(1f, 0.9f, 0.45f, 1f) : Color.white;
        }
    }
}