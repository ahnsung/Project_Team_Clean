using TMPro;
using UnityEngine;

public class StatTestUI : MonoBehaviour
{
    public TMP_Text statText;

    private void Update()
    {
        RefreshText();
    }

    private void RefreshText()
    {
        if (statText == null || PlayerStats.Instance == null)
            return;

        PlayerStats s = PlayerStats.Instance;

        statText.text =
            $"STR : {s.STR}\n" +
            $"DEX : {s.DEX}\n" +
            $"CON : {s.CON}\n" +
            $"INT : {s.INT}\n\n" +
            $"공격력 : {s.GetFinalAttackDamage()}\n" +
            $"최대 체력 : {s.MaxHealth}\n" +
            $"최대 배고픔 : {s.MaxHunger}\n" +
            $"최대 정신력 : {s.MaxMental}\n" +
            $"인벤토리 칸 : {s.InventoryCapacity}\n" +
            $"도주 성공률 : {s.GetRunSuccessPercent()}%";
    }

    public void AddSTR() => PlayerStats.Instance.AddSTR();
    public void SubSTR() => PlayerStats.Instance.SubSTR();

    public void AddDEX() => PlayerStats.Instance.AddDEX();
    public void SubDEX() => PlayerStats.Instance.SubDEX();

    public void AddCON() => PlayerStats.Instance.AddCON();
    public void SubCON() => PlayerStats.Instance.SubCON();

    public void AddINT() => PlayerStats.Instance.AddINT();
    public void SubINT() => PlayerStats.Instance.SubINT();
}