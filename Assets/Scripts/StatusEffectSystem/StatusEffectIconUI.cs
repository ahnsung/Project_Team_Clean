using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusEffectIconUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI durationText;

    private ActiveStatusEffect effect;
    private StatusEffectUIManager owner;

    public void Setup(
        ActiveStatusEffect activeEffect,
        StatusEffectUIManager uiManager,
        Sprite sprite)
    {
        effect = activeEffect;
        owner = uiManager;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (effect == null ||
            effect.Data == null)
        {
            return;
        }

        if (durationText != null)
        {
            if (effect.IsInfinite)
            {
                durationText.text = "∞";
            }
            else
            {
                durationText.text =
                    effect.RemainingDuration.ToString();
            }
        }
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (owner == null ||
            effect == null ||
            effect.Data == null)
        {
            return;
        }

        owner.ShowTooltip(effect);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (owner != null)
        {
            owner.HideTooltip();
        }
    }
}