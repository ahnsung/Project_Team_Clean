using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject warningPanel;

    void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
            continueButton.SetActive(true);
        else
            continueButton.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    // Continue 버튼
    public void ContinueGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

    // New Game 버튼
    public void NewGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            // 저장 데이터가 있으면 경고창
            warningPanel.SetActive(true);
        }
        else
        {
            StartNewGameCutscene();
        }
    }

    // 경고창 Yes
    public void ConfirmNewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        warningPanel.SetActive(false);

        StartNewGameCutscene();
    }

    // 경고창 No
    public void CancelNewGame()
    {
        warningPanel.SetActive(false);
    }

    // 캐릭터 선택 / 닉네임 생성 없이 새 게임 컷씬 시작
    private void StartNewGameCutscene()
    {
        // 이전 CharacterSelect용 플래그가 남아 있다면 제거
        PlayerPrefs.DeleteKey("AfterCutsceneGoToCharacterSelect");

        // CutsceneManager가 새 게임 컷씬임을 구분하는 플래그
        PlayerPrefs.SetInt("NewGameCutscene", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("StartScene");
    }
}
