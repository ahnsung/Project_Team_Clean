using System.Collections;
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Follow")]
    public CameraFollow cameraFollow;

    [Header("Zoom")]
    public float impactZoomSize = 3.6f;

    [Header("Timing")]
    public float zoomInTime = 0.08f;
    public float holdTime = 0.08f;
    public float zoomOutTime = 0.16f;

    [Header("Shake")]
    public float shakeTime = 0.08f;
    public float shakePower = 0.06f;

    [Header("Focus")]
    public float enemyHitOffsetX = 0.8f;
    public float playerHitOffsetX = 0.35f;
    public float focusOffsetY = 0.25f;

    private Coroutine currentRoutine;
    private Vector3 lockedStartPos;
    private float lockedStartSize;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (cameraFollow == null && targetCamera != null)
            cameraFollow = targetCamera.GetComponent<CameraFollow>();
    }

    public IEnumerator AttackImpactZoom(Transform attacker, Transform target)
    {
        if (targetCamera == null)
            yield break;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            ForceRestore();
        }

        lockedStartPos = targetCamera.transform.position;
        lockedStartSize = targetCamera.orthographicSize;

        currentRoutine = StartCoroutine(ImpactRoutine(attacker, target));
        yield return currentRoutine;
    }

    private IEnumerator ImpactRoutine(Transform attacker, Transform target)
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        Vector3 originalPos = lockedStartPos;
        float originalSize = lockedStartSize;

        Vector3 focusPos = originalPos;

        if (attacker != null && target != null)
        {
            float offsetX = target.CompareTag("Player") ? playerHitOffsetX : enemyHitOffsetX;

            Vector3 focusTarget = target.position;

            if (target.position.x > attacker.position.x)
                focusTarget.x += offsetX;
            else
                focusTarget.x -= offsetX;

            focusPos = new Vector3(
                focusTarget.x,
                focusTarget.y + focusOffsetY,
                originalPos.z
            );
        }

        float t = 0f;

        while (t < zoomInTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / zoomInTime);

            targetCamera.orthographicSize = Mathf.Lerp(originalSize, impactZoomSize, r);
            targetCamera.transform.position = Vector3.Lerp(originalPos, focusPos, r);

            yield return null;
        }

        targetCamera.orthographicSize = impactZoomSize;
        targetCamera.transform.position = focusPos;

        yield return StartCoroutine(Shake(focusPos));

        yield return new WaitForSeconds(holdTime);

        t = 0f;

        while (t < zoomOutTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / zoomOutTime);

            targetCamera.orthographicSize = Mathf.Lerp(impactZoomSize, originalSize, r);
            targetCamera.transform.position = Vector3.Lerp(focusPos, originalPos, r);

            yield return null;
        }

        targetCamera.orthographicSize = originalSize;
        targetCamera.transform.position = originalPos;

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        currentRoutine = null;
    }

    private IEnumerator Shake(Vector3 basePos)
    {
        float t = 0f;

        while (t < shakeTime)
        {
            t += Time.deltaTime;

            float x = Random.Range(-shakePower, shakePower);
            float y = Random.Range(-shakePower, shakePower);

            targetCamera.transform.position = basePos + new Vector3(x, y, 0f);

            yield return null;
        }

        targetCamera.transform.position = basePos;
    }

    private void ForceRestore()
    {
        if (targetCamera != null)
        {
            targetCamera.transform.position = lockedStartPos;
            targetCamera.orthographicSize = lockedStartSize;
        }

        if (cameraFollow != null)
            cameraFollow.enabled = true;

        currentRoutine = null;
    }
}