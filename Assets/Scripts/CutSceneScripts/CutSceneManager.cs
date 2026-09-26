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
        // 새 게임 진행 중인지 확인
        bool goToCharacterSelectAfterCutscene =
            PlayerPrefs.GetInt("AfterCutsceneGoToCharacterSelect", 0) == 1;

        // 새 게임 진행 중이 아니고, 컷씬을 이미 봤으면 메뉴로 바로 이동
        if (!goToCharacterSelectAfterCutscene &&
            SaveManager.Instance != null &&
            SaveManager.Instance.HasPlayedCutscene())
        {
            SceneManager.LoadScene("MenuScene");
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
            float alpha = t / fadeDuration;

            SetAlpha(imageA, 1f - alpha);
            SetAlpha(imageB, alpha);

            yield return null;
        }

        Image temp = imageA;
        imageA = imageB;
        imageB = temp;

        SetAlpha(imageB, 0f);

        isFading = false;
    }

    IEnumerator EndCutscene()
    {
        isFading = true;

        float t = 0f;

        // 화면 서서히 어둡게
        while (t < endFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / endFadeDuration;
            SetAlpha(fadeImage, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // 컷씬 봤다고 저장
        PlayerPrefs.SetInt("CutscenePlayed", 1);

        // 새 게임 플래그가 있으면 캐릭터 선택 씬으로 바로 이동
        if (PlayerPrefs.GetInt("AfterCutsceneGoToCharacterSelect", 0) == 1)
        {
            PlayerPrefs.DeleteKey("AfterCutsceneGoToCharacterSelect");
            SceneManager.LoadScene("CharacterSelectScene");
        }
        else
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}