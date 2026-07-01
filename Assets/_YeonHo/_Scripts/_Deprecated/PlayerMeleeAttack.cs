using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private LayerMask enemyHitMask;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private Vector3 attackBoxSize = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private Vector3 attackBoxOffset = new Vector3(0.8f, 0f, 0f);

    private PlayerPrototypeController playerController;
    private float lastAttackTime = -999f;

    // 플레이어 컨트롤러 참조를 준비합니다
    private void Awake()
    {
        playerController = GetComponent<PlayerPrototypeController>();
    }

    // 공격 입력을 받았을 때 근접 공격을 시도합니다
    public void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        DoAttackHitCheck();
    }

    // 공격 판정 박스 안의 일반몹을 찾아 데미지를 줍니다
    private void DoAttackHitCheck()
    {
        Vector3 center = GetAttackBoxCenter();
        Vector3 halfSize = attackBoxSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            center,
            halfSize,
            Quaternion.identity,
            enemyHitMask,
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
        float direction = IsFacingRight() ? 1f : -1f;
        Vector3 offset = new Vector3(attackBoxOffset.x * direction, attackBoxOffset.y, attackBoxOffset.z);

        return transform.position + offset;
    }

    // 플레이어가 바라보는 방향을 확인합니다
    private bool IsFacingRight()
    {
        if (playerController != null)
        {
            return playerController.IsFacingRight;
        }

        return transform.localScale.x >= 0f;
    }

    // Scene 뷰에서 플레이어 공격 판정 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
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