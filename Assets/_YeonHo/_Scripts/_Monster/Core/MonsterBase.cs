using UnityEngine;

[RequireComponent(typeof(MonsterHealth))]
[RequireComponent(typeof(MonsterStats))]
[RequireComponent(typeof(MonsterFacing))]
[RequireComponent(typeof(MonsterTargetSensor))]
public abstract class MonsterBase : MonoBehaviour
{
    [Header("Depth")]
    [SerializeField] private float fixedZ = 0f;

    [Header("Fallback")]
    [SerializeField] private float fallbackMoveSpeed = 2.5f;

    private MonsterHealth monsterHealth;
    private MonsterStats monsterStats;
    private MonsterFacing monsterFacing;
    private MonsterTargetSensor targetSensor;
    private MonsterGroundMotor groundMotor;
    private bool deadStateEntered;

    protected Transform Target
    {
        get { return targetSensor != null ? targetSensor.Target : null; }
    }

    protected PlayerStatus TargetStatus
    {
        get { return targetSensor != null ? targetSensor.TargetStatus : null; }
    }

    protected MonsterHealth SelfHealth
    {
        get { return monsterHealth; }
    }

    protected MonsterGroundMotor GroundMotor
    {
        get { return groundMotor; }
    }

    protected float FixedZ
    {
        get { return fixedZ; }
    }

    protected float DetectionRange
    {
        get { return targetSensor != null ? targetSensor.DetectionRange : 0f; }
    }

    protected float VerticalTolerance
    {
        get { return targetSensor != null ? targetSensor.VerticalTolerance : 0f; }
    }

    protected bool FacingRight
    {
        get { return monsterFacing != null && monsterFacing.FacingRight; }
    }

    protected bool IsDead
    {
        get { return monsterHealth != null && monsterHealth.IsDead; }
    }

    protected virtual bool UsesCharacterMotor
    {
        get { return true; }
    }

    // 몬스터 공통 컴포넌트를 준비합니다
    protected virtual void Awake()
    {
        monsterHealth = GetComponent<MonsterHealth>();
        monsterStats = GetComponent<MonsterStats>();
        monsterFacing = GetComponent<MonsterFacing>();
        targetSensor = GetComponent<MonsterTargetSensor>();
        groundMotor = GetComponent<MonsterGroundMotor>();

        if (groundMotor != null)
        {
            groundMotor.Initialize(fixedZ);
        }

        FixDepthPosition();
    }

    // 몬스터의 공통 생명주기와 AI 틱을 실행합니다
    protected virtual void Update()
    {
        if (IsDead)
        {
            EnterDeadStateIfNeeded();
            return;
        }

        if (targetSensor != null)
        {
            targetSensor.RefreshTarget();
        }

        if (UsesCharacterMotor && groundMotor != null)
        {
            groundMotor.ResetHorizontalVelocity();
        }

        if (Target != null)
        {
            TickMonster();
        }

        if (UsesCharacterMotor && groundMotor != null)
        {
            groundMotor.TickMovement();
        }
        else
        {
            FixDepthPosition();
        }
    }

    // 자식 클래스에서 몬스터별 행동을 구현합니다
    protected abstract void TickMonster();

    // 사망 상태에 한 번만 진입합니다
    private void EnterDeadStateIfNeeded()
    {
        if (deadStateEntered)
        {
            return;
        }

        deadStateEntered = true;
        StopHorizontalMovement();
        OnDeadStateEntered();
    }

    // 자식 클래스에서 사망 상태 진입 후처리를 확장합니다
    protected virtual void OnDeadStateEntered()
    {
    }

    // 대상이 기본 감지 범위 안에 있는지 확인합니다
    protected bool IsTargetInDetectionRange()
    {
        if (targetSensor == null)
        {
            return false;
        }

        return targetSensor.IsTargetInDetectionRange(transform.position);
    }

    // 대상이 지정한 수평 수직 범위 안에 있는지 확인합니다
    protected bool IsTargetInRange(float horizontalRange, float allowedVerticalRange)
    {
        if (targetSensor == null)
        {
            return false;
        }

        return targetSensor.IsTargetInRange(transform.position, horizontalRange, allowedVerticalRange);
    }

    // 대상 방향으로 바라보도록 방향 값을 갱신합니다
    protected void FaceTarget()
    {
        if (monsterFacing == null)
        {
            return;
        }

        monsterFacing.FaceTarget(Target);
    }

    // 몬스터의 바라보는 방향을 설정합니다
    protected void SetFacingDirection(bool lookRight)
    {
        if (monsterFacing == null)
        {
            return;
        }

        monsterFacing.SetFacingDirection(lookRight);
    }

    // 대상 방향으로 수평 이동합니다
    protected void ChaseTarget()
    {
        if (Target == null)
        {
            return;
        }

        float direction = Target.position.x > transform.position.x ? 1f : -1f;
        SetHorizontalVelocity(direction * GetMoveSpeed());
    }

    // 수평 이동 속도를 설정합니다
    protected void SetHorizontalVelocity(float velocity)
    {
        if (groundMotor == null)
        {
            return;
        }

        groundMotor.SetHorizontalVelocity(velocity);
    }

    // 수평 이동을 멈춥니다
    protected void StopHorizontalMovement()
    {
        if (groundMotor == null)
        {
            return;
        }

        groundMotor.StopHorizontalMovement();
    }

    // 몬스터 이동 속도를 가져옵니다
    protected float GetMoveSpeed()
    {
        if (monsterStats == null)
        {
            return fallbackMoveSpeed;
        }

        return monsterStats.GetMoveSpeedOrFallback(fallbackMoveSpeed);
    }

    // 몬스터 공격력을 가져옵니다
    protected float GetAttackPower(float fallbackDamage)
    {
        if (monsterStats == null)
        {
            return fallbackDamage;
        }

        return monsterStats.GetAttackPowerOrFallback(fallbackDamage);
    }

    // 현재 바라보는 방향을 월드 방향으로 반환합니다
    protected Vector3 GetFacingDirectionVector()
    {
        if (monsterFacing == null)
        {
            return Vector3.right;
        }

        return monsterFacing.GetFacingDirectionVector();
    }

    // 현재 바라보는 방향에 맞춰 오프셋을 계산합니다
    protected Vector3 GetFacingOffset(Vector3 offset)
    {
        if (monsterFacing == null)
        {
            return offset;
        }

        return monsterFacing.GetFacingOffset(offset);
    }

    // 대상의 조준 위치를 반환합니다
    protected Vector3 GetTargetAimPosition()
    {
        Vector3 fallbackPosition = transform.position + GetFacingDirectionVector();

        if (targetSensor == null)
        {
            return fallbackPosition;
        }

        return targetSensor.GetTargetAimPosition(fallbackPosition);
    }

    // 지정한 위치에서 대상 조준 위치로 향하는 방향을 계산합니다
    protected Vector3 GetDirectionToTarget(Vector3 origin)
    {
        Vector3 direction = GetTargetAimPosition() - origin;
        direction.z = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return GetFacingDirectionVector();
        }

        return direction.normalized;
    }

    // CharacterController 사용 여부를 변경합니다
    protected void SetCharacterControllerEnabled(bool enabled)
    {
        if (groundMotor == null)
        {
            return;
        }

        groundMotor.SetCharacterControllerEnabled(enabled);
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    protected void FixDepthPosition()
    {
        transform.position = FixDepthVector(transform.position);
    }

    // Vector3의 Z 위치를 고정합니다
    protected Vector3 FixDepthVector(Vector3 position)
    {
        position.z = fixedZ;
        return position;
    }

    // Scene 뷰에서 공통 감지 범위를 표시합니다
    protected void DrawDetectionGizmo()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(DetectionRange * 2f, VerticalTolerance * 2f, 1f)
        );
    }
}
