using UnityEngine;

public class Camera2 : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;
    public Transform cam;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float movementSmoothFactor = 12f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float zoomSmoothFactor = 12f;
    public float minZoomDistance = 5f;
    public float maxZoomDistance = 25f;

    [Header("Zoom Pitch")]
    [Range(-1f, 1f)]
    public float ZoomPitchFactor = 0f; // 0 = no pitch effect
    [Min(0f)]
    public float MaxZoomPitchEffect = 20f; // degrees at factor=1

    private Vector3 pivotTargetPos;
    private float targetZoomDistance;
    private float currentZoomDistance;

    private Quaternion baseCamLocalRot;

    void Awake()
    {
        if (cameraPivot == null) cameraPivot = transform;
        if (cam == null) cam = Camera.main != null ? Camera.main.transform : null;

        cameraPivot.rotation = Quaternion.identity;

        pivotTargetPos = cameraPivot.position;

        targetZoomDistance = Mathf.Clamp(maxZoomDistance, minZoomDistance, maxZoomDistance);
        currentZoomDistance = targetZoomDistance;

        if (cam != null)
            baseCamLocalRot = cam.localRotation;

        ApplyZoomLocalPosition();
        ApplyZoomPitch();
    }

    void Update()
    {
        cameraPivot.rotation = Quaternion.identity;

        HandleMovement();
        HandleZoom();

        cameraPivot.position = Vector3.Lerp(
            cameraPivot.position,
            pivotTargetPos,
            1f - Mathf.Exp(-movementSmoothFactor * Time.deltaTime)
        );
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(h, 0f, v).normalized;
        pivotTargetPos += moveDir * moveSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoomDistance -= scroll * zoomSpeed;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }

        currentZoomDistance = Mathf.Lerp(
            currentZoomDistance,
            targetZoomDistance,
            1f - Mathf.Exp(-zoomSmoothFactor * Time.deltaTime)
        );

        ApplyZoomLocalPosition();
        ApplyZoomPitch();
    }

    private void ApplyZoomLocalPosition()
    {
        if (cam == null) return;

        Vector3 axisBack = cam.localRotation * Vector3.back;
        cam.localPosition = axisBack * currentZoomDistance;
    }

    private void ApplyZoomPitch()
    {
        if (cam == null) return;

        float denom = Mathf.Max(0.0001f, maxZoomDistance - minZoomDistance);
        float zoom01 = Mathf.Clamp01((currentZoomDistance - minZoomDistance) / denom);
        // zoom01: 0 = zoomed IN (min distance), 1 = zoomed OUT (max distance)

        float pitchDelta = ZoomPitchFactor * MaxZoomPitchEffect * zoom01; // degrees
        Quaternion pitchRot = Quaternion.Euler(pitchDelta, 0f, 0f);

        cam.localRotation = baseCamLocalRot * pitchRot;
    }

    public void ResetCamera()
    {
        pivotTargetPos = Vector3.zero;
        cameraPivot.position = Vector3.zero;
        cameraPivot.rotation = Quaternion.identity;

        targetZoomDistance = Mathf.Clamp(maxZoomDistance, minZoomDistance, maxZoomDistance);
        currentZoomDistance = targetZoomDistance;

        if (cam != null)
        {
            cam.localRotation = baseCamLocalRot;
            ApplyZoomLocalPosition();
            ApplyZoomPitch();
        }
    }
}