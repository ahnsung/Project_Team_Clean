using System.Collections;
using UnityEngine;

public class BattleSpriteAnimator : MonoBehaviour
{
    [Header("Sprite Renderer")]
    public SpriteRenderer spriteRenderer;

    [Header("Battle Sprites")]
    public Sprite idleSprite;
    public Sprite attackSprite;
    public Sprite hitSprite;
    public Sprite deadSprite;

    [Header("Timing")]
    public float attackTime = 0.25f;
    public float hitTime = 0.25f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        SetIdle();
    }

    public void SetIdle()
    {
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    public void PlayAttack()
    {
        PlayTemporarySprite(attackSprite, attackTime);
    }

    public void PlayHit()
    {
        PlayTemporarySprite(hitSprite, hitTime);
    }

    public void PlayDead()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        if (spriteRenderer != null && deadSprite != null)
            spriteRenderer.sprite = deadSprite;
    }

    private void PlayTemporarySprite(Sprite targetSprite, float duration)
    {
        if (spriteRenderer == null)
            return;

        if (targetSprite == null)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(SpriteRoutine(targetSprite, duration));
    }

    private IEnumerator SpriteRoutine(Sprite targetSprite, float duration)
    {
        spriteRenderer.sprite = targetSprite;

        yield return new WaitForSeconds(duration);

        SetIdle();

        currentRoutine = null;
    }
}