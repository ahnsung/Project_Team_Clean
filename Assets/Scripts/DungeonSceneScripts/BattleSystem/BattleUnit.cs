using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [Header("Info")]
    public string unitName;

    [Header("Monster Data")]
    [Tooltip("몬스터 유닛일 경우 자신을 생성한 BattleMonsterData")]
    public BattleMonsterData monsterData;

    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP = 10;
    public int attackPower = 2;
    public int accuracy = 90;
    public int evasion = 10;

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public Image hpBarFill;
    public GameObject targetArrow;

    public bool IsDead => currentHP <= 0;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (targetArrow != null)
        {
            targetArrow.SetActive(false);
        }
    }

    public void Setup(
        string name,
        int hp,
        int attack,
        int acc,
        int eva)
    {
        unitName = name;
        maxHP = hp;
        currentHP = hp;
        attackPower = attack;
        accuracy = acc;
        evasion = eva;

        UpdateHPUI();

        if (targetArrow != null)
        {
            targetArrow.SetActive(false);
        }
    }

    public void SetupMonster(
        BattleMonsterData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "[BattleUnit] BattleMonsterData가 null입니다."
            );

            return;
        }

        monsterData = data;

        Setup(
            data.monsterName,
            data.maxHP,
            data.attackPower,
            data.accuracy,
            data.evasion
        );
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        currentHP -= damage;
        currentHP = Mathf.Max(
            currentHP,
            0
        );

        PlayHitAnimation();
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead)
        {
            return;
        }

        currentHP += amount;
        currentHP = Mathf.Min(
            currentHP,
            maxHP
        );

        UpdateHPUI();
    }

    public void PlayAttackAnimation()
    {
        if (animator == null)
        {
            return;
        }

        /*
         * 현재 몬스터 Animator는 Idle 전용인 경우가 많아서
         * Attack 파라미터가 없으면 Trigger를 보내지 않는다.
         */
        if (HasAnimatorParameter("Attack"))
        {
            animator.SetTrigger("Attack");
        }
    }

    public void PlayHitAnimation()
    {
        if (animator == null)
        {
            return;
        }

        /*
         * PlagueRat처럼 Idle Animator만 사용하는 몬스터는
         * Hit Trigger가 없으므로 오류를 방지한다.
         */
        if (HasAnimatorParameter("Hit"))
        {
            animator.SetTrigger("Hit");
        }
    }

    public void SetArrow(bool value)
    {
        if (targetArrow != null)
        {
            targetArrow.SetActive(value);
        }
    }

    private void UpdateHPUI()
    {
        if (hpBarFill == null)
        {
            return;
        }

        if (maxHP <= 0)
        {
            hpBarFill.fillAmount = 0f;
            return;
        }

        hpBarFill.fillAmount =
            (float)currentHP / maxHP;
    }

    private void Die()
    {
        if (
            animator != null &&
            HasAnimatorParameter("Die")
        )
        {
            animator.SetTrigger("Die");
        }

        gameObject.SetActive(false);
    }

    private bool HasAnimatorParameter(
        string parameterName)
    {
        if (
            animator == null ||
            animator.runtimeAnimatorController == null
        )
        {
            return false;
        }

        AnimatorControllerParameter[] parameters =
            animator.parameters;

        foreach (
            AnimatorControllerParameter parameter
            in parameters
        )
        {
            if (
                parameter.name ==
                parameterName
            )
            {
                return true;
            }
        }

        return false;
    }
}