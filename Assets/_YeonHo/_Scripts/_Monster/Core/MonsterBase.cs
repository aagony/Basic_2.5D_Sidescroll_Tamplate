using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MonsterStatus))]
public abstract class MonsterBase : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetAimOffset = Vector3.up;
    [SerializeField] private bool autoFindPlayerStatus = true;

    [Header("Move")]
    [SerializeField] private float fallbackMoveSpeed = 2.5f;
    [SerializeField] private float fixedZ = 0f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedStickForce = -2f;
    [SerializeField] private bool invertVisualFacing = false;

    [Header("Detect")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float verticalTolerance = 1.5f;

    [Header("Death")]
    [SerializeField] private Animator animator;
    [SerializeField] private string deadTriggerName = "Dead";
    [SerializeField] private bool destroyAfterDeath = true;
    [SerializeField] private float destroyDelay = 1.5f;
    [SerializeField] private bool disableCharacterControllerOnDeath = true;

    private CharacterController characterController;
    private MonsterStatus monsterStatus;
    private PlayerStatus targetStatus;
    private float verticalVelocity;
    private float horizontalVelocity;
    private bool facingRight = true;
    private bool deadStateEntered;

    protected Transform Target
    {
        get { return target; }
    }

    protected PlayerStatus TargetStatus
    {
        get { return targetStatus; }
    }

    protected MonsterStatus SelfStatus
    {
        get { return monsterStatus; }
    }

    protected CharacterController MonsterCharacterController
    {
        get { return characterController; }
    }

    protected float FixedZ
    {
        get { return fixedZ; }
    }

    protected float DetectionRange
    {
        get { return detectionRange; }
    }

    protected float VerticalTolerance
    {
        get { return verticalTolerance; }
    }

    protected bool FacingRight
    {
        get { return facingRight; }
    }

    protected bool IsDead
    {
        get { return monsterStatus != null && monsterStatus.Status != null && monsterStatus.Status.IsDead; }
    }

    protected virtual bool UsesCharacterMotor
    {
        get { return true; }
    }

    // 몬스터 공통 컴포넌트와 기본 위치를 준비합니다
    protected virtual void Awake()
    {
        characterController = GetComponent<CharacterController>();
        monsterStatus = GetComponent<MonsterStatus>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        FixDepthPosition();
    }

    // 몬스터의 공통 생명주기와 이동 처리를 실행합니다
    protected virtual void Update()
    {
        if (IsDead)
        {
            EnterDeadStateIfNeeded();
            return;
        }

        TryFindTarget();

        horizontalVelocity = 0f;

        if (target != null)
        {
            TickMonster();
        }

        if (UsesCharacterMotor)
        {
            ApplyGravity();
            ApplyMovement();
        }

        FixDepthPosition();
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
        horizontalVelocity = 0f;
        verticalVelocity = 0f;

        if (disableCharacterControllerOnDeath && characterController != null)
        {
            characterController.enabled = false;
        }

        if (animator != null && !string.IsNullOrEmpty(deadTriggerName))
        {
            animator.SetTrigger(deadTriggerName);
        }

        OnDeadStateEntered();

        if (destroyAfterDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // 자식 클래스에서 사망 상태 진입 후처리를 확장합니다
    protected virtual void OnDeadStateEntered()
    {
    }

    // 플레이어 Status를 가진 대상을 자동으로 찾습니다
    protected void TryFindTarget()
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

    // 대상이 감지 범위 안에 있는지 확인합니다
    protected bool IsTargetInDetectionRange()
    {
        return IsTargetInRange(detectionRange, verticalTolerance);
    }

    // 대상이 지정한 수평 수직 범위 안에 있는지 확인합니다
    protected bool IsTargetInRange(float horizontalRange, float allowedVerticalRange)
    {
        if (target == null)
        {
            return false;
        }

        float distanceX = Mathf.Abs(target.position.x - transform.position.x);
        float distanceY = Mathf.Abs(target.position.y - transform.position.y);

        return distanceX <= horizontalRange && distanceY <= allowedVerticalRange;
    }

    // 대상 방향으로 바라보도록 방향 값을 갱신합니다
    protected void FaceTarget()
    {
        if (target == null)
        {
            return;
        }

        if (target.position.x > transform.position.x)
        {
            SetFacingDirection(true);
        }
        else if (target.position.x < transform.position.x)
        {
            SetFacingDirection(false);
        }
    }

    // 몬스터의 바라보는 방향을 설정합니다
    protected void SetFacingDirection(bool lookRight)
    {
        facingRight = lookRight;

        float visualSign = facingRight ? 1f : -1f;

        if (invertVisualFacing)
        {
            visualSign *= -1f;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * visualSign;
        transform.localScale = scale;
    }

    // 대상 방향으로 수평 이동합니다
    protected void ChaseTarget()
    {
        if (target == null)
        {
            return;
        }

        float direction = target.position.x > transform.position.x ? 1f : -1f;
        SetHorizontalVelocity(direction * GetMoveSpeed());
    }

    // 수평 이동 속도를 설정합니다
    protected void SetHorizontalVelocity(float velocity)
    {
        horizontalVelocity = velocity;
    }

    // 수평 이동을 멈춥니다
    protected void StopHorizontalMovement()
    {
        horizontalVelocity = 0f;
    }

    // 몬스터 스탯 기준 이동 속도를 가져옵니다
    protected float GetMoveSpeed()
    {
        if (monsterStatus == null)
        {
            return fallbackMoveSpeed;
        }

        float statusMoveSpeed = monsterStatus.GetMoveSpeed();

        if (statusMoveSpeed <= 0f)
        {
            return fallbackMoveSpeed;
        }

        return statusMoveSpeed;
    }

    // 몬스터 스탯 기준 공격력을 가져옵니다
    protected float GetAttackPower(float fallbackDamage)
    {
        if (monsterStatus == null)
        {
            return fallbackDamage;
        }

        float statusAttackPower = monsterStatus.GetAttackPower();

        if (statusAttackPower <= 0f)
        {
            return fallbackDamage;
        }

        return statusAttackPower;
    }

    // 현재 바라보는 방향을 월드 방향으로 반환합니다
    protected Vector3 GetFacingDirectionVector()
    {
        return facingRight ? Vector3.right : Vector3.left;
    }

    // 현재 바라보는 방향에 맞춰 오프셋을 계산합니다
    protected Vector3 GetFacingOffset(Vector3 offset)
    {
        float direction = facingRight ? 1f : -1f;
        return new Vector3(offset.x * direction, offset.y, offset.z);
    }

    // 대상의 조준 위치를 반환합니다
    protected Vector3 GetTargetAimPosition()
    {
        if (target == null)
        {
            return transform.position + GetFacingDirectionVector();
        }

        return target.position + targetAimOffset;
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
        if (characterController == null)
        {
            return;
        }

        characterController.enabled = enabled;
    }

    // 중력 값을 계산합니다
    private void ApplyGravity()
    {
        if (characterController == null || !characterController.enabled)
        {
            return;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    // 계산된 이동 값을 CharacterController에 적용합니다
    private void ApplyMovement()
    {
        if (characterController == null || !characterController.enabled)
        {
            return;
        }

        Vector3 movement = new Vector3(horizontalVelocity, verticalVelocity, 0f);
        characterController.Move(movement * Time.deltaTime);
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
            new Vector3(detectionRange * 2f, verticalTolerance * 2f, 1f)
        );
    }
}
