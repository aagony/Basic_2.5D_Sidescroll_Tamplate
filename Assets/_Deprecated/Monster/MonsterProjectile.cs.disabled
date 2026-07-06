using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class MonsterProjectile : MonoBehaviour, IParryableProjectile
{
    private const string DefaultHitLayerName = "Player";

    [Header("Movement")]
    [SerializeField] private float defaultSpeed = 25f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float fixedZ = 0f;

    [Header("Hit")]
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private LayerMask destroyMask;

    [Header("Parry")]
    [SerializeField] private bool canBeParried = true;
    [SerializeField] private Color parryReadyColor = Color.yellow;

    private Vector3 moveDirection;
    private float speed;
    private float damage;
    private bool isInitialized;
    private bool isHit;
    private GameObject owner;
    private Collider projectileCollider;
    private float projectileRadius = 0.1f;
    private Renderer[] targetRenderers;
    private Material[][] targetMaterials;
    private Color[][] originalColors;
    private string[][] colorProperties;
    private bool isParryReadyVisualActive;

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

        EnsureDefaultHitMask();
        CacheVisualMaterials();
        FixDepthPosition();
    }

    // 인스펙터 값 변경 시 기본 레이어 마스크를 보정합니다
    private void OnValidate()
    {
        EnsureDefaultHitMask();
    }

    // 발사 시점에 이동 방향과 데미지와 발사자를 설정합니다
    public void Init(Vector3 direction, float projectileSpeed, float attackDamage, GameObject attackOwner = null)
    {
        Vector3 normalizedDirection = direction.normalized;

        if (normalizedDirection == Vector3.zero)
        {
            normalizedDirection = Vector3.right;
        }

        moveDirection = normalizedDirection;
        speed = projectileSpeed > 0f ? projectileSpeed : defaultSpeed;
        damage = attackDamage;
        owner = attackOwner;
        isInitialized = true;

        Destroy(gameObject, lifeTime);
    }

    // 매 프레임 투사체를 이동시키고 이동 경로 충돌을 검사합니다
    private void Update()
    {
        if (!isInitialized || isHit)
        {
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

    // 플레이어 패링 입력이 성공했을 때 투사체를 제거합니다
    public bool TryParry(GameObject parryOwner)
    {
        if (!canBeParried)
        {
            return false;
        }

        if (isHit)
        {
            return false;
        }

        isHit = true;
        owner = parryOwner;
        Destroy(gameObject);
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

    // 빠른 투사체가 프레임 사이에 대상을 관통하지 않도록 이동 경로를 검사합니다
    private bool TryHitAlongMovement(Vector3 movement)
    {
        float distance = movement.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            return false;
        }

        int combinedMask = hitMask.value | destroyMask.value;

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

        int closestHitIndex = -1;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;

            if (ShouldIgnoreCollider(hitCollider))
            {
                continue;
            }

            if (!IsInLayerMask(hitCollider.gameObject, hitMask) && !IsInLayerMask(hitCollider.gameObject, destroyMask))
            {
                continue;
            }

            if (hits[i].distance >= closestDistance)
            {
                continue;
            }

            closestHitIndex = i;
            closestDistance = hits[i].distance;
        }

        if (closestHitIndex < 0)
        {
            return false;
        }

        RaycastHit hit = hits[closestHitIndex];
        Vector3 hitPoint = hit.collider.ClosestPoint(
            transform.position + moveDirection * hit.distance
        );

        transform.position = FixDepthVector(hitPoint);
        HandleHit(hit.collider, hitPoint);
        return true;
    }

    // 충돌한 대상에 따라 데미지 또는 파괴 처리를 수행합니다
    private void HandleHit(Collider other, Vector3 hitPoint)
    {
        if (isHit)
        {
            return;
        }

        if (ShouldIgnoreCollider(other))
        {
            return;
        }

        if (IsInLayerMask(other.gameObject, hitMask))
        {
            HitPlayer(other, hitPoint);
            return;
        }

        if (IsInLayerMask(other.gameObject, destroyMask))
        {
            isHit = true;
            Destroy(gameObject);
        }
    }

    // PlayerStatus에 DamageInfo를 전달하고 투사체를 제거합니다
    private void HitPlayer(Collider other, Vector3 hitPoint)
    {
        isHit = true;

        PlayerStatus playerStatus = other.GetComponentInParent<PlayerStatus>();

        if (playerStatus != null)
        {
            DamageInfo damageInfo = new DamageInfo(
                playerStatus.gameObject,
                other,
                owner,
                hitPoint,
                moveDirection,
                damage
            );

            playerStatus.TakeDamage(damageInfo);
        }
        else
        {
            Debug.LogWarning("PlayerStatus was not found on hit target: " + other.gameObject.name);
        }

        Destroy(gameObject);
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

        return IsOwnerCollider(other);
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
    private void EnsureDefaultHitMask()
    {
        if (hitMask.value != 0 && hitMask.value != ~0)
        {
            return;
        }

        int playerMask = LayerMask.GetMask(DefaultHitLayerName);

        if (playerMask == 0)
        {
            return;
        }

        hitMask = playerMask;
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
}
