using UnityEngine;

[CreateAssetMenu(
    fileName = "NewMonsterData",
    menuName = "Battle/Monster Data"
)]
public class BattleMonsterData : ScriptableObject
{
    [Header("Info")]
    public string monsterName = "Dog Monster";

    [Header("Prefab")]
    public GameObject monsterPrefab;

    [Header("Stats")]
    public int maxHP = 20;
    public int attackPower = 10;
    public int accuracy = 70;
    public int evasion = 30;

    [Header("Battle Cut-In")]
    [Tooltip("몬스터가 플레이어를 공격할 때 표시할 컷인 이미지")]
    public Sprite attackCutInSprite;

    [Tooltip("몬스터 공격 컷인의 그림자 이미지")]
    public Sprite attackShadowSprite;

    [Tooltip("몬스터가 플레이어에게 공격받았을 때 표시할 컷인 이미지")]
    public Sprite hitCutInSprite;

    [Tooltip("몬스터 피격 컷인의 그림자 이미지")]
    public Sprite hitShadowSprite;

    [Header("Cut-In Option")]
    [Tooltip(
        "체크하면 이 몬스터의 공격/피격 컷인과 Shadow만 좌우 반전됩니다.\n" +
        "Idle Sprite에는 영향을 주지 않습니다."
    )]
    public bool flipCutInX = false;

    [Min(0.1f)]
    [Tooltip(
        "몬스터가 공격할 때 나오는 컷인 이미지와 그림자의 크기입니다.\n" +
        "1 = 기본 크기"
    )]
    public float attackCutInScale = 1f;

    [Min(0.1f)]
    [Tooltip(
        "몬스터가 공격받았을 때 나오는 피격 컷인 이미지와 그림자의 크기입니다.\n" +
        "1 = 기본 크기"
    )]
    public float hitCutInScale = 1f;

    [Header("Battle Sound")]
    [Tooltip("몬스터 공격 시 재생할 효과음")]
    public AudioClip attackSound;
}