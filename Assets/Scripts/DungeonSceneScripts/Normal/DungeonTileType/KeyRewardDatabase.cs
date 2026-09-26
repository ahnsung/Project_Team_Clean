using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyRewardDatabase : MonoBehaviour
{
    public static KeyRewardDatabase Instance
    {
        get;
        private set;
    }

    [Serializable]
    public class KeyRewardEntry
    {
        [Tooltip("기획서의 Key ID. 예: K_1")]
        public string keyID;

        [Tooltip("실제 인벤토리에 들어갈 ItemDatabase ItemID")]
        public int itemID;
    }

    [Header("Key Reward Mapping")]
    [SerializeField]
    private List<KeyRewardEntry> keyRewards =
        new List<KeyRewardEntry>();

    private readonly Dictionary<string, int>
        lookup =
            new Dictionary<string, int>();


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildLookup();
    }


    private void BuildLookup()
    {
        lookup.Clear();

        foreach (
            KeyRewardEntry entry
            in keyRewards)
        {
            if (entry == null)
                continue;

            if (string.IsNullOrWhiteSpace(
                entry.keyID))
            {
                continue;
            }

            if (entry.itemID <= 0)
            {
                Debug.LogWarning(
                    "[KeyRewardDatabase] " +
                    "잘못된 ItemID입니다.\n" +
                    $"KeyID: {entry.keyID}\n" +
                    $"ItemID: {entry.itemID}"
                );

                continue;
            }

            string key =
                entry.keyID.Trim();

            if (lookup.ContainsKey(key))
            {
                Debug.LogWarning(
                    "[KeyRewardDatabase] " +
                    "중복 KeyID: " +
                    key
                );

                continue;
            }

            lookup.Add(
                key,
                entry.itemID
            );
        }

        Debug.Log(
            "[KeyRewardDatabase] 등록 완료: " +
            lookup.Count +
            "개"
        );
    }


    public bool TryGetItemID(
        string keyID,
        out int itemID)
    {
        itemID = -1;

        if (string.IsNullOrWhiteSpace(
            keyID))
        {
            return false;
        }

        return lookup.TryGetValue(
            keyID.Trim(),
            out itemID
        );
    }


    public int GetItemID(
        string keyID)
    {
        if (TryGetItemID(
            keyID,
            out int itemID))
        {
            return itemID;
        }

        return -1;
    }
}