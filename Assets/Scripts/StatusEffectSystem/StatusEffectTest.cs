using UnityEngine;

public class StatusEffectTest : MonoBehaviour
{
    // =========================================================
    // Target
    // =========================================================

    [Header("Target")]
    [SerializeField]
    private StatusEffectController target;


    // =========================================================
    // Test Setting
    // =========================================================

    [Header("Test Setting")]

    [Tooltip("1~4 키로 추가하는 테스트 상태이상의 지속시간")]
    [Min(1)]
    [SerializeField]
    private int testDuration = 3;


    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        ResolveTarget();
    }


    private void Update()
    {
        ResolveTarget();

        if (target == null)
            return;


        // =====================================================
        // 숫자 1
        // 부상
        // =====================================================

        if (
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1)
        )
        {
            AddTrapStatusEffect(1);
        }


        // =====================================================
        // 숫자 2
        // 피로
        // =====================================================

        if (
            Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2)
        )
        {
            AddTrapStatusEffect(2);
        }


        // =====================================================
        // 숫자 3
        // 좌절
        // =====================================================

        if (
            Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3)
        )
        {
            AddTrapStatusEffect(3);
        }


        // =====================================================
        // 숫자 4
        // 중독
        // =====================================================

        if (
            Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Keypad4)
        )
        {
            AddTrapStatusEffect(4);
        }


        // =====================================================
        // R
        // 모든 상태이상 제거
        // =====================================================

        if (Input.GetKeyDown(KeyCode.R))
        {
            target.ClearAllStatusEffects();

            Debug.Log(
                "[StatusEffectTest] " +
                "모든 상태이상을 제거했습니다."
            );
        }


        // =====================================================
        // 기존 타이밍 테스트
        // =====================================================

        // H
        // 플레이어 팀 종료
        if (Input.GetKeyDown(KeyCode.H))
        {
            target.ProcessTiming(
                StatusEffectTiming.PlayerTeamEnd
            );

            Debug.Log(
                "[StatusEffectTest] " +
                "PlayerTeamEnd 처리"
            );
        }


        // J
        // 적 팀 종료
        if (Input.GetKeyDown(KeyCode.J))
        {
            target.ProcessTiming(
                StatusEffectTiming.EnemyTeamEnd
            );

            Debug.Log(
                "[StatusEffectTest] " +
                "EnemyTeamEnd 처리"
            );
        }


        // K
        // 전체 턴 종료
        if (Input.GetKeyDown(KeyCode.K))
        {
            target.ProcessTiming(
                StatusEffectTiming.TurnEnd
            );

            Debug.Log(
                "[StatusEffectTest] " +
                "TurnEnd 처리"
            );
        }
    }


    // =========================================================
    // Target
    // =========================================================

    private void ResolveTarget()
    {
        if (target != null)
            return;


        target =
            GetComponent<StatusEffectController>();


        if (target != null)
            return;


        target =
            GetComponentInChildren<StatusEffectController>();
    }


    // =========================================================
    // Add Trap Status Effect
    // =========================================================

    private void AddTrapStatusEffect(
        int trapType)
    {
        StatusEffectData data =
            TrapStatusEffectFactory.Create(
                trapType,
                testDuration
            );


        if (data == null)
        {
            Debug.LogWarning(
                "[StatusEffectTest] " +
                "상태이상 생성 실패\n" +
                "TrapType: " +
                trapType
            );

            return;
        }


        bool result =
            target.AddStatusEffect(
                data
            );


        Debug.Log(
            "[StatusEffectTest] " +
            $"[{trapType}] {data.buffName} 추가\n" +
            $"지속시간: {testDuration}턴\n" +
            $"추가 결과: {result}"
        );
    }
}