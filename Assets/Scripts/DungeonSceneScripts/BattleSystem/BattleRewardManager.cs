using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 승리 보상 UI를 담당한다.
///
/// 흐름:
/// 1) BattleManager가 추첨된 Item ID를 전달한다.
/// 2) "승리 / 보상" 패널과 아이템 정보를 표시한다.
/// 3) 획득한다 / 무시한다 버튼 입력을 기다린다.
/// 4) 획득한다 선택 시 기존 InventoryManager.AddItem()으로 지급한다.
/// 5) 선택이 끝나면 패널을 닫고 코루틴을 종료한다.
///
/// 보상 확률 추첨 자체는 EncounterRewardManager가 담당한다.
/// </summary>
public class BattleRewardManager : MonoBehaviour
{
    public static BattleRewardManager Instance
    {
        get;
        private set;
    }

    [Header("Root")]
    [SerializeField]
    private GameObject battleRewardPanel;

    [Header("Texts")]
    [SerializeField]
    private TextMeshProUGUI victoryText;

    [SerializeField]
    private TextMeshProUGUI rewardTitleText;

    [SerializeField]
    private TextMeshProUGUI rewardNameText;

    [Header("Reward")]
    [SerializeField]
    private Image rewardIcon;

    [Header("Buttons")]
    [SerializeField]
    private Button acquireButton;

    [SerializeField]
    private Button ignoreButton;

    [Header("Button Texts - Optional")]
    [SerializeField]
    private TextMeshProUGUI acquireButtonText;

    [SerializeField]
    private TextMeshProUGUI ignoreButtonText;

    private int currentRewardItemId;
    private bool waitingForChoice;
    private bool choiceFinished;

    public bool LastRewardAcquired
    {
        get;
        private set;
    }

    public int CurrentRewardItemId =>
        currentRewardItemId;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (acquireButton != null)
        {
            acquireButton.onClick.RemoveListener(
                OnClickAcquire
            );

            acquireButton.onClick.AddListener(
                OnClickAcquire
            );
        }

        if (ignoreButton != null)
        {
            ignoreButton.onClick.RemoveListener(
                OnClickIgnore
            );

            ignoreButton.onClick.AddListener(
                OnClickIgnore
            );
        }

        HideImmediate();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


    // =========================================================
    // Public
    // =========================================================

    /// <summary>
    /// 전투 승리 보상 UI를 띄우고 플레이어 선택이 끝날 때까지 기다린다.
    /// </summary>
    public IEnumerator ShowBattleReward(
        int itemId)
    {
        if (itemId <= 0)
        {
            Debug.LogError(
                "[BattleRewardManager] 잘못된 보상 Item ID: " +
                itemId
            );

            yield break;
        }

        if (ItemDatabase.Instance == null)
        {
            Debug.LogError(
                "[BattleRewardManager] ItemDatabase.Instance가 없습니다."
            );

            yield break;
        }

        ItemData itemData =
            ItemDatabase.Instance.GetItem(
                itemId
            );

        if (itemData == null)
        {
            Debug.LogError(
                "[BattleRewardManager] ItemDatabase에서 보상 아이템을 찾지 못했습니다." +
                " / Item ID: " +
                itemId
            );

            yield break;
        }

        currentRewardItemId =
            itemId;

        LastRewardAcquired = false;
        waitingForChoice = true;
        choiceFinished = false;

        ApplyFixedTexts();
        ApplyRewardVisual(itemData);

        if (battleRewardPanel != null)
        {
            battleRewardPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "[BattleRewardManager] Battle Reward Panel이 연결되지 않았습니다."
            );

            waitingForChoice = false;
            yield break;
        }

        SetButtonsInteractable(true);

        Debug.Log(
            "[BattleRewardManager] 전투 보상 UI 표시" +
            " / Item ID: " +
            itemId +
            " / Item: " +
            itemData.itemName
        );

        while (!choiceFinished)
        {
            yield return null;
        }

        waitingForChoice = false;

        HideImmediate();

        Debug.Log(
            "[BattleRewardManager] 전투 보상 선택 종료" +
            " / Item ID: " +
            itemId +
            " / 획득 여부: " +
            LastRewardAcquired
        );
    }


    // =========================================================
    // Buttons
    // =========================================================

    public void OnClickAcquire()
    {
        if (
            !waitingForChoice ||
            choiceFinished
        )
        {
            return;
        }

        SetButtonsInteractable(false);

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "[BattleRewardManager] InventoryManager.Instance가 없습니다."
            );

            SetButtonsInteractable(true);
            return;
        }

        bool added =
            InventoryManager.Instance.AddItem(
                currentRewardItemId,
                1
            );

        if (!added)
        {
            Debug.LogWarning(
                "[BattleRewardManager] 보상을 획득하지 못했습니다." +
                " 인벤토리 공간 또는 아이템 데이터를 확인해주세요." +
                " / Item ID: " +
                currentRewardItemId
            );

            // 공간 부족 등의 경우 보상을 버린 것으로 처리하지 않는다.
            // 플레이어가 다시 획득을 시도하거나 무시를 선택할 수 있다.
            SetButtonsInteractable(true);
            return;
        }

        LastRewardAcquired = true;
        choiceFinished = true;

        Debug.Log(
            "[BattleRewardManager] 전투 보상 획득" +
            " / Item ID: " +
            currentRewardItemId
        );
    }

    public void OnClickIgnore()
    {
        if (
            !waitingForChoice ||
            choiceFinished
        )
        {
            return;
        }

        SetButtonsInteractable(false);

        LastRewardAcquired = false;
        choiceFinished = true;

        Debug.Log(
            "[BattleRewardManager] 전투 보상 무시" +
            " / Item ID: " +
            currentRewardItemId
        );
    }


    // =========================================================
    // UI
    // =========================================================

    private void ApplyFixedTexts()
    {
        if (victoryText != null)
        {
            victoryText.text =
                "승리";
        }

        if (rewardTitleText != null)
        {
            rewardTitleText.text =
                "보상";
        }

        if (acquireButtonText != null)
        {
            acquireButtonText.text =
                "획득한다.";
        }

        if (ignoreButtonText != null)
        {
            ignoreButtonText.text =
                "무시한다.";
        }
    }

    private void ApplyRewardVisual(
        ItemData itemData)
    {
        if (itemData == null)
            return;

        if (rewardNameText != null)
        {
            rewardNameText.text =
                itemData.itemName;
        }

        if (rewardIcon != null)
        {
            rewardIcon.sprite =
                itemData.icon;

            rewardIcon.enabled =
                itemData.icon != null;

            rewardIcon.preserveAspect = true;
        }
    }

    private void SetButtonsInteractable(
        bool interactable)
    {
        if (acquireButton != null)
        {
            acquireButton.interactable =
                interactable;
        }

        if (ignoreButton != null)
        {
            ignoreButton.interactable =
                interactable;
        }
    }

    private void HideImmediate()
    {
        waitingForChoice = false;
        choiceFinished = false;

        if (battleRewardPanel != null)
        {
            battleRewardPanel.SetActive(false);
        }
    }
}
