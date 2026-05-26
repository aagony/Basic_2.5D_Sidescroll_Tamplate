using UnityEngine;

public class SideViewCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] private float followSpeed = 10f;

    // 카메라가 플레이어를 부드럽게 따라가도록 처리합니다
    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    // 외부에서 카메라 추적 대상을 설정합니다
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}