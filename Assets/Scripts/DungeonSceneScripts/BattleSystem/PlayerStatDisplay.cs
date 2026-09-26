using TMPro;
using UnityEngine;

public class PlayerStatDisplay : MonoBehaviour
{
    public TextMeshProUGUI strText;
    public TextMeshProUGUI dexText;
    public TextMeshProUGUI conText;
    public TextMeshProUGUI intText;

    private void Update()
    {
        if (PlayerStats.Instance == null)
            return;

        strText.text = "STR : " + PlayerStats.Instance.STR;
        dexText.text = "DEX : " + PlayerStats.Instance.DEX;
        conText.text = "CON : " + PlayerStats.Instance.CON;
        intText.text = "INT : " + PlayerStats.Instance.INT;
    }

    public void IncreaseSTR()
    {
        if (PlayerStats.Instance == null)
            return;

        PlayerStats.Instance.STR++;
    }

    public void DecreaseSTR()
    {
        if (PlayerStats.Instance == null)
            return;

        PlayerStats.Instance.STR--;

        if (PlayerStats.Instance.STR < 0)
            PlayerStats.Instance.STR = 0;
    }
}