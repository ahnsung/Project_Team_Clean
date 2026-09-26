using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonRewardUI : MonoBehaviour
{
    public static DungeonRewardUI Instance
    {
        get;
        private set;
    }

    [Header("Main")]
    [SerializeField]
    private GameObject rewardPanel;

    [SerializeField]
    private TMP_Text titleText;

    [Header("Reward List")]
    [SerializeField]
    private Transform rewardContent;

    [SerializeField]
    private DungeonRewardItemUI rewardRowPrefab;

    [Header("Bottom Buttons")]
    [SerializeField]
    private Button acquireButton;

    [SerializeField]
    private Button ignoreButton;

    private readonly List<DungeonRewardItemUI>
        rewardRows =
            new List<DungeonRewardItemUI>();

    private bool isOpen;

    private bool anyItemAcquired;

    public bool AnyItemAcquired =>
        anyItemAcquired;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (acquireButton != null)
        {
            acquireButton.onClick
                .RemoveAllListeners();

            acquireButton.onClick
                .AddListener(OnAcquirePressed);
        }

        if (ignoreButton != null)
        {
            ignoreButton.onClick
                .RemoveAllListeners();

            ignoreButton.onClick
                .AddListener(OnIgnorePressed);
        }
    }

    public IEnumerator ShowChestRewards(
        List<ChestItemData> rewards)
    {
        if (!ValidateReferences())
        {
            yield break;
        }

        ClearRows();

        anyItemAcquired = false;
        isOpen = true;

        if (titleText != null)
        {
            titleText.text =
                "보상";
        }

        if (rewards != null)
        {
            foreach (
                ChestItemData reward
                in rewards)
            {
                if (reward == null ||
                    reward.itemID <= 0 ||
                    reward.amount <= 0)
                {
                    continue;
                }

                // ==========================
                // 중요:
                // 같은 아이템 ×2라면
                // 카드도 두 개 생성
                // ==========================
                for (
                    int i = 0;
                    i < reward.amount;
                    i++)
                {
                    CreateRewardRow(
                        reward.itemID
                    );
                }
            }
        }

        rewardPanel.SetActive(true);

        Debug.Log(
            $"[DungeonRewardUI] 보상창 열림 / " +
            $"아이템 수: {rewardRows.Count}"
        );

        while (isOpen)
        {
            yield return null;
        }

        rewardPanel.SetActive(false);

        Debug.Log(
            "[DungeonRewardUI] 보상창 닫힘"
        );
    }

    private void CreateRewardRow(
        int itemID)
    {
        DungeonRewardItemUI row =
            Instantiate(
                rewardRowPrefab,
                rewardContent
            );

        row.gameObject.SetActive(true);

        row.Setup(itemID);

        rewardRows.Add(row);
    }

    private void OnAcquirePressed()
    {
        if (!isOpen)
            return;

        if (InventoryManager.Instance ==
            null)
        {
            Debug.LogError(
                "[DungeonRewardUI] " +
                "InventoryManager가 없습니다."
            );

            return;
        }

        // ==========================
        // 모든 아이템에 V/X 결정을 했는지
        // ==========================
        foreach (
            DungeonRewardItemUI row
            in rewardRows)
        {
            if (row == null ||
                row.IsAcquired)
            {
                continue;
            }

            if (!row.HasDecision)
            {
                if (titleText != null)
                {
                    titleText.text =
                        "모든 아이템을 V 또는 X로 선택해주세요.";
                }

                Debug.Log(
                    "[DungeonRewardUI] " +
                    "아직 선택하지 않은 아이템이 있습니다."
                );

                return;
            }
        }

        int successCount = 0;
        int failCount = 0;

        foreach (
            DungeonRewardItemUI row
            in rewardRows)
        {
            if (row == null ||
                row.IsAcquired ||
                !row.IsSelected)
            {
                continue;
            }

            bool success =
                InventoryManager.Instance
                    .AddItem(
                        row.ItemID
                    );

            if (success)
            {
                row.MarkAcquired();

                successCount++;

                anyItemAcquired = true;
            }
            else
            {
                row.MarkInventoryFull();

                failCount++;
            }
        }

        Debug.Log(
            "[DungeonRewardUI] 획득 처리\n" +
            $"성공: {successCount}\n" +
            $"실패: {failCount}"
        );

        // 인벤토리 공간 부족 아이템이 있다면
        // 창을 닫지 않는다.
        if (failCount > 0)
        {
            if (titleText != null)
            {
                titleText.text =
                    "인벤토리 공간이 부족합니다.";
            }

            return;
        }

        // 전부 정상 처리
        CloseWindow();
    }

    private void OnIgnorePressed()
    {
        if (!isOpen)
            return;

        Debug.Log(
            "[DungeonRewardUI] " +
            "남은 보상을 무시했습니다."
        );

        CloseWindow();
    }

    private void CloseWindow()
    {
        isOpen = false;
    }

    private bool ValidateReferences()
    {
        if (rewardPanel == null)
        {
            Debug.LogError(
                "[DungeonRewardUI] " +
                "RewardPanel이 연결되지 않았습니다."
            );

            return false;
        }

        if (rewardContent == null)
        {
            Debug.LogError(
                "[DungeonRewardUI] " +
                "RewardContent가 연결되지 않았습니다."
            );

            return false;
        }

        if (rewardRowPrefab == null)
        {
            Debug.LogError(
                "[DungeonRewardUI] " +
                "RewardRowPrefab이 연결되지 않았습니다."
            );

            return false;
        }

        return true;
    }

    private void ClearRows()
    {
        rewardRows.Clear();

        if (rewardContent == null)
            return;

        for (
            int i =
                rewardContent.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                rewardContent
                    .GetChild(i)
                    .gameObject
            );
        }
    }
}