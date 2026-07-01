using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MonsterStatus))]
public class MonsterMeleeAI : MonsterBase
{
    private const string DefaultPlayerLayerName = "Player";

    [Header("Melee")]
    [SerializeField] private LayerMask playerHitMask;
    [SerializeField] private float attackRange = 1.3f;
    [SerializeField] private float fallbackAttackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private float attackRecovery = 0.35f;
    [SerializeField] private Vector3 attackBoxSize = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private Vector3 attackBoxOffset = new Vector3(0.8f, 0f, 0f);

    private float lastAttackEndTime = -999f;
    private bool isAttacking;

    // 컴포넌트 초기화 후 기본 레이어 마스크를 보정합니다
    protected override void Awake()
    {
        base.Awake();
        EnsureDefaultPlayerHitMask();
    }

    // 인스펙터 값 변경 시 기본 레이어 마스크를 보정합니다
    private void OnValidate()
    {
        EnsureDefaultPlayerHitMask();
    }

    // 근접 일반몹의 추적과 공격 판단을 처리합니다
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
        StopHorizontalMovement();

        yield return new WaitForSeconds(attackWindup);

        if (!IsDead)
        {
            DoAttackHitCheck();
        }

        yield return new WaitForSeconds(attackRecovery);

        lastAttackEndTime = Time.time;
        isAttacking = false;
    }

    // 공격 판정 박스 안의 플레이어를 찾아 데미지를 줍니다
    private void DoAttackHitCheck()
    {
        Vector3 center = GetAttackBoxCenter();
        Vector3 halfSize = attackBoxSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            center,
            halfSize,
            Quaternion.identity,
            playerHitMask,
            QueryTriggerInteraction.Ignore
        );

        HashSet<PlayerStatus> damagedTargets = new HashSet<PlayerStatus>();

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus playerStatus = hits[i].GetComponentInParent<PlayerStatus>();

            if (playerStatus == null)
            {
                continue;
            }

            if (damagedTargets.Contains(playerStatus))
            {
                continue;
            }

            damagedTargets.Add(playerStatus);
            ApplyDamageToPlayer(playerStatus, hits[i]);
        }
    }

    // PlayerStatus에 DamageInfo를 전달합니다
    private void ApplyDamageToPlayer(PlayerStatus playerStatus, Collider hitCollider)
    {
        Vector3 hitPoint = hitCollider.ClosestPoint(transform.position);
        Vector3 hitDirection = GetFacingDirectionVector();
        float damage = GetAttackPower(fallbackAttackDamage);

        DamageInfo damageInfo = new DamageInfo(
            playerStatus.gameObject,
            hitCollider,
            gameObject,
            hitPoint,
            hitDirection,
            damage
        );

        playerStatus.TakeDamage(damageInfo);
    }

    // 현재 바라보는 방향 기준으로 공격 판정 박스 중심을 계산합니다
    private Vector3 GetAttackBoxCenter()
    {
        Vector3 offset = GetFacingOffset(attackBoxOffset);
        return transform.position + offset;
    }

    // 기본 Player 레이어 마스크를 설정합니다
    private void EnsureDefaultPlayerHitMask()
    {
        if (playerHitMask.value != 0 && playerHitMask.value != ~0)
        {
            return;
        }

        int playerMask = LayerMask.GetMask(DefaultPlayerLayerName);

        if (playerMask == 0)
        {
            return;
        }

        playerHitMask = playerMask;
    }

    // Scene 뷰에서 감지 범위와 공격 판정 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        DrawDetectionGizmo();

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackBoxCenterForGizmo(), attackBoxSize);
    }

    // Gizmo 표시용 공격 판정 박스 중심을 계산합니다
    private Vector3 GetAttackBoxCenterForGizmo()
    {
        float direction = transform.localScale.x >= 0f ? 1f : -1f;
        Vector3 offset = new Vector3(attackBoxOffset.x * direction, attackBoxOffset.y, attackBoxOffset.z);

        return transform.position + offset;
    }
}
