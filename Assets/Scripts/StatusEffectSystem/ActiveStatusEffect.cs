using System;
using UnityEngine;

[Serializable]
public class ActiveStatusEffect
{
    [SerializeField]
    private StatusEffectData data;

    [SerializeField]
    private int remainingDuration;

    [SerializeField]
    private int stackCount = 1;

    public StatusEffectData Data
    {
        get
        {
            return data;
        }
    }

    public int RemainingDuration
    {
        get
        {
            return remainingDuration;
        }
    }

    public int StackCount
    {
        get
        {
            return stackCount;
        }
    }

    public bool IsInfinite
    {
        get
        {
            return data != null &&
                   data.IsInfiniteDuration;
        }
    }

    public bool IsExpired
    {
        get
        {
            if (data == null)
                return true;

            if (IsInfinite)
                return false;

            return remainingDuration <= 0;
        }
    }

    public ActiveStatusEffect(
        StatusEffectData sourceData)
    {
        if (sourceData == null)
        {
            Debug.LogError(
                "[ActiveStatusEffect] " +
                "StatusEffectData가 null입니다."
            );

            return;
        }

        data =
            sourceData.Clone();

        remainingDuration =
            sourceData.buffDuration;

        stackCount = 1;
    }

    public void AddStack(
        int addedDuration)
    {
        if (data == null)
            return;

        stackCount++;

        if (IsInfinite)
            return;

        remainingDuration +=
            Mathf.Max(
                0,
                addedDuration
            );
    }

    public void DecreaseDuration(
        int amount = 1)
    {
        if (data == null)
            return;

        if (IsInfinite)
            return;

        if (amount <= 0)
            return;

        remainingDuration =
            Mathf.Max(
                0,
                remainingDuration - amount
            );
    }

    public void SetRemainingDuration(
        int duration)
    {
        if (data == null ||
            IsInfinite)
        {
            return;
        }

        remainingDuration =
            Mathf.Max(
                0,
                duration
            );
    }
}