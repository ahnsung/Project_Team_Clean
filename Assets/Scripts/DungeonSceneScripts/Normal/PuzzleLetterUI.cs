using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleLetterUI : MonoBehaviour
{
    public static PuzzleLetterUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;

    private bool isOpen;

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
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }
    }

    public IEnumerator ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (panel != null)
            panel.SetActive(true);

        isOpen = true;

        while (isOpen)
            yield return null;
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (panel != null)
            panel.SetActive(false);
    }
}