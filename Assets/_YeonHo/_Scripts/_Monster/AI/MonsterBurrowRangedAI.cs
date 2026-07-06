using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MonsterGroundMotor))]
public class MonsterBurrowRangedAI : MonsterBase
{
    [Header("Burrow")]
    [SerializeField] private float burrowDepth = 2.5f;
    [SerializeField] private float burrowDownDuration = 0.35f;
    [SerializeField] private float hiddenDuration = 0.6f;
    [SerializeField] private float emergeDuration = 0.35f;
    [SerializeField] private float reappearDistanceFromPlayer = 6f;
    [SerializeField] private float reappearDistanceRandomRange = 2f;
    [SerializeField] private bool alternateReappearSide = true;
    [SerializeField] private bool useTargetYForReappear = false;
    [SerializeField] private float reappearYOffset = 0f;

    [Header("Shoot")]
    [SerializeField] private CommonProjectile projectilePrefab;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private Vector3 firePointOffset = new Vector3(0.7f, 0.4f, 0f);
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float fallbackProjectileDamage = 10f;
    [SerializeField] private int projectilePenetrationCount = 0;
    [SerializeField] private float shootWindup = 0.25f;
    [SerializeField] private float patternCooldown = 1.5f;

    private Renderer[] renderers;
    private float lastPatternEndTime = -999f;
    private bool isRunningPattern;
    private int reappearSideSign = 1;
    private float lastSurfaceY;

    protected override bool UsesCharacterMotor
    {
        get { return !isRunningPattern; }
    }

    // 땅파기 원거리 몹에 필요한 참조를 준비합니다
    protected override void Awake()
    {
        base.Awake();
        renderers = GetComponentsInChildren<Renderer>();
        lastSurfaceY = transform.position.y;
    }

    // 땅파기 원거리 몹의 패턴 시작 조건을 처리합니다
    protected override void TickMonster()
    {
        if (isRunningPattern)
        {
            return;
        }

        if (Time.time < lastPatternEndTime + patternCooldown)
        {
            return;
        }

        if (!IsTargetInDetectionRange())
        {
            return;
        }

        StartCoroutine(BurrowPatternRoutine());
    }

    // 땅속으로 숨고 다른 위치에서 나온 뒤 원거리 공격을 실행합니다
    private IEnumerator BurrowPatternRoutine()
    {
        isRunningPattern = true;
        lastSurfaceY = transform.position.y;

        yield return StartCoroutine(BurrowDownRoutine());

        if (IsDead)
        {
            yield break;
        }

        SetVisible(false);

        Vector3 emergePosition = FindReappearPosition();
        Vector3 hiddenPosition = emergePosition + Vector3.down * burrowDepth;
        transform.position = FixDepthVector(hiddenPosition);

        yield return new WaitForSeconds(hiddenDuration);

        if (IsDead)
        {
            yield break;
        }

        SetVisible(true);

        yield return StartCoroutine(EmergeUpRoutine(emergePosition));

        if (IsDead)
        {
            yield break;
        }

        FaceTarget();

        yield return new WaitForSeconds(shootWindup);

        if (!IsDead)
        {
            ShootAtTarget();
        }

        lastPatternEndTime = Time.time;
        isRunningPattern = false;
    }

    // 현재 위치에서 아래로 내려가며 사라지는 연출을 처리합니다
    private IEnumerator BurrowDownRoutine()
    {
        SetCharacterControllerEnabled(false);

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * burrowDepth;

        yield return StartCoroutine(MoveTransformRoutine(startPosition, endPosition, burrowDownDuration));
    }

    // 땅 아래 위치에서 위로 올라오는 연출을 처리합니다
    private IEnumerator EmergeUpRoutine(Vector3 emergePosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = emergePosition;

        yield return StartCoroutine(MoveTransformRoutine(startPosition, endPosition, emergeDuration));

        SetCharacterControllerEnabled(true);
    }

    // 지정된 시작 위치에서 목표 위치까지 Transform을 보간 이동합니다
    private IEnumerator MoveTransformRoutine(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        if (duration <= 0f)
        {
            transform.position = FixDepthVector(endPosition);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = Mathf.Clamp01(elapsed / duration);
            transform.position = FixDepthVector(Vector3.Lerp(startPosition, endPosition, ratio));

            yield return null;
        }

        transform.position = FixDepthVector(endPosition);
    }

    // 플레이어에게서 먼 위치의 재등장 좌표를 계산합니다
    private Vector3 FindReappearPosition()
    {
        Vector3 fallbackPosition = transform.position;

        if (Target == null)
        {
            return FixDepthVector(fallbackPosition);
        }

        float randomDistance = Random.Range(
            -reappearDistanceRandomRange,
            reappearDistanceRandomRange
        );

        float finalDistance = Mathf.Max(0f, reappearDistanceFromPlayer + randomDistance);
        int sideSign = GetNextReappearSideSign();
        float candidateX = Target.position.x + sideSign * finalDistance;
        float candidateY = useTargetYForReappear ? Target.position.y + reappearYOffset : lastSurfaceY + reappearYOffset;

        Vector3 position = new Vector3(candidateX, candidateY, FixedZ);
        return FixDepthVector(position);
    }

    // 다음 재등장 방향을 결정합니다
    private int GetNextReappearSideSign()
    {
        if (!alternateReappearSide)
        {
            return Random.value >= 0.5f ? 1 : -1;
        }

        reappearSideSign *= -1;
        return reappearSideSign;
    }

    // 플레이어 현재 위치를 향해 직선 탄환을 발사합니다
    private void ShootAtTarget()
    {
        Vector3 firePosition = GetFirePosition();
        Vector3 shootDirection = GetDirectionToTarget(firePosition);
        float damage = GetAttackPower(fallbackProjectileDamage);

        ProjectileLaunchData launchData = new ProjectileLaunchData(
            firePosition,
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

    // 현재 바라보는 방향 기준으로 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePosition()
    {
        Vector3 offset = GetFacingOffset(firePointOffset);
        return FixDepthVector(transform.position + offset);
    }

    // Renderer 표시 여부를 바꿉니다
    private void SetVisible(bool visible)
    {
        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = visible;
        }
    }

    // 사망 상태에 들어갈 때 숨겨진 Renderer와 Collider를 복구합니다
    protected override void OnDeadStateEntered()
    {
        SetVisible(true);
        SetCharacterControllerEnabled(false);
        isRunningPattern = false;
    }

    // Scene 뷰에서 감지 범위와 발사 위치를 표시합니다
    private void OnDrawGizmosSelected()
    {
        DrawDetectionGizmo();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetFirePositionForGizmo(), 0.15f);
    }

    // Gizmo 표시용 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePositionForGizmo()
    {
        float direction = transform.localScale.x >= 0f ? 1f : -1f;

        Vector3 offset = new Vector3(
            firePointOffset.x * direction,
            firePointOffset.y,
            firePointOffset.z
        );

        Vector3 firePosition = transform.position + offset;
        firePosition.z = FixedZ;

        return firePosition;
    }
}
