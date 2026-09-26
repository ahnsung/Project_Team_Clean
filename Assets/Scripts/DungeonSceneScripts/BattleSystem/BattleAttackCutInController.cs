using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleAttackCutInController : MonoBehaviour
{
    [Header("Root")]
    public GameObject effectRoot;

    [Header("Background")]
    public Transform battleBackground;
    public float backgroundZoomScale = 1.15f;
    public float playerAttackBackgroundRotateAngle = -1.8f;
    public float dogAttackBackgroundRotateAngle = 1.8f;

    [Header("Speed Lines")]
    public GameObject speedLineA;
    public GameObject speedLineB;
    public float speedLineInterval = 0.1f;

    [Header("Player Attack Objects")]
    public RectTransform playerAttackImage;
    public RectTransform playerAttackShadow;

    [Tooltip("공용 몬스터 피격 이미지")]
    public RectTransform dogHitImage;

    [Tooltip("공용 몬스터 피격 그림자")]
    public RectTransform dogHitShadow;

    public RectTransform swordEffect;

    [Header("Enemy Attack Objects")]
    [Tooltip("공용 몬스터 공격 이미지")]
    public RectTransform dogAttackImage;

    [Tooltip("공용 몬스터 공격 그림자")]
    public RectTransform dogAttackShadow;

    public RectTransform playerHitImage;
    public RectTransform playerHitShadow;

    [Header("Timing")]
    public float enterTime = 0.2f;
    public float hitMoveTime = 0.2f;
    public float holdTime = 0.8f;
    public float exitTime = 0.2f;

    [Header("Player Attack Position")]
    public Vector2 playerStart = new Vector2(-1200f, -60f);
    public Vector2 playerNear = new Vector2(-350f, -60f);
    public Vector2 playerHit = new Vector2(-350f, -60f);
    public Vector2 playerEnd = new Vector2(-1200f, -60f);

    public Vector2 dogHitStart = new Vector2(1200f, -60f);
    public Vector2 dogHitNear = new Vector2(350f, -60f);
    public Vector2 dogHitPos = new Vector2(350f, -60f);
    public Vector2 dogHitEnd = new Vector2(1200f, -60f);

    [Header("Enemy Attack Position")]
    public Vector2 dogStart = new Vector2(1200f, -60f);
    public Vector2 dogNear = new Vector2(350f, -60f);
    public Vector2 dogAttackPos = new Vector2(350f, -60f);
    public Vector2 dogEnd = new Vector2(1200f, -60f);

    public Vector2 playerHitStart = new Vector2(-1200f, -60f);
    public Vector2 playerHitNear = new Vector2(-350f, -60f);
    public Vector2 playerHitPos = new Vector2(-350f, -60f);
    public Vector2 playerHitEnd = new Vector2(-1200f, -60f);

    [Header("Offsets")]
    public Vector2 attackerShadowOffset = new Vector2(0f, -20f);
    public Vector2 targetShadowOffset = new Vector2(0f, -20f);
    public Vector2 swordEffectOffset = new Vector2(120f, 90f);

    [Header("Player Attack Shadow Move")]
    public Vector2 playerAttackShadowMove = new Vector2(-80f, 0f);
    public Vector2 dogHitShadowMove = new Vector2(80f, 0f);

    [Header("Enemy Attack Shadow Move")]
    public Vector2 dogAttackShadowMove = new Vector2(80f, 0f);
    public Vector2 playerHitShadowMove = new Vector2(-80f, 0f);

    [Header("Original Unit Restore")]
    public float originalUnitRestoreDistance = 0.5f;
    public float originalUnitRestoreTime = 0.2f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip playerAttackSound;

    [Tooltip(
        "MonsterData에 공격음이 없을 경우 사용하는 기본 몬스터 공격음"
    )]
    public AudioClip dogAttackSound;

    private Coroutine speedLineRoutine;

    private Vector3 originalBgScale;
    private Quaternion originalBgRotation;

    private bool isPlaying;

    // 몬스터 컷인 UI의 Inspector 기본 Scale 저장
    private Vector3 dogAttackBaseScale = Vector3.one;
    private Vector3 dogAttackShadowBaseScale = Vector3.one;

    private Vector3 dogHitBaseScale = Vector3.one;
    private Vector3 dogHitShadowBaseScale = Vector3.one;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (effectRoot == null)
            effectRoot = gameObject;

        SaveMonsterCutInBaseScales();

        effectRoot.SetActive(false);

        HideAllCutInObjects();
        HideSpeedLines();

        SaveBackgroundOriginal();
    }


    // =========================================================
    // Base Scale
    // =========================================================

    private void SaveMonsterCutInBaseScales()
    {
        if (dogAttackImage != null)
        {
            dogAttackBaseScale =
                dogAttackImage.localScale;
        }

        if (dogAttackShadow != null)
        {
            dogAttackShadowBaseScale =
                dogAttackShadow.localScale;
        }

        if (dogHitImage != null)
        {
            dogHitBaseScale =
                dogHitImage.localScale;
        }

        if (dogHitShadow != null)
        {
            dogHitShadowBaseScale =
                dogHitShadow.localScale;
        }
    }


    // =========================================================
    // Player Attack
    // =========================================================

    public IEnumerator PlayPlayerAttackCutIn(
        BattleUnit attacker,
        BattleUnit target)
    {
        // 공격받는 몬스터의 피격 컷인 적용
        ApplyMonsterHitVisual(target);

        yield return PlayCutInRoutine(
            attacker,
            target,

            playerAttackImage,
            playerAttackShadow,

            dogHitImage,
            dogHitShadow,

            swordEffect,

            playerStart,
            playerNear,
            playerHit,
            playerEnd,

            dogHitStart,
            dogHitNear,
            dogHitPos,
            dogHitEnd,

            playerAttackShadowMove,
            dogHitShadowMove,

            true,

            playerAttackSound,

            playerAttackBackgroundRotateAngle
        );
    }


    // =========================================================
    // Enemy Attack
    // =========================================================

    public IEnumerator PlayEnemyAttackCutIn(
        BattleUnit attacker,
        BattleUnit target)
    {
        // 공격 중인 몬스터의 공격 컷인 적용
        ApplyMonsterAttackVisual(attacker);

        AudioClip attackSound =
            GetMonsterAttackSound(attacker);

        yield return PlayCutInRoutine(
            attacker,
            target,

            dogAttackImage,
            dogAttackShadow,

            playerHitImage,
            playerHitShadow,

            null,

            dogStart,
            dogNear,
            dogAttackPos,
            dogEnd,

            playerHitStart,
            playerHitNear,
            playerHitPos,
            playerHitEnd,

            dogAttackShadowMove,
            playerHitShadowMove,

            false,

            attackSound,

            dogAttackBackgroundRotateAngle
        );
    }


    // =========================================================
    // Monster Attack Visual
    // =========================================================

    private void ApplyMonsterAttackVisual(
        BattleUnit monster)
    {
        if (monster == null)
        {
            Debug.LogWarning(
                "[BattleCutIn] 공격 몬스터가 null입니다."
            );

            return;
        }

        BattleMonsterData data =
            monster.monsterData;

        if (data == null)
        {
            Debug.LogWarning(
                "[BattleCutIn] " +
                monster.unitName +
                "의 MonsterData가 없습니다."
            );

            return;
        }

        Debug.Log(
            "[BattleCutIn] 공격 컷인 적용 | " +
            "Monster = " + monster.unitName +
            " | Data = " + data.name +
            " | Attack = " +
            GetSpriteName(data.attackCutInSprite) +
            " | Shadow = " +
            GetSpriteName(data.attackShadowSprite) +
            " | Scale = " +
            data.attackCutInScale
        );

        SetImageSprite(
            dogAttackImage,
            data.attackCutInSprite
        );

        SetImageSprite(
            dogAttackShadow,
            data.attackShadowSprite
        );

        ApplyMonsterCutInScale(
            dogAttackImage,
            dogAttackBaseScale,
            data.attackCutInScale,
            data.flipCutInX
        );

        ApplyMonsterCutInScale(
            dogAttackShadow,
            dogAttackShadowBaseScale,
            data.attackCutInScale,
            data.flipCutInX
        );
    }


    // =========================================================
    // Monster Hit Visual
    // =========================================================

    private void ApplyMonsterHitVisual(
        BattleUnit monster)
    {
        if (monster == null)
        {
            Debug.LogWarning(
                "[BattleCutIn] 피격 몬스터가 null입니다."
            );

            return;
        }

        BattleMonsterData data =
            monster.monsterData;

        if (data == null)
        {
            Debug.LogWarning(
                "[BattleCutIn] " +
                monster.unitName +
                "의 MonsterData가 없습니다."
            );

            return;
        }

        Debug.Log(
            "[BattleCutIn] 피격 컷인 적용 | " +
            "Monster = " + monster.unitName +
            " | Data = " + data.name +
            " | Hit = " +
            GetSpriteName(data.hitCutInSprite) +
            " | Shadow = " +
            GetSpriteName(data.hitShadowSprite) +
            " | Scale = " +
            data.hitCutInScale
        );

        SetImageSprite(
            dogHitImage,
            data.hitCutInSprite
        );

        SetImageSprite(
            dogHitShadow,
            data.hitShadowSprite
        );

        ApplyMonsterCutInScale(
            dogHitImage,
            dogHitBaseScale,
            data.hitCutInScale,
            data.flipCutInX
        );

        ApplyMonsterCutInScale(
            dogHitShadow,
            dogHitShadowBaseScale,
            data.hitCutInScale,
            data.flipCutInX
        );
    }


    // =========================================================
    // Cut-In Scale + Flip
    // =========================================================

    private void ApplyMonsterCutInScale(
        RectTransform rect,
        Vector3 baseScale,
        float scaleMultiplier,
        bool flipX)
    {
        if (rect == null)
            return;

        float safeScale =
            Mathf.Max(
                0.1f,
                scaleMultiplier
            );

        Vector3 resultScale =
            baseScale * safeScale;

        float absoluteX =
            Mathf.Abs(resultScale.x);

        resultScale.x =
            flipX
                ? -absoluteX
                : absoluteX;

        rect.localScale =
            resultScale;
    }


    // =========================================================
    // Sprite Name
    // =========================================================

    private string GetSpriteName(
        Sprite sprite)
    {
        return sprite != null
            ? sprite.name
            : "NULL";
    }


    // =========================================================
    // Monster Sound
    // =========================================================

    private AudioClip GetMonsterAttackSound(
        BattleUnit monster)
    {
        if (
            monster != null &&
            monster.monsterData != null &&
            monster.monsterData.attackSound != null
        )
        {
            return monster.monsterData.attackSound;
        }

        return dogAttackSound;
    }


    // =========================================================
    // Image
    // =========================================================

    private void SetImageSprite(
        RectTransform rect,
        Sprite sprite)
    {
        if (rect == null)
        {
            Debug.LogWarning(
                "[BattleCutIn] RectTransform이 null입니다."
            );

            return;
        }

        if (sprite == null)
        {
            Debug.LogWarning(
                "[BattleCutIn] " +
                rect.name +
                "에 적용할 Sprite가 null입니다."
            );

            return;
        }

        Image image =
            rect.GetComponent<Image>();

        if (image == null)
        {
            image =
                rect.GetComponentInChildren<Image>(
                    true
                );
        }

        if (image == null)
        {
            Debug.LogError(
                "[BattleCutIn] " +
                rect.name +
                "에서 Image 컴포넌트를 찾지 못했습니다."
            );

            return;
        }

        image.sprite =
            sprite;

        // ==========================================
        // Sprite 교체 시 자동 Set Native Size
        // ==========================================
        image.SetNativeSize();

        // Rect 크기가 변경되더라도 이미지 비율 유지
        image.preserveAspect = true;

        Debug.Log(
            "[BattleCutIn] Image 교체 완료 | " +
            rect.name +
            " → " +
            sprite.name +
            " | Native Size = " +
            rect.sizeDelta
        );
    }


    // =========================================================
    // Main Cut-In
    // =========================================================

    private IEnumerator PlayCutInRoutine(
        BattleUnit attacker,
        BattleUnit target,

        RectTransform attackerImage,
        RectTransform attackerShadow,

        RectTransform targetImage,
        RectTransform targetShadow,

        RectTransform effectImage,

        Vector2 attackerStart,
        Vector2 attackerNear,
        Vector2 attackerHit,
        Vector2 attackerEnd,

        Vector2 targetStart,
        Vector2 targetNear,
        Vector2 targetHit,
        Vector2 targetEnd,

        Vector2 attackerShadowMove,
        Vector2 targetShadowMove,

        bool useSwordEffect,

        AudioClip attackSound,

        float rotateAngle)
    {
        if (isPlaying)
            yield break;

        if (
            attacker == null ||
            target == null
        )
        {
            yield break;
        }

        isPlaying = true;

        SaveBackgroundOriginal();

        Vector3 attackerOriginalPos =
            attacker.transform.position;

        Vector3 targetOriginalPos =
            target.transform.position;

        effectRoot.SetActive(true);

        HideAllCutInObjects();

        SetObjectActive(
            attackerImage,
            true
        );

        SetObjectActive(
            attackerShadow,
            true
        );

        SetObjectActive(
            targetImage,
            true
        );

        SetObjectActive(
            targetShadow,
            true
        );

        if (useSwordEffect)
        {
            SetObjectActive(
                effectImage,
                true
            );
        }

        // ==========================================
        // 중요:
        // 공격하는 쪽을 항상 피격자보다 앞으로 표시
        //
        // 플레이어 공격:
        // attackerImage = playerAttackImage
        //
        // 몬스터 공격:
        // attackerImage = dogAttackImage
        //
        // 따라서 공격자가 누구든 이 코드 하나로
        // 공격자가 항상 화면 앞에 위치한다.
        // ==========================================
        ForceAttackerToFront(
            attackerImage,
            attackerShadow,
            effectImage,
            useSwordEffect
        );

        HideOriginalUnit(
            attacker,
            true
        );

        HideOriginalUnit(
            target,
            true
        );

        ApplyBackgroundEffect(
            rotateAngle
        );

        SetPairPosition(
            attackerImage,
            attackerShadow,

            targetImage,
            targetShadow,

            effectImage,

            attackerStart,
            targetStart,

            useSwordEffect
        );

        speedLineRoutine =
            StartCoroutine(
                SpeedLineLoop()
            );

        yield return MovePair(
            attackerImage,
            attackerShadow,

            targetImage,
            targetShadow,

            effectImage,

            attackerStart,
            attackerNear,

            targetStart,
            targetNear,

            useSwordEffect,

            enterTime
        );

        if (
            audioSource != null &&
            attackSound != null
        )
        {
            audioSource.PlayOneShot(
                attackSound
            );
        }

        yield return MovePair(
            attackerImage,
            attackerShadow,

            targetImage,
            targetShadow,

            effectImage,

            attackerNear,
            attackerHit,

            targetNear,
            targetHit,

            useSwordEffect,

            hitMoveTime
        );

        yield return HoldImpact(
            attackerImage,
            attackerShadow,

            targetImage,
            targetShadow,

            effectImage,

            attackerHit,
            targetHit,

            attackerShadowMove,
            targetShadowMove,

            useSwordEffect
        );

        yield return MovePair(
            attackerImage,
            attackerShadow,

            targetImage,
            targetShadow,

            effectImage,

            attackerHit,
            attackerEnd,

            targetHit,
            targetEnd,

            useSwordEffect,

            exitTime
        );

        StopSpeedLines();

        ClearBackgroundEffect();

        HideAllCutInObjects();

        effectRoot.SetActive(false);

        yield return RestoreOriginalUnits(
            attacker,
            target,

            attackerOriginalPos,
            targetOriginalPos,

            attackerStart,
            targetStart
        );

        isPlaying = false;
    }


    // =========================================================
    // Layer Order
    // =========================================================

    private void ForceAttackerToFront(
       RectTransform attackerImage,
       RectTransform attackerShadow,
       RectTransform effectImage,
       bool useEffect)
    {
        if (attackerImage == null)
            return;

        // ==========================================
        // 중요
        //
        // 현재 Hierarchy:
        //
        // BattleEffectRoot
        // ├─ DogMonsetSet
        // │  ├─ DogAttackShadow
        // │  └─ CutInDogAttack
        // │
        // └─ Playerset
        //    ├─ PlayerAttackShadow
        //    ├─ SwordEffect
        //    └─ CutInPlayerAttack
        //
        // 개별 Image가 아니라
        // 공격자 Image의 부모 Set 전체를 앞으로 보낸다.
        // ==========================================

        Transform attackerSet =
            attackerImage.parent;

        if (attackerSet != null)
        {
            attackerSet.SetAsLastSibling();

            Debug.Log(
                "[BattleCutIn] 공격자 Set을 최상단으로 이동 | " +
                attackerSet.name
            );
        }

        // 같은 Set 내부에서는
        // 공격자 그림자 → 공격자 본체 순서
        if (attackerShadow != null)
        {
            attackerShadow.SetAsLastSibling();
        }

        if (attackerImage != null)
        {
            attackerImage.SetAsLastSibling();
        }

        // 플레이어 공격 시 검 이펙트는 가장 앞
        if (
            useEffect &&
            effectImage != null
        )
        {
            effectImage.SetAsLastSibling();
        }
    }

    // =========================================================
    // Movement
    // =========================================================

    private IEnumerator MovePair(
        RectTransform attackerImage,
        RectTransform attackerShadow,

        RectTransform targetImage,
        RectTransform targetShadow,

        RectTransform effectImage,

        Vector2 attackerFrom,
        Vector2 attackerTo,

        Vector2 targetFrom,
        Vector2 targetTo,

        bool useSwordEffect,

        float duration)
    {
        if (duration <= 0f)
        {
            SetPairPosition(
                attackerImage,
                attackerShadow,

                targetImage,
                targetShadow,

                effectImage,

                attackerTo,
                targetTo,

                useSwordEffect
            );

            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float ratio =
                Mathf.Clamp01(
                    t / duration
                );

            Vector2 attackerPos =
                Vector2.Lerp(
                    attackerFrom,
                    attackerTo,
                    ratio
                );

            Vector2 targetPos =
                Vector2.Lerp(
                    targetFrom,
                    targetTo,
                    ratio
                );

            SetPairPosition(
                attackerImage,
                attackerShadow,

                targetImage,
                targetShadow,

                effectImage,

                attackerPos,
                targetPos,

                useSwordEffect
            );

            yield return null;
        }

        SetPairPosition(
            attackerImage,
            attackerShadow,

            targetImage,
            targetShadow,

            effectImage,

            attackerTo,
            targetTo,

            useSwordEffect
        );
    }


    // =========================================================
    // Impact
    // =========================================================

    private IEnumerator HoldImpact(
        RectTransform attackerImage,
        RectTransform attackerShadow,

        RectTransform targetImage,
        RectTransform targetShadow,

        RectTransform effectImage,

        Vector2 attackerBase,
        Vector2 targetBase,

        Vector2 attackerShadowMove,
        Vector2 targetShadowMove,

        bool useSwordEffect)
    {
        if (holdTime <= 0f)
            yield break;

        float t = 0f;

        while (t < holdTime)
        {
            t += Time.deltaTime;

            float ratio =
                Mathf.Clamp01(
                    t / holdTime
                );

            SetAnchored(
                attackerImage,
                attackerBase
            );

            SetAnchored(
                targetImage,
                targetBase
            );

            SetAnchored(
                attackerShadow,
                attackerBase +
                attackerShadowOffset +
                attackerShadowMove *
                ratio
            );

            SetAnchored(
                targetShadow,
                targetBase +
                targetShadowOffset +
                targetShadowMove *
                ratio
            );

            if (useSwordEffect)
            {
                SetAnchored(
                    effectImage,
                    attackerBase +
                    swordEffectOffset
                );
            }

            yield return null;
        }
    }


    // =========================================================
    // Pair Position
    // =========================================================

    private void SetPairPosition(
        RectTransform attackerImage,
        RectTransform attackerShadow,

        RectTransform targetImage,
        RectTransform targetShadow,

        RectTransform effectImage,

        Vector2 attackerPos,
        Vector2 targetPos,

        bool useSwordEffect)
    {
        SetAnchored(
            attackerImage,
            attackerPos
        );

        SetAnchored(
            attackerShadow,
            attackerPos +
            attackerShadowOffset
        );

        SetAnchored(
            targetImage,
            targetPos
        );

        SetAnchored(
            targetShadow,
            targetPos +
            targetShadowOffset
        );

        if (useSwordEffect)
        {
            SetAnchored(
                effectImage,
                attackerPos +
                swordEffectOffset
            );
        }
    }


    // =========================================================
    // Restore
    // =========================================================

    private IEnumerator RestoreOriginalUnits(
        BattleUnit attacker,
        BattleUnit target,

        Vector3 attackerOriginalPos,
        Vector3 targetOriginalPos,

        Vector2 attackerStart,
        Vector2 targetStart)
    {
        Vector3 attackerRestoreDir =
            attackerStart.x < 0f
                ? Vector3.right
                : Vector3.left;

        Vector3 targetRestoreDir =
            targetStart.x < 0f
                ? Vector3.right
                : Vector3.left;

        Vector3 attackerRestoreStart =
            attackerOriginalPos +
            attackerRestoreDir *
            originalUnitRestoreDistance;

        Vector3 targetRestoreStart =
            targetOriginalPos +
            targetRestoreDir *
            originalUnitRestoreDistance;

        attacker.transform.position =
            attackerRestoreStart;

        target.transform.position =
            targetRestoreStart;

        HideOriginalUnit(
            attacker,
            false
        );

        HideOriginalUnit(
            target,
            false
        );

        float t = 0f;

        while (
            t <
            originalUnitRestoreTime
        )
        {
            t += Time.deltaTime;

            float ratio =
                originalUnitRestoreTime <= 0f
                    ? 1f
                    : Mathf.Clamp01(
                        t /
                        originalUnitRestoreTime
                    );

            attacker.transform.position =
                Vector3.Lerp(
                    attackerRestoreStart,
                    attackerOriginalPos,
                    ratio
                );

            target.transform.position =
                Vector3.Lerp(
                    targetRestoreStart,
                    targetOriginalPos,
                    ratio
                );

            yield return null;
        }

        attacker.transform.position =
            attackerOriginalPos;

        target.transform.position =
            targetOriginalPos;
    }


    // =========================================================
    // Visibility
    // =========================================================

    private void HideAllCutInObjects()
    {
        SetObjectActive(
            playerAttackImage,
            false
        );

        SetObjectActive(
            playerAttackShadow,
            false
        );

        SetObjectActive(
            dogHitImage,
            false
        );

        SetObjectActive(
            dogHitShadow,
            false
        );

        SetObjectActive(
            swordEffect,
            false
        );

        SetObjectActive(
            dogAttackImage,
            false
        );

        SetObjectActive(
            dogAttackShadow,
            false
        );

        SetObjectActive(
            playerHitImage,
            false
        );

        SetObjectActive(
            playerHitShadow,
            false
        );
    }


    private void SetObjectActive(
        RectTransform rect,
        bool value)
    {
        if (rect == null)
            return;

        rect.gameObject.SetActive(
            value
        );
    }


    // =========================================================
    // Background
    // =========================================================

    private void SaveBackgroundOriginal()
    {
        if (battleBackground == null)
            return;

        originalBgScale =
            battleBackground.localScale;

        originalBgRotation =
            battleBackground.localRotation;
    }


    private void ApplyBackgroundEffect(
        float rotateAngle)
    {
        if (battleBackground == null)
            return;

        battleBackground.localScale =
            originalBgScale *
            backgroundZoomScale;

        battleBackground.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                rotateAngle
            );
    }


    private void ClearBackgroundEffect()
    {
        if (battleBackground == null)
            return;

        battleBackground.localScale =
            originalBgScale;

        battleBackground.localRotation =
            originalBgRotation;
    }


    // =========================================================
    // Rect
    // =========================================================

    private void SetAnchored(
        RectTransform rect,
        Vector2 pos)
    {
        if (rect == null)
            return;

        rect.anchoredPosition =
            pos;
    }


    // =========================================================
    // Speed Line
    // =========================================================

    private IEnumerator SpeedLineLoop()
    {
        while (true)
        {
            if (speedLineA != null)
                speedLineA.SetActive(true);

            if (speedLineB != null)
                speedLineB.SetActive(false);

            yield return new WaitForSeconds(
                speedLineInterval
            );

            if (speedLineA != null)
                speedLineA.SetActive(false);

            if (speedLineB != null)
                speedLineB.SetActive(true);

            yield return new WaitForSeconds(
                speedLineInterval
            );
        }
    }


    private void StopSpeedLines()
    {
        if (speedLineRoutine != null)
        {
            StopCoroutine(
                speedLineRoutine
            );
        }

        speedLineRoutine = null;

        HideSpeedLines();
    }


    private void HideSpeedLines()
    {
        if (speedLineA != null)
            speedLineA.SetActive(false);

        if (speedLineB != null)
            speedLineB.SetActive(false);
    }


    // =========================================================
    // Original Unit
    // =========================================================

    private void HideOriginalUnit(
        BattleUnit unit,
        bool hide)
    {
        if (unit == null)
            return;

        SpriteRenderer[] renderers =
            unit.GetComponentsInChildren<
                SpriteRenderer
            >();

        foreach (
            SpriteRenderer sr
            in renderers)
        {
            sr.enabled =
                !hide;
        }
    }
}