using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MonsterFlyingMotor))]
public class MonsterFlyingDropAI : MonsterBase
{
    [Header("Flying")]
    [SerializeField] private MonsterFlyingMotor flyingMotor;
    [SerializeField] private float patrolSpeedMultiplier = 1f;
    [SerializeField] private float chaseSpeedMultiplier = 1.15f;
    [SerializeField] private float hoverHeightAboveTarget = 4f;
    [SerializeField] private float flyingDetectionVerticalRange = 8f;
    [SerializeField] private float chaseStopDistance = 0.15f;

    [Header("Drop Attack")]
    [SerializeField] private CommonProjectile projectilePrefab;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private Vector3 dropPointOffset = new Vector3(0f, -0.4f, 0f);
    [SerializeField] private float dropHorizontalRange = 1.2f;
    [SerializeField] private float dropVerticalRange = 8f;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float fallbackProjectileDamage = 10f;
    [SerializeField] private int projectilePenetrationCount = 0;
    [SerializeField] private float projectileLifeTime = 3f;
    [SerializeField] private float attackCooldown = 1.4f;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private bool aimAtTarget = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "Attack";

    private float lastAttackTime = -999f;
    private bool isAttacking;

    protected override bool UsesCharacterMotor
    {
        get { return false; }
    }

    // 공중 투하 몬스터에 필요한 참조를 준비합니다
    protected override void Awake()
    {
        base.Awake();

        if (flyingMotor == null)
        {
            flyingMotor = GetComponent<MonsterFlyingMotor>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (flyingMotor != null)
        {
            flyingMotor.Initialize(FixedZ);
        }
    }

    // 공중 투하 몬스터의 순찰과 추적과 투하 공격을 처리합니다
    protected override void TickMonster()
    {
        if (flyingMotor == null)
        {
            return;
        }

        if (isAttacking)
        {
            return;
        }

        if (!IsTargetInFlyingDetectionRange())
        {
            Patrol();
            return;
        }

        FaceTarget();

        if (IsTargetInDropRange())
        {
            TryStartDropAttack();
            return;
        }

        ChaseTargetInAir();
    }

    // 플레이어가 공중 몬스터 전용 감지 범위 안에 있는지 확인합니다
    private bool IsTargetInFlyingDetectionRange()
    {
        return IsTargetInRange(DetectionRange, flyingDetectionVerticalRange);
    }

    // 플레이어가 투하 공격 범위 안에 있는지 확인합니다
    private bool IsTargetInDropRange()
    {
        if (Target == null)
        {
            return false;
        }

        float distanceX = Mathf.Abs(Target.position.x - transform.position.x);
        float distanceY = Mathf.Abs(Target.position.y - transform.position.y);
        bool targetIsBelow = Target.position.y <= transform.position.y;

        return targetIsBelow && distanceX <= dropHorizontalRange && distanceY <= dropVerticalRange;
    }

    // 초기 위치를 기준으로 공중 순찰을 실행합니다
    private void Patrol()
    {
        float speed = GetMoveSpeed() * patrolSpeedMultiplier;
        flyingMotor.PatrolAroundHome(speed);
    }

    // 플레이어 위쪽의 공중 위치를 향해 이동합니다
    private void ChaseTargetInAir()
    {
        if (Target == null)
        {
            Patrol();
            return;
        }

        float speed = GetMoveSpeed() * chaseSpeedMultiplier;
        flyingMotor.MoveTowardHoverPoint(
            Target.position,
            hoverHeightAboveTarget,
            speed,
            chaseStopDistance
        );
    }

    // 공격 쿨타임이 끝났다면 투하 공격을 시작합니다
    private void TryStartDropAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        StartCoroutine(DropAttackRoutine());
    }

    // 공격 선딜 후 아래 방향으로 투사체를 투하합니다
    private IEnumerator DropAttackRoutine()
    {
        isAttacking = true;

        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        Vector3 holdPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < attackWindup)
        {
            if (IsDead)
            {
                isAttacking = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            flyingMotor.HoldAt(holdPosition);
            yield return null;
        }

        if (!IsDead)
        {
            DropProjectile();
        }

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // 투하 위치와 방향을 계산해 투사체를 생성합니다
    private void DropProjectile()
    {
        Vector3 spawnPosition = GetDropPosition();
        Vector3 dropDirection = aimAtTarget ? GetDirectionToTarget(spawnPosition) : Vector3.down;
        float damage = GetAttackPower(fallbackProjectileDamage);

        ProjectileLaunchData launchData = new ProjectileLaunchData(
            spawnPosition,
            dropDirection,
            projectileSpeed,
            damage,
            gameObject,
            projectilePenetrationCount,
            projectileLifeTime
        );

        SpawnProjectile(launchData);
    }

    // 풀 또는 프리팹으로 투사체를 생성합니다
    private void SpawnProjectile(ProjectileLaunchData launchData)
    {
        if (projectilePool != null)
        {
            projectilePool.Spawn(launchData);
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("CommonProjectile prefab is missing", this);
            return;
        }

        CommonProjectile projectile = Instantiate(projectilePrefab, launchData.Position, Quaternion.identity);
        projectile.MarkTakenFromPool();
        projectile.Launch(launchData);
    }

    // 투사체가 생성될 위치를 계산합니다
    private Vector3 GetDropPosition()
    {
        if (dropPoint != null)
        {
            return FixDepthVector(dropPoint.position);
        }

        return FixDepthVector(transform.position + dropPointOffset);
    }

    // Scene 뷰에서 공중 감지 범위와 투하 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(DetectionRange * 2f, flyingDetectionVerticalRange * 2f, 1f)
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(dropHorizontalRange * 2f, dropVerticalRange * 2f, 1f)
        );

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetDropPositionForGizmo(), 0.12f);
    }

    // Gizmo 표시용 투하 위치를 계산합니다
    private Vector3 GetDropPositionForGizmo()
    {
        if (dropPoint != null)
        {
            return dropPoint.position;
        }

        return transform.position + dropPointOffset;
    }
}