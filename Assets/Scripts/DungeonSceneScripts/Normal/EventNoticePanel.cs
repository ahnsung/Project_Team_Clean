using System.Collections;
using TMPro;
using UnityEngine;

public class EventNoticeUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject noticePanel;
    public TextMeshProUGUI noticeText;

    public IEnumerator ShowNotice(string message, float duration)
    {
        if (noticePanel != null)
            noticePanel.SetActive(true);

        if (noticeText != null)
            noticeText.text = message;

        yield return new WaitForSeconds(duration);

        if (noticePanel != null)
            noticePanel.SetActive(false);
    }

    public void Hide()
    {
        if (noticePanel != null)
            noticePanel.SetActive(false);
    }
}