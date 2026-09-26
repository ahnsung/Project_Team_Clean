using UnityEngine;

public class ItemStatSelectionUI : MonoBehaviour
{
    public static ItemStatSelectionUI Instance
    {
        get;
        private set;
    }

    [Header("Root")]
    [SerializeField]
    private GameObject root;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (root == null)
            root = gameObject;

        root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void SelectSTR()
    {
        Apply("STR");
    }

    public void SelectDEX()
    {
        Apply("DEX");
    }

    public void SelectCON()
    {
        Apply("CON");
    }

    public void SelectINT()
    {
        Apply("INT");
    }

    public void Cancel()
    {
        Hide();
    }

    private void Apply(
        string statName)
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance
            .CompleteModificationDeviceUse(
                statName
            );

        Hide();
    }
}
