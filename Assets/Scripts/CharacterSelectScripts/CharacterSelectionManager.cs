using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject confirmPanel;

    /*
     * 기존 CharacterSelectScene의 Inspector 연결이 끊기지 않도록
     * namePanel / nameInputField 필드는 당장은 남겨 둔다.
     * 실제 게임 흐름에서는 더 이상 사용하지 않는다.
     */
    public GameObject namePanel;

    [Header("Texts")]
    public TextMeshProUGUI confirmText;

    [Header("Legacy Name Input - Not Used")]
    public TMP_InputField nameInputField;

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    public bool isConfirmOpen = false;

    // CharacterHoverEffect와 기존 참조 호환용.
    // 닉네임 패널은 더 이상 열리지 않으므로 항상 false 상태다.
    public bool isNamePanelOpen = false;

    private int selectedCharacter = -1;
    private bool isStartingGame = false;

    void Start()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // 닉네임 입력 단계 제거
        if (namePanel != null)
            namePanel.SetActive(false);

        isConfirmOpen = false;
        isNamePanelOpen = false;
        isStartingGame = false;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void SelectCharacter(int characterID)
    {
        if (isConfirmOpen || isStartingGame)
            return;

        selectedCharacter = characterID;
        isConfirmOpen = true;

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (confirmText == null)
            return;

        if (characterID == 0)
            confirmText.text = "제로를 선택하시겠습니까?";
        else if (characterID == 1)
            confirmText.text = "마법사를 선택하시겠습니까?";
        else if (characterID == 2)
            confirmText.text = "궁수를 선택하시겠습니까?";
        else
            confirmText.text = "이 캐릭터를 선택하시겠습니까?";
    }

    public void ConfirmCharacterSelection()
    {
        if (isStartingGame)
            return;

        if (selectedCharacter < 0)
        {
            Debug.LogWarning(
                "[CharacterSelectionManager] 선택된 캐릭터가 없습니다."
            );
            return;
        }

        isConfirmOpen = false;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // 닉네임 입력창으로 가지 않고 바로 새 게임 생성
        StartCoroutine(CreateSaveAndLoadLobby());
    }

    public void CancelCharacterSelection()
    {
        if (isStartingGame)
            return;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        isConfirmOpen = false;
    }

    /*
     * 예전 NamePanel 버튼이 씬에 연결된 상태여도
     * Missing Method 오류가 나지 않도록 메서드는 남겨 둔다.
     * 실제 새 게임 흐름에서는 호출되지 않는다.
     */
    public void ConfirmNameInput()
    {
        if (isStartingGame)
            return;

        if (selectedCharacter < 0)
        {
            Debug.LogWarning(
                "[CharacterSelectionManager] 선택된 캐릭터가 없습니다."
            );
            return;
        }

        StartCoroutine(CreateSaveAndLoadLobby());
    }

    public void CancelNameInput()
    {
        if (namePanel != null)
            namePanel.SetActive(false);

        isNamePanelOpen = false;
    }

    private IEnumerator CreateSaveAndLoadLobby()
    {
        isStartingGame = true;
        isConfirmOpen = false;
        isNamePanelOpen = false;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (namePanel != null)
            namePanel.SetActive(false);

        /*
         * 현재 SaveManager에는 기존 코드 호환용
         * CreateNewSave(int, string) 오버로드가 남아 있다.
         *
         * 캐릭터 선택값은 반드시 보존해야 하므로
         * 선택한 characterID를 넘긴다.
         * 닉네임은 제거되었으므로 빈 문자열을 넘긴다.
         *
         * 다음 SaveManager 단계에서 이 메서드가
         * 캐릭터 ID 저장 + 완전한 New Game 초기화를
         * 담당하도록 수정한다.
         */
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CreateNewSave(
                selectedCharacter,
                ""
            );
        }
        else
        {
            Debug.LogError(
                "[CharacterSelectionManager] SaveManager.Instance가 없습니다."
            );

            isStartingGame = false;
            yield break;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;

            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;

                float alpha =
                    fadeDuration <= 0f
                        ? 1f
                        : t / fadeDuration;

                fadeCanvasGroup.alpha =
                    Mathf.Lerp(
                        0f,
                        1f,
                        alpha
                    );

                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadScene("LobbyScene");
    }
}
