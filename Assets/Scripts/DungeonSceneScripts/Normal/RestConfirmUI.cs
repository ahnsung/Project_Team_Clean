using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestConfirmUI : MonoBehaviour
{
    public static RestConfirmUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private bool waitingForAnswer;
    private bool answered;
    private bool result;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    private void Start()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(OnYes);
            yesButton.onClick.AddListener(OnYes);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(OnNo);
            noButton.onClick.AddListener(OnNo);
        }
    }

    public IEnumerator ShowConfirm()
    {
        answered = false;
        result = false;
        waitingForAnswer = true;

        if (messageText != null)
        {
            messageText.text =
                "휴식하시겠습니까?\n\n" +
                "던전을 재진입하기 전까지\n" +
                "이 장소에서는 다시 휴식할 수 없습니다.";
        }

        if (panel != null)
            panel.SetActive(true);

        while (!answered)
            yield return null;

        if (panel != null)
            panel.SetActive(false);

        waitingForAnswer = false;
    }

    public bool GetResult()
    {
        return result;
    }

    private void OnYes()
    {
        if (!waitingForAnswer)
            return;

        result = true;
        answered = true;
    }

    private void OnNo()
    {
        if (!waitingForAnswer)
            return;

        result = false;
        answered = true;
    }
}