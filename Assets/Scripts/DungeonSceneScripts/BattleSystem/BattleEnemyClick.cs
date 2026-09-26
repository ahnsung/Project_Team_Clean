using UnityEngine;

public class BattleEnemyClick : MonoBehaviour
{
    public BattleUnit enemyUnit;
    public BattleManager battleManager;

    private void Awake()
    {
        if (enemyUnit == null)
            enemyUnit = GetComponent<BattleUnit>();
    }

    private void OnMouseEnter()
    {
        if (battleManager != null)
            battleManager.HoverEnemy(enemyUnit);
    }

    private void OnMouseExit()
    {
        if (battleManager != null)
            battleManager.ExitHoverEnemy(enemyUnit);
    }

    private void OnMouseDown()
    {
        if (battleManager != null)
            battleManager.ClickEnemy(enemyUnit);
    }
}