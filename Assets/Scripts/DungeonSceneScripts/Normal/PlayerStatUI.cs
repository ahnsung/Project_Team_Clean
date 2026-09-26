using TMPro;
using UnityEngine;

public class PlayerStatUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private PlayerStats playerStats;

    [Header("Stat Text")]
    [SerializeField]
    private TextMeshProUGUI strText;

    [SerializeField]
    private TextMeshProUGUI dexText;

    [SerializeField]
    private TextMeshProUGUI conText;

    [SerializeField]
    private TextMeshProUGUI intText;


    private void Start()
    {
        ResolveReferences();
        Refresh();
    }


    private void Update()
    {
        Refresh();
    }


    private void ResolveReferences()
    {
        if (playerStats == null)
        {
            playerStats =
                PlayerStats.Instance;
        }

        if (playerStats == null)
        {
            playerStats =
                FindFirstObjectByType<PlayerStats>();
        }
    }


    public void Refresh()
    {
        if (playerStats == null)
        {
            ResolveReferences();

            if (playerStats == null)
                return;
        }


        if (strText != null)
        {
            strText.text =
                "STR : " +
                playerStats.TotalSTR;
        }


        if (dexText != null)
        {
            dexText.text =
                "DEX : " +
                playerStats.TotalDEX;
        }


        if (conText != null)
        {
            conText.text =
                "CON : " +
                playerStats.TotalCON;
        }


        if (intText != null)
        {
            intText.text =
                "INT : " +
                playerStats.TotalINT;
        }
    }
}