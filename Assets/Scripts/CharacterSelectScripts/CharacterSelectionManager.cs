using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject confirmPanel;
    public GameObject namePanel;

    [Header("Texts")]
    public TextMeshProUGUI confirmText;

    [Header("Input")]
    public TMP_InputField nameInputField;

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    public bool isConfirmOpen = false;
    public bool isNamePanelOpen = false;

    private int selectedCharacter = -1;

    void Start()
    {
        confirmPanel.SetActive(false);
        namePanel.SetActive(false);

        isConfirmOpen = false;
        isNamePanelOpen = false;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void SelectCharacter(int characterID)
    {
        // ConfirmPanel이나 NamePanel이 열려 있으면 중복 입력 방지
        if (isConfirmOpen || isNamePanelOpen)
            return;

        selectedCharacter = characterID;
        isConfirmOpen = true;
        confirmPanel.SetActive(true);

        if (characterID == 0)
            confirmText.text = "도적을 하시겠습니까?";
        else if (characterID == 1)
            confirmText.text = "전사를 하시겠습니까?";
        else if (characterID == 2)
            confirmText.text = "부랑자를 하시겠습니까?";
    }

    public void ConfirmCharacterSelection()
    {
        // 첫 번째 확인창 닫기
        confirmPanel.SetActive(false);
        isConfirmOpen = false;

        // 이름 입력창 열기
        namePanel.SetActive(true);
        isNamePanelOpen = true;

        // 이전 입력값 비우기
        nameInputField.text = "";
        nameInputField.ActivateInputField();
    }

    public void CancelCharacterSelection()
    {
        confirmPanel.SetActive(false);
        isConfirmOpen = false;
    }

    public void ConfirmNameInput()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("이름을 입력하세요");
            return;
        }

        SaveManager.Instance.CreateNewSave(selectedCharacter, playerName);

        namePanel.SetActive(false);
        isNamePanelOpen = false;

        StartCoroutine(FadeAndLoadNextScene());
    }

    public void CancelNameInput()
    {
        namePanel.SetActive(false);
        isNamePanelOpen = false;
    }

    private IEnumerator FadeAndLoadNextScene()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;

            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene("LobbyScene");
    }
}