using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private LayerMask targetHitMask;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float fixedZ = 0f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Detect")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float verticalTolerance = 1.5f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private float attackRecovery = 0.35f;
    [SerializeField] private Vector3 attackBoxSize = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private Vector3 attackBoxOffset = new Vector3(0.8f, 0f, 0f);

    private CharacterController characterController;
    private Damageable selfDamageable;
    private float verticalVelocity;
    private float horizontalVelocity;
    private float lastAttackEndTime = -999f;
    private bool isAttacking;
    private bool facingRight = false;

    // 필요한 컴포넌트를 준비합니다
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        selfDamageable = GetComponent<Damageable>();

        FixDepthPosition();
    }

    // 매 프레임 일반몹의 감지 추적 공격을 처리합니다
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

    // 대상과의 거리에 따라 추적 또는 공격을 선택합니다
    private void UpdateBehavior()
    {
        float distanceX = Mathf.Abs(target.position.x - transform.position.x);
        float distanceY = Mathf.Abs(target.position.y - transform.position.y);

        bool canDetect = distanceX <= detectionRange && distanceY <= verticalTolerance;
        bool canAttack = distanceX <= attackRange && distanceY <= verticalTolerance;

        if (!canDetect)
        {
            return;
        }

        FaceTarget();

        if (canAttack)
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

    // 공격이 가능하면 공격 코루틴을 시작합니다
    private void TryStartAttack()
    {
        if (isAttacking)
        {
            return;
        }

        if (Time.time < lastAttackEndTime + attackCooldown)
        {
            return;
        }

        StartCoroutine(AttackRoutine());
    }

    // 공격 선딜 후 피격 판정을 실행합니다
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        horizontalVelocity = 0f;

        yield return new WaitForSeconds(attackWindup);

        DoAttackHitCheck();

        yield return new WaitForSeconds(attackRecovery);

        lastAttackEndTime = Time.time;
        isAttacking = false;
    }

    // 공격 판정 박스 안의 대상을 찾아 데미지를 줍니다
    private void DoAttackHitCheck()
    {
        Vector3 center = GetAttackBoxCenter();
        Vector3 halfSize = attackBoxSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            center,
            halfSize,
            Quaternion.identity,
            targetHitMask,
            QueryTriggerInteraction.Ignore
        );

        HashSet<Damageable> damagedTargets = new HashSet<Damageable>();

        for (int i = 0; i < hits.Length; i++)
        {
            Damageable damageable = hits[i].GetComponentInParent<Damageable>();

            if (damageable == null)
            {
                continue;
            }

            if (damageable == selfDamageable)
            {
                continue;
            }

            if (!damageable.IsAlive)
            {
                continue;
            }

            if (damagedTargets.Contains(damageable))
            {
                continue;
            }

            damagedTargets.Add(damageable);
            damageable.TakeDamage(attackDamage, gameObject);
        }
    }

    // 현재 바라보는 방향 기준으로 공격 판정 박스 중심을 계산합니다
    private Vector3 GetAttackBoxCenter()
    {
        float direction = facingRight ? 1f : -1f;
        Vector3 offset = new Vector3(attackBoxOffset.x * direction, attackBoxOffset.y, attackBoxOffset.z);

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

    // Scene 뷰에서 감지 범위와 공격 판정 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(detectionRange * 2f, verticalTolerance * 2f, 1f)
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackBoxCenterForGizmo(), attackBoxSize);
    }

    // Gizmo 표시용 공격 판정 박스 중심을 계산합니다
    private Vector3 GetAttackBoxCenterForGizmo()
    {
        float direction = facingRight ? 1f : -1f;
        Vector3 offset = new Vector3(attackBoxOffset.x * direction, attackBoxOffset.y, attackBoxOffset.z);

        return transform.position + offset;
    }
}