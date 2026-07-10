using UnityEngine;

public class WorldSpaceCanvasBillboard : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Rotation")]
    [SerializeField] private bool keepWorldUp = true;

    // 시작할 때 사용할 카메라를 준비합니다
    private void Awake()
    {
        EnsureCameraReference();
    }

    // 카메라 이동이 끝난 뒤 Canvas가 카메라를 바라보게 합니다
    private void LateUpdate()
    {
        EnsureCameraReference();

        if (targetCamera == null)
        {
            return;
        }

        FaceCamera();
    }

    // 메인 카메라가 지정되지 않았다면 자동으로 찾습니다
    private void EnsureCameraReference()
    {
        if (targetCamera != null)
        {
            return;
        }

        targetCamera = Camera.main;
    }

    // Canvas의 앞면이 카메라를 향하도록 회전시킵니다
    private void FaceCamera()
    {
        Vector3 direction =
            transform.position -
            targetCamera.transform.position;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        if (keepWorldUp)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            return;
        }

        transform.rotation =
            Quaternion.LookRotation(direction.normalized);
    }
}