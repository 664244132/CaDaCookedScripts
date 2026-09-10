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
    private Transform targetCameraTransform;

    private void Start()
    {
        // ค้นหาและแคชกล้องหลักในฉากตั้งแต่ Start() เพื่อลด C++ Engine calls ใน LateUpdate (Rule 6)
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }

        if (targetCamera != null)
        {
            targetCameraTransform = targetCamera.transform;
        }
    }

    private void LateUpdate()
    {
        // ถ้าหากล้องไม่เจอให้ข้ามไป
        if (targetCameraTransform == null)
        {
            if (targetCamera != null)
            {
                targetCameraTransform = targetCamera.transform;
            }
            else
            {
                return;
            }
        }

        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(targetCameraTransform);
                break;
            case Mode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - targetCameraTransform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            case Mode.CameraForward:
                transform.forward = targetCameraTransform.forward;
                break;
            case Mode.CameraForwardInverted:
                transform.forward = -targetCameraTransform.forward;
                break;
        }
    }
}
