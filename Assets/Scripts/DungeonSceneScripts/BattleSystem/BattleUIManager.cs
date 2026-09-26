using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    [Header("Always On")]
    public GameObject battleRoot;
    public GameObject middleMenuPanel;

    [Header("Battle Only")]
    public GameObject encounterPanel;
    public GameObject battleMenuPanel;
    public GameObject actionMenuPanel;

    private void Awake()
    {
        KeepAlwaysUIOn();
        HideBattleUI();
    }

    private void KeepAlwaysUIOn()
    {
        if (battleRoot != null)
            battleRoot.SetActive(true);

        if (middleMenuPanel != null)
            middleMenuPanel.SetActive(true);
    }

    public void ShowBattleUI()
    {
        KeepAlwaysUIOn();

        if (encounterPanel != null)
            encounterPanel.SetActive(true);

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(true);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(false);
    }

    public void HideBattleUI()
    {
        KeepAlwaysUIOn();

        if (encounterPanel != null)
            encounterPanel.SetActive(false);

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(false);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(false);
    }

    public void ShowMainBattleMenu()
    {
        KeepAlwaysUIOn();

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(true);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(false);
    }

    public void ShowActionMenu()
    {
        KeepAlwaysUIOn();

        if (battleMenuPanel != null)
            battleMenuPanel.SetActive(false);

        if (actionMenuPanel != null)
            actionMenuPanel.SetActive(true);
    }
}