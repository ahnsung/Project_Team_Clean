using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmingItemGroupDatabase : MonoBehaviour
{
    public static FarmingItemGroupDatabase Instance
    {
        get;
        private set;
    }

    [Serializable]
    public class FarmingItemGroup
    {
        public string groupID;

        [Tooltip(
            "이 그룹에서 랜덤으로 등장할 ItemID 목록"
        )]
        public List<int> itemIDs =
            new List<int>();
    }


    [Header("Farming Item Groups")]
    [SerializeField]
    private List<FarmingItemGroup> groups =
        new List<FarmingItemGroup>();


    private readonly Dictionary<
        string,
        FarmingItemGroup
    > lookup =
        new Dictionary<
            string,
            FarmingItemGroup
        >();


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
            FarmingItemGroup group
            in groups)
        {
            if (group == null)
                continue;

            if (string.IsNullOrWhiteSpace(
                group.groupID))
            {
                continue;
            }

            string key =
                group.groupID.Trim();

            if (lookup.ContainsKey(key))
            {
                Debug.LogWarning(
                    "[FarmingItemGroupDatabase] " +
                    "중복 Group ID: " +
                    key
                );

                continue;
            }

            lookup.Add(
                key,
                group
            );
        }

        Debug.Log(
            "[FarmingItemGroupDatabase] " +
            "ItemGroup 등록 완료: " +
            lookup.Count +
            "개"
        );
    }


    public bool HasGroup(
        string groupID)
    {
        if (string.IsNullOrWhiteSpace(
            groupID))
        {
            return false;
        }

        return lookup.ContainsKey(
            groupID.Trim()
        );
    }


    public List<int> GetItemIDs(
        string groupID)
    {
        if (string.IsNullOrWhiteSpace(
            groupID))
        {
            return null;
        }

        if (lookup.TryGetValue(
            groupID.Trim(),
            out FarmingItemGroup group))
        {
            return group.itemIDs;
        }

        return null;
    }


    public int GetRandomItemID(
        string groupID)
    {
        List<int> itemIDs =
            GetItemIDs(groupID);

        if (itemIDs == null ||
            itemIDs.Count == 0)
        {
            return -1;
        }

        int index =
            UnityEngine.Random.Range(
                0,
                itemIDs.Count
            );

        return itemIDs[index];
    }
}