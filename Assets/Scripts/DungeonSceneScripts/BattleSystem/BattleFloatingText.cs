using TMPro;
using UnityEngine;

public class BattleFloatingText : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    private float lifetime = 0.8f;

    [SerializeField]
    private float moveSpeed = 70f;

    [Header("Fade")]
    [SerializeField]
    private float fadeStartTime = 0.35f;

    private TextMeshProUGUI textUI;
    private CanvasGroup canvasGroup;

    private float elapsedTime;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        textUI =
            GetComponent<TextMeshProUGUI>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }
    }


    private void OnEnable()
    {
        elapsedTime = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }


    private void Update()
    {
        elapsedTime +=
            Time.deltaTime;


        // =====================================================
        // 위로 이동
        // =====================================================

        transform.position +=
            Vector3.up *
            moveSpeed *
            Time.deltaTime;


        // =====================================================
        // Fade Out
        // =====================================================

        if (
            canvasGroup != null &&
            elapsedTime >= fadeStartTime
        )
        {
            float fadeDuration =
                Mathf.Max(
                    0.01f,
                    lifetime - fadeStartTime
                );

            float fadeProgress =
                (elapsedTime - fadeStartTime) /
                fadeDuration;

            canvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    fadeProgress
                );
        }


        // BattleManager에서도 0.8초 뒤 Destroy하지만,
        // 단독 사용해도 안전하도록 여기도 처리
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}