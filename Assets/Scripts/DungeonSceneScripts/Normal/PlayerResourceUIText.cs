using TMPro;
using UnityEngine;

public class PlayerResourceUIText : MonoBehaviour
{
    [Header("Resource Texts")]
    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI mentalText;

    [SerializeField]
    private TextMeshProUGUI hungerText;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        PlayerResourceManager resource =
            PlayerResourceManager.Instance;

        if (resource == null)
            return;

        if (healthText != null)
        {
            healthText.text =
                resource.CurrentHealth +
                " / " +
                resource.MaxHealth;
        }

        if (mentalText != null)
        {
            mentalText.text =
                resource.CurrentMental +
                " / " +
                resource.MaxMental;
        }

        if (hungerText != null)
        {
            hungerText.text =
                resource.CurrentHunger +
                " / " +
                resource.MaxHunger;
        }
    }
}