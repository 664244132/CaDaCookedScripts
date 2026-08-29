using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted,
    }

    [SerializeField] private Mode mode;
    private Camera targetCamera;

    private void Start()
    {
        // ค้นหากล้องหลักในฉาก
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }
    }

    private void LateUpdate()
    {
        // ถ้าหากล้องไม่เจอให้ข้ามไป
        if (targetCamera == null) return;

        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(targetCamera.transform);
                break;
            case Mode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - targetCamera.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            case Mode.CameraForward:
                transform.forward = targetCamera.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                transform.forward = -targetCamera.transform.forward;
                break;
        }
    }
}
