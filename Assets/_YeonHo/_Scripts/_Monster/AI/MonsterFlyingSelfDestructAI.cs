using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterFlyingMotor))]
public class MonsterFlyingSelfDestructAI : MonsterBase
{
    private const string DefaultPlayerLayerName = "Player";

    private enum SelfDestructState
    {
        Patrol,
        Chase,
        Warning,
        Dive,
        Exploded
    }

    [Header("Flying")]
    [SerializeField] private MonsterFlyingMotor flyingMotor;
    [SerializeField] private float patrolSpeedMultiplier = 1f;
    [SerializeField] private float chaseSpeedMultiplier = 1.25f;
    [SerializeField] private float hoverHeightAboveTarget = 3.5f;
    [SerializeField] private float flyingDetectionVerticalRange = 8f;
    [SerializeField] private float chaseStopDistance = 0.15f;

    [Header("Self Destruct")]
    [SerializeField] private float prepareHorizontalRange = 3f;
    [SerializeField] private float prepareVerticalRange = 5f;
    [SerializeField] private float prepareDuration = 1f;
    [SerializeField] private float diveSpeed = 14f;
    [SerializeField] private float explodeDistance = 0.6f;
    [SerializeField] private float maxDiveDuration = 2f;

    [Header("Explosion")]
    [SerializeField] private LayerMask playerHitMask;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float fallbackExplosionDamage = 25f;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private bool killSelfOnExplosion = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string prepareTriggerName = "Prepare";
    [SerializeField] private string diveTriggerName = "Dive";
    [SerializeField] private string explodeTriggerName = "Explode";

    private SelfDestructState currentState = SelfDestructState.Patrol;
    private bool isRunningRoutine;

    protected override bool UsesCharacterMotor
    {
        get { return false; }
    }

    // 공중 자폭 몬스터에 필요한 참조와 기본 마스크를 준비합니다
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

        EnsureDefaultPlayerHitMask();
    }

    // 인스펙터 값 변경 시 기본 레이어 마스크를 보정합니다
    private void OnValidate()
    {
        EnsureDefaultPlayerHitMask();
    }

    // 공중 자폭 몬스터의 순찰과 추적과 자폭 준비를 처리합니다
    protected override void TickMonster()
    {
        if (flyingMotor == null)
        {
            return;
        }

        if (isRunningRoutine || currentState == SelfDestructState.Exploded)
        {
            return;
        }

        if (!IsTargetInFlyingDetectionRange())
        {
            currentState = SelfDestructState.Patrol;
            Patrol();
            return;
        }

        FaceTarget();

        if (IsTargetInPrepareRange())
        {
            StartCoroutine(SelfDestructRoutine());
            return;
        }

        currentState = SelfDestructState.Chase;
        ChaseTargetInAir();
    }

    // 플레이어가 공중 몬스터 전용 감지 범위 안에 있는지 확인합니다
    private bool IsTargetInFlyingDetectionRange()
    {
        return IsTargetInRange(DetectionRange, flyingDetectionVerticalRange);
    }

    // 플레이어가 자폭 준비 범위 안에 있는지 확인합니다
    private bool IsTargetInPrepareRange()
    {
        return IsTargetInRange(prepareHorizontalRange, prepareVerticalRange);
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

    // 자폭 준비 후 플레이어를 향해 돌진하고 폭발합니다
    private IEnumerator SelfDestructRoutine()
    {
        isRunningRoutine = true;
        currentState = SelfDestructState.Warning;

        if (animator != null && !string.IsNullOrEmpty(prepareTriggerName))
        {
            animator.SetTrigger(prepareTriggerName);
        }

        Vector3 preparePosition = transform.position;
        flyingMotor.BeginShake();

        float prepareElapsed = 0f;

        while (prepareElapsed < prepareDuration)
        {
            if (IsDead)
            {
                StopSelfDestructRoutine();
                yield break;
            }

            prepareElapsed += Time.deltaTime;
            FaceTarget();
            flyingMotor.HoldAt(preparePosition);
            yield return null;
        }

        flyingMotor.EndShake();
        currentState = SelfDestructState.Dive;

        if (animator != null && !string.IsNullOrEmpty(diveTriggerName))
        {
            animator.SetTrigger(diveTriggerName);
        }

        float diveElapsed = 0f;

        while (diveElapsed < maxDiveDuration)
        {
            if (IsDead)
            {
                StopSelfDestructRoutine();
                yield break;
            }

            diveElapsed += Time.deltaTime;
            FaceTarget();

            Vector3 targetPosition = GetDiveTargetPosition();
            bool reached = flyingMotor.MoveTowardPosition(
                targetPosition,
                diveSpeed,
                explodeDistance,
                false
            );

            if (reached)
            {
                Explode();
                yield break;
            }

            yield return null;
        }

        Explode();
    }

    // 자폭 루틴을 중단하고 이동 연출 상태를 정리합니다
    private void StopSelfDestructRoutine()
    {
        flyingMotor.EndShake();
        isRunningRoutine = false;
    }

    // 돌진 목표 위치를 계산합니다
    private Vector3 GetDiveTargetPosition()
    {
        if (Target == null)
        {
            return FixDepthVector(transform.position + GetFacingDirectionVector());
        }

        return FixDepthVector(GetTargetAimPosition());
    }

    // 폭발 이펙트와 범위 피해와 자폭 사망을 처리합니다
    private void Explode()
    {
        if (currentState == SelfDestructState.Exploded)
        {
            return;
        }

        currentState = SelfDestructState.Exploded;
        isRunningRoutine = false;
        flyingMotor.EndShake();

        if (animator != null && !string.IsNullOrEmpty(explodeTriggerName))
        {
            animator.SetTrigger(explodeTriggerName);
        }

        SpawnExplosionEffect();
        ApplyExplosionDamage();

        if (killSelfOnExplosion)
        {
            KillSelfByExplosion();
        }
    }

    // 폭발 위치에 이펙트 프리팹을 생성합니다
    private void SpawnExplosionEffect()
    {
        if (explosionEffectPrefab == null)
        {
            return;
        }

        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
    }

    // 폭발 반경 안의 IDamageable 대상에게 범위 피해를 줍니다
    private void ApplyExplosionDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            playerHitMask,
            QueryTriggerInteraction.Ignore
        );

        HashSet<Object> damagedTargets = new HashSet<Object>();

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable damageable = hits[i].GetComponentInParent<IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            Object damageTargetKey = GetDamageTargetKey(hits[i], damageable);

            if (damageTargetKey != null && damagedTargets.Contains(damageTargetKey))
            {
                continue;
            }

            if (!damageable.CanTakeDamage)
            {
                continue;
            }

            if (damageTargetKey != null)
            {
                damagedTargets.Add(damageTargetKey);
            }

            ApplyDamageToTarget(damageable, hits[i]);
        }
    }

    // IDamageable에 데미지만 전달하고 피격 정보는 이벤트로 알립니다
    private void ApplyDamageToTarget(IDamageable damageable, Collider hitCollider)
    {
        Vector3 hitPoint = hitCollider.ClosestPoint(transform.position);
        Vector3 hitDirection = GetDamageDirection(hitCollider);
        float damage = GetAttackPower(fallbackExplosionDamage);

        damageable.TakeDamage(damage);
        PublishDamageHitEvent(GetDamageableGameObject(hitCollider, damageable), hitCollider, hitPoint, hitDirection, damage);
    }

    // 폭발 중심에서 대상 방향으로 향하는 피격 방향을 계산합니다
    private Vector3 GetDamageDirection(Collider hitCollider)
    {
        Vector3 hitDirection = hitCollider.transform.position - transform.position;
        hitDirection.z = 0f;

        if (hitDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return GetFacingDirectionVector();
        }

        return hitDirection.normalized;
    }

    // 중복 데미지 방지에 사용할 대상을 반환합니다
    private Object GetDamageTargetKey(Collider hitCollider, IDamageable damageable)
    {
        Object damageableObject = damageable as Object;

        if (damageableObject != null)
        {
            return damageableObject;
        }

        if (hitCollider.attachedRigidbody != null)
        {
            return hitCollider.attachedRigidbody;
        }

        return hitCollider.transform.root;
    }

    // 피격 이벤트에 사용할 대상 오브젝트를 반환합니다
    private GameObject GetDamageableGameObject(Collider hitCollider, IDamageable damageable)
    {
        Component damageableComponent = damageable as Component;

        if (damageableComponent != null)
        {
            return damageableComponent.gameObject;
        }

        return hitCollider.gameObject;
    }

    // 데미지 적용 사실을 이벤트 버스로 알립니다
    private void PublishDamageHitEvent(GameObject targetObject, Collider hitCollider, Vector3 hitPoint, Vector3 hitDirection, float damage)
    {
        DamageHitEvent hitEvent = new DamageHitEvent(
            targetObject,
            gameObject,
            hitCollider,
            hitPoint,
            hitDirection,
            damage
        );

        EventBus<DamageHitEvent>.Publish(hitEvent);
    }

    // 자폭 후 자신의 체력을 0으로 만들어 기존 사망 흐름을 사용합니다
    private void KillSelfByExplosion()
    {
        if (SelfHealth == null || IsDead)
        {
            Destroy(gameObject);
            return;
        }

        SelfHealth.Kill();
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

    // 사망 상태에 들어갈 때 자폭 루틴과 떨림 연출을 정리합니다
    protected override void OnDeadStateEntered()
    {
        if (flyingMotor != null)
        {
            flyingMotor.EndShake();
        }

        currentState = SelfDestructState.Exploded;
        isRunningRoutine = false;
        SetCharacterControllerEnabled(false);
    }

    // Scene 뷰에서 감지 범위와 자폭 준비 범위와 폭발 범위를 표시합니다
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(DetectionRange * 2f, flyingDetectionVerticalRange * 2f, 1f)
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(prepareHorizontalRange * 2f, prepareVerticalRange * 2f, 1f)
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
