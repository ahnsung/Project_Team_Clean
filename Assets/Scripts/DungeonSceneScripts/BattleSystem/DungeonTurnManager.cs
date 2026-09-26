using TMPro;
using UnityEngine;

public class DungeonTurnManager : MonoBehaviour
{
    public static DungeonTurnManager Instance;

    [Header("Turn")]
    public int currentTurn = 1;

    [Header("UI")]
    public TextMeshProUGUI currentTurnText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        RefreshUI();
    }

    public void AddTurn(int amount = 1)
    {
        currentTurn += amount;
        RefreshUI();
        Debug.Log("Current Turn : " + currentTurn);
    }

    public void RefreshUI()
    {
        if (currentTurnText != null)
            currentTurnText.text = "Current Turn : " + currentTurn;
    }
}