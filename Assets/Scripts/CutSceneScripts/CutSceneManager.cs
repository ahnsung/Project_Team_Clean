using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public Image imageA;
    public Image imageB;
    public Image fadeImage;

    public Sprite[] cutsceneSprites;

    public float fadeDuration = 1f;
    public float endFadeDuration = 1f;

    int currentIndex = 0;
    bool isFading = false;

    void Start()
    {
        // New Game으로 들어온 컷씬인지 확인
        bool isNewGameCutscene =
            PlayerPrefs.GetInt("NewGameCutscene", 0) == 1;

        // 새 게임 진행 중이 아니고 이미 컷씬을 본 상태라면
        // 기존처럼 MenuScene으로 바로 이동
        if (!isNewGameCutscene &&
            SaveManager.Instance != null &&
            SaveManager.Instance.HasPlayedCutscene())
        {
            SceneManager.LoadScene("MenuScene");
            return;
        }

        if (cutsceneSprites == null || cutsceneSprites.Length == 0)
        {
            Debug.LogError("[CutsceneManager] cutsceneSprites가 비어 있습니다.");
            return;
        }

        imageA.sprite = cutsceneSprites[0];
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);
        SetAlpha(fadeImage, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isFading)
        {
            NextCutscene();
        }
    }

    void NextCutscene()
    {
        if (currentIndex + 1 >= cutsceneSprites.Length)
        {
            StartCoroutine(EndCutscene());
            return;
        }

        StartCoroutine(CrossFade());
    }

    IEnumerator CrossFade()
    {
        isFading = true;

        currentIndex++;
        imageB.sprite = cutsceneSprites[currentIndex];

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha =
                fadeDuration <= 0f
                    ? 1f
                    : t / fadeDuration;

            SetAlpha(imageA, 1f - alpha);
            SetAlpha(imageB, alpha);

            yield return null;
        }

        Image temp = imageA;
        imageA = imageB;
        imageB = temp;

        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);

        isFading = false;
    }

    IEnumerator EndCutscene()
    {
        isFading = true;

        float t = 0f;

        while (t < endFadeDuration)
        {
            t += Time.deltaTime;
            float alpha =
                endFadeDuration <= 0f
                    ? 1f
                    : t / endFadeDuration;

            SetAlpha(fadeImage, alpha);
            yield return null;
        }

        SetAlpha(fadeImage, 1f);

        yield return new WaitForSeconds(1f);

        // New Game 컷씬 종료:
        // 여기서는 세이브를 생성하지 않는다.
        // 캐릭터 선택 후 CharacterSelectionManager에서 생성한다.
        if (PlayerPrefs.GetInt("NewGameCutscene", 0) == 1)
        {
            PlayerPrefs.DeleteKey("NewGameCutscene");

            // 컷씬을 완료했다는 사실만 기록
            PlayerPrefs.SetInt("CutscenePlayed", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene("CharacterSelectScene");
            yield break;
        }

        // 일반 StartScene 진입 시 기존 동작
        PlayerPrefs.SetInt("CutscenePlayed", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MenuScene");
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null)
            return;

        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
