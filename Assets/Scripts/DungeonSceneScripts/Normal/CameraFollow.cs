using UnityEngine;

// 메인 카메라가 플레이어를 따라가도록 만드는 스크립트
// 현재는 2D 가로 진행 방식이라서 X축만 따라가고,
// 배경 밖으로 카메라가 나가지 않도록 Clamp도 넣어둔 상태
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // 따라갈 대상 (플레이어)
    [SerializeField] private float followSpeed = 8f; // 따라가는 속도
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 오프셋

    [Header("Clamp")]
    [SerializeField] private float minX = -5f; // 카메라가 갈 수 있는 최소 X
    [SerializeField] private float maxX = 5f;  // 카메라가 갈 수 있는 최대 X

    private void LateUpdate()
    {
        // 타겟이 없으면 아무 것도 안 함
        if (target == null) return;

        // 현재 카메라 위치 가져오기
        Vector3 pos = transform.position;

        // 플레이어 X 위치를 향해 부드럽게 이동
        // Mathf.Lerp를 써서 딱딱하지 않게 따라가게 함
        pos.x = Mathf.Lerp(pos.x, target.position.x, followSpeed * Time.deltaTime);

        // 카메라가 배경 밖으로 나가지 않도록 X 범위를 제한
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        // Y와 Z는 고정
        // 현재 게임은 좌우 진행이므로 Y는 안 따라가게 설정
        pos.y = offset.y;
        pos.z = offset.z;

        // 최종 위치 적용
        transform.position = pos;
    }
}