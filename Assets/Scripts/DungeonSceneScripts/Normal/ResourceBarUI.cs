using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceBarUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private PlayerResourceManager resourceManager;

    [Header("Top Text")]
    [SerializeField] private TMP_Text currentTurnText;
    [SerializeField] private TMP_Text currentEnvironmentText;

    [Header("Value Text")]
    [SerializeField] private TMP_Text healthValueText;
    [SerializeField] private TMP_Text mentalValueText;
    [SerializeField] private TMP_Text hungerValueText;

    [Header("Fill Images")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image mentalFillImage;
    [SerializeField] private Image hungerFillImage;

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (dungeonManager == null || resourceManager == null)
            return;

        if (currentTurnText != null)
            currentTurnText.text = $"Current Turn : {dungeonManager.CurrentTurn}";

        if (currentEnvironmentText != null)
            currentEnvironmentText.text = $"Current Envir : {dungeonManager.CurrentEnvironment}";

        if (healthValueText != null)
            healthValueText.text = $"{resourceManager.CurrentHealth}/{resourceManager.MaxHealth}";

        if (mentalValueText != null)
            mentalValueText.text = $"{resourceManager.CurrentMental}/{resourceManager.MaxMental}";

        if (hungerValueText != null)
            hungerValueText.text = $"{resourceManager.CurrentHunger}/{resourceManager.MaxHunger}";

        if (healthFillImage != null)
            healthFillImage.fillAmount = GetRatio(resourceManager.CurrentHealth, resourceManager.MaxHealth);

        if (mentalFillImage != null)
            mentalFillImage.fillAmount = GetRatio(resourceManager.CurrentMental, resourceManager.MaxMental);

        if (hungerFillImage != null)
            hungerFillImage.fillAmount = GetRatio(resourceManager.CurrentHunger, resourceManager.MaxHunger);
    }

    private float GetRatio(int current, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Clamp01((float)current / max);
    }
}