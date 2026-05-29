using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyBurrowRangedAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float fixedZ = 0f;
    [SerializeField] private float raycastStartHeight = 10f;
    [SerializeField] private float raycastDistance = 25f;
    [SerializeField] private float groundSpawnHeightOffset = 1.05f;

    [Header("Detect")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float verticalTolerance = 4f;

    [Header("Burrow")]
    [SerializeField] private float burrowDepth = 2.5f;
    [SerializeField] private float burrowDownDuration = 0.35f;
    [SerializeField] private float hiddenDuration = 0.6f;
    [SerializeField] private float emergeDuration = 0.35f;
    [SerializeField] private float reappearDistanceFromPlayer = 6f;
    [SerializeField] private float reappearDistanceRandomRange = 2f;
    [SerializeField] private bool alternateReappearSide = true;

    [Header("Shoot")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Vector3 firePointOffset = new Vector3(0.7f, 0.4f, 0f);
    [SerializeField] private Vector3 aimOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float shootWindup = 0.25f;
    [SerializeField] private float patternCooldown = 1.5f;

    private CharacterController characterController;
    private Damageable selfDamageable;
    private Renderer[] renderers;
    private float lastPatternEndTime = -999f;
    private bool isRunningPattern;
    private bool facingRight = false;
    private int reappearSideSign = 1;

    // 필요한 컴포넌트 참조를 준비합니다
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        selfDamageable = GetComponent<Damageable>();
        renderers = GetComponentsInChildren<Renderer>();

        FixDepthPosition();
    }

    // 매 프레임 대상 감지와 땅파기 패턴 시작을 처리합니다
    private void Update()
    {
        if (selfDamageable != null && !selfDamageable.IsAlive)
        {
            return;
        }

        TryFindTarget();

        if (target == null)
        {
            return;
        }

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

    // 대상이 감지 범위 안에 있는지 확인합니다
    private bool IsTargetInDetectionRange()
    {
        float distanceX = Mathf.Abs(target.position.x - transform.position.x);
        float distanceY = Mathf.Abs(target.position.y - transform.position.y);

        return distanceX <= detectionRange && distanceY <= verticalTolerance;
    }

    // 땅속으로 숨고 다른 위치에서 나온 뒤 원거리 공격을 실행합니다
    private IEnumerator BurrowPatternRoutine()
    {
        isRunningPattern = true;

        yield return StartCoroutine(BurrowDownRoutine());

        SetVisible(false);

        Vector3 emergePosition = FindReappearPosition();
        Vector3 hiddenPosition = emergePosition + Vector3.down * burrowDepth;
        transform.position = FixDepthVector(hiddenPosition);

        yield return new WaitForSeconds(hiddenDuration);

        SetVisible(true);

        yield return StartCoroutine(EmergeUpRoutine(emergePosition));

        FaceTarget();

        yield return new WaitForSeconds(shootWindup);

        ShootAtTarget();

        lastPatternEndTime = Time.time;
        isRunningPattern = false;
    }

    // 현재 위치에서 아래로 내려가며 사라지는 연출을 처리합니다
    private IEnumerator BurrowDownRoutine()
    {
        characterController.enabled = false;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * burrowDepth;

        yield return StartCoroutine(MoveTransformRoutine(startPosition, endPosition, burrowDownDuration));
    }

    // 땅 아래 위치에서 Ground 위로 올라오는 연출을 처리합니다
    private IEnumerator EmergeUpRoutine(Vector3 emergePosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = emergePosition;

        yield return StartCoroutine(MoveTransformRoutine(startPosition, endPosition, emergeDuration));

        characterController.enabled = true;
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

    // 플레이어에게서 먼 Ground 위 재등장 위치를 찾습니다
    private Vector3 FindReappearPosition()
    {
        Vector3 fallbackPosition = transform.position;

        float randomDistance = Random.Range(
            -reappearDistanceRandomRange,
            reappearDistanceRandomRange
        );

        int sideSign = GetNextReappearSideSign();
        float candidateX = target.position.x + sideSign * (reappearDistanceFromPlayer + randomDistance);

        Vector3 rayOrigin = new Vector3(
            candidateX,
            target.position.y + raycastStartHeight,
            fixedZ
        );

        Ray ray = new Ray(rayOrigin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 position = hit.point + Vector3.up * groundSpawnHeightOffset;
            position.z = fixedZ;
            return position;
        }

        Debug.LogWarning("Ground was not found for burrow reappear position");

        fallbackPosition.z = fixedZ;
        return fallbackPosition;
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

    // 플레이어가 있는 방향을 바라봅니다
    private void FaceTarget()
    {
        if (target == null)
        {
            return;
        }

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

    // 플레이어 현재 위치를 향해 직선 탄환을 발사합니다
    private void ShootAtTarget()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab is missing");
            return;
        }

        if (target == null)
        {
            return;
        }

        Vector3 firePosition = GetFirePosition();
        Vector3 aimPosition = target.position + aimOffset;
        Vector3 shootDirection = aimPosition - firePosition;
        shootDirection.z = 0f;

        EnemyProjectile projectile = Instantiate(
            projectilePrefab,
            firePosition,
            Quaternion.identity
        );

        projectile.Initialize(shootDirection, projectileSpeed, projectileDamage, gameObject);
    }

    // 현재 바라보는 방향 기준으로 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePosition()
    {
        float direction = facingRight ? 1f : -1f;

        Vector3 offset = new Vector3(
            firePointOffset.x * direction,
            firePointOffset.y,
            firePointOffset.z
        );

        Vector3 firePosition = transform.position + offset;
        firePosition.z = fixedZ;

        return firePosition;
    }

    // Renderer 표시 여부를 바꿉니다
    private void SetVisible(bool visible)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = visible;
        }
    }

    // Vector3의 Z 위치를 고정합니다
    private Vector3 FixDepthVector(Vector3 position)
    {
        position.z = fixedZ;
        return position;
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    private void FixDepthPosition()
    {
        transform.position = FixDepthVector(transform.position);
    }

    // Scene 뷰에서 감지 범위와 발사 위치를 표시합니다
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(detectionRange * 2f, verticalTolerance * 2f, 1f)
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetFirePositionForGizmo(), 0.15f);
    }

    // Gizmo 표시용 탄환 생성 위치를 계산합니다
    private Vector3 GetFirePositionForGizmo()
    {
        float direction = facingRight ? 1f : -1f;

        Vector3 offset = new Vector3(
            firePointOffset.x * direction,
            firePointOffset.y,
            firePointOffset.z
        );

        Vector3 firePosition = transform.position + offset;
        firePosition.z = fixedZ;

        return firePosition;
    }
}