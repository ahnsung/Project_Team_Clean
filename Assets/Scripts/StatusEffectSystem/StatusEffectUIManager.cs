using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusEffectUIManager : MonoBehaviour
{
    // =========================================================
    // Target
    // =========================================================

    [Header("Target")]
    [SerializeField]
    private StatusEffectController targetController;


    // =========================================================
    // Icon UI
    // =========================================================

    [Header("Icon UI")]
    [SerializeField]
    private Transform iconContainer;

    [SerializeField]
    private GameObject iconPrefab;


    // =========================================================
    // Common Icons
    // =========================================================

    [Header("Common Icons")]

    [Tooltip("일반적인 긍정 버프에 사용")]
    [SerializeField]
    private Sprite buffUpIcon;

    [Tooltip("일반적인 부정 디버프에 사용")]
    [SerializeField]
    private Sprite buffDownIcon;


    // =========================================================
    // Special Icons
    // =========================================================

    [Header("Special Status Icons")]

    [SerializeField]
    private Sprite guardIcon;

    [SerializeField]
    private Sprite poisonIcon;

    [SerializeField]
    private Sprite stunIcon;

    [SerializeField]
    private Sprite corrosionIcon;


    // =========================================================
    // Hunger
    // =========================================================

    [Header("Hunger Icons")]

    [SerializeField]
    private Sprite hungerIcon;

    [SerializeField]
    private Sprite hungerDownIcon;


    // =========================================================
    // Frustration / Mental
    // =========================================================

    [Header("Frustration Icons")]

    [SerializeField]
    private Sprite frustrationIcon;

    [SerializeField]
    private Sprite frustrationDownIcon;


    // =========================================================
    // Tooltip
    // =========================================================

    [Header("Tooltip")]

    [SerializeField]
    private GameObject tooltipRoot;

    [SerializeField]
    private TextMeshProUGUI tooltipNameText;

    [SerializeField]
    private TextMeshProUGUI tooltipDescriptionText;

    [SerializeField]
    private TextMeshProUGUI tooltipDurationText;


    // =========================================================
    // Runtime
    // =========================================================

    private readonly List<StatusEffectIconUI> iconUIs =
        new List<StatusEffectIconUI>();


    // =========================================================
    // Unity
    // =========================================================

    private void Start()
    {
        ResolveTarget();

        HideTooltip();

        if (targetController != null)
        {
            targetController.OnStatusEffectsChanged +=
                RefreshUI;
        }

        RefreshUI();
    }


    private void OnDisable()
    {
        /*
         * UI 오브젝트가 비활성화될 때도
         * 툴팁이 남지 않도록 강제 종료.
         */
        HideTooltip();
    }


    private void OnDestroy()
    {
        if (targetController != null)
        {
            targetController.OnStatusEffectsChanged -=
                RefreshUI;
        }

        HideTooltip();
    }


    // =========================================================
    // Target
    // =========================================================

    private void ResolveTarget()
    {
        if (targetController != null)
            return;

        StatusEffectController[] controllers =
            FindObjectsByType<StatusEffectController>(
                FindObjectsSortMode.None
            );

        foreach (StatusEffectController controller in controllers)
        {
            if (controller == null)
                continue;

            BattleUnit unit =
                controller.GetComponent<BattleUnit>();

            if (
                BattleManager.Instance != null &&
                BattleManager.Instance.playerUnit != null &&
                unit == BattleManager.Instance.playerUnit
            )
            {
                targetController = controller;
                return;
            }
        }

        Debug.LogWarning(
            "[StatusEffectUIManager] 플레이어 StatusEffectController를 찾지 못했습니다."
        );
    }


    // =========================================================
    // Refresh
    // =========================================================

    public void RefreshUI()
    {
        /*
         * 매우 중요:
         *
         * 상태이상 아이콘 위에 마우스를 올린 상태에서
         * 턴이 지나 해당 상태이상이 사라질 경우,
         * 아이콘이 Destroy되면서 OnPointerExit가
         * 호출되지 않을 수 있다.
         *
         * 따라서 아이콘을 지우기 전에
         * 툴팁을 먼저 강제로 닫는다.
         */
        HideTooltip();

        ClearIcons();

        if (
            targetController == null ||
            iconContainer == null ||
            iconPrefab == null
        )
        {
            return;
        }

        foreach (
            ActiveStatusEffect effect
            in targetController.ActiveEffects)
        {
            if (
                effect == null ||
                effect.Data == null
            )
            {
                continue;
            }

            Sprite sprite =
                GetIcon(effect);

            if (sprite == null)
            {
                Debug.LogWarning(
                    "[StatusEffectUIManager] 아이콘을 찾지 못했습니다.\n" +
                    $"이름: {effect.Data.buffName}\n" +
                    $"Type: {effect.Data.effectType}"
                );

                continue;
            }

            GameObject iconObject =
                Instantiate(
                    iconPrefab,
                    iconContainer
                );

            StatusEffectIconUI iconUI =
                iconObject.GetComponent<
                    StatusEffectIconUI
                >();

            if (iconUI == null)
            {
                Debug.LogError(
                    "[StatusEffectUIManager] " +
                    "StatusEffectIconPrefab에 " +
                    "StatusEffectIconUI가 없습니다."
                );

                Destroy(iconObject);
                continue;
            }

            iconUI.Setup(
                effect,
                this,
                sprite
            );

            iconUIs.Add(
                iconUI
            );
        }
    }


    // =========================================================
    // Icon Select
    // =========================================================

    private Sprite GetIcon(
        ActiveStatusEffect effect)
    {
        if (
            effect == null ||
            effect.Data == null
        )
        {
            return null;
        }

        StatusEffectData data =
            effect.Data;


        // =====================================================
        // 1순위
        // 데이터 자체 아이콘
        // =====================================================

        if (data.icon != null)
        {
            return data.icon;
        }


        // =====================================================
        // 2순위
        // 특수 상태
        // =====================================================

        switch (data.effectType)
        {
            case StatusEffectType.Guard:
                return guardIcon;

            case StatusEffectType.Poison:
                return poisonIcon;

            case StatusEffectType.Stun:
                return stunIcon;
        }


        // =====================================================
        // 3순위
        // 이름 기준 특수 상태
        // =====================================================

        string buffName =
            data.buffName ?? string.Empty;


        // -----------------------------------------------------
        // 부식
        // -----------------------------------------------------

        if (
            buffName.Contains("부식")
        )
        {
            return corrosionIcon;
        }


        // -----------------------------------------------------
        // Hunger
        // -----------------------------------------------------

        if (
            buffName.Contains("배고픔")
        )
        {
            if (
                buffName.Contains("감소") ||
                buffName.Contains("악화") ||
                buffName.Contains("25") ||
                buffName.Contains("50")
            )
            {
                return hungerDownIcon;
            }

            return hungerIcon;
        }


        // -----------------------------------------------------
        // Frustration / Mental
        // -----------------------------------------------------

        if (
            buffName.Contains("좌절") ||
            buffName.Contains("정신력")
        )
        {
            if (
                buffName.Contains("감소") ||
                buffName.Contains("악화") ||
                buffName.Contains("25") ||
                buffName.Contains("50")
            )
            {
                return frustrationDownIcon;
            }

            return frustrationIcon;
        }


        // =====================================================
        // 4순위
        // 일반 Positive / Negative
        // =====================================================

        switch (data.tendency)
        {
            case StatusEffectTendency.Positive:
                return buffUpIcon;

            case StatusEffectTendency.Negative:
                return buffDownIcon;
        }


        return null;
    }


    // =========================================================
    // Clear
    // =========================================================

    private void ClearIcons()
    {
        /*
         * 아이콘을 Destroy하기 전에
         * 혹시 열려 있는 Tooltip을 한 번 더 닫는다.
         */
        HideTooltip();

        foreach (
            StatusEffectIconUI iconUI
            in iconUIs)
        {
            if (iconUI != null)
            {
                Destroy(
                    iconUI.gameObject
                );
            }
        }

        iconUIs.Clear();
    }


    // =========================================================
    // Tooltip
    // =========================================================

    public void ShowTooltip(
        ActiveStatusEffect effect)
    {
        if (
            effect == null ||
            effect.Data == null
        )
        {
            HideTooltip();
            return;
        }

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(true);
        }

        if (tooltipNameText != null)
        {
            tooltipNameText.text =
                effect.Data.buffName;
        }

        if (tooltipDescriptionText != null)
        {
            tooltipDescriptionText.text =
                effect.Data.description;
        }

        if (tooltipDurationText != null)
        {
            tooltipDurationText.text =
                effect.IsInfinite
                    ? "남은 턴: ∞"
                    : "남은 턴: " +
                      effect.RemainingDuration;
        }
    }


    public void HideTooltip()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }

        /*
         * 텍스트도 같이 비워둔다.
         * 다음 Tooltip이 열릴 때 이전 내용이
         * 잠깐 보이는 현상까지 방지.
         */

        if (tooltipNameText != null)
        {
            tooltipNameText.text =
                string.Empty;
        }

        if (tooltipDescriptionText != null)
        {
            tooltipDescriptionText.text =
                string.Empty;
        }

        if (tooltipDurationText != null)
        {
            tooltipDurationText.text =
                string.Empty;
        }
    }
}