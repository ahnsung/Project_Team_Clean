using System;

[Serializable]
public class EquipmentStatModifier
{
    public int str;
    public int dex;
    public int con;
    public int intelligence;

    public int attackPower;
    public int accuracyBonus;

    public void Clear()
    {
        str = 0;
        dex = 0;
        con = 0;
        intelligence = 0;

        attackPower = 0;
        accuracyBonus = 0;
    }

    public void Add(EquipmentStatModifier other)
    {
        if (other == null)
            return;

        str += other.str;
        dex += other.dex;
        con += other.con;
        intelligence += other.intelligence;

        attackPower += other.attackPower;
        accuracyBonus += other.accuracyBonus;
    }
}