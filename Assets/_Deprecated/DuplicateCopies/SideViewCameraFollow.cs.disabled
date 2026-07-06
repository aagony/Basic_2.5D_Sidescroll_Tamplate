using UnityEngine;

public class SideViewCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] private float followSpeed = 10f;

    private Transform previousTarget;
    private bool isLocked;

    // 카메라가 추적 대상 또는 고정 지점을 부드럽게 따라가도록 처리합니다
    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = isLocked ? target.position : target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    // 외부에서 카메라 추적 대상을 설정합니다
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (!isLocked)
        {
            previousTarget = newTarget;
        }
    }

    // 카메라를 지정한 지점에 고정합니다
    public void LockTo(Transform lockTarget)
    {
        if (lockTarget == null)
        {
            return;
        }

        if (!isLocked)
        {
            previousTarget = target;
        }

        target = lockTarget;
        isLocked = true;
    }

    // 카메라 고정을 해제하고 이전 추적 대상으로 되돌립니다
    public void Unlock()
    {
        isLocked = false;

        if (previousTarget != null)
        {
            target = previousTarget;
        }
    }
}
