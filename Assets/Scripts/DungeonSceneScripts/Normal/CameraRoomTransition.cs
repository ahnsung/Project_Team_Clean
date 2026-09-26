using System.Collections;
using UnityEngine;

public class CameraRoomTransition : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Horizontal Move")]
    public float horizontalMoveDistance = 3f;
    public float moveDuration = 0.25f;

    private Vector3 originalPosition;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            originalPosition = targetCamera.transform.position;
    }

    public IEnumerator PlayRoomMove(MoveDirection direction)
    {
        if (targetCamera == null)
            yield break;

        originalPosition = targetCamera.transform.position;

        // 위/아래 이동은 카메라 이동 없이 끝
        if (direction == MoveDirection.Up || direction == MoveDirection.Down)
            yield break;

        Vector3 targetPosition = originalPosition;

        if (direction == MoveDirection.Left)
            targetPosition += Vector3.left * horizontalMoveDistance;
        else if (direction == MoveDirection.Right)
            targetPosition += Vector3.right * horizontalMoveDistance;

        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            targetCamera.transform.position =
                Vector3.Lerp(originalPosition, targetPosition, t / moveDuration);

            yield return null;
        }

        targetCamera.transform.position = targetPosition;
    }

    public void ResetCameraPosition()
    {
        if (targetCamera != null)
            targetCamera.transform.position = originalPosition;
    }
}