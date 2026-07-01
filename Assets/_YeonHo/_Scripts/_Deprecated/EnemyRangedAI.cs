using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyRangedAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float fixedZ = 0f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Detect")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float verticalTolerance = 1.5f;

    [Header("Shoot")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Vector3 firePointOffset = new Vector3(0.7f, 0.4f, 0f);
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackWindup = 0.25f;

    private CharacterController characterController;
    private Damageable selfDamageable;
    private float verticalVelocity;
    private float horizontalVelocity;
    private float lastAttackTime = -999f;
    private bool isAttacking;
    private bool facingRight = false;

    // 필요한 컴포넌트 참조를 준비합니다
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        selfDamageable = GetComponent<Damageable>();

        FixDepthPosition();
    }

    // 매 프레임 원거리 일반몹의 감지 추적 공격을 처리합니다
    private void Update()
    {
        if (selfDamageable != null && !selfDamageable.IsAlive)
        {
            return;
        }

        TryFindTarget();

        horizontalVelocity = 0f;

        if (target != null)
        {
            UpdateBehavior();
        }

        ApplyGravity();
        ApplyMovement();
        FixDepthPosition();
    }

    // 플레이어 대상을 자동으로 찾습니다
    private void TryFindTarget()
    {
        if (target != null)
        {
            return;
        }

        GameObject foundTarget = GameObject.FindGameObjectWithTag(targetTag);

        if (foundTarget != null)
        {
            target = foundTarget.transform;
        }
    }

    // 대상과의 거리에 따라 추적 또는 원거리 공격을 선택합니다
    private void UpdateBehavior()
    {
        float distanceX = Mathf.Abs(target.position.x - transform.position.x);
        float distanceY = Mathf.Abs(target.position.y - transform.position.y);

        bool canDetect = distanceX <= detectionRange && distanceY <= verticalTolerance;
        bool canShoot = distanceX <= attackRange && distanceY <= verticalTolerance;

        if (!canDetect)
        {
            return;
        }

        FaceTarget();

        if (canShoot)
        {
            TryStartAttack();
            return;
        }

        if (!isAttacking)
        {
            ChaseTarget();
        }
    }

    // 대상 방향으로 바라보도록 방향 값을 갱신합니다
    private void FaceTarget()
    {
        if (target.position.x > transform.position.x)
        {
            facingRight = true;
        }
        else if (target.position.x < transform.position.x)
        {
            facingRight = false;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    // 대상 방향으로 수평 이동합니다
    private void ChaseTarget()
    {
        float direction = target.position.x > transform.position.x ? 1f : -1f;
        horizontalVelocity = direction * moveSpeed;
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
        horizontalVelocity = 0f;

        yield return new WaitForSeconds(attackWindup);

        ShootProjectile();

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // 바라보는 방향으로 탄환을 생성하고 발사합니다
    private void ShootProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab is missing");
            return;
        }

        Vector3 spawnPosition = GetFirePosition();
        Vector3 shootDirection = facingRight ? Vector3.right : Vector3.left;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Initialize(shootDirection, projectileSpeed, projectileDamage, gameObject);
    }

    // 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        float direction = facingRight ? 1f : -1f;
        Vector3 offset = new Vector3(firePointOffset.x * direction, firePointOffset.y, firePointOffset.z);

        return transform.position + offset;
    }

    // 중력 값을 계산합니다
    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    // 계산된 이동 값을 CharacterController에 적용합니다
    private void ApplyMovement()
    {
        Vector3 movement = new Vector3(horizontalVelocity, verticalVelocity, 0f);
        characterController.Move(movement * Time.deltaTime);
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    private void FixDepthPosition()
    {
        Vector3 position = transform.position;
        position.z = fixedZ;
        transform.position = position;
    }

    // Scene 뷰에서 감지 범위와 공격 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(detectionRange * 2f, verticalTolerance * 2f, 1f)
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(attackRange * 2f, verticalTolerance * 2f, 1f)
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

        float direction = facingRight ? 1f : -1f;
        Vector3 offset = new Vector3(firePointOffset.x * direction, firePointOffset.y, firePointOffset.z);

        return transform.position + offset;
    }
}