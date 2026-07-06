using UnityEngine;

public class MonsterTargetSensor : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetAimOffset = Vector3.up;
    [SerializeField] private bool autoFindPlayerStatus = true;

    [Header("Detect")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float verticalTolerance = 1.5f;

    private PlayerStatus targetStatus;

    public Transform Target
    {
        get { return target; }
    }

    public PlayerStatus TargetStatus
    {
        get { return targetStatus; }
    }

    public float DetectionRange
    {
        get { return detectionRange; }
    }

    public float VerticalTolerance
    {
        get { return verticalTolerance; }
    }

    // 인스펙터 값이 잘못 들어갔을 때 최소 값을 보장합니다
    private void OnValidate()
    {
        detectionRange = Mathf.Max(0f, detectionRange);
        verticalTolerance = Mathf.Max(0f, verticalTolerance);
    }

    // 외부에서 추적 대상을 직접 설정합니다
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetStatus = target != null ? target.GetComponentInParent<PlayerStatus>() : null;
    }

    // 필요하면 PlayerStatus를 가진 대상을 자동으로 찾습니다
    public void RefreshTarget()
    {
        if (!autoFindPlayerStatus)
        {
            return;
        }

        if (target != null && targetStatus != null)
        {
            return;
        }

        PlayerStatus foundTargetStatus = FindFirstObjectByType<PlayerStatus>();

        if (foundTargetStatus == null)
        {
            return;
        }

        targetStatus = foundTargetStatus;
        target = foundTargetStatus.transform;
    }

    // 대상이 기본 감지 범위 안에 있는지 확인합니다
    public bool IsTargetInDetectionRange(Vector3 origin)
    {
        return IsTargetInRange(origin, detectionRange, verticalTolerance);
    }

    // 대상이 지정한 수평 수직 범위 안에 있는지 확인합니다
    public bool IsTargetInRange(Vector3 origin, float horizontalRange, float allowedVerticalRange)
    {
        if (target == null)
        {
            return false;
        }

        float distanceX = Mathf.Abs(target.position.x - origin.x);
        float distanceY = Mathf.Abs(target.position.y - origin.y);

        return distanceX <= horizontalRange && distanceY <= allowedVerticalRange;
    }

    // 대상의 조준 위치를 반환합니다
    public Vector3 GetTargetAimPosition(Vector3 fallbackPosition)
    {
        if (target == null)
        {
            return fallbackPosition;
        }

        return target.position + targetAimOffset;
    }
}
