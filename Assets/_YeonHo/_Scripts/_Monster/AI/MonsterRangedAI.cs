using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MonsterGroundMotor))]
public class MonsterRangedAI : MonsterBase
{
    [Header("Ranged")]
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private CommonProjectile projectilePrefab;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Vector3 firePointOffset = new Vector3(0.7f, 0.4f, 0f);
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float fallbackProjectileDamage = 10f;
    [SerializeField] private int projectilePenetrationCount = 0;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private bool aimAtTarget = false;

    private float lastAttackTime = -999f;
    private bool isAttacking;

    // 원거리 일반몹의 추적과 발사 판단을 처리합니다
    protected override void TickMonster()
    {
        if (!IsTargetInDetectionRange())
        {
            return;
        }

        FaceTarget();

        if (IsTargetInRange(attackRange, VerticalTolerance))
        {
            TryStartAttack();
            return;
        }

        if (!isAttacking)
        {
            ChaseTarget();
        }
    }

    // 원거리 공격이 가능하면 공격 코루틴을 시작합니다
    private void TryStartAttack()
    {
        if (isAttacking)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        StartCoroutine(ShootRoutine());
    }

    // 공격 선딜 후 탄환을 발사합니다
    private IEnumerator ShootRoutine()
    {
        isAttacking = true;
        StopHorizontalMovement();

        yield return new WaitForSeconds(attackWindup);

        if (!IsDead)
        {
            ShootProjectile();
        }

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // 바라보는 방향 또는 플레이어 방향으로 탄환을 생성하고 발사합니다
    private void ShootProjectile()
    {
        Vector3 spawnPosition = GetFirePosition();
        Vector3 shootDirection = aimAtTarget ? GetDirectionToTarget(spawnPosition) : GetFacingDirectionVector();
        float damage = GetAttackPower(fallbackProjectileDamage);

        ProjectileLaunchData launchData = new ProjectileLaunchData(
            spawnPosition,
            shootDirection,
            projectileSpeed,
            damage,
            gameObject,
            projectilePenetrationCount
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

    // 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return FixDepthVector(firePoint.position);
        }

        Vector3 offset = GetFacingOffset(firePointOffset);
        return FixDepthVector(transform.position + offset);
    }

    // Scene 뷰에서 감지 범위와 공격 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        DrawDetectionGizmo();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(attackRange * 2f, VerticalTolerance * 2f, 1f)
        );

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetFirePositionForGizmo(), 0.12f);
    }

    // Gizmo 표시용 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePositionForGizmo()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        float direction = transform.localScale.x >= 0f ? 1f : -1f;
        Vector3 offset = new Vector3(firePointOffset.x * direction, firePointOffset.y, firePointOffset.z);

        return transform.position + offset;
    }
}
