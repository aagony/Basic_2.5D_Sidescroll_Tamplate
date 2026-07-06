using UnityEngine;

// 패리와 피격 흐름을 확인하기 위한 임시 원거리 몬스터입니다.
// 일정 주기로 테스트 몬스터 투사체를 플레이어 방향으로 발사합니다.
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(MonsterStatus))]
public class ParryTestMonster : MonoBehaviour
{
    [Header("Target")]
    // 비워두면 씬에서 PlayerStatus를 가진 오브젝트를 자동으로 찾아 사용합니다.
    [SerializeField] private Transform target;

    // 발사 방향이 플레이어 발밑이 아니라 몸통 쪽을 향하도록 더하는 보정값입니다.
    [SerializeField] private Vector3 targetAimOffset = Vector3.up;

    [Header("Ranged Attack")]
    // 테스트용 몬스터 투사체 프리팹입니다.
    [SerializeField] private TestMonsterProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;

    // 활성화 직후 첫 발사까지 기다리는 시간입니다.
    [SerializeField] private float firstRangedAttackDelay = 0.5f;

    // 원거리 공격을 반복하는 주기입니다.
    [SerializeField] private float rangedAttackInterval = 1.5f;

    // 테스트 투사체가 요청할 데미지입니다.
    [SerializeField] private float rangedAttackDamage = 5f;

    private float rangedAttackTimer;

    private void Awake()
    {
        EnsureMonsterStatus();
    }

    private void OnEnable()
    {
        rangedAttackTimer = firstRangedAttackDelay;
    }

    private void Update()
    {
        FindTargetIfNeeded();
        if (target == null) return;

        FaceTarget();
        UpdateRangedAttack();
    }

    private void UpdateRangedAttack()
    {
        // 타이머가 끝날 때마다 한 발 발사하고 다음 주기를 다시 설정합니다.
        rangedAttackTimer -= Time.deltaTime;
        if (rangedAttackTimer > 0f) return;

        FireRangedAttack();
        rangedAttackTimer = rangedAttackInterval;
    }

    private void FireRangedAttack()
    {
        if (bulletPrefab == null) return;

        // firePoint가 없으면 몬스터 자신의 위치에서 발사합니다.
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = (target.position + targetAimOffset) - spawnPosition;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            direction = transform.forward;

        TestMonsterProjectile projectile = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.LookRotation(direction.normalized)
        );

        projectile.Init(direction, rangedAttackDamage, gameObject);
    }

    private void FaceTarget()
    {
        // y축 차이는 무시하고 수평 방향으로만 플레이어를 바라봅니다.
        Vector3 direction = GetFlatDirectionToTarget();
        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private Vector3 GetFlatDirectionToTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return transform.forward;

        return direction.normalized;
    }

    private void FindTargetIfNeeded()
    {
        if (target != null) return;

        // 테스트 편의용 자동 탐색입니다. 실제 몬스터 구현에서는 별도 타겟 주입으로 교체할 수 있습니다.
        PlayerStatus playerStatus = FindFirstObjectByType<PlayerStatus>();
        if (playerStatus != null)
            target = playerStatus.transform;
    }

    private void EnsureMonsterStatus()
    {
        if (GetComponent<MonsterStatus>() != null) return;

        gameObject.AddComponent<MonsterStatus>();
    }
}
