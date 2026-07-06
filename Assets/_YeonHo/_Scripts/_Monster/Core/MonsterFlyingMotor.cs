using UnityEngine;

public class MonsterFlyingMotor : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float patrolPointTolerance = 0.15f;

    [Header("Idle Motion")]
    [SerializeField] private float hoverAmplitude = 0.18f;
    [SerializeField] private float hoverFrequency = 2f;

    [Header("Shake")]
    [SerializeField] private float shakeAmplitude = 0.08f;
    [SerializeField] private float shakeFrequency = 35f;

    private Vector3 homePosition;
    private float fixedZ;
    private int patrolDirection = 1;
    private bool isInitialized;
    private bool isShaking;

    // 공중 이동 기준 위치와 고정 Z값을 준비합니다
    public void Initialize(float newFixedZ)
    {
        fixedZ = newFixedZ;
        homePosition = FixDepthVector(transform.position);
        transform.position = homePosition;
        isInitialized = true;
    }

    // 초기 위치를 기준으로 좌우 순찰 이동을 처리합니다
    public void PatrolAroundHome(float speed)
    {
        EnsureInitialized();

        Vector3 targetPosition = homePosition + Vector3.right * patrolDirection * patrolDistance;
        bool reached = MoveTowardPosition(targetPosition, speed, patrolPointTolerance, true);

        if (reached)
        {
            patrolDirection *= -1;
        }
    }

    // 특정 대상 위의 공중 위치를 향해 이동합니다
    public bool MoveTowardHoverPoint(Vector3 anchorPosition, float heightOffset, float speed, float stopDistance)
    {
        Vector3 targetPosition = new Vector3(anchorPosition.x, anchorPosition.y + heightOffset, fixedZ);
        return MoveTowardPosition(targetPosition, speed, stopDistance, true);
    }

    // 특정 월드 위치를 향해 이동합니다
    public bool MoveTowardPosition(Vector3 targetPosition, float speed, float stopDistance, bool useIdleMotion)
    {
        EnsureInitialized();

        Vector3 fixedTargetPosition = FixDepthVector(targetPosition);

        if (useIdleMotion)
        {
            fixedTargetPosition += GetIdleMotionOffset();
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            fixedTargetPosition,
            Mathf.Max(0f, speed) * Time.deltaTime
        );

        transform.position = FixDepthVector(transform.position);

        return Vector3.Distance(transform.position, fixedTargetPosition) <= Mathf.Max(0f, stopDistance);
    }

    // 특정 위치에 머무르며 부유 연출을 적용합니다
    public void HoldAt(Vector3 anchorPosition)
    {
        EnsureInitialized();

        Vector3 holdPosition = FixDepthVector(anchorPosition) + GetIdleMotionOffset();
        transform.position = FixDepthVector(holdPosition);
    }

    // 자폭 준비처럼 떨림이 필요한 상태를 시작합니다
    public void BeginShake()
    {
        isShaking = true;
    }

    // 떨림 상태를 종료합니다
    public void EndShake()
    {
        isShaking = false;
    }

    // 현재 위치를 기준으로 순찰 중심을 다시 잡습니다
    public void ResetHomePosition()
    {
        EnsureInitialized();
        homePosition = FixDepthVector(transform.position);
    }

    // 현재 위치의 Z값을 고정값으로 보정합니다
    public void SnapToFixedZ()
    {
        EnsureInitialized();
        transform.position = FixDepthVector(transform.position);
    }

    // 초기화가 누락된 테스트 배치에서도 최소 동작을 보장합니다
    private void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        Initialize(transform.position.z);
    }

    // 부유와 떨림 연출에 사용할 오프셋을 계산합니다
    private Vector3 GetIdleMotionOffset()
    {
        float hoverOffsetY = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        Vector3 offset = Vector3.up * hoverOffsetY;

        if (isShaking)
        {
            float shakeOffsetX = Mathf.Sin(Time.time * shakeFrequency) * shakeAmplitude;
            float shakeOffsetY = Mathf.Cos(Time.time * shakeFrequency * 0.73f) * shakeAmplitude;
            offset += new Vector3(shakeOffsetX, shakeOffsetY, 0f);
        }

        return offset;
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    private Vector3 FixDepthVector(Vector3 position)
    {
        position.z = fixedZ;
        return position;
    }
}