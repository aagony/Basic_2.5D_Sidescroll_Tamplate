using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(SphereCollider))]
public class CommonProjectile : MonoBehaviour, IParryableProjectile
{
    private const string DefaultPlayerLayerName = "Player";

    [Header("Movement")]
    [SerializeField] private float defaultSpeed = 25f;
    [SerializeField] private float defaultLifeTime = 3f;
    [SerializeField] private float fixedZ = 0f;

    [Header("Hit")]
    [SerializeField] private LayerMask damageMask = ~0;
    [SerializeField] private LayerMask blockMask;
    [SerializeField] private bool ignoreOwner = true;
    [SerializeField] private bool fallbackToPlayerStatus = true;

    [Header("Parry")]
    [SerializeField] private bool canBeParried = true;
    [SerializeField] private Color parryReadyColor = Color.yellow;

    private IObjectPool<CommonProjectile> pool;
    private Vector3 moveDirection;
    private float speed;
    private float damage;
    private float lifeTime;
    private float elapsedLifeTime;
    private int remainingPenetrationCount;
    private bool isActiveProjectile;
    private bool isHit;
    private bool isInPool = true;
    private GameObject owner;
    private Collider projectileCollider;
    private float projectileRadius = 0.1f;
    private Renderer[] targetRenderers;
    private Material[][] targetMaterials;
    private Color[][] originalColors;
    private string[][] colorProperties;
    private bool isParryReadyVisualActive;
    private readonly List<Object> damagedTargets = new List<Object>(8);

    public Vector3 Position
    {
        get { return transform.position; }
    }

    // 투사체에 필요한 컴포넌트와 시각 정보를 준비합니다
    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        projectileCollider.isTrigger = true;
        projectileRadius = GetProjectileRadius(projectileCollider);

        EnsureDefaultDamageMask();
        CacheVisualMaterials();
        FixDepthPosition();
    }

    // 인스펙터 값 변경 시 기본 레이어 마스크를 보정합니다
    private void OnValidate()
    {
        EnsureDefaultDamageMask();
    }

    // 매 프레임 투사체 수명과 이동을 처리합니다
    private void Update()
    {
        if (!isActiveProjectile || isHit)
        {
            return;
        }

        elapsedLifeTime += Time.deltaTime;

        if (elapsedLifeTime >= lifeTime)
        {
            ReturnToPoolOrDestroy();
            return;
        }

        Vector3 movement = moveDirection * speed * Time.deltaTime;

        if (TryHitAlongMovement(movement))
        {
            return;
        }

        transform.position = FixDepthVector(transform.position + movement);
    }

    // Trigger Collider에 닿았을 때 충돌 처리를 시도합니다
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other, transform.position);
    }

    // 오브젝트 풀 참조를 설정합니다
    public void SetPool(IObjectPool<CommonProjectile> projectilePool)
    {
        pool = projectilePool;
    }

    // 풀에서 꺼낸 직후 상태를 표시합니다
    public void MarkTakenFromPool()
    {
        isInPool = false;
    }

    // 발사 시점의 위치와 방향과 데미지를 설정합니다
    public void Launch(ProjectileLaunchData launchData)
    {
        Vector3 normalizedDirection = launchData.Direction.normalized;

        if (normalizedDirection == Vector3.zero)
        {
            normalizedDirection = Vector3.right;
        }

        transform.position = FixDepthVector(launchData.Position);
        moveDirection = normalizedDirection;
        speed = launchData.Speed > 0f ? launchData.Speed : defaultSpeed;
        damage = launchData.Damage;
        owner = launchData.Owner;
        remainingPenetrationCount = launchData.PenetrationCount;
        lifeTime = launchData.LifeTime > 0f ? launchData.LifeTime : defaultLifeTime;
        elapsedLifeTime = 0f;
        isActiveProjectile = true;
        isHit = false;
        damagedTargets.Clear();
        RestoreOriginalColors();
    }

    // 플레이어 패링 입력이 성공했을 때 투사체를 반납합니다
    public bool TryParry(GameObject parryOwner)
    {
        if (!canBeParried || isHit)
        {
            return false;
        }

        isHit = true;
        owner = parryOwner;
        ReturnToPoolOrDestroy();
        return true;
    }

    // 전달된 오브젝트가 이 투사체의 발사자인지 확인합니다
    public bool IsOwnedBy(GameObject candidate)
    {
        if (candidate == null || owner == null)
        {
            return false;
        }

        return owner == candidate || owner.transform.IsChildOf(candidate.transform);
    }

    // 패리 가능 범위 안에 있을 때 시각 표시를 켜거나 끕니다
    public void SetParryReadyVisual(bool isReady)
    {
        if (!canBeParried)
        {
            return;
        }

        if (isParryReadyVisualActive == isReady)
        {
            return;
        }

        isParryReadyVisualActive = isReady;

        if (isReady)
        {
            SetColor(parryReadyColor);
            return;
        }

        RestoreOriginalColors();
    }

    // 풀에 반납되기 전에 내부 상태를 초기화합니다
    public void ResetForPool()
    {
        isActiveProjectile = false;
        isHit = false;
        isParryReadyVisualActive = false;
        elapsedLifeTime = 0f;
        remainingPenetrationCount = 0;
        owner = null;
        damagedTargets.Clear();
        RestoreOriginalColors();
        FixDepthPosition();
    }

    // 빠른 투사체가 프레임 사이에 대상을 관통하지 않도록 이동 경로를 검사합니다
    private bool TryHitAlongMovement(Vector3 movement)
    {
        float distance = movement.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            return false;
        }

        int combinedMask = damageMask.value | blockMask.value;

        if (combinedMask == 0)
        {
            return false;
        }

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            projectileRadius,
            moveDirection,
            distance,
            combinedMask,
            QueryTriggerInteraction.Collide
        );

        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        SortHitsByDistance(hits);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;

            if (ShouldIgnoreCollider(hitCollider))
            {
                continue;
            }

            if (!IsInLayerMask(hitCollider.gameObject, damageMask) && !IsInLayerMask(hitCollider.gameObject, blockMask))
            {
                continue;
            }

            Vector3 hitPoint = hitCollider.ClosestPoint(transform.position + moveDirection * hits[i].distance);
            transform.position = FixDepthVector(hitPoint);

            bool consumed = HandleHit(hitCollider, hitPoint);

            if (consumed)
            {
                return true;
            }
        }

        return false;
    }

    // 충돌 정보를 가까운 순서로 정렬합니다
    private void SortHitsByDistance(RaycastHit[] hits)
    {
        System.Array.Sort(hits, CompareRaycastHitDistance);
    }

    // 두 충돌 정보의 거리를 비교합니다
    private int CompareRaycastHitDistance(RaycastHit a, RaycastHit b)
    {
        return a.distance.CompareTo(b.distance);
    }

    // 충돌한 대상에 따라 데미지 또는 차단 처리를 수행합니다
    private bool HandleHit(Collider other, Vector3 hitPoint)
    {
        if (isHit)
        {
            return true;
        }

        if (ShouldIgnoreCollider(other))
        {
            return false;
        }

        if (IsInLayerMask(other.gameObject, blockMask))
        {
            isHit = true;
            ReturnToPoolOrDestroy();
            return true;
        }

        if (IsInLayerMask(other.gameObject, damageMask))
        {
            return TryDamageTarget(other, hitPoint);
        }

        return false;
    }

    // 데미지를 받을 수 있는 대상에게 DamageInfo를 전달합니다
    private bool TryDamageTarget(Collider other, Vector3 hitPoint)
    {
        Object damageTargetKey = null;
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageTargetKey = damageable as Object;

            if (damageTargetKey != null && damagedTargets.Contains(damageTargetKey))
            {
                return false;
            }

            if (!damageable.CanTakeDamage)
            {
                return false;
            }

            DamageInfo damageInfo = CreateDamageInfo(other, hitPoint);
            damageable.TakeDamage(damageInfo);

            if (damageTargetKey != null)
            {
                damagedTargets.Add(damageTargetKey);
            }

            return ConsumePenetrationOrContinue();
        }

        if (fallbackToPlayerStatus)
        {
            PlayerStatus playerStatus = other.GetComponentInParent<PlayerStatus>();

            if (playerStatus != null)
            {
                damageTargetKey = playerStatus;

                if (damagedTargets.Contains(damageTargetKey))
                {
                    return false;
                }

                DamageInfo damageInfo = CreateDamageInfo(other, hitPoint);
                playerStatus.TakeDamage(damageInfo);
                damagedTargets.Add(damageTargetKey);

                return ConsumePenetrationOrContinue();
            }
        }

        return false;
    }

    // DamageInfo 객체를 생성합니다
    private DamageInfo CreateDamageInfo(Collider hitCollider, Vector3 hitPoint)
    {
        return new DamageInfo(
            hitCollider.gameObject,
            hitCollider,
            owner,
            hitPoint,
            moveDirection,
            damage
        );
    }

    // 관통 횟수를 처리하고 투사체 소비 여부를 반환합니다
    private bool ConsumePenetrationOrContinue()
    {
        if (remainingPenetrationCount == -1)
        {
            return false;
        }

        if (remainingPenetrationCount > 0)
        {
            remainingPenetrationCount--;
            return false;
        }

        isHit = true;
        ReturnToPoolOrDestroy();
        return true;
    }

    // 무시해야 하는 Collider인지 확인합니다
    private bool ShouldIgnoreCollider(Collider other)
    {
        if (other == null)
        {
            return true;
        }

        if (other == projectileCollider)
        {
            return true;
        }

        if (other.transform == transform || other.transform.IsChildOf(transform))
        {
            return true;
        }

        if (ignoreOwner && IsOwnerCollider(other))
        {
            return true;
        }

        return false;
    }

    // 발사자와 그 자식 Collider인지 확인합니다
    private bool IsOwnerCollider(Collider other)
    {
        if (owner == null)
        {
            return false;
        }

        return other.gameObject == owner || other.transform.IsChildOf(owner.transform);
    }

    // 대상 오브젝트가 LayerMask에 포함되는지 확인합니다
    private bool IsInLayerMask(GameObject targetObject, LayerMask layerMask)
    {
        return (layerMask.value & (1 << targetObject.layer)) != 0;
    }

    // 기본 Player 레이어 마스크를 설정합니다
    private void EnsureDefaultDamageMask()
    {
        if (damageMask.value != 0 && damageMask.value != ~0)
        {
            return;
        }

        int playerMask = LayerMask.GetMask(DefaultPlayerLayerName);

        if (playerMask == 0)
        {
            return;
        }

        damageMask = playerMask;
    }

    // SphereCast에 사용할 투사체 반지름을 계산합니다
    private float GetProjectileRadius(Collider collider)
    {
        if (collider == null)
        {
            return projectileRadius;
        }

        float maxScale = GetMaxAbsScale(transform.lossyScale);

        if (collider is SphereCollider sphereCollider)
        {
            return Mathf.Max(0.01f, sphereCollider.radius * maxScale);
        }

        if (collider is CapsuleCollider capsuleCollider)
        {
            return Mathf.Max(0.01f, capsuleCollider.radius * maxScale);
        }

        return Mathf.Max(0.01f, collider.bounds.extents.magnitude);
    }

    // Vector3 스케일 중 가장 큰 절댓값을 반환합니다
    private float GetMaxAbsScale(Vector3 scale)
    {
        return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
    }

    // 패리 가능 표시를 위해 Renderer와 머티리얼 색상 정보를 저장합니다
    private void CacheVisualMaterials()
    {
        targetRenderers = GetComponentsInChildren<Renderer>();
        targetMaterials = new Material[targetRenderers.Length][];
        originalColors = new Color[targetRenderers.Length][];
        colorProperties = new string[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            targetMaterials[i] = targetRenderers[i].materials;
            originalColors[i] = new Color[targetMaterials[i].Length];
            colorProperties[i] = new string[targetMaterials[i].Length];

            for (int j = 0; j < targetMaterials[i].Length; j++)
            {
                colorProperties[i][j] = GetColorProperty(targetMaterials[i][j]);

                if (string.IsNullOrEmpty(colorProperties[i][j]))
                {
                    continue;
                }

                originalColors[i][j] = targetMaterials[i][j].GetColor(colorProperties[i][j]);
            }
        }
    }

    // 저장된 머티리얼 색상을 지정 색상으로 바꿉니다
    private void SetColor(Color color)
    {
        if (targetMaterials == null)
        {
            return;
        }

        for (int i = 0; i < targetMaterials.Length; i++)
        {
            for (int j = 0; j < targetMaterials[i].Length; j++)
            {
                if (string.IsNullOrEmpty(colorProperties[i][j]))
                {
                    continue;
                }

                targetMaterials[i][j].SetColor(colorProperties[i][j], color);
            }
        }
    }

    // 투사체 머티리얼 색상을 원래 색상으로 되돌립니다
    private void RestoreOriginalColors()
    {
        if (targetMaterials == null || originalColors == null || colorProperties == null)
        {
            return;
        }

        for (int i = 0; i < targetMaterials.Length; i++)
        {
            for (int j = 0; j < targetMaterials[i].Length; j++)
            {
                if (string.IsNullOrEmpty(colorProperties[i][j]))
                {
                    continue;
                }

                targetMaterials[i][j].SetColor(colorProperties[i][j], originalColors[i][j]);
            }
        }
    }

    // 머티리얼에서 색상 프로퍼티 이름을 찾습니다
    private string GetColorProperty(Material material)
    {
        if (material == null)
        {
            return string.Empty;
        }

        if (material.HasProperty("_BaseColor"))
        {
            return "_BaseColor";
        }

        if (material.HasProperty("_Color"))
        {
            return "_Color";
        }

        return string.Empty;
    }

    // 2.5D 횡스크롤 이동을 위해 Z 위치를 고정합니다
    private void FixDepthPosition()
    {
        transform.position = FixDepthVector(transform.position);
    }

    // Vector3의 Z 위치를 고정합니다
    private Vector3 FixDepthVector(Vector3 position)
    {
        position.z = fixedZ;
        return position;
    }

    // 풀로 반납하거나 풀 참조가 없으면 제거합니다
    private void ReturnToPoolOrDestroy()
    {
        if (isInPool)
        {
            return;
        }

        if (pool != null)
        {
            isInPool = true;
            pool.Release(this);
            return;
        }

        ResetForPool();
        Destroy(gameObject);
    }
}
