using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 화면 전체를 검게 덮는 FadePanel의 Image 알파값을 조절해서
// FadeOut / FadeIn 연출을 만드는 스크립트
public class FadeController : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // FadePanel에 붙어 있는 Image
    [SerializeField] private float duration = 0.3f; // 페이드 지속 시간

    private void Awake()
    {
        // 인스펙터에서 안 넣었으면 자기 자신에서 Image를 찾아옴
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();

        // 게임 시작 시 화면은 보이도록 알파 0으로 시작
        SetAlpha(0f);
    }

    // 화면을 서서히 검게 만드는 코루틴
    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f);
    }

    // 화면을 서서히 다시 보이게 만드는 코루틴
    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    // from 알파에서 to 알파까지 부드럽게 바꾸는 공통 코루틴
    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            // 현재 시간 비율에 따라 알파값 계산
            float value = Mathf.Lerp(from, to, t / duration);
            SetAlpha(value);

            yield return null;
        }

        // 마지막 값 정확히 맞춰주기
        SetAlpha(to);
    }

    // 실제 Image의 알파값을 바꾸는 함수
    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}