using System.Collections.Generic;
using UnityEngine;

public class ItemEffectDatabase : MonoBehaviour
{
    public static ItemEffectDatabase Instance { get; private set; }

    private readonly Dictionary<int, List<ItemEffectData>> database =
        new Dictionary<int, List<ItemEffectData>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateEffects();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void CreateEffects()
    {
        database.Clear();

        // 원본 "아이템 효과 테이블" 그대로 등록.
        AddEffect(new ItemEffectData(1001, 1, 5f, 0, 0, 2001));
        AddEffect(new ItemEffectData(1002, 1, 20f, 0, 0, 2002));
        AddEffect(new ItemEffectData(1003, 1, 50f, 0, 0, 2003));
        AddEffect(new ItemEffectData(1004, 2, 5f, 0, 0, 2004));
        AddEffect(new ItemEffectData(1005, 2, 20f, 0, 0, 2005));
        AddEffect(new ItemEffectData(1006, 2, 50f, 0, 0, 2006));
        AddEffect(new ItemEffectData(1007, 3, 5f, 0, 0, 2007));
        AddEffect(new ItemEffectData(1008, 3, 20f, 0, 0, 2008));
        AddEffect(new ItemEffectData(1009, 3, 50f, 0, 0, 2009));
        AddEffect(new ItemEffectData(1010, 1, 7f, 0, 0, 2010));
        AddEffect(new ItemEffectData(1010, 3, 7f, 0, 1, 2011));

        // 1011 손전등: 전투 확률 *1.5 / 아이템 획득량 *1.5, 10턴, 던전 이동 중, 중첩 O
        AddEffect(new ItemEffectData(1011, 10, 1.5f, 0, 0, 2012, 0, 10, 0, 2, true));
        AddEffect(new ItemEffectData(1011, 15, 1.5f, 0, 1, 2013, 0, 10, 0, 2, true));

        // 1012 텐트: 휴식 그리드에서 휴식
        AddEffect(new ItemEffectData(1012, 16, 0f, 0, 0, 2014, 0, 0, 0, 4, false));

        // 1013 벽돌: 적 하나 10 피해
        AddEffect(new ItemEffectData(1013, 1, 10f, 0, 0, 2015, 1, 0, 0, 1, false));

        // 1014 화염병: 적 전체 7 피해
        AddEffect(new ItemEffectData(1014, 1, 7f, 0, 0, 2016, 2, 0, 0, 1, false));

        // 1015 1회용 에너지 보호막: 공격에 맞을 시 공격 1회 무시
        AddEffect(new ItemEffectData(1015, 14, 0f, 0, 0, 2017, 0, 0, 2, 1, false));

        // 1016 강화 에너지 보호막: 1턴 동안 공격 무시
        AddEffect(new ItemEffectData(1016, 14, 0f, 0, 0, 2018, 0, 1, 2, 1, false));

        // 1017 자석: 다음 공격/무기스킬 명중률 보정 -100 (원본 테이블 값)
        AddEffect(new ItemEffectData(1017, 9, -100f, 0, 0, 2019, 1, 1, 1, 1, false));

        // 1018 자극제: 다음 공격 공격력 *2
        AddEffect(new ItemEffectData(1018, 11, 2f, 0, 0, 2020, 0, 0, 1, 1, false));

        // 1019 개조 장치: 스탯 영구 증가 (증가량/선택 스탯은 원본 효과 테이블에 미기재)
        AddEffect(new ItemEffectData(1019, 12, 0f, 0, 0, 2021, 0, 0, 0, 2, false));

        // 1020 솔라스톤: 효과 테이블에 구체 효과 없음
        AddEffect(new ItemEffectData(1020, 0, 0f, 0, 0, 2022));

        // 1021 열쇠: 잠금 그리드 해제
        AddEffect(new ItemEffectData(1021, 13, 0f, 0, 0, 2023, 4, 0, 0, 3, false));

        // 1022 연막탄: 적 명중률 -30, 1턴
        AddEffect(new ItemEffectData(1022, 8, -30f, 0, 0, 2024, 1, 1, 0, 1, false));

        // 1023 투명 장치: 전투 발생 확률 -25, 10턴, 던전 이동 중, 중첩 O
        AddEffect(new ItemEffectData(1023, 10, -25f, 0, 0, 2025, 4, 10, 0, 2, true));

        Debug.Log($"[ItemEffectDatabase] 원본 아이템 효과 테이블 등록 완료 / 아이템 수: {database.Count}");
    }

    private void AddEffect(ItemEffectData effect)
    {
        if (effect == null || effect.itemId <= 0) return;

        if (!database.TryGetValue(effect.itemId, out List<ItemEffectData> list))
        {
            list = new List<ItemEffectData>();
            database.Add(effect.itemId, list);
        }

        list.Add(effect);
        list.Sort((a, b) => a.order.CompareTo(b.order));
    }

    public List<ItemEffectData> GetEffects(int itemId)
    {
        if (!database.TryGetValue(itemId, out List<ItemEffectData> list))
            return new List<ItemEffectData>();

        return new List<ItemEffectData>(list);
    }

    public bool HasEffects(int itemId)
    {
        return database.TryGetValue(itemId, out List<ItemEffectData> list) &&
               list != null && list.Count > 0;
    }

    public int GetEffectCount(int itemId)
    {
        return database.TryGetValue(itemId, out List<ItemEffectData> list)
            ? list.Count
            : 0;
    }

    [ContextMenu("DEBUG - Print All Item Effects")]
    private void DebugPrintAllEffects()
    {
        Debug.Log("========== ITEM EFFECT DATABASE ==========");

        foreach (KeyValuePair<int, List<ItemEffectData>> pair in database)
        {
            Debug.Log($"[ItemEffectDatabase] Item ID: {pair.Key} / Effect Count: {pair.Value.Count}");

            foreach (ItemEffectData e in pair.Value)
            {
                Debug.Log(
                    $"  EffectID:{e.effectId} / Order:{e.order} / Target:{e.target} / " +
                    $"Duration:{e.duration} / EffectType:{e.effectType} / EffectValue:{e.effectValue} / " +
                    $"Timing:{e.effectTiming} / Condition:{e.useCondition} / Stackable:{e.stackable} / StatusID:{e.statusId}"
                );
            }
        }
    }
}
