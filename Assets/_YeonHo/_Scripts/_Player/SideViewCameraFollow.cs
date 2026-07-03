using UnityEngine;

public class SideViewCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] private float followSpeed = 10f;

    private bool isLocked;
    private Transform lockPoint;

    // 카메라가 추적 대상 또는 고정 지점을 부드럽게 따라가도록 처리합니다
    private void LateUpdate()
    {
        if (!TryGetDesiredPosition(out Vector3 desiredPosition))
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    // 외부에서 카메라 추적 대상을 설정합니다
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 카메라를 지정한 고정 지점에 묶습니다
    public void LockTo(Transform newLockPoint)
    {
        if (newLockPoint == null)
        {
            return;
        }

        lockPoint = newLockPoint;
        isLocked = true;
    }

    // 카메라 고정을 해제하고 기존 추적 대상으로 돌아갑니다
    public void Unlock()
    {
        isLocked = false;
        lockPoint = null;
    }

    // 현재 카메라 상태에 맞는 목표 위치를 계산합니다
    private bool TryGetDesiredPosition(out Vector3 desiredPosition)
    {
        desiredPosition = transform.position;

        if (isLocked)
        {
            if (lockPoint == null)
            {
                return false;
            }

            desiredPosition = lockPoint.position + offset;
            return true;
        }

        if (target == null)
        {
            return false;
        }

        desiredPosition = target.position + offset;
        return true;
    }
}